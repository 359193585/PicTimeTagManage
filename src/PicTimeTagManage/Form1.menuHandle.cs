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
using System.Data;
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
        private async void menuExifTimeModifyFilename_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("请先选择要处理的文件", "提醒");
                return;
            }
            if (dataGridView1.SelectedRows.Count > 20)
            {
                DialogResult dialogResult20 = MessageBox.Show("读取exif时间是耗时操作，请每次选择不要超过20个文件,如要继续，请选择 YES 。", "提醒", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (dialogResult20 != DialogResult.Yes) return;
            }
            string s1 = $"请确认你选择的{dataGridView1.SelectedRows.Count}个文件的exif日期准确，再点击确定修改！";
            DialogResult dialogResult = MessageBox.Show(s1, "重要提醒", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (dialogResult != DialogResult.Yes) return;

            _cancellationTokenSource = new CancellationTokenSource();
            try
            {
                var selectedRows = dataGridView1.SelectedRows.Cast<DataGridViewRow>()
                              .OrderBy(r => r.Index)
                              .ToList();
                await Task.Run(() =>
                {
                    for (int i = 0; i < selectedRows.Count; i++)
                    {
                        var row = selectedRows[i];
                        _ = ModifyNameByExifTime(row, i, selectedRows.Count);
                        if (i % 10 == 0)
                        {
                            Task.Delay(2000);
                        }
                    }

                    return Task.CompletedTask;
                }, _cancellationTokenSource.Token
                );
                MessageBox.Show("处理将在后台继续，如果信息不准确，请稍后重新调整！", "提醒");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

        }
        private async Task<object> ReadExifTimeGpsInfo(DataGridViewRow row, int currentIndex, int totalCount)
        {
            if (row.IsNewRow) return Task.FromResult<object>(false);
            string filePath = row.Cells["FullPath"].Value?.ToString();
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) return Task.FromResult<object>(false);

            //origin time
            string exifTime = await Task.Run(() => GetExifTimeAsync(filePath));
            //GPS arguments
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("-GPSLatitude");
            sb.AppendLine("-GPSLongitude");
            sb.AppendLine("-GPSLatitudeRef");
            sb.AppendLine("-GPSLongitudeRef");
            sb.AppendLine("-GPSAltitude");
            sb.AppendLine($"\"{filePath}\"");
            string arguments = sb.ToString().Replace(Environment.NewLine, " ");

            string exifGps = await Task.Run(() => GetExifInfoAsync(filePath, arguments, "GetExifGps"));
            this.BeginInvoke(new Action(() =>
            {
                row.Cells["ExifTime"].Value = exifTime;
                row.Cells["ExifGPS"].Value = new MyMethod().FormatGPSStrB(exifGps);
            }));
            return Task.FromResult<object>(true);
        }
        private async Task<object> ReadExifTimeGpsInfoA(DataGridViewRow row, int currentIndex, int totalCount)
        {
            if (row.IsNewRow) return Task.FromResult<object>(false);
            string filePath = row.Cells["FullPath"].Value?.ToString();
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) return Task.FromResult<object>(false);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("-all");
            sb.AppendLine($"\"{filePath}\"");
            string arguments = sb.ToString().Replace(Environment.NewLine, " ");

            string receiveStr = await Task.Run(() => GetExifInfoAsync(filePath, arguments, "Get All Exif"));
            ExiftToolOutputParser exifToolOutputParser = new ExiftToolOutputParser(receiveStr);
            string exifTime = exifToolOutputParser.GetValue("Date/Time Original");
            (double Latitude, double Longitude) = GpsConverter.ConvertToSignedDecimal(
                       exifToolOutputParser.GetValue("GPS Latitude"),
                       exifToolOutputParser.GetValue("GPS Latitude Ref"),
                       exifToolOutputParser.GetValue("GPS Longitude"),
                       exifToolOutputParser.GetValue("GPS Longitude Ref"));
            string exifGps = $"{Latitude:F6},{Longitude:F6}";
            this.BeginInvoke(new Action(() =>
            {
                row.Cells["ExifTime"].Value = exifTime;
                row.Cells["ExifGPS"].Value = exifGps;
                progressBar1.Value +=1;
            }));
            return Task.FromResult<object>(true);
        }
        private async Task<object> ModifyNameByExifTime(DataGridViewRow row, int currentIndex, int totalCount)
        {
            if (row.IsNewRow) return Task.FromResult<object>(false);
            string filePath = row.Cells["FullPath"].Value?.ToString();// FullPath,存储文件完整路径
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) return Task.FromResult<object>(false);
            FileInfoDisplay fileInfoDisplay = new FileInfoDisplay()
            {
                FullPath = filePath,
                ExifTime = row.Cells["ExifTime"].Value == null ? "" : row.Cells["ExifTime"].Value.ToString()
            };
            if (fileInfoDisplay.ExifTime == "" || fileInfoDisplay.ExifTime == "N/A")
            {
                fileInfoDisplay.ExifTime =  await Task.Run(() => GetExifTimeAsync(filePath));
            }
            if (fileInfoDisplay.ExifTime == "" || fileInfoDisplay.ExifTime == "N/A") return Task.FromResult<object>(false);

            //lock (_exifToolLockExifTime)
            {
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
                    this.BeginInvoke(new Action(() =>
                    {
                        row.Cells["FileName"].Value = Path.GetFileName(newFilePath);
                        row.Cells["FullPath"].Value = newFilePath;
                        row.Cells["ExifTime"].Value = fileInfoDisplay.ExifTime;
                    }));

                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
            
            return Task.FromResult<object>(true);
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
        private async void menuReadTimeGpsFromExif_Click(object sender, EventArgs e)
        {
            // 获取选中的行
            var selectedRows = dataGridView1.SelectedRows.Cast<DataGridViewRow>()
                                      .OrderBy(r => r.Index)
                                      .ToList();
            if (selectedRows.Count == 0)
            {
                MessageBox.Show("请先选择要处理的行");
                return;
            }
            if (selectedRows.Count >= 20)
            {
                string Mesg = $"你选择了{selectedRows.Count}个文件，读取文件exif信息将非常耗时，界面会假死，请耐心等待！如果你不想继续操作，请点击按钮“否”。";
                DialogResult dialogResult = MessageBox.Show(Mesg, "重要提醒", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (dialogResult != DialogResult.Yes) return;
            }
            // 禁用UI更新以提高性能
            //dataGridView1.SuspendLayout();
            _cancellationTokenSource = new CancellationTokenSource();
            progressBar1.Minimum = 0;
            progressBar1.Maximum = selectedRows.Count;
            progressBar1.Value = 0;
            try
            {
                await Task.Run(async () =>
                {
                    for (int i = 0; i < selectedRows.Count; i++)
                    {
                        var row = selectedRows[i];
                        _ = ReadExifTimeGpsInfoA(row,i,selectedRows.Count);
                       
                        if (i % 10 == 0)
                        {
                            //await Task.Delay(2000);
                        }
                    }
                    return Task.CompletedTask;
                }, _cancellationTokenSource.Token
                );
            }

            //    //// 获取EXIF信息
            //    //var exifTime = GetExifTime(filePath);
            //    //var exifGps = GetExifGps(filePath);
            //    ////更新DataGridView中的值
            //    //row.Cells["ExifTime"].Value = exifTime;
            //    //row.Cells["ExifGPS"].Value = exifGps;

            //    //_ = ReadExifTime(filePath, row.Index); //异步 
            //    //_ = ReadExifGps(filePath, row.Index);
            //    //


            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
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
            string FileName = selectedFiles[0];
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
            string Mesg = MultilFileExifDateModifyWarningMesg(dataGridView1.Rows.Count - 1);

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
        private void menuCopyTimeToClipboard_Click(object sender, EventArgs e)
        {
            DataGridViewRow row = dataGridView1.SelectedRows[0];
            if (row != null)
            {
                string timeValue = row.Cells["ExifTime"].Value.ToString();
                Clipboard.Clear();
                Clipboard.SetText(timeValue);
            }
        }
        private void menuCopyGPSToClipboard_Click(object sender, EventArgs e)
        {
            DataGridViewRow row = dataGridView1.SelectedRows[0];
            if (row != null)
            {
                string gpsValus = row.Cells["ExifGPS"].Value.ToString();
                Clipboard.Clear();
                Clipboard.SetText(gpsValus);
            }
        }
        public (int rowIndex, int columnIndex) FindCellIndexByValue(DataGridView dgv, string targetValue)
        {
            for (int rowIndex = 0; rowIndex < dgv.Rows.Count; rowIndex++)
            {
                for (int colIndex = 0; colIndex < dgv.Rows[rowIndex].Cells.Count; colIndex++)
                {
                    object cellValue = dgv.Rows[rowIndex].Cells[colIndex].Value;
                    if (cellValue != null && cellValue.ToString().Equals(targetValue))
                    {
                        return (rowIndex, colIndex);
                    }
                }
            }
            return (-1, -1);
        }
        public int FindCellIndexByValueColumn(DataGridView dgv, string targetValue,string colName)
        {
            for (int rowIndex = 0; rowIndex < dgv.Rows.Count; rowIndex++)
            {
                object cellValue = dgv.Rows[rowIndex].Cells[colName].Value;
                    if (cellValue != null && cellValue.ToString().Equals(targetValue))
                    {
                        return rowIndex;
                    }
            }
            return -1;
        }
        private async void menuRotation_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menuItem = sender as ToolStripMenuItem;
            if (menuItem == null) return;
            try
            {
                int rotationAngle = Convert.ToInt32(menuItem.Tag);
                List<string> selectedFiles = GetSelectedFiles();//如多选，显示第一个被选的
                if (selectedFiles.Count > 0)
                {
                    await Task.Run(() =>
                    {
                        foreach (string filePath in selectedFiles)
                        {
                            //1 = 正常（不旋转）3 = 旋转180度 6 = 顺时针旋转90度 8 = 逆时针旋转90度
                            PicRotation(filePath, rotationAngle);
                            ShowImgInThumbnail(filePath);
                            int GridCol = dataGridView1.Columns["ImageColumn"].Index;
                            int GridRow = FindCellIndexByValueColumn(dataGridView1, filePath, "FullPath");
                            DataGridViewCellValueEventArgs eGrid = new DataGridViewCellValueEventArgs(GridCol, GridRow);
                            Image thumbnail = new GenerateThumbnail(false).GetThumbnailImg(filePath, THUMBNAIL_WIDTH, THUMBNAIL_HEIGHT);
                            dataGridView1.Rows[GridRow].Cells[GridCol].Value = thumbnail;
                            thumbnail.Dispose();
                        }
                    });
                }

            }
            catch { }
        }
    }
}