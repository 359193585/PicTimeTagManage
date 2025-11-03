/*****************************************
 *
 * 项目名称： PicTimeTagManage  
 * 文 件 名:  Form1.Method.cs
 * 命名空间： PicTimeTagManage
 * 描    述:  
 * 
 * 版    本：  V1.0
 * 创 建 者：  liuxin
 * 电子邮件：  359193585@qq.com(leison)
 * 创建时间：  2025/10/17 9:17
 * ======================================
 * 历史更新记录
 * 版本：V          修改时间：         修改人：
 * 修改内容：
 * ======================================
*********************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PicTimeTagManage
{
    partial class Form1
    {
        private string GenerateUniqueFilePath(string directory, string baseName, string extension)
        {
            string newPath = Path.Combine(directory, baseName + extension);
            int counter = 1;

            while (File.Exists(newPath))
            {
                newPath = Path.Combine(directory, $"{baseName} ({counter}){extension}");
                counter++;
            }
            return newPath;
        }
        /// <summary>
        /// 将标准格式的exif日期信息的转换为文件名和时间
        /// </summary>
        /// <param name="exifTime"></param>
        /// <returns></returns>
        private (string fileNameStr, DateTime fileDatetime) ExifTimeToFilename(string exifTime)
        {
            StringDateInfo stringInfo = StringDateExtractor.ExtractDateInfo(exifTime);
            DateTime fileDatetime = (DateTime)stringInfo.DateTime;
            string fileNameStr = fileDatetime.ToString("yyyy-MM-dd_HHmmss");
            return (fileNameStr, fileDatetime);
        }
        private static void KillExistingExifTool(string exifProcessName)
        {
            if (exifProcessName is null)
            {
                throw new ArgumentNullException(nameof(exifProcessName));
            }

            try
            {
                // 1. 查找所有名为 "exiftool.exe" 的进程
                Process[] existingProcesses = Process.GetProcessesByName(exifProcessName);

                // 2. 遍历并强制终止每一个找到的进程
                foreach (Process process in existingProcesses)
                {
                    try
                    {
                        Console.WriteLine($"正在强制终止 exiftool 进程 (PID: {process.Id})");
                        process.Kill(); // 发送强制终止信号
                        process.WaitForExit(5000); // 等待最多5秒，确保进程完全退出
                        Console.WriteLine($"成功终止 exiftool 进程 (PID: {process.Id})");
                    }
                    catch (Exception ex)
                    {
                        // 处理终止单个进程时可能出现的异常（如权限不足、进程已退出）
                        Console.WriteLine($"终止进程 (PID: {process.Id}) 时出错: {ex.Message}");
                    }
                    finally
                    {
                        // 释放进程对象相关的资源
                        process.Dispose();
                    }
                }

                if (existingProcesses.Length == 0)
                {
                    Console.WriteLine($"当前没有发现正在运行的{exifProcessName} 进程。");
                }
            }
            catch (Exception ex)
            {
                // 处理获取进程列表时可能出现的异常
                Console.WriteLine($"检查 exiftool 进程时发生错误: {ex.Message}");
            }
        }

        #region GetExifTimeGps的简单方法
        private readonly object _exifToolLockExifTime = new object();
        private readonly object _exifToolLockExifGps = new object();
        private string GetExifTime(string fullName)
        {
            if (!File.Exists(fullName))
            {
                Console.WriteLine($"文件不存在: {fullName}");
                return "N/A (文件不存在)";
            }
            lock (_exifToolLockExifTime)
            {
                string receiveStr = "";
                try
                {
                    ExifToolProcessor _exifToolProcessor = new ExifToolProcessor(exifProcessName, selectedFolderPath);
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("-DateTimeOriginal");
                    sb.AppendLine($"\"{fullName}\"");
                    string arguments = sb.ToString().Replace(Environment.NewLine, " ");
                    receiveStr = _exifToolProcessor.ExecuteCommandBlocking(arguments);
                    //receiveStr = _exifToolProcessor.ExecuteCommandAsync(arguments);
                    string pattern = @"\d{4}:\d{2}:\d{2} \d{2}:\d{2}:\d{2}";
                    Match match = Regex.Match(receiveStr, pattern);
                    return match.Success ? match.Value : "N/A match.error";
                }
                catch (FileNotFoundException ex)
                {
                    Debug.WriteLine($"读取文件{fullName}exiftime错误:{ex.Message}");
                }
                catch (Exception ex)
                {
                    string msgStr = $"GetExifTime 初始化ExifTool处理器时出错: {ex.Message}{ex.StackTrace}";
                    Debug.WriteLine(msgStr);
                }
                return "N/A";
            }
        }
        private string GetExifTimeCondition(string fullName)
        {
            if (!checkBox1.Checked) return "";
            return GetExifTime(fullName);
        }
        private async Task<string> GetExifTimeAsync(string fullName)
        {
            return await Task.Run(() => GetExifTime(fullName));
        }
        private string GetExifTimeInRefresh(int limitExifCheck, string fullName)
        {
            if (!checkBox1.Enabled) return "";
            string NoticeMesg = "默认只自动读取前20个文件";
            return limitExifCheck <= 20 ? GetExifTimeCondition(fullName) : NoticeMesg;
        }
        private string GetExifGps(string fullName)
        {
            lock (_exifToolLockExifGps)
            {
                string receiveStr;
                try
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("-GPSLatitude");
                    sb.AppendLine("-GPSLongitude");
                    sb.AppendLine("-GPSLatitudeRef");
                    sb.AppendLine("-GPSLongitudeRef");
                    sb.AppendLine("-GPSAltitude");
                    sb.AppendLine($"\"{fullName}\"");
                    string arguments = sb.ToString().Replace(Environment.NewLine, " ");
                    ExifToolProcessor _exifToolProcessor = new ExifToolProcessor(exifProcessName, selectedFolderPath);
                    receiveStr = _exifToolProcessor.ExecuteCommandBlocking(arguments);

                    return new MyMethod().FormatGPSStrB(receiveStr);
                }
                catch (FileNotFoundException ex)
                {
                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    string ErrMesg = $"GetExifGps 初始化ExifTool处理器时出错: {ex.Message}";
                    Debug.WriteLine(ErrMesg);
                }
                return "N/A";
            }
        }
        private string GetExifGpsCondition(string fullName)
        {
            if (!checkBox2.Checked) return "";
            return GetExifGps(fullName);
        }
        private string GetExifGpsInRefresh(int limitExifCheck, string fullName)
        {
            if (!checkBox2.Enabled) return "";
            string NoticeMesg = "默认只自动读取前20个文件";
            return limitExifCheck <= 20 ? GetExifGpsCondition(fullName) : NoticeMesg;
        }
        private (string exifTime, string exifGps) GetExifInfo(string fullName)
        {
            string exifTime = "N/A";
            string exifGps = "N/A";
            exifTime = GetExifTime(fullName);
            exifGps = GetExifGps(fullName);
            return (exifTime, exifGps);
        }
        private (string exifTime, string exifGps) GetExifInfoCondition(string fullName)
        {
            string exifTime = "N/A";
            string exifGps = "N/A";
            exifTime = GetExifTimeCondition(fullName);
            exifGps = GetExifGpsCondition(fullName);
            return (exifTime, exifGps);
        }

        #endregion
        #region 读取exif信息的综合方法
        private void GetMultipleMetaData(string filePath)
        {
            // 获取EXIF信息
            PhotoMetadata photoMetadata = new PhotoMetadata();
            photoMetadata.FileCreateTime = File.GetCreationTime(filePath);
            photoMetadata.FileModifyTime = File.GetLastWriteTime(filePath);
            // 构建要执行的命令列表
            var sb = new StringBuilder();
            sb.AppendLine("-DateTimeOriginal");
            sb.AppendLine("-DateTimeDigitized");
            sb.AppendLine("-DateTimeModified");
            sb.AppendLine("-GPSLatitude");
            sb.AppendLine("-GPSLatitudeRef");
            sb.AppendLine("-GPSLongitude");
            sb.AppendLine("-GPSLongitudeRef");
            sb.AppendLine($"\"{filePath}\"");
            string arguments = sb.ToString().Replace(Environment.NewLine, " ");
            string receiveStr = "";
            try
            {
                ExifToolProcessor _exifToolProcessor = new ExifToolProcessor(exifProcessName, Path.GetDirectoryName(filePath));
                receiveStr = _exifToolProcessor.ExecuteCommandBlocking(arguments);

                ExiftToolOutputParser exifToolOutputParser = new ExiftToolOutputParser(receiveStr);
                (double Latitude, double Longitude) = GpsConverter.ConvertToSignedDecimal(
                   exifToolOutputParser.GetValue("GPS Latitude"),
                   exifToolOutputParser.GetValue("GPS Latitude Ref"),
                   exifToolOutputParser.GetValue("GPS Longitude"),
                   exifToolOutputParser.GetValue("GPS Longitude Ref"));
                photoMetadata.Exif.GPSLatitude = Latitude;
                photoMetadata.Exif.GPSLatitudeRef = exifToolOutputParser.GetValue("GPS Latitude Ref");
                photoMetadata.Exif.GPSLongitude = Longitude;
                photoMetadata.Exif.GPSLongitudeRef = exifToolOutputParser.GetValue("GPS Longitude Ref");

                StringDateInfo stringDateInfo = StringDateExtractor.ExtractDateInfo(exifToolOutputParser.GetValue("Date/Time Original"));
                photoMetadata.Exif.DateTimeOriginal = stringDateInfo.DateTime;


                //显示在可编辑文本框里
                textBoxOrignTime.Text = exifToolOutputParser.GetValue("Date/Time Original");
                textBoxGpsLocation.Text = $"{Latitude:F6},{Longitude:F6}";

                //显示到表格中
                InitDataGridBindDataTable();//初始化，清空旧数据
                keyValueExifData.Rows.Add("文件创建时间", photoMetadata.FileCreateTime);
                keyValueExifData.Rows.Add("文件修改时间", photoMetadata.FileModifyTime);
                keyValueExifData.Rows.Add("--", "--");
                keyValueExifData.Rows.Add("Exif:拍摄时间", exifToolOutputParser.GetValue("Date/Time Original"));
                keyValueExifData.Rows.Add("Exif:纬度", $"{Latitude:F6}");
                keyValueExifData.Rows.Add("Exif:经度", $"{Longitude:F6}");
                keyValueExifData.Rows.Add("--", "--");
                keyValueExifData.Rows.Add("XMP:拍摄时间", "");
                keyValueExifData.Rows.Add("XMP:纬度", "");
                keyValueExifData.Rows.Add("XMP:经度", "");

            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                string msgStr = $"GetMultipleMetaData 初始化ExifTool处理器时出错: {ex.Message}{ex.StackTrace}";
                Debug.WriteLine(msgStr);
            }
        }
        private void GetMultipleMetaDataAll(string filePath)
        {
            PhotoMetadata photoMetadata = new PhotoMetadata();
            photoMetadata.FileCreateTime = File.GetCreationTime(filePath);
            photoMetadata.FileModifyTime = File.GetLastWriteTime(filePath);
            // 构建要执行的命令列表
            var sb = new StringBuilder();
            sb.AppendLine("-all");
            sb.AppendLine($"\"{filePath}\"");
            string arguments = sb.ToString().Replace(Environment.NewLine, " ");
            string receiveStr = "";
            //清空单个文件的元数据显示表格和文本框的内容
            InitDataGridBindDataTable();//清空旧数据
            textBoxOrignTime.Text = "";
            textBoxGpsLocation.Text = "";
            textBoxGpsLocation.ForeColor = System.Drawing.Color.Black;
            DataGridViewRow row = dataGridView1.SelectedRows[0];
            try
            {
                // 获取EXIF信息
                ExifToolProcessor _exifToolProcessor = new ExifToolProcessor(exifProcessName, Path.GetDirectoryName(filePath));
                receiveStr = _exifToolProcessor.ExecuteCommandBlocking(arguments);
                if (receiveStr == null) return;

                ExiftToolOutputParser exifToolOutputParser = new ExiftToolOutputParser(receiveStr);
                try
                {
                    StringDateInfo stringDateInfo = StringDateExtractor.ExtractDateInfo(exifToolOutputParser.GetValue("Date/Time Original"));
                    photoMetadata.Exif.DateTimeOriginal = stringDateInfo.DateTime;
                    //显示在可编辑文本框里
                    textBoxOrignTime.Text = exifToolOutputParser.GetValue("Date/Time Original");
                    //显示到单个文件元数据表格中
                    keyValueExifData.Rows.Add("文件创建时间", photoMetadata.FileCreateTime);
                    keyValueExifData.Rows.Add("文件修改时间", photoMetadata.FileModifyTime);
                    keyValueExifData.Rows.Add("--", "--");
                    keyValueExifData.Rows.Add("拍摄时间", exifToolOutputParser.GetValue("Date/Time Original"));
                    //显示到主文件信息列表中
                    row.Cells["CreationTime"].Value = photoMetadata.FileCreateTime;
                    row.Cells["LastWriteTime"].Value = photoMetadata.FileModifyTime;
                    row.Cells["ExifTime"].Value = exifToolOutputParser.GetValue("Date/Time Original");
                }
                catch { }

                try
                {
                    (double Latitude, double Longitude) = GpsConverter.ConvertToSignedDecimal(
                       exifToolOutputParser.GetValue("GPS Latitude"),
                       exifToolOutputParser.GetValue("GPS Latitude Ref"),
                       exifToolOutputParser.GetValue("GPS Longitude"),
                       exifToolOutputParser.GetValue("GPS Longitude Ref"));
                    photoMetadata.Exif.GPSLatitude = Latitude;
                    photoMetadata.Exif.GPSLatitudeRef = exifToolOutputParser.GetValue("GPS Latitude Ref");
                    photoMetadata.Exif.GPSLongitude = Longitude;
                    photoMetadata.Exif.GPSLongitudeRef = exifToolOutputParser.GetValue("GPS Longitude Ref");

                    //显示在可编辑文本框里
                    textBoxGpsLocation.Text = $"{Latitude:F6},{Longitude:F6}";
                    //显示到单个文件元数据表格中
                    keyValueExifData.Rows.Add("纬度", $"{photoMetadata.Exif.GPSLatitude:F6}");
                    keyValueExifData.Rows.Add("经度", $"{photoMetadata.Exif.GPSLongitude:F6}");
                    keyValueExifData.Rows.Add("--", "--");
                    //显示到主文件信息列表中
                    row.Cells["ExifGPS"].Value = textBoxGpsLocation.Text;
                }
                catch { }

                try
                {
                    //显示到单文件元数据表格中
                    photoMetadata.Exif.Width = int.Parse(exifToolOutputParser.GetValue("Image Width"));
                    photoMetadata.Exif.Height = int.Parse(exifToolOutputParser.GetValue("Image Height"));
                    keyValueExifData.Rows.Add("宽度", $"{photoMetadata.Exif.Width}");
                    keyValueExifData.Rows.Add("高度", $"{photoMetadata.Exif.Height}");
                }
                catch { }

            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                string msgStr = $"GetExifTime 初始化ExifTool处理器时出错: {ex.Message}{ex.StackTrace}";
                Debug.WriteLine(msgStr);
            }
        }
        #endregion
        private Timer selectionTimer;
        private void SetupSelectionTimer()
        {
            selectionTimer = new Timer();
            selectionTimer.Interval = 300; // 设置延迟时间，如500毫秒，可根据体验调整
            selectionTimer.Tick += SelectionTimer_Tick;
        }

        private void SelectionTimer_Tick(object sender, EventArgs e)
        {
            selectionTimer.Stop(); // 停止计时器

            if (!string.IsNullOrEmpty(pendingFilePath))
            {
                ShowImgInThumbnail(pendingFilePath);
                GetMultipleMetaDataAll(pendingFilePath);
            }
        }
    }
}
