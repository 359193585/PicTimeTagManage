/*****************************************
 *
 * 项目名称： PicTimeTagManage  
 * 文 件 名:  Form1Thumbnail.cs
 * 命名空间： PicTimeTagManage
 * 描    述:  
 * 
 * 版    本：  V1.0
 * 创 建 者：  liuxin
 * 电子邮件：  359193585@qq.com(leison)
 * 创建时间：  2025/10/27 13:32
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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PicTimeTagManage
{
    partial class Form1
    {
        // 存储所有图片文件的路径
        private List<string> _imageFiles = new List<string>();
        // 缓存已生成的缩略图，键为文件路径
        private Dictionary<string, Image> _thumbnailCache = new Dictionary<string, Image>();

        // 用于取消异步任务的令牌源
        private CancellationTokenSource _cancellationTokenSource;

        private const int THUMBNAIL_WIDTH = 100;  // 缩略图宽度
        private const int THUMBNAIL_HEIGHT = 100; // 缩略图高度
        private const int PRELOAD_AHEAD = 0;     // 预加载当前可见区域之前的行数
        private const int PRELOAD_BEHIND = 0;    // 预加载当前可见区域之后的行数（可设置大一些）
       
        private void DataGridView1_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _imageFiles.Count) return;
            string imagePath = _imageFiles[e.RowIndex];
            string columnName = dataGridView1.Columns[e.ColumnIndex].Name;
            Debug.WriteLine($"columnName:{columnName}");
            if (checkBoxThumbnail.Checked && columnName == "ImageColumn")
            {
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
                    //Application.DoEvents();
                }
                //// 遍历所有现有行，设置行高
                foreach (DataGridViewRow row in dataGridView1.Rows) row.Height = THUMBNAIL_HEIGHT + 10;
                // 设置当前行高
                //dataGridView1.Rows[e.RowIndex].Height = THUMBNAIL_HEIGHT + 10;
            }
            if (checkBox1.Checked && columnName== "ExifTime")
            {
                // 如果缓存中有，直接使用
                if (_exifTimeCache.ContainsKey(imagePath))
                {
                    _fileBindingList[e.RowIndex].ExifTime = _exifTimeCache[imagePath];
                }
                else
                    _ = ReadExifTime(imagePath, e.RowIndex); // 使用弃元，不等待

            }
            if (checkBox2.Checked && columnName == "ExifGPS")
            {
                // 如果缓存中有，直接使用
                if (_exifGpsCache.ContainsKey(imagePath))
                {
                    _fileBindingList[e.RowIndex].ExifGps = _exifGpsCache[imagePath];
                }
                else
                    _ = ReadExifGps(imagePath, e.RowIndex); // 使用弃元，不等待
            }
           
        }
        // 滚动事件：用于触发对可见区域之外图片的预加载
        private void DataGridView1_Scroll(object sender, ScrollEventArgs e)
        {
            if (e.ScrollOrientation == ScrollOrientation.VerticalScroll)
            {
                // 用户滚动时，触发后台预加载
                //_ = PreloadThumbnailsAsync();
            }
        }

        #region 核心方法：异步加载单张缩略图并更新UI
        private Task LoadThumbnailAsync(string imagePath, int rowIndex)
        {
            return LoadThumbnailAsync(imagePath, rowIndex, isHighPriority: true);
        }
        private async Task LoadThumbnailAsync(string imagePath, int rowIndex, bool isHighPriority = false)
        {
            try
            {
                Image thumbnail = await Task.Run(() =>  new GenerateThumbnail(false).GetThumbnailImg(imagePath, THUMBNAIL_WIDTH, THUMBNAIL_HEIGHT));
                //_fileBindingList[rowIndex].ExifTime = GetExifTime(imagePath);//顺便读取时间
                //_fileBindingList[rowIndex].ExifGps = GetExifGps(imagePath);
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
                this.Invoke(new Action(() =>
                {
                    _thumbnailCache[imagePath] = Properties.Resources.ErrorImage;
                    dataGridView1.InvalidateRow(rowIndex);
                }));
                Debug.WriteLine($"加载图片 {imagePath} 失败: {ex.Message}");
            }
        }
        #endregion
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

    }
}
