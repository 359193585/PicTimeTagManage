/*****************************************
 *
 * 项目名称： PicTimeTagManage  
 * 文 件 名:  ModuleClass.cs
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
using System.Windows.Forms;

namespace PicTimeTagManage
{
    public class FileInfoDisplay
    {
        public int FixedRowNumber { get; set; }
        public string FileName { get; set; } = string.Empty;
        public DateTime CreationTime { get; set; }
        public DateTime LastWriteTime { get; set; }
        public string FullPath { get; set; } = string.Empty;
        public string ExifTime { get; set; } = string.Empty;
        public string ExifGps { get; set; } = string.Empty;

    }
    public class MyMethod
    {
        public void ShowGPSSelection(string GPSString)
        {
            if (Properties.Settings.Default.DoNotShowPrompt)
            {
                GpsCoordinateValidator.GetMapUrl(GPSString, out string url);
                Process.Start(url);
            }
            else
            {
                FormLoadMap formLoadMap = new FormLoadMap();
                var DiaResult = formLoadMap.ShowDialog();
                if (DiaResult == DialogResult.OK)
                {
                    GpsCoordinateValidator.GetMapUrl(GPSString, out string url);
                    Process.Start(url);
                }
            }
        }
        /// <summary>
        /// 打开文件所在目录并选中文件
        /// </summary>
        /// <param name="filePath">文件的完整路径</param>
        public void OpenFileDirectory(string filePath)
        {
            try
            {
                string absolutePath;
                if (Path.IsPathRooted(filePath))
                {
                    absolutePath = filePath; // 已经是绝对路径
                }
                else
                {
                    // 将相对路径转换为基于程序基目录的绝对路径
                    absolutePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filePath);
                }
                string cleanAbsolutePath = Path.GetFullPath(absolutePath);
                // 检查文件是否存在
                if (File.Exists(cleanAbsolutePath))
                {
                    // 使用 explorer 打开目录并选中文件
                    // 参数 /select, 后接文件绝对路径，用引号包裹以防止空格等特殊字符
                    Process.Start("explorer.exe", $"/select,\"{cleanAbsolutePath}\"");
                }
                else
                {
                    // 如果文件不存在，只打开所在目录
                    string directory = Path.GetDirectoryName(cleanAbsolutePath);
                    if (Directory.Exists(directory))
                    {
                        Process.Start("explorer.exe", $"\"{directory}\"");
                    }
                    else
                    {
                        MessageBox.Show("指定的目录不存在。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开目录时出错: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public string FormatGPSStrB(string SourceGPSStr)
        {
            ExiftToolOutputParser exifToolOutputParser = new ExiftToolOutputParser(SourceGPSStr);
            (double Latitude, double Longitude) = GpsConverter.ConvertToSignedDecimal(
               exifToolOutputParser.GetValue("GPS Latitude"),
               exifToolOutputParser.GetValue("GPS Latitude Ref"),
               exifToolOutputParser.GetValue("GPS Longitude"),
               exifToolOutputParser.GetValue("GPS Longitude Ref"));
            return $"{Latitude:F6},{Longitude:F6}";
        }
        public string FormatGPSStr(string SourceGPSStr)
        {
            if (string.IsNullOrEmpty(SourceGPSStr)) 
                return "N/A";
            string _return = SourceGPSStr.Replace(" ", "").Replace("\r\n", ";");
            string[] strings = SourceGPSStr.Replace("\r\n", "\r").Trim(new char[] { '\r', ' ' }).Split('\r');

            return _return;
        }
    }
    public static class GlobalFileFilters
    {
        // 集中管理图片文件扩展名（不含点号，不区分大小写）
        public static readonly HashSet<string> ImageExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "jpg", "jpeg", "png", "gif", "bmp", "tiff", "tif",
        "webp", "heic", "heif", "ico", "svg", "raw", "cr2", "nef", "arw"
    };

        public static bool IsImageFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return false;

            string extension = Path.GetExtension(filePath);
            if (string.IsNullOrEmpty(extension))
                return false;

            // 去除点号，并与已知图片扩展名比较
            return ImageExtensions.Contains(extension.TrimStart('.'));
        }
    }
}
