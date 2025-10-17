/*****************************************
 *
 * 项目名称： PicTimeTagManage  
 * 文 件 名:  Form1.cs
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
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace PicTimeTagManage
{
   
    public partial class Form1 : Form
    {
        #region 用于调用Windows API的辅助类
        internal static class NativeMethods
        {
            public const int WM_CLIPBOARDUPDATE = 0x031D;

            [System.Runtime.InteropServices.DllImport("user32.dll")]
            public static extern bool AddClipboardFormatListener(IntPtr hwnd);

            [System.Runtime.InteropServices.DllImport("user32.dll")]
            public static extern bool RemoveClipboardFormatListener(IntPtr hwnd);
        }
        #endregion
        private BindingList<FileInfoDisplay> _fileBindingList = new BindingList<FileInfoDisplay>();
        private BindingList<PhotoMetadata> _exifMetadataBindingList = new BindingList<PhotoMetadata>();
        private DataTable keyValueExifData = new DataTable();

        private string exifProcessName = "exiftool.exe";  //经参数传递，全局都使用
        private string selectedFolderPath = "";

        private string pendingFilePath; // 用于暂存待读取的文件路径

        private string _lastSortedColumn = string.Empty;
        private SortOrder _currentSortDirection = SortOrder.None;
        public Form1()
        {
            InitializeComponent();
            InitDataGrid();
            InitContextMenu();
            InitDataGridBindDataTable();
            KillExistingExifTool(Path.GetFileNameWithoutExtension(exifProcessName));

            comboBoxFolders.DropDownStyle = ComboBoxStyle.DropDownList;
            // 将当前窗口设置为可以接收剪贴板更新消息
            NativeMethods.AddClipboardFormatListener(this.Handle);
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            LoadFolderHistory();//加载历史目录
        }
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            btnRefresh.Enabled = false;
            RefreshFileList();
            btnRefresh.Enabled = true;
        }
        private void btnModifyGps_Click(object sender, EventArgs e)
        {
            string gpsStr = textBoxGpsLocation.Text;
            if (string.IsNullOrEmpty(gpsStr)) return;
            CreateGpsWriteArguments(gpsStr, out List<string> commands, true);
            ExifToolProcessor exifTool = new ExifToolProcessor(exifProcessName, commands, "", null);
            exifTool.ExecuteCommand(commands[0]+ $" \"{pendingFilePath}\"");
            textBoxGpsLocation.ForeColor = Color.Black;
        }
        private void btnSelectFolder_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                folderBrowserDialog.Description = "请选择目录";
                folderBrowserDialog.RootFolder = Environment.SpecialFolder.Desktop; // 设置初始目录为桌面
                folderBrowserDialog.ShowNewFolderButton = false; // 不显示新建文件夹按钮

                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    selectedFolderPath = folderBrowserDialog.SelectedPath;
                    RefreshFileList(selectedFolderPath);
                    UpdateFolderHistory(selectedFolderPath);
                }
            }
        }
        /// <summary>
        /// 调用系统默认浏览器打开open street map
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLoadMap_Click(object sender, EventArgs e)
        {
            FormLoadMap formLoadMap = new FormLoadMap();
            var DiaResult = formLoadMap.ShowDialog();
            if (DiaResult == DialogResult.OK)
            {
                GpsCoordinateValidator.GetMapUrl(textBoxGpsLocation.Text, out string url);
                Process.Start(url);
            }
        }
        /// <summary>
        /// 单文件选择后，读取具体信息显示到底部栏
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnFlash_Click(object sender, EventArgs e)
        {
            List<string> selectedFiles = GetSelectedFiles();//如多选，显示第一个被选的
            if (selectedFiles.Count > 0)
            {
                string filePath = selectedFiles[0];
                ShowImgInThumbnail(filePath);
                GetMultipleMetaDataAll(filePath);
            }
        }
        private void btnLeft90_Click(object sender, EventArgs e)
        {
            List<string> selectedFiles = GetSelectedFiles();//如多选，显示第一个被选的
            if (selectedFiles.Count > 0)
            {
                string filePath = selectedFiles[0];
                //1 = 正常（不旋转）3 = 旋转180度 6 = 顺时针旋转90度 8 = 逆时针旋转90度
                PicRoattion(filePath, 8);
                ShowImgInThumbnail(filePath);
                GetMultipleMetaDataAll(filePath);
            }
        }
        private void btnRight90_Click(object sender, EventArgs e)
        {
            List<string> selectedFiles = GetSelectedFiles();//如多选，显示第一个被选的
            if (selectedFiles.Count > 0)
            {
                string filePath = selectedFiles[0];
                //1 = 正常（不旋转）3 = 旋转180度 6 = 顺时针旋转90度 8 = 逆时针旋转90度
                PicRoattion(filePath, 6);
                ShowImgInThumbnail(filePath);
                GetMultipleMetaDataAll(filePath);

            }
        }
        private void btnModifyOriginTime_Click(object sender, EventArgs e)
        {
            string timeStr = textBoxOrignTime.Text;
            var match = Regex.Match(timeStr, @"(\d{4})[:](\d{2})[:](\d{2})[\ ](\d{2})[:](\d{2})[:](\d{2})");
            if (!match.Success || match.Groups.Count < 6)
            {
                MessageBox.Show($"无效的时间格式 {nameof(timeStr)}");
                return;
            }
            DateTime dateTime = new DateTime(
                                    int.Parse(match.Groups[1].Value),
                                    int.Parse(match.Groups[2].Value),
                                    int.Parse(match.Groups[3].Value),
                                    int.Parse(match.Groups[4].Value),
                                    int.Parse(match.Groups[5].Value),
                                    int.Parse(match.Groups[6].Value));
            string customFormatted = dateTime.ToString("yyyy-MM-ddTHH:mm:ss");
            string TimeCheck = "";//  @"-if ""not $AllDates""";

            List<string> selectedFiles = GetSelectedFiles();
            // 构建要执行的命令列表
            List<string> commands = new List<string> { };
            foreach (string filename in selectedFiles)
            {
                var sb = new StringBuilder();
                sb.AppendLine($"\"{TimeCheck}\"");
                sb.AppendLine($"-EXIF:DateTimeOriginal=\"{timeStr}\"");
                sb.AppendLine($"-XMP:DateTimeOriginal=\"{customFormatted}\"");
                sb.AppendLine($"-Artist =\"leison\"");
                sb.AppendLine($"-overwrite_original");
                sb.AppendLine($"\"{filename}\"");

                commands.Add(sb.ToString().Replace(Environment.NewLine, " "));
                commands.Add(@"""-FileCreateDate<AllDates"" -overwrite_original """ + filename + @"""");
                commands.Add(@"""-FileModifyDate<AllDates"" -overwrite_original """ + filename + @"""");
            }
            ExifToolProcess formEdit = new ExifToolProcess(selectedFiles, commands, exifProcessName);
            formEdit.Show();
        }
        private void CreateGpsWriteArguments(string gpsStr, out List<string> commands, bool isOverWrite)
        {
            commands = new List<string> { };
            var detailedResult = GpsCoordinateValidator.ValidateAndParseGpsCoordinate(gpsStr);
            if (detailedResult.isValid)
            {
                Console.WriteLine($"解析结果 - 纬度: {detailedResult.latitude}, 经度: {detailedResult.longitude}");
            }
            else
            {
                MessageBox.Show($"你输入的坐标位置可能错误:{detailedResult}", "提醒");
                return;
            }
            string GPSCheck = isOverWrite ? "" : @"-if ""not defined $GPSLatitude and not defined $GPSLongitude""" ;
            string gpsArgu = ExiftoolArgumentsFormat.ConvertCoordinatesToExifToolParams($"{detailedResult.latitude},{detailedResult.longitude}");

            // 构建要执行的命令列表
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(GPSCheck);
            sb.AppendLine(gpsArgu);
            sb.AppendLine("\"-FileCreateDate<AllDates\"");
            sb.AppendLine("\"-FileModifyDate<AllDates\"");
            //sb.AppendLine("-overwrite_original"); //ConvertCoordinatesToExifToolParams中已经含该参数，如修改为不含该参数，请根据需要取消注释

            string cmd = sb.ToString().Replace(Environment.NewLine, " ");
            commands.Add(cmd);
        }
        public string ExportDataGridToTemporaryTxt(DataGridView dataGridView)
        {
            string tempDir = "./";
            string tempFileName = $"DataGridExport_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            string fullTempPath = Path.Combine(tempDir, tempFileName);

            try
            {
                using (FileStream fs = new FileStream(fullTempPath, FileMode.Create, FileAccess.Write))
                using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8)) // 推荐使用UTF8编码以支持更多字符
                {
                    // 计算每列所需的最大宽度（基于列标题和单元格内容）
                    int[] columnWidths = new int[dataGridView.Columns.Count];
                    for (int i = 0; i < dataGridView.Columns.Count; i++)
                    {
                        // 列标题的宽度
                        columnWidths[i] = dataGridView.Columns[i].HeaderText.Length;
                        // 遍历所有行，查找该列中最长的单元格内容
                        foreach (DataGridViewRow row in dataGridView.Rows)
                        {
                            if (!row.IsNewRow) // 跳过末尾的“新行”
                            {
                                string cellValue = row.Cells[i].Value?.ToString() ?? "";
                                if (cellValue.Length > columnWidths[i])
                                {
                                    columnWidths[i] = cellValue.Length;
                                }
                            }
                        }
                        // 额外添加填充空间，使看起来不拥挤
                        columnWidths[i] += 2;
                    }

                    // 写入列标题
                    for (int j = 0; j < dataGridView.Columns.Count; j++)
                    {
                        string header = dataGridView.Columns[j].HeaderText;
                        // 将每个标题填充到该列计算出的宽度，并右对齐（PadLeft）
                        sw.Write(header.PadLeft(columnWidths[j]));
                        // 在列之间添加一个空格作为分隔符（可选）
                        if (j < dataGridView.Columns.Count - 1) sw.Write(" ");
                    }
                    sw.WriteLine(); // 换行

                    // 写入所有数据行
                    foreach (DataGridViewRow row in dataGridView.Rows)
                    {
                        if (row.IsNewRow) continue; // 跳过用于添加新行的空行

                        for (int j = 0; j < dataGridView.Columns.Count; j++)
                        {
                            string cellValue = row.Cells[j].Value?.ToString() ?? "";
                            // 将每个单元格内容填充到该列计算出的宽度，并右对齐
                            sw.Write(cellValue.PadLeft(columnWidths[j]));
                            // 在列之间添加一个空格作为分隔符（可选）
                            if (j < dataGridView.Columns.Count - 1) sw.Write(" ");
                        }
                        sw.WriteLine(); // 换行
                    }
                }
                //返回生成的临时文件完整路径
                return fullTempPath;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导出数据到临时文件时出错: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }
        private void ShowImgInThumbnail(string filePath)
        {
            if (pictureBox1.Image != null)
            {
                var oldImage = pictureBox1.Image;
                pictureBox1.Image = null; // 断开关联
                oldImage.Dispose(); // 释放资源
            }
            pictureBox1.Image = new GenerateThumbnail(false).GetThumbnailImg(filePath, pictureBox1.Width, pictureBox1.Height);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
        }
        private void RefreshFileList(string directoryPath = null)
        {
            dataGridView1.SuspendLayout();
            try
            {
                // 清空现有列表
                _fileBindingList.Clear();

                // 使用当前目录或文本框中的路径
                string path = directoryPath ?? selectedFolderPath;

                if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
                {
                    MessageBox.Show("目录不存在或路径为空");
                    return;
                }

                var files = Directory.GetFiles(path);

                int LimitExifCheck = 0;
                // 添加到绑定列表
                foreach (var file in files)
                {
                    LimitExifCheck++;
                    FileInfo fileInfo = new FileInfo(file);
                    _fileBindingList.Add(new FileInfoDisplay
                    {
                        FileName = fileInfo.Name,
                        CreationTime = fileInfo.CreationTime,
                        LastWriteTime = fileInfo.LastWriteTime,
                        FullPath = fileInfo.FullName,
                        ExifTime = GetExifTimeInRefresh(LimitExifCheck, fileInfo.FullName),
                        ExifGPS = GetExifGpsInRefresh(LimitExifCheck, fileInfo.FullName)
                    });
                }
                label2.Text = $"共有{_fileBindingList.Count.ToString()}个文件";
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("无法访问目录：权限不足");
            }
            catch (IOException ex)
            {
                MessageBox.Show($"读取文件时出错：{ex.Message}");
            }
            finally
            {
                dataGridView1.ResumeLayout();
            }
        }
        private List<string> GetSelectedFiles()
        {
            List<string> selectedFiles = new List<string>();
            try
            {
                foreach (DataGridViewRow row in dataGridView1.SelectedRows)
                {
                    if (row.IsNewRow) continue;
                    // 第4列FullPath,存储文件完整路径
                    string filePath = row.Cells["FullPath"].Value.ToString();
                    if (File.Exists(filePath))
                    {
                        selectedFiles.Add(filePath);
                    }
                }
            }
            catch { }
            return selectedFiles;
        }
        private object GetPropertyValue(object obj, string propertyName)
        {
            if (obj == null || string.IsNullOrEmpty(propertyName)) return null;
            var prop = obj.GetType().GetProperty(propertyName);
            return prop?.GetValue(obj);
        }
        private void dataGridView1_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
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
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // 检查是否点击了左上角单元格（行头-1，列头-1）
            if (e.RowIndex == -1 && e.ColumnIndex == -1)
            {
                // 清除当前所有选择（可选，取决于你是否希望在点击全选时清除原有选择）
                // dataGridView1.ClearSelection();

                // 遍历所有行，设置 Selected 属性为 true
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (!row.IsNewRow)
                    {
                        row.Selected = true;
                    }
                }
            }
        }
        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
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
                // 全局的当前选择文件路径名
                pendingFilePath = selectedRow.Cells["FullPath"].Value?.ToString();

                // 停止之前的计时器（如果正在运行）
                selectionTimer.Stop();
                // 重新启动计时器，开始新的延迟等待
                selectionTimer.Start();
            }
            #endregion
        }
        private void dataGridView1_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            string filePath = dataGridView1.SelectedRows[0].Cells["FullPath"].Value.ToString();
            ShowImgInThumbnail(filePath);
            GetMultipleMetaData(filePath);
        }
        private void dataGridView2_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            var column = dataGridView2.Columns[e.ColumnIndex];
            if (column.DataPropertyName.Contains("."))
            {
                var row = dataGridView2.Rows[e.RowIndex];
                if (row.DataBoundItem != null)
                {
                    // 分割属性路径：如 "A1.GPSLongitude" -> ["A1", "GPSLongitude"]
                    string[] propertyNames = column.DataPropertyName.Split('.');
                    object currentValue = row.DataBoundItem;

                    // 递归获取嵌套属性值
                    foreach (string propName in propertyNames)
                    {
                        if (currentValue == null) break;

                        var propInfo = currentValue.GetType().GetProperty(propName);
                        if (propInfo != null)
                            currentValue = propInfo.GetValue(currentValue);
                        else
                            currentValue = null;
                    }

                    e.Value = currentValue;
                    e.FormattingApplied = true; // 标记已处理
                }
            }
        }
        private void comboBoxFolders_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxFolders.SelectedItem != null)
            {
                selectedFolderPath = comboBoxFolders.SelectedItem.ToString();
                if (Directory.Exists(selectedFolderPath))
                {
                    RefreshFileList(selectedFolderPath); // 如果路径存在，刷新文件列表
                }
            }
        }
        private void comboBoxFolders_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (comboBoxFolders.SelectedItem != null)
            {
                selectedFolderPath = comboBoxFolders.SelectedItem.ToString();
                if (Directory.Exists(selectedFolderPath))
                {
                    RefreshFileList(selectedFolderPath); // 根据选择刷新文件列表
                }
                UpdateFolderHistory(selectedFolderPath); //调整所选路径到最新
            }
        }
        private void PicRoattion(string filePath, int angle)
        {
            // 构建要执行的命令列表
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"-Orientation={angle.ToString()}");
            sb.AppendLine($"-XMP:Orientation={angle.ToString()}");
            sb.AppendLine("-n");
            sb.AppendLine(@"""-FileModifyDate<AllDates""");
            sb.AppendLine(@"""-FileCreateDate<AllDates""");
            sb.AppendLine(@"-Artist=""leison""");
            sb.AppendLine("-overwrite_original");
            sb.AppendLine($"\"{filePath}\"");

            string cmd = sb.ToString().Replace(Environment.NewLine, " ");
            try
            {
                ExifToolProcessor _exifToolProcessor = new ExifToolProcessor(exifProcessName, selectedFolderPath);
                string receiveStr = _exifToolProcessor.ExecuteCommandBlocking(cmd);
            }
            catch { }

            //预先存储文件的时间属性
            DateTime fileDatetime = File.GetCreationTime(filePath);

            //List<string> commands = new List<string>{ cmd };
            //ExifToolProcess formEdit = new ExifToolProcess(exifProcessName, Path.GetDirectoryName(filePath), commands, filePath);
            //formEdit.Show();

            new GenerateThumbnail(true).ReSaveImage(filePath);

            // 修改文件时间属性至原先时间
            File.SetCreationTime(filePath, fileDatetime);
            File.SetLastWriteTime(filePath, fileDatetime);
        }
        #region 系统剪切板监听
        protected override void WndProc(ref Message m)
        {
            // 监听剪贴板更新消息（WM_CLIPBOARDUPDATE）
            if (m.Msg == NativeMethods.WM_CLIPBOARDUPDATE)
            {
                OnClipboardUpdated();
            }
            base.WndProc(ref m);
        }

        private void OnClipboardUpdated()
        {
            // 安全地在UI线程上执行
            if (InvokeRequired)
            {
                Invoke(new Action(OnClipboardUpdated));
                return;
            }

            try
            {
                if (Clipboard.ContainsText())
                {
                    string copiedText = Clipboard.GetText();
                    // 处理获取到的文本，显示在TextBox中
                    var result = GpsCoordinateValidator.ValidateGpsCoordinate(copiedText);
                    if (result.isValid) textBoxGpsLocation.Text = copiedText;
                    textBoxGpsLocation.ForeColor = Color.Red;
                    Console.WriteLine($"捕获到文本: {copiedText}");
                }
            }
            catch (Exception ex)
            {
                // 处理剪贴板访问冲突（其他进程可能正在使用剪贴板）
                Console.WriteLine($"访问剪贴板失败: {ex.Message}");
            }
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
        }
        #endregion

    }
}

