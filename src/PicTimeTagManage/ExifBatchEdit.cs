/*****************************************
 *
 * 项目名称： PicTimeTagManage  
 * 文 件 名:  ExifBatchEdit.cs
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
using System.Windows.Forms;

namespace PicTimeTagManage
{
    public partial class FormBatchEditFileName: BaseForm
    {
        private List<string> _filePaths;
        public FormBatchEditFileName(List<string> filePaths)
        {
            InitializeComponent();
            _filePaths = filePaths;
        }
        private void FormBatchEdit_Load(object sender, EventArgs e)
        {
            label4.Text = "根据文件名猜测照片和视频的创建日期，点击预览，将显示分析结果，如果分析结果准确，请点击执行，将修改文件的创建日期和修改日期\r\n";

            label3.Text = $"总共需要处理{_filePaths.Count()}个文件";
            ListAllFileName(_filePaths);
            btnExecute.Enabled = false;
        }
        private void btnExecute_Click(object sender, EventArgs e)
        {
            btnExecute.Enabled = false;
           // 执行文件处理
            ProcessFiles(_filePaths, "");
            label3.Text += $"        处理结束！";

        }
        private void btnReview_Click(object sender, EventArgs e)
        {
            ReviewProcess(_filePaths, "");
            btnExecute.Enabled = true;
        }
        private void ReviewProcess(List<string> filePaths, string pattern)
        {
            listBox1.Items.Clear();
            foreach (string filePath in filePaths)
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                StringDateInfo extractedTime = ExtractDateTime(fileName, pattern);
                if (extractedTime.DateTime.HasValue)
                {
                    string newFileName = MergeNewFileName(extractedTime);
                    string newFilePath = GenerateUniqueFilePath(Path.GetDirectoryName(filePath), newFileName, Path.GetExtension(filePath));
                    string processInfo = $"【原文件名】：{fileName}    ==> 【新】文件名：{newFileName}, 猜测文件时间：{extractedTime.DateTime.ToString()}";
                    listBox1.Items.Add(processInfo);
                }
            }
        }
        private void ListAllFileName(List<string> filePaths)
        {
            listBox1.Items.Clear();
            foreach (string filePath in filePaths)
            {
                string processInfo = $"【原文件名】：{Path.GetFileNameWithoutExtension(filePath)} ";
                listBox1.Items.Add(processInfo);
            }
        }
        private string MergeNewFileName(StringDateInfo extractedTime)
        {
            if (string.IsNullOrEmpty(extractedTime.RemainingText))
            {
                return extractedTime.DateString;
            }
            else
            {
                return extractedTime.DateString + "_"  +extractedTime.RemainingText;
            }
        }
        private void ProcessFiles(List<string> filePaths, string pattern)
        {
            foreach (string filePath in filePaths)
            {
                try
                {
                    string fileName = Path.GetFileNameWithoutExtension(filePath);
                    StringDateInfo extractedTime = ExtractDateTime(fileName, pattern);

                    if (extractedTime.DateTime.HasValue)
                    {
                        // 修改文件时间属性
                        File.SetCreationTime(filePath, (DateTime)extractedTime.DateTime);
                        File.SetLastWriteTime(filePath, (DateTime)extractedTime.DateTime);

                        // 生成新文件名
                        string newFileName = MergeNewFileName(extractedTime);
                        try
                        {
                            File.Move(filePath, Path.Combine(Path.GetDirectoryName(filePath), newFileName + Path.GetExtension(filePath)));
                        }
                        catch
                        {
                            string newFilePath = GenerateUniqueFilePath(Path.GetDirectoryName(filePath), newFileName, Path.GetExtension(filePath));
                            File.Move(filePath, newFilePath);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // 记录错误或显示通知
                    Debug.WriteLine($"处理文件 {filePath} 时出错: {ex.Message}");
                }
            }
        }
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
        private StringDateInfo ExtractDateTime(string fileName, string pattern)
        {
            try
            {
                //正则模式匹配方法
                var result = StringDateExtractor.ExtractDateInfo(fileName);

                if (result != null)
                {
                    string timeStr = result.DateTime.ToString();
                    return result;
                }

            }
            catch { }
            return null;


        }
    }
}
