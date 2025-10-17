/*****************************************
 *
 * 项目名称： PicTimeTagManage  
 * 文 件 名:  ExifToolProcess.cs
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
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PicTimeTagManage
{
    public partial class ExifToolProcess: BaseForm
    {
        private ExifToolProcessor _exifToolProcessor;
        private string _myWorkPath=".\\";
        private string _exifProcessName;
        private List<string> _commands;
        //private List<string> _selectedFiles;
        private string _workingDirectory;
        private string initMesgStr="";
        /// <summary>
        /// 处理指定目录下所有文件
        /// </summary>
        /// <param name="exifProcessName">exif程序指定</param>
        /// <param name="myWorkPath">指定目录</param>
        /// <param name="commands">命令参数</param>
        public ExifToolProcess(string exifProcessName, string myWorkPath, List<string> commands)
        {
            _exifProcessName = exifProcessName;
            _myWorkPath = myWorkPath;
            _commands = commands;
            InitializeComponent();
            InitializeExifToolProcessor();
            initMesgStr = MultiFileProcess();
        }
        public ExifToolProcess(string exifProcessName, string myWorkPath, List<string> commands,string filePath)
        {
            _exifProcessName = exifProcessName;
            _commands = commands;
            InitializeComponent();
            InitializeExifToolProcessor();
            initMesgStr = SingleFileProcess(commands, filePath);
        }
        /// <summary>
        /// 处理文件列表selectedFiles里的文件，写入GPS数据
        /// </summary>
        /// <param name="exifProcessName">exif程序指定</param>
        /// <param name="selectedFiles">文件列表</param>
        /// <param name="commands">命令参数</param>
        public ExifToolProcess(List<string> selectedFiles,List<string> commands, string exifProcessName = "exiftool.exe")
        {
            _exifProcessName = exifProcessName;
            InitializeComponent();
            InitializeExifToolProcessor();
            initMesgStr = SingleFileProcessGpsEdit(selectedFiles);
            _commands = CreateCommandsNew(commands, selectedFiles);

        }
        /// <summary>
        /// 处理文件列表selectedFiles里的文件，写入datetime数据
        /// </summary>
        /// <param name="commands">命令列表</param>
        /// <param name="workingDirectory">待处理文件所在目录</param>
        /// <param name="selectedFiles">待处理文件列表</param>
        /// <param name="exifProcessName">exiftool程序名称，可选</param>
        public ExifToolProcess( List<string> commands,string workingDirectory, List<string> selectedFiles, string exifProcessName="exiftool.exe")
        {
            _workingDirectory = workingDirectory;
            _exifProcessName = exifProcessName;
            InitializeComponent();
            InitializeExifToolProcessor();
            string tempFileName;
            (initMesgStr , tempFileName )= SingleFileProcessTimeEdit(selectedFiles);
            //编组文件名，减少命令的行数
            _commands = CreateCommandsNew(commands, selectedFiles);

        }

        private List<string> CreateCommands(List<string> commands ,string tempFileName)
        {
            List<string> result = new List<string>();
            foreach (string command in commands)
            {
                result.Add(command + "@\"" + tempFileName + "\"");
            }
            return result;
        }
        private List<string> CreateCommandsNew(List<string> commands, List<string>selectedFiles)
        {
            //微软官方给出的命令行最长字符可以是8191
            int MaxCharLen = 8191;
            // 命令参数的最大长度
            int maxLength = commands.Max(cmd => cmd.Length);
            List<string> multiFilename = new List<string>();
            string tempFileName = "";
            foreach (string file in selectedFiles)
            {
                if ((maxLength + tempFileName.Length + file.Length) < MaxCharLen)  //超过命令行字符长度限制，创建新命令
                {
                    tempFileName += $"\"{file}\" ";
                }
                else
                {
                    tempFileName = tempFileName.Trim();
                    multiFilename.Add(tempFileName);
                    tempFileName = $" \"{file}\" ";
                }
            }
            multiFilename.Add(tempFileName);

            List<string> result = new List<string>();
            foreach (string command in commands)
            {
                foreach (var argumenst in multiFilename)
                {
                    result.Add(command + " " + argumenst.Trim() + "");
                }
            }
            return result;
        }
        private (string initMesgStr,string tempFileName) SingleFileProcessTimeEdit(List<string> selectedFiles)
        {
            this.Text = "逐个文件写入datetime信息 " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string _rtn = $"共需要处理{selectedFiles.Count}个文件" + Environment.NewLine;
            
            string tempDir = "./";
            string tempFileName = $"FileList{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            string fullTempPath = Path.GetFullPath(Path.Combine(tempDir, tempFileName));
            using (FileStream fs = new FileStream(fullTempPath, FileMode.Create, FileAccess.Write))
            using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8)) // 推荐使用UTF8编码以支持更多字符
            {
                foreach (string file in selectedFiles)
                {
                    sw.WriteLine(file);
                    _rtn += $"filename=\"{file}\"" + Environment.NewLine;
                }
            }
            
            _rtn += Environment.NewLine;
            _rtn += "【重要提醒】点击“执行”按钮后，如果文件支持写入exif时间信息，将会把文件的修改日期作为照片原始日期写入exif,启动后无法停止，如果文件较多，会处理较长时间。如你不想此刻处理，请关闭窗口取消本次操作！" + Environment.NewLine;
            return (_rtn, fullTempPath);
        }
        private void ExportFileListToFile(List<string> selectedFiles)
        {
            string tempDir = "./";
            string tempFileName = $"FileList{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            string fullTempPath = Path.Combine(tempDir, tempFileName);
            using (FileStream fs = new FileStream(fullTempPath, FileMode.Create, FileAccess.Write))
            using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8)) // 推荐使用UTF8编码以支持更多字符
            {
                foreach (string file in selectedFiles)
                {
                    sw.WriteLine(file);
                }
            }
        }
        private void InitializeExifToolProcessor()
        {
            try
            {
                _exifToolProcessor = new ExifToolProcessor(_exifProcessName,_myWorkPath);
                _exifToolProcessor.OutputReceived += (sender, message) =>
                {
                    if (txtOutput.InvokeRequired)
                    {
                        txtOutput.Invoke(new Action(() =>
                        {
                            txtOutput.AppendText(message + Environment.NewLine);
                        }));
                    }
                    else
                    {
                        txtOutput.AppendText(message + Environment.NewLine);
                    }
                };
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"初始化ExifTool处理器时出错: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnRunCommands_Click(object sender, EventArgs e)
        {
            btnRunCommands.Enabled = false;  //只允许一次性操作
            // 在新线程中执行命令，避免UI卡死
            System.Threading.Tasks.Task.Run(() =>
            {
                _exifToolProcessor?.ExecuteCommands(_commands);
            });
        }
        private void ExifToolProcess_Load(object sender, EventArgs e)
        {
            txtOutput.Text = initMesgStr;
        }
        private string SingleFileProcessGpsEdit(List<string> selectedFiles)
        {
            this.Text = "写入GPS信息到文件 " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string _rtn = $"共需要处理{selectedFiles.Count}个文件" + Environment.NewLine;
            foreach (string file in selectedFiles)
            {
                _rtn += $"filename={file}" + Environment.NewLine;
            }
            _rtn += Environment.NewLine;
            _rtn += "【重要提醒】点击“执行”按钮后，如果文件支持写入GPS信息，将把你输入的GPS坐标写入文件EXIF，当前你选择的文件的拍摄位置信息都将被更新为你刚刚输入的经纬度，请谨慎操作！"+ Environment.NewLine;
            return _rtn;
        }
        private string MultiFileProcess()
        {
            this.Text = "处理目录下所有文件 " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string _rtn = $"即将处理{_myWorkPath}目录下所有文件，并执行以下操作：" + Environment.NewLine;
            foreach (string cmd in _commands)
            {
                _rtn += $"{cmd}" + Environment.NewLine;
            }
            _rtn += Environment.NewLine;
            _rtn += "【重要提醒】点击“执行”按钮后，如果文件支持EXIF日期信息，将会把你文件的创建日期写入文件EXIF！" + Environment.NewLine;
            return _rtn;
        }
        private string SingleFileProcess(List<string> commands, string filePath)
        {
            this.Text = "处理指定文件 " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string _rtn = $"即将处理文件{filePath}，执行以下操作：" + Environment.NewLine;
            foreach (string cmd in commands)
            {
                _rtn += $"{cmd}" + Environment.NewLine;
            }
            _rtn += Environment.NewLine;
            _rtn += "【重要提醒】点击“执行”按钮后，会对原始图片的内置元数据进行修改，请务必慎重处理！" + Environment.NewLine;
            return _rtn;
        }
    }
}
