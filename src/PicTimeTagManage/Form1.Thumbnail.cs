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
using System.Linq;
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

            //Debug.WriteLine($"columnName:{columnName}");
            //Debug.WriteLine($"CellValueNeeded：[{Thread.CurrentThread.ManagedThreadId}] GetThumbnail called");

            if (checkBoxThumbnail.Checked && columnName == "ImageColumn")
            {
                if (_thumbnailService.TryGetCached(imagePath, out var thumbnail))
                {
                    e.Value = thumbnail;
                    return;
                }
                e.Value = Properties.Resources.LoadingPlaceholder;

                // 触发服务请求（不在乎是否缓存）
                    _ = _thumbnailService.LoadThumbnailAsync(imagePath, THUMBNAIL_WIDTH, THUMBNAIL_HEIGHT, e.RowIndex);
            }
                
                // 仅调整当前行高
                //dataGridView1.Rows[e.RowIndex].Height = THUMBNAIL_HEIGHT + 10;
            
            if (checkBox1.Checked && columnName == "ExifTime")
            {
                // 如果缓存中有，直接使用
                if (_exifTimeCache.ContainsKey(imagePath))
                {
                    _fileBindingList[e.RowIndex].ExifTime = _exifTimeCache[imagePath];
                }
                else
                    _ = ReadExifTimeAsync(imagePath, e.RowIndex); // 使用弃元，不等待
            }
            if (checkBox2.Checked && columnName == "ExifGPS")
            {
                // 如果缓存中有，直接使用
                if (_exifGpsCache.ContainsKey(imagePath))
                {
                    _fileBindingList[e.RowIndex].ExifGps = _exifGpsCache[imagePath];
                }
                else
                    _ = ReadExifGpsAsync(imagePath, e.RowIndex); // 使用弃元，不等待
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
        private void DataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dataGridView1.Columns[e.ColumnIndex].Name == "ImageColumn" && (e.Value == null || e.Value == DBNull.Value))
            {
                e.Value = Properties.Resources.LoadingPlaceholder;
                e.FormattingApplied = true;
            }
            //Debug.WriteLine($"Formatting cell [{e.RowIndex},{e.ColumnIndex}] at {DateTime.Now:HH:mm:ss.fff}");
        }
        private void DataGridView1_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
        }
        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // 检查是否点击了左上角单元格（行头-1，列头-1）
            if (e.RowIndex == -1 && e.ColumnIndex == -1)
            {
                // 遍历所有行，设置 Selected 属性为 true
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (!row.IsNewRow)
                    {
                        row.Selected = true;
                    }
                }
            }
            pendingRowIndex = e.RowIndex;
            Debug.WriteLine($"pendingFilePath={pendingFilePath}");
        }
        private void DataGridView1_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            // 确保点击的是列头
            if (e.RowIndex == -1 && e.ColumnIndex >= 0)
            {
                string columnName = dataGridView1.Columns[e.ColumnIndex].DataPropertyName; // 获取绑定字段名
                List<FileInfoDisplay> sortedList;

                // 判断排序方向
                // 如果点击的是新列，默认升序；如果点击的是同一列，切换排序方向
                if (_lastSortedColumn != columnName)
                {
                    _currentSortDirection = SortOrder.Ascending;
                }
                else
                {
                    // 切换方向
                    _currentSortDirection = _currentSortDirection == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
                }

                _lastSortedColumn = columnName; // 更新最后一次排序的列

                // 使用 LINQ 进行排序
                switch (_currentSortDirection)
                {
                    case SortOrder.Ascending:
                        sortedList = _fileBindingList.OrderBy(x => GetPropertyValue(x, columnName)).ToList();
                        break;
                    case SortOrder.Descending:
                        sortedList = _fileBindingList.OrderByDescending(x => GetPropertyValue(x, columnName)).ToList();
                        break;
                    default:
                        sortedList = _fileBindingList.OrderBy(x => GetPropertyValue(x, columnName)).ToList();
                        break;
                }

                // 清空绑定列表并重新添加排序后的数据
                _fileBindingList.Clear();
                foreach (var item in sortedList)
                {
                    _fileBindingList.Add(item);
                }

            }
        }

        private void DataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            // 获取选中的行数
            int selectedRowCount = dataGridView1.SelectedRows.Count;

            // 如果至少有一行被选中
            if (selectedRowCount > 0)
            {
                // 假设我们显示第一列的数据，可以根据需要调整
                string firstCellValue = dataGridView1.SelectedRows[0].Cells[0].Value?.ToString() ?? "";
                label5.Text = $"选中了 {selectedRowCount} 行";
            }
            else
            {
                label5.Text = "没有行被选中";
            }

            #region 当DataGridView的选择发生变化时触发读取文件
            // 确保有选中的行
            if (dataGridView1.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = dataGridView1.SelectedRows[0];
                // 全局的当前选择文件路径名 最后一个被选中的行
                pendingFilePath = selectedRow.Cells["FullPath"].Value?.ToString();

                // 停止之前的计时器（如果正在运行）
                isSelectionStable = false;
                //selectionTimer.Stop();
                selectionTimer2.Change(100, Timeout.Infinite);
                // 重新启动计时器，开始新的延迟等待
                //selectionTimer.Start();
            }

            #endregion
        }
        private void ThumbnailService_ThumbnailLoaded(object sender, ThumbnailLoadedEventArgs e)
        {
            if (_isClosing || dataGridView1.IsDisposed || dataGridView1.Disposing)
                return;
            if (!(e.UserState is int rowIndex) || rowIndex < 0 || rowIndex >= dataGridView1.Rows.Count)
                return;

            // 检查该行是否仍然可见
            if (!IsRowVisible(rowIndex)) return;

            int imageColIndex = dataGridView1.Columns["ImageColumn"].Index;
            dataGridView1.InvalidateCell(imageColIndex, rowIndex);
            Debug.WriteLine($"{e.IsSuccess}  {e.UserState}  {e.ImagePath} ");
        }

        #region 核心方法：异步加载单张缩略图并更新UI
       
        //private async Task LoadThumbnailAsync(string imagePath, int rowIndex, bool isHighPriority = false)
        //{
        //    try
        //    {
        //        if (_thumbnailCache.ContainsKey(imagePath)) return;
        //        Image thumbnail = await _thumbnailService.LoadThumbnailAsync(imagePath, THUMBNAIL_WIDTH, THUMBNAIL_HEIGHT);
        //        this.BeginInvoke(new Action(() =>
        //        {
        //            if (!_thumbnailCache.ContainsKey(imagePath))
        //            {
        //                _thumbnailCache[imagePath] = thumbnail;
        //                UpdateThumbnailOnUI(imagePath, rowIndex, Properties.Resources.ErrorImage);
        //            }
        //            else
        //            {
        //                thumbnail.Dispose();
        //            }
        //        }));
        //    }
        //    catch (Exception ex)
        //    {
        //        this.BeginInvoke(new Action(() =>
        //        {
        //            UpdateThumbnailOnUI(imagePath, rowIndex, Properties.Resources.ErrorImage);
        //        }));
        //        Debug.WriteLine($"加载图片 {imagePath} 失败: {ex.Message}");
        //    }
        //}
        //private void UpdateThumbnailOnUI(string imagePath, int rowIndex, Image thumbnail)
        //{
        //    if (IsRowVisible(rowIndex))
        //    {
        //        dataGridView1.InvalidateRow(rowIndex);
        //    }
        //}
        private bool IsRowVisible(int rowIndex)
        {
            return rowIndex >= dataGridView1.FirstDisplayedScrollingRowIndex &&
                   rowIndex < dataGridView1.FirstDisplayedScrollingRowIndex + dataGridView1.DisplayedRowCount(true);
        }
       
        #endregion

        
    }
}
