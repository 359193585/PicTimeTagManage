/*****************************************
 *
 * 项目名称： PicTimeTagManage  
 * 文 件 名:  FormImageBrowser.cs
 * 命名空间： PicTimeTagManage
 * 描    述:  
 * 
 * 版    本：  V1.0
 * 创 建 者：  liuxin
 * 电子邮件：  359193585@qq.com(leison)
 * 创建时间：  2025/10/24 18:50
 * ======================================
 * 历史更新记录
 * 版本：V          修改时间：         修改人：
 * 修改内容：
 * ======================================
*********************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PicTimeTagManage
{
    public partial class ImageBrowserForm : Form
    {
        private SplitContainer splitContainer1;
        private DataGridView dataGridView1;
        private Label lblStatus;
        private Button btnLoadFolder;
        private Button button1;

        // 存储所有图片文件的路径
        private List<string> _imageFiles = new List<string>();
        // 缓存已生成的缩略图，键为文件路径
        private Dictionary<string, Image> _thumbnailCache = new Dictionary<string,Image>();

        // 用于取消异步任务的令牌源
        private CancellationTokenSource _cancellationTokenSource;

        private const int THUMBNAIL_WIDTH = 100;  // 缩略图宽度
        private const int THUMBNAIL_HEIGHT = 100; // 缩略图高度
        private const int PRELOAD_AHEAD = 20;     // 预加载当前可见区域之前的行数
        private const int PRELOAD_BEHIND = 40;    // 预加载当前可见区域之后的行数（可设置大一些）

        public ImageBrowserForm(List<string> ImageFiles)
        {
            _imageFiles = ImageFiles;
            InitializeComponent();
            SetupDataGridView();
          
        }
        private void SetupDataGridView()
        {
            dataGridView1.RowHeadersVisible = false; // 隐藏行头
            dataGridView1.ColumnHeadersVisible = false; // 隐藏列头
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; 
            dataGridView1.CellBorderStyle = DataGridViewCellBorderStyle.None; 
            dataGridView1.RowTemplate.Height = THUMBNAIL_HEIGHT + 10; // 增加一些边距

            DataGridViewImageColumn imageColumn = new DataGridViewImageColumn();
            imageColumn.Name = "ImageColumn";
            imageColumn.ImageLayout = DataGridViewImageCellLayout.Zoom; // 等比例缩放图片，保持不失真
            dataGridView1.Columns.Add(imageColumn);

            // 启用虚拟模式
            dataGridView1.VirtualMode = true;
            dataGridView1.CellValueNeeded += DataGridView1_CellValueNeeded; // 为需要显示的单元格提供数据
                                                                            // 监听滚动事件以触发预加载
            dataGridView1.Scroll += DataGridView1_Scroll;
        }

        // 滚动事件：用于触发对可见区域之外图片的预加载
        private void DataGridView1_Scroll(object sender, ScrollEventArgs e)
        {
            if (e.ScrollOrientation == ScrollOrientation.VerticalScroll)
            {
                // 用户滚动时，触发后台预加载
                _ = PreloadThumbnailsAsync();
            }
        }
        // 异步生成缩略图并更新UI
        private async Task LoadThumbnailAsync(string imagePath, int rowIndex)
        {
            try
            {
                // 在后台线程中执行耗时操作
                Image thumbnail = await Task.Run(() => GenerateThumbnail(imagePath, THUMBNAIL_WIDTH, THUMBNAIL_HEIGHT)); // 按照约定大小，创建缩略图

                // 回到UI线程更新缓存和界面
                this.Invoke(new Action(() =>
                {
                    if (!_thumbnailCache.ContainsKey(imagePath)) // 再次检查，避免重复添加
                    {
                        _thumbnailCache[imagePath] = thumbnail;

                        // 只刷新当前行，优化性能
                        if (rowIndex < dataGridView1.RowCount)
                        {
                            dataGridView1.InvalidateRow(rowIndex);
                        }
                    }
                    else
                    {
                        // 如果已缓存（可能由预加载先完成），则释放刚生成的图片避免内存泄漏
                        thumbnail.Dispose();
                    }
                }));
            }
            catch (Exception ex)
            {
                // 处理错误，例如设置一个错误图片
                this.Invoke(new Action(() =>
                {
                    _thumbnailCache[imagePath] = Properties.Resources.ErrorImage;
                    dataGridView1.InvalidateRow(rowIndex);
                }));
                System.Diagnostics.Debug.WriteLine($"加载图片 {imagePath} 失败: {ex.Message}");
            }
        }
        // 核心方法：异步加载单张缩略图
        private async Task LoadThumbnailAsync(string imagePath, int rowIndex, bool isHighPriority = false)
        {
            try
            {
                Image thumbnail = await Task.Run(() =>
                {
                    // 使用高质量的缩略图生成方法
                    using (FileStream fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                    using (Image originalImage = Image.FromStream(fs))
                    {
                        Bitmap thumbnailBitmap = new Bitmap(THUMBNAIL_WIDTH, THUMBNAIL_HEIGHT);
                        using (Graphics g = Graphics.FromImage(thumbnailBitmap))
                        {
                            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                            g.DrawImage(originalImage, 0, 0, THUMBNAIL_WIDTH, THUMBNAIL_HEIGHT);
                        }
                        return thumbnailBitmap;
                    }
                });

                // 回到UI线程更新缓存和界面
                this.Invoke(new Action(() =>
                {
                    if (!_thumbnailCache.ContainsKey(imagePath))
                    {
                        _thumbnailCache[imagePath] = thumbnail;
                        // 只刷新当前行（如果它在可见区域内）
                        if (rowIndex >= dataGridView1.FirstDisplayedScrollingRowIndex &&
                            rowIndex < dataGridView1.FirstDisplayedScrollingRowIndex + dataGridView1.DisplayedRowCount(true))
                        {
                            dataGridView1.InvalidateRow(rowIndex);
                        }
                    }
                    else
                    {
                        // 如果已缓存（可能由预加载先完成），则释放刚生成的图片避免内存泄漏
                        thumbnail.Dispose();
                    }
                }));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"加载图片 {imagePath} 失败: {ex.Message}");
                // 可以设置一个错误图片
            }
        }
        // 智能预加载方法：加载当前可见区域前后的图片
        private async Task PreloadThumbnailsAsync()
        {
            if (_imageFiles.Count == 0) return;

            int firstVisibleRow = dataGridView1.FirstDisplayedScrollingRowIndex;
            int visibleRowCount = dataGridView1.DisplayedRowCount(true);
            if (firstVisibleRow < 0) return;

            int startPreloadIndex = Math.Max(0, firstVisibleRow - PRELOAD_AHEAD);
            int endPreloadIndex = Math.Min(_imageFiles.Count - 1, firstVisibleRow + visibleRowCount + PRELOAD_BEHIND);

            var token = _cancellationTokenSource.Token;

            // 遍历预加载范围内的所有行
            for (int rowIndex = startPreloadIndex; rowIndex <= endPreloadIndex; rowIndex++)
            {
                // 如果取消请求，则退出循环
                if (token.IsCancellationRequested) break;

                string imagePath = _imageFiles[rowIndex];

                // 如果尚未加载，则启动一个低优先级的异步任务进行预加载
                if (!_thumbnailCache.ContainsKey(imagePath))
                {
                    // 使用Task.Run并配置为低优先级（通过LongRunning提示）
                    await Task.Run(() =>
                    {
                        // 这里可以模拟低优先级，例如在真正密集操作前短暂休眠
                        // Thread.Sleep(10);
                        // 实际加载图片
                        LoadThumbnailAsync(imagePath, rowIndex, isHighPriority: false).ConfigureAwait(false);
                    }).ConfigureAwait(false);
                }
            }
        }
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImageBrowserForm));
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.lblStatus = new System.Windows.Forms.Label();
            this.btnLoadFolder = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(0, 0);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.RowTemplate.Height = 23;
            this.dataGridView1.Size = new System.Drawing.Size(884, 438);
            this.dataGridView1.TabIndex = 0;
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(35, 16);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(41, 12);
            this.lblStatus.TabIndex = 1;
            this.lblStatus.Text = "label1";
            // 
            // btnLoadFolder
            // 
            this.btnLoadFolder.Location = new System.Drawing.Point(37, 55);
            this.btnLoadFolder.Name = "btnLoadFolder";
            this.btnLoadFolder.Size = new System.Drawing.Size(77, 46);
            this.btnLoadFolder.TabIndex = 2;
            this.btnLoadFolder.Text = "button1";
            this.btnLoadFolder.UseVisualStyleBackColor = true;
            this.btnLoadFolder.Click += new System.EventHandler(this.btnLoadFolder_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.dataGridView1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.button1);
            this.splitContainer1.Panel2.Controls.Add(this.lblStatus);
            this.splitContainer1.Panel2.Controls.Add(this.btnLoadFolder);
            this.splitContainer1.Size = new System.Drawing.Size(884, 561);
            this.splitContainer1.SplitterDistance = 438;
            this.splitContainer1.TabIndex = 3;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(143, 55);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(74, 46);
            this.button1.TabIndex = 3;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // ImageBrowserForm
            // 
            this.ClientSize = new System.Drawing.Size(884, 561);
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ImageBrowserForm";
            this.Load += new System.EventHandler(this.ImageBrowserForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private async void LoadInitFiles()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            CancellationToken token = _cancellationTokenSource.Token;

            _thumbnailCache.Clear();
            try
            {
                await Task.Run(() =>
                {
                    ;
                }, token);
                dataGridView1.RowCount = _imageFiles.Count;
                lblStatus.Text = $"已找到 {_imageFiles.Count} 个图片文件。";
            }
            catch (OperationCanceledException)
            {
                lblStatus.Text = "操作已取消。";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载文件夹时出错: {ex.Message}");
            }
        }
        private async void btnLoadFolder_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    // 取消之前的加载任务
                    _cancellationTokenSource?.Cancel();
                    _cancellationTokenSource = new CancellationTokenSource();
                    CancellationToken token = _cancellationTokenSource.Token;

                    // 清空缓存和列表
                    _imageFiles.Clear();
                    _thumbnailCache.Clear();

                    try
                    {
                        // 异步获取所有图片文件路径
                        await Task.Run(() =>
                        {
                            string[] extensions = { "*.jpg", "*.jpeg", "*.png", "*.bmp", "*.gif" };
                            foreach (string ext in extensions)
                            {
                                // 不包含子目录
                                _imageFiles.AddRange(Directory.GetFiles(folderDialog.SelectedPath, ext, SearchOption.TopDirectoryOnly));
                            }
                        }, token);

                        // 设置DataGridView行数，触发虚拟模式
                        dataGridView1.RowCount = _imageFiles.Count;
                        lblStatus.Text = $"已找到 {_imageFiles.Count} 个图片文件。";
                    }
                    catch (OperationCanceledException)
                    {
                        // 任务被取消，正常处理
                        lblStatus.Text = "操作已取消。";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"加载文件夹时出错: {ex.Message}");
                    }
                }
            }
        }

        // 【虚拟模式核心事件】当DataGridView需要显示某个单元格的值时触发
        private void DataGridView1_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _imageFiles.Count) return;
            string imagePath = _imageFiles[e.RowIndex];
            // 如果缓存中已有缩略图，直接使用
            if (_thumbnailCache.ContainsKey(imagePath))
            {
                e.Value = _thumbnailCache[imagePath];
            }
            else
            {
                // 缓存中没有，则使用默认的“加载中”图片占位，并异步加载真实缩略图
                e.Value = Properties.Resources.LoadingPlaceholder; 
                // 异步加载和生成缩略图
                _ = LoadThumbnailAsync(imagePath, e.RowIndex); // 使用弃元，不等待
            }
        }


        // 生成缩略图的辅助方法
        private Image GenerateThumbnail(string imagePath, int width, int height)
        {
            using (FileStream fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
            using (Image originalImage = Image.FromStream(fs))
            {
                // 创建缩略图Bitmap
                Bitmap thumbnail = new Bitmap(width, height);
                using (Graphics g = Graphics.FromImage(thumbnail))
                {
                    // 设置高质量插值模式，使缩略图更清晰
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    g.DrawImage(originalImage, 0, 0, width, height);
                }
                return thumbnail;
            }
        }

        private void ImageBrowserForm_Load(object sender, EventArgs e)
        {
            LoadInitFiles();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            LoadInitFiles();

        }
    }
}
