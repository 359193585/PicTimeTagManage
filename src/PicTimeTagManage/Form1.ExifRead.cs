/*****************************************
 *
 * 项目名称： PicTimeTagManage  
 * 文 件 名:  From1ExifRead.cs
 * 命名空间： PicTimeTagManage
 * 描    述:  
 * 
 * 版    本：  V1.0
 * 创 建 者：  liuxin
 * 电子邮件：  359193585@qq.com(leison)
 * 创建时间：  2025/10/28 18:06
 * ======================================
 * 历史更新记录
 * 版本：V          修改时间：         修改人：
 * 修改内容：
 * ======================================
*********************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PicTimeTagManage
{
    public partial class Form1
    {
        // 缓存已读取的时间值，键为文件路径
        private Dictionary<string, string> _exifTimeCache = new Dictionary<string, string>();
        // 缓存已读取的GGPS值，键为文件路径
        private Dictionary<string, string> _exifGpsCache = new Dictionary<string, string>();
        private readonly object _exifToolLock = new object();
        private async Task ReadExifTimeAsync(string imagePath, int rowIndex)
        {
            try
            {
                string ExifTime = await Task.Run(() =>
                {
                    lock (_exifToolLock)
                    {
                        return GetExifTime(imagePath);
                    }
                });
                Debug.WriteLine($"最终获取到的 ExifTime: {ExifTime}");

                this.Invoke(new Action(() =>
                {
                    //if (!_exitGpsCache.ContainsKey(imagePath))
                    {
                        //_exitGpsCache[imagePath] = ExifGps;
                        _fileBindingList[rowIndex].ExifTime = ExifTime;
                        // 只刷新当前行（如果它在可见区域内）
                        if (rowIndex >= dataGridView1.FirstDisplayedScrollingRowIndex &&
                            rowIndex < dataGridView1.FirstDisplayedScrollingRowIndex + dataGridView1.DisplayedRowCount(true))
                        {
                            dataGridView1.InvalidateRow(rowIndex);
                        }
                    }

                }));
            }
            catch { }
            #region bak
            //string ExifTime ="";
            //try
            //{
            //    //string ExifTime =  GetExifTime(imagePath);
            //    //string ExifTime = await GetExifTimeAsync(imagePath);
            //    //string ExifTime = await Task.Run(() =>
            //    //{
            //    //    lock (_exifToolLock)
            //    //    {
            //    //        try
            //    //        {
            //    //            Console.WriteLine($"[AsyncTask] 开始处理文件: {imagePath}");
            //    //            string result = GetExifTimeAsync(imagePath);
            //    //            Console.WriteLine($"[AsyncTask] 文件 {imagePath} 处理结果: '{result}'");
            //    //            return result;
            //    //        }
            //    //        catch (Exception ex)
            //    //        {
            //    //            // 这里捕获的是在后台线程中执行时发生的异常
            //    //            Console.WriteLine($"[AsyncTask] 处理文件 {imagePath} 时发生异常: {ex.ToString()}");
            //    //            return "N/A (异步任务内部异常)"; // 返回一个明确的错误标识
            //    //        }
            //    //    }
            //    //});

            //    if (!File.Exists(imagePath))
            //    {
            //        Console.WriteLine($"文件不存在: {imagePath}");
            //        return;
            //    }
            //    lock (_exifToolLockExifTime)
            //    {
            //        string receiveStr = "";
            //        try
            //        {
            //            ExifToolProcessor _exifToolProcessor = new ExifToolProcessor(exifProcessName, selectedFolderPath);
            //            StringBuilder sb = new StringBuilder();
            //            sb.AppendLine("-DateTimeOriginal");
            //            sb.AppendLine($"\"{imagePath}\"");
            //            string arguments = sb.ToString().Replace(Environment.NewLine, " ");
            //            receiveStr = _exifToolProcessor.ExecuteCommandBlocking(arguments);
            //            //receiveStr = _exifToolProcessor.ExecuteCommandAsync(arguments);
            //            string pattern = @"\d{4}:\d{2}:\d{2} \d{2}:\d{2}:\d{2}";
            //            Match match = Regex.Match(receiveStr, pattern);
            //            ExifTime =  match.Success ? match.Value : "N/A match.error";
            //            Debug.WriteLine($"最终获取到的ExifTime: {ExifTime}");
            //        }
            //        catch (FileNotFoundException ex)
            //        {
            //            Debug.WriteLine($"读取文件{imagePath}exiftime错误:{ex.Message}");
            //        }
            //        catch (Exception ex)
            //        {
            //            string msgStr = $"GetExifTime 初始化ExifTool处理器时出错: {ex.Message}{ex.StackTrace}";
            //            Debug.WriteLine(msgStr);
            //        }
            //    }

            //    this.Invoke(new Action(() =>
            //    {
            //        //if (!_exitTimeCache.ContainsKey(imagePath))
            //        {
            //            //_exitTimeCache[imagePath] = ExifTime;
            //            _fileBindingList[rowIndex].ExifTime = ExifTime;
            //            // 只刷新当前行（如果它在可见区域内）
            //            if (rowIndex >= dataGridView1.FirstDisplayedScrollingRowIndex &&
            //                rowIndex < dataGridView1.FirstDisplayedScrollingRowIndex + dataGridView1.DisplayedRowCount(true))
            //            {
            //                dataGridView1.InvalidateRow(rowIndex);
            //            }
            //        }

            //    }));
            //}
            //catch { }
            #endregion
        }
        private async Task ReadExifGpsAsync(string imagePath, int rowIndex)
        {
            try
            {
                string ExifGps = await Task.Run(() =>
                {
                    lock (_exifToolLock)
                    {
                        return GetExifGps(imagePath);
                    }
                });
                Debug.WriteLine($"最终获取到的 ExifGps: {ExifGps}");

                this.Invoke(new Action(() =>
                {
                    //if (!_exitGpsCache.ContainsKey(imagePath))
                    {
                        //_exitGpsCache[imagePath] = ExifGps;
                        _fileBindingList[rowIndex].ExifGps = ExifGps;
                        // 只刷新当前行（如果它在可见区域内）
                        if (rowIndex >= dataGridView1.FirstDisplayedScrollingRowIndex &&
                            rowIndex < dataGridView1.FirstDisplayedScrollingRowIndex + dataGridView1.DisplayedRowCount(true))
                        {
                            dataGridView1.InvalidateRow(rowIndex);
                        }
                    }

                }));
            }
            catch { }

        }
    }
}
