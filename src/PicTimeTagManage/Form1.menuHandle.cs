/*****************************************
 *
 * 项目名称： PicTimeTagManage  
 * 文 件 名:  Form1.menuHandle.cs
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
using System.Text;
using System.Windows.Forms;
namespace PicTimeTagManage
{
    public partial class Form1
    {
        private void menuCleanSavedMesgFile_Click(object sender, EventArgs e)
        {
            string tempDir = Path.GetFullPath("./");
            string tempFileName = $"DataGridExport_";
            var files = Directory.GetFiles($"{tempDir}", $"{tempFileName}*");
            foreach (var file in files)
            {
                File.Delete(file);
            }
        }
        /// <summary>
        /// 表格内所有文件的时间和gps信息写入日志文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuSaveTimeGpsMesgToFile_Click(object sender, EventArgs e)
        {
            string exportedFilePath = ExportDataGridToTemporaryTxt(dataGridView1);
            new MyMethod().OpenFileDirectory(exportedFilePath);
        }
        /// <summary>
        /// 使用exif日期修改所选文件的文件名
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuExifTimeModifyFilename_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("请先选择要处理的文件", "提醒");
                return;
            }
            string s1 = $"请确认你选择的{dataGridView1.SelectedRows.Count}个文件的exif日期准确，再点击确定修改！";
            DialogResult dialogResult = MessageBox.Show(s1, "重要提醒", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (dialogResult == DialogResult.Yes)
            {
                try
                {
                    foreach (DataGridViewRow row in dataGridView1.SelectedRows)
                    {
                        if (row.IsNewRow) continue;
                        string filePath = row.Cells["FullPath"].Value?.ToString();// 第4列FullPath,存储文件完整路径
                        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) continue;
                        FileInfoDisplay fileInfoDisplay = new FileInfoDisplay()
                        {
                            FullPath = filePath,
                            ExifTime = row.Cells["ExifTime"].Value.ToString()
                        };
                        if (fileInfoDisplay.ExifTime == "" || fileInfoDisplay.ExifTime == "N/A")
                        {
                            fileInfoDisplay.ExifTime = GetExifTime(filePath);
                        }
                        if (fileInfoDisplay.ExifTime == "" || fileInfoDisplay.ExifTime == "N/A") continue;

                        (string newFileNameStr, DateTime fileDatetime) = ExifTimeToFilename(fileInfoDisplay.ExifTime);
                        string newFullName = Path.GetFullPath(Path.GetDirectoryName(filePath) + "\\" + newFileNameStr);
                        try
                        {
                            // 修改文件时间属性
                            File.SetCreationTime(filePath, fileDatetime);
                            File.SetLastWriteTime(filePath, fileDatetime);
                            // 检查文件名唯一性后，重命名文件
                            string newFilePath = GenerateUniqueFilePath(Path.GetDirectoryName(filePath), newFullName, Path.GetExtension(filePath));
                            File.Move(filePath, newFilePath);


                            //修改后的信息在表格里更新
                            row.Cells["FileName"].Value = Path.GetFileName(newFilePath);
                            row.Cells["FullPath"].Value = newFilePath;
                            // 更新数据源中的对象
                            if (row.DataBoundItem is FileInfoDisplay fileInfo)
                            {
                                fileInfo.FileName = Path.GetFileName(newFilePath);
                                fileInfo.FullPath = newFilePath;
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
            MessageBox.Show("本次处理结束，如果不准确，请重新调整！", "提醒");
        }
        /// <summary>
        /// 将选中的文件，从文件名猜测时间后，修改文件的创建时间,
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuFileCreateTimeBatchEdit_Click(object sender, EventArgs e)
        {
            List<string> selectedFiles = GetSelectedFiles();
            if (selectedFiles.Count == 0)
            {
                MessageBox.Show("请先选择要处理的文件");
                return;
            }
            FormBatchEditFileName formBatchEditFileName = new FormBatchEditFileName(selectedFiles);
            formBatchEditFileName.Show();
        }
        private void menuReadTimeGpsFromExif_Click(object sender, EventArgs e)
        {
            // 获取选中的行
            var selectedRows = dataGridView1.SelectedRows;
            if (selectedRows.Count == 0)
            {
                MessageBox.Show("请先选择要处理的行");
                return;
            }
            if (selectedRows.Count >= 20)
            {
                string Mesg = $"你选择了{selectedRows.Count}个文件，读取文件exif信息将非常耗时，界面会假死，请耐心等待！如果你不想继续操作，请点击按钮“否”。";
                DialogResult dialogResult = MessageBox.Show(Mesg, "重要提醒", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (dialogResult == DialogResult.Yes)
                {
                }
                else
                {
                    return;
                }
            }
            // 禁用UI更新以提高性能
            dataGridView1.SuspendLayout();
            try
            {
                foreach (DataGridViewRow row in selectedRows)
                {
                    if (row.IsNewRow) continue;

                    // 获取文件路径
                    string filePath = row.Cells["FullPath"].Value?.ToString();
                    if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                        continue;

                    // 获取EXIF信息
                    var exifTime = GetExifTime(filePath);
                    var exifGps = GetExifGps(filePath);

                    // 更新DataGridView中的值
                    row.Cells["ExifTime"].Value = exifTime;
                    row.Cells["ExifGPS"].Value = exifGps;

                    // 更新数据源中的对象
                    if (row.DataBoundItem is FileInfoDisplay fileInfo)
                    {
                        fileInfo.ExifTime = exifTime;
                        fileInfo.ExifGPS = exifGps;
                    }
                }
            }
            finally
            {
                // 恢复UI更新
                dataGridView1.ResumeLayout();
            }
        }

        /// <summary>
        /// Gps信息写入exif，支持选择部分文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuWriteGpsToExif_Click(object sender, EventArgs e)
        {
            List<string> selectedFiles = GetSelectedFiles();
            if (selectedFiles.Count == 0)
            {
                MessageBox.Show("请先选择要处理的文件");
                return;
            }
            string FileName= selectedFiles[0];
            string FirstFileGpsString = GetExifGps(FileName);
            using (LatLongInputDialog inputDialog = new LatLongInputDialog())
            {
                inputDialog.textBoxGpsLocation.Text = FirstFileGpsString;
                if (inputDialog.ShowDialog() == DialogResult.OK)
                {
                    // 使用用户输入的经纬度值
                    string gpsStr = $"{inputDialog.Latitude},{inputDialog.Longitude}";
                    List<string> commands = new List<string>();
                    CreateGpsWriteArguments(gpsStr, out commands, inputDialog.Overwrite);
                    ExifToolProcess formEdit = new ExifToolProcess(selectedFiles, commands, exifProcessName);
                    formEdit.Show();
                }
                else
                {
                    //MessageBox.Show("已取消操作");
                    return;
                }
            }
        }
        private void menuWriteModifyTimeToExifInSelectedFile_Click(object sender, EventArgs e)
        {
            List<string> selectedFiles = GetSelectedFiles();
            if (selectedFiles.Count == 0)
            {
                MessageBox.Show("请先选择要处理的文件");
                return;
            }
            if (selectedFiles.Count >= 50)
            {
                MessageBox.Show("文件处理操作将消耗大量时间，每次请不要超过50个文件，如果需要全部处理，请从右键弹出菜单选择“拍摄日期写入exif <= 从文件的创建时间，全部文件”，进行异步处理！");
                return;
            }
            string Mesg = MultilFileExifDateModifyWarningMesg(selectedFiles.Count);
            DialogResult dialogResult = MessageBox.Show(Mesg, "重要提醒", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
            string TimeCheck;
            if (dialogResult == DialogResult.Yes)
            {
                TimeCheck = "";  //覆盖原exit日期信息，不做if判断
            }
            else if (dialogResult == DialogResult.No)
            {
                TimeCheck = @"-if ""not $AllDates""";
            }
            else
            {
                return;
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(TimeCheck);
            sb.AppendLine(@"""-AllDates<FileModifyDate""");
            sb.AppendLine(@"""-FileModifyDate<AllDates""");
            sb.AppendLine(@"""-FileCreateDate<AllDates""");
            sb.AppendLine(@"-Artist=""leison""");
            sb.AppendLine("-overwrite_original");
            List<string> commands = new List<string>
            {
                sb.ToString().Replace(Environment.NewLine," ")
            };
            string workingDirectory = selectedFolderPath;
            ExifToolProcess formEdit = new ExifToolProcess(commands, workingDirectory, selectedFiles, exifProcessName);
            formEdit.Show();
        }

        private string MultilFileExifDateModifyWarningMesg(int filecounts)
        {
            return $"请确认你选择的{filecounts}个文件，创建日期或修改日期是你期望的照片原始拍摄日期，此日期将写入照片exif,请务必慎重处理。\r\n如果文件已经有exif日期信息，覆盖写入点击“是”，不覆盖写入点击“否”，不操作点击“取消”或关闭对话框";
        }

        /// <summary>
        /// 所有文件的时间写入exif
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuWriteModifyTimeToExifInAllFile_Click(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count <= 0) return;
            string Mesg = MultilFileExifDateModifyWarningMesg(dataGridView1.Rows.Count-1);

            DialogResult dialogResult = MessageBox.Show(Mesg, "重要提醒", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
            string TimeCheck;
            if (dialogResult == DialogResult.Yes)
            {
                TimeCheck = "";  //覆盖原exit日期信息，不做if判断
            }
            else if (dialogResult == DialogResult.No)
            {
                TimeCheck = @"-if ""not $AllDates""";
            }
            else
            {
                return;
            }

            // 构建要执行的命令列表,处理selectedFolderPath目录下的所有文件
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(TimeCheck);
            sb.AppendLine(@"""-AllDates<FileModifyDate""");
            sb.AppendLine(@"""-FileModifyDate<AllDates""");
            sb.AppendLine(@"""-FileCreateDate<AllDates""");
            sb.AppendLine(@"-Artist=""leison""");
            sb.AppendLine("-overwrite_original");
            sb.AppendLine("*");

            List<string> commands = new List<string>
            {
                sb.ToString().Replace(Environment.NewLine," ")
            };
            ExifToolProcess formEdit = new ExifToolProcess(exifProcessName, selectedFolderPath, commands);
            formEdit.Show();
        }

    }
}