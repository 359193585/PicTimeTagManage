/*****************************************
 *
 * 项目名称： PicTimeTagManage  
 * 文 件 名:  LoadExiftool.cs
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PicTimeTagManage
{
    #region 保存相片元数据的类
    /// <summary>
    /// 照片元数据主容器，区分EXIF和XMP
    /// </summary>
    public class PhotoMetadata
    {
        /// <summary>
        /// 源文件路径
        /// </summary>
        public string SourceFile { get; set; }
        public DateTime FileCreateTime { get; set; }
        public DateTime FileModifyTime { get; set; }
        public int FileSize { get; set; }
        /// <summary>
        /// 从相机设备获取的原始EXIF元数据
        /// </summary>
        public ExifData Exif { get; set; } = new ExifData();

        /// <summary>
        /// 由后期软件添加或扩展的XMP元数据
        /// </summary>
        public XmpData Xmp { get; set; } = new XmpData();

        /// <summary>
        /// 获取主要拍摄时间（优先使用XMP中的创作日期，其次为EXIF原始时间）
        /// </summary>
        public DateTime? GetPrimaryDateTime()
        {
            return Xmp?.DateCreated ?? Exif?.DateTimeOriginal;
        }

        /// <summary>
        /// 检查是否包含有效的GPS坐标数据
        /// </summary>
        public bool HasGPSData()
        {
            // 优先使用可能更精确的XMP GPS数据，若无则回退至EXIF数据
            return (Xmp?.GPSLatitude.HasValue == true && Xmp?.GPSLongitude.HasValue == true) ||
                   (Exif?.GPSLatitude.HasValue == true && Exif?.GPSLongitude.HasValue == true);
        }
        public override string ToString()
        {
            var primaryTime = GetPrimaryDateTime();
            string timeInfo = primaryTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "时间未知";
            string sourceInfo = $"EXIF条目: {Exif?.AdditionalData?.Count ?? 0}, XMP条目: {Xmp?.AdditionalData?.Count ?? 0}";
            string gpsInfo = HasGPSData() ? ", 包含GPS数据" : ", 无GPS数据";
            return $"照片元数据 [时间: {timeInfo}, {sourceInfo}{gpsInfo}]";
        }
    }

    /// <summary>
    /// 相机拍摄时记录的原始EXIF数据
    /// </summary>
    public class ExifData
    {
        // 基本时间信息
        public DateTime? DateTimeOriginal { get; set; } // 0x9003
        public DateTime? DateTimeDigitized { get; set; } // 0x9004
        public DateTime? DateTimeModified { get; set; }  // 0x0132

        // GPS信息
        public double? GPSLatitude { get; set; }
        public string GPSLatitudeRef { get; set; }
        public double? GPSLongitude { get; set; }
        public string GPSLongitudeRef { get; set; }
        public double? GPSAltitude { get; set; }
        public DateTime? GPSTimeStamp { get; set; }
        public string GPSProcessingMethod { get; set; }
        public int Width {  get; set; }
        public int Height { get; set; }
        // 其他EXIF原生属性（示例）
        public string CameraMaker { get; set; }        // 0x010F
        public string CameraModel { get; set; }        // 0x0110
        public double? ExposureTime { get; set; }       // 0x829A
        public double? FNumber { get; set; }            // 0x829D
        public ushort? ISOSpeed { get; set; }           // 0x8827

        /// <summary>
        /// 存储未在以上属性中列出的其他EXIF标签，键为标签ID（如 "0xA002"）
        /// </summary>
        public Dictionary<string, object> AdditionalData { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// 主要由后期处理软件添加或增强的XMP元数据
    /// </summary>
    public class XmpData
    {
        // Dublin Core 核心属性（常用）
        public string Title { get; set; }               // dc:title
        public string Creator { get; set; }            // dc:creator
        public string Description { get; set; }        // dc:description
        public DateTime? DateCreated { get; set; }     // dc:date

        // Photoshop 属性
        public string AuthorsPosition { get; set; }     // photoshop:AuthorsPosition
        public string City { get; set; }               // photoshop:City
        public string Country { get; set; }            // photoshop:Country

        // EXIF XMP 扩展（可能与原始EXIF数据重复或修正）
        public double? GPSLatitude { get; set; }        // exif:GPSLatitude
        public double? GPSLongitude { get; set; }      // exif:GPSLongitude
        public string GPSMapDatum { get; set; }         // exif:GPSMapDatum

        // 自定义或软件特定的XMP属性
        public string Rating { get; set; }              // xmp:Rating (0-5)
        public string Label { get; set; }               // xmp:Label
        public string Copyright { get; set; }           // dc:rights

        /// <summary>
        /// 存储未在以上属性中列出的其他XMP属性，键为完整的命名空间路径
        /// </summary>
        public Dictionary<string, object> AdditionalData { get; set; } = new Dictionary<string, object>();
    }
    #endregion

    #region 调用exiftool处理相片元数据的核心方法
    public class ExifToolProcessor
    {
        private string _exiftoolExeFileName { get; set; }
        private string _workingDirectory { get; set; } //exiftool.exe运行的工作目录
        public string _exiftoolExeFullPathName { get; private set; }
        private readonly object _outputLock = new object();
        private readonly object _errorLock = new object();
        private static SemaphoreSlim _processThrottler = new SemaphoreSlim(10, 10);

        public event EventHandler<string> OutputReceived; // 事件，用于通知输出

        /// <summary>
        /// 
        /// </summary>
        /// <param name="exiftoolExeFileName">主程序可指定执行程序名称，默认值exiftool.exe</param>
        /// <param name="workingDirectory">需要处理的文件所在目录，作为exittool的工作目录</param>
        public ExifToolProcessor(string exiftoolExeFileName = "exiftool.exe", string workingDirectory = null)
        {
            _exiftoolExeFileName = exiftoolExeFileName;
            _workingDirectory = workingDirectory;
            _exiftoolExeFullPathName = FindExifTool();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="exiftoolExeFileName">主程序可指定执行程序名称，默认值exiftool.exe</param>
        /// <param name="commands">exiftool的命令运行的参数列表</param>
        /// <param name="workingDirectory">需要处理的文件所在目录，作为exittool的工作目录</param>
        /// <param name="selectedFiles">需要处理的文件名列表，若不是全路径文件名，则在workingDirectory目录下工作</param>
        public ExifToolProcessor(string exiftoolExeFileName = "exiftool.exe", List<string> commands = null, string workingDirectory = null, List<string> selectedFiles = null)
        {
            _exiftoolExeFileName = exiftoolExeFileName;
            _workingDirectory = workingDirectory;
            _exiftoolExeFullPathName = FindExifTool();
        }
        private string FindExifTool()
        {
            // 1. 检查当前目录
            string currentDirTool = Path.Combine(Application.StartupPath, _exiftoolExeFileName);
            if (File.Exists(currentDirTool))
            {
                return currentDirTool;
            }
            // 2. 检查系统PATH环境变量
            string pathEnvVar = Environment.GetEnvironmentVariable("PATH");
            if (!string.IsNullOrEmpty(pathEnvVar))
            {
                string[] paths = pathEnvVar.Split(';');
                foreach (string path in paths)
                {
                    try
                    {
                        string toolPath = Path.Combine(path, _exiftoolExeFileName);
                        if (File.Exists(toolPath))
                        {
                            return toolPath;
                        }
                    }
                    catch (ArgumentException)
                    {
                        continue;
                    }
                }
            }
            // 3. 如果都没找到，抛出自定义异常
            throw new FileNotFoundException("未在当前目录或系统PATH中找到exiftool.exe。请确保已安装ExifTool并将其放置于正确位置。");
        }

        /// <summary>
        /// 触发OutputReceived事件
        /// </summary>
        protected virtual void OnOutputReceived(string message)
        {
            OutputReceived?.Invoke(this, message);
        }

        #region 执行一系列exiftool命令
        /// <summary>
        /// 逐次执行commands传输的一系列exiftool命令，主要用于独立窗口执行时的日志输出
        /// </summary>
        /// <param name="commands">List<string>一系列独立的arguments</param>
        public void ExecuteCommands(List<string> commands)
        {
            foreach (string cmd in commands)
            {
                OnOutputReceived($"*********");
                OnOutputReceived($"正在执行命令: {cmd}");
                int exitCode = ExecuteCommand(cmd);
                OnOutputReceived($"命令执行完毕，退出代码: {exitCode}");
            }
            OnOutputReceived($"====所有命令执行完毕====");
        }
        public int ExecuteCommand(string arguments)
        {
            if (string.IsNullOrEmpty(_exiftoolExeFullPathName))
                throw new InvalidOperationException("exiftool.exe路径未确定。");

            using (Process process = new Process())
            {
                process.StartInfo = ProcessInfoInit(arguments);

                // 异步读取输出和错误
                process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        OnOutputReceived(e.Data); // 触发事件
                    }
                };
                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        OnOutputReceived($"错误: {e.Data}"); // 触发事件
                    }
                };

                try
                {
                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    process.WaitForExit(); // 等待进程退出
                    return process.ExitCode;
                }
                catch (Exception ex)
                {
                    string errorMsg = $"执行命令时出错: {ex.Message}";
                    OnOutputReceived(errorMsg);
                    throw new Exception(errorMsg, ex);
                }
            }// using块结束，Process 被 Dispose
        }
        public void ExecuteCommand(List<string> commands)
        {
            foreach (string cmd in commands)
            {
                ExecuteCommand(cmd);
            }
        }
        #endregion

        /// <summary>
        /// 异步执行exiftool命令并返回结果
        /// </summary>
        /// <param name="arguments"></param>
        /// <param name="timeoutMs"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<string> ExecuteCommandAsync(string arguments, int timeoutMs = 60000)
        {
            if (string.IsNullOrEmpty(_exiftoolExeFullPathName))
                    throw new InvalidOperationException("exiftool路径未确定。");
            await _processThrottler.WaitAsync().ConfigureAwait(false);

            try
            {
                using (var process = new Process())
                {
                    process.StartInfo = ProcessInfoInit(arguments); 
                    var outputBuilder = new StringBuilder();
                    var errorBuilder = new StringBuilder();

                    // 创建信号量，用于等待读取完成
                    var outputFinished = new TaskCompletionSource<bool>();
                    var errorFinished = new TaskCompletionSource<bool>();
                    var processExited = new TaskCompletionSource<int>();
                    process.OutputDataReceived += (sender, e) =>
                    {
                        if (e.Data == null)
                        {
                            outputFinished.SetResult(true);// 接收到null表示输出流已关闭
                        }
                        else
                        {
                            lock (_outputLock)
                            {
                                outputBuilder.AppendLine(e.Data);
                            }
                        }
                    };
                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (e.Data == null)
                        {
                            errorFinished.SetResult(true);// 接收到null表示错误流已关闭
                        }
                        else
                        {
                            lock (_errorLock)
                            {
                                errorBuilder.AppendLine(e.Data);
                            }
                        }

                    };

                    process.Exited += (sender, e) =>
                    {
                        processExited.SetResult(process.ExitCode);
                    };
                    process.EnableRaisingEvents = true; // 确保 Exited 事件能触发
                    // 启动进程
                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    using (var cts = new CancellationTokenSource(timeoutMs))
                    {
                        var completedTask = await Task.WhenAny(processExited.Task, Task.Delay(timeoutMs, cts.Token)).ConfigureAwait(false);
                        if (completedTask != processExited.Task)
                        {
                            try { process.Kill(); } catch { }
                            throw new TimeoutException($"exiftool 执行超时（{timeoutMs}ms）。");
                        }
                        int exitCode = await processExited.Task.ConfigureAwait(false);
                        var streamWaitTask = Task.WhenAll(outputFinished.Task, errorFinished.Task);
                        completedTask = await Task.WhenAny(streamWaitTask, Task.Delay(5000, cts.Token)).ConfigureAwait(false); // 给流结束一个额外的缓冲时间
                        if (completedTask != streamWaitTask)
                        {
                            throw new TimeoutException("等待exiftool输出流结束超时。");
                        }
                        await streamWaitTask.ConfigureAwait(false); // 确保流读取完成
                        // 获取完整输出
                        string output = outputBuilder.ToString();
                        string error = errorBuilder.ToString();

                        // 错误处理：如果进程退出代码非0或错误流有内容，抛出异常
                        if (exitCode != 0 || !string.IsNullOrEmpty(error))
                        {
                            throw new Exception($"exiftool 错误 (代码:{exitCode}): {error}");
                        }
                        return output;
                    }// using结束，CancellationTokenSource 会被 Dispose
                }// using块结束，Process 会被 Dispose
            }
            catch (Exception ex)
            {
                LogExecution("", arguments, "", $"异常: {ex.ToString()}", 100);
                throw;
            }
            finally
            {
                // 释放信号量，允许下一个等待的进程启动
                _processThrottler.Release();
            }

        }
        /// <summary>
        /// 同步执行exiftool命令并返回结果（阻塞式）
        /// </summary>
        /// <param name="arguments">命令行参数</param>
        /// <param name="timeoutMs">超时时间（毫秒），默认1分钟</param>
        /// <returns>命令执行结果</returns>
        public string ExecuteCommandBlocking(string arguments, int timeoutMs = 60000)
        {
            if (string.IsNullOrEmpty(_exiftoolExeFullPathName))
                throw new InvalidOperationException("exiftool路径未确定。");

            try
            {
                using (var process = new Process())
                {
                    ProcessStartInfo startInfo = ProcessInfoInit(arguments);
                    process.StartInfo = startInfo;
                    // 使用异步事件处理输出
                    var outputBuilder = new StringBuilder();
                    var errorBuilder = new StringBuilder();
                    // 创建信号量，用于等待读取完成
                    var outputFinished = new ManualResetEvent(false);
                    var errorFinished = new ManualResetEvent(false);

                    process.OutputDataReceived += (sender, e) =>
                    {
                        if (e.Data == null)
                        {
                            // 接收到null表示输出流已关闭
                            outputFinished.Set();
                        }
                        else
                        {
                            lock (_outputLock)
                            {
                                outputBuilder.AppendLine(e.Data);
                            }
                        }
                    };
                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (e.Data == null)
                        {
                            // 接收到null表示错误流已关闭
                            errorFinished.Set();
                        }
                        else
                        {
                            lock (_errorLock)
                            {
                                errorBuilder.AppendLine(e.Data);
                            }
                        }
                    };

                    // 启动进程  开始异步读取
                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    // 等待退出（带超时）
                    if (!process.WaitForExit(timeoutMs)) // 默认是60秒
                    {
                        process.Kill();
                        throw new TimeoutException("exiftool 执行超时");
                    }
                    // 等待两个异步读取操作完成，设置合理的超时时间
                    if (!outputFinished.WaitOne(50000) || !errorFinished.WaitOne(50000))
                    {
                        throw new TimeoutException("等待exiftool输出超时");
                    }
                    // 获取完整输出
                    string output = outputBuilder.ToString();
                    string error = errorBuilder.ToString();

                    //LogExecution("", arguments, output, error, process.ExitCode);
                    if (process.ExitCode != 0 || !string.IsNullOrEmpty(error))
                        throw new Exception($"exiftool 错误 (代码:{process.ExitCode}): {error}");

                    return output;
                }// using块结束，Process 会被 Dispose

            }
            catch (Exception ex)
            {
                LogExecution("", arguments, "", $"异常: {ex.ToString()}", 100);
                throw; // 重新抛出，让调用方感知到失败
            }
        }
        private ProcessStartInfo ProcessInfoInit(string arguments)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = _exiftoolExeFullPathName,
                Arguments = arguments,
                WorkingDirectory = _workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                // 同时设置输出编码为UTF-8，以便正确读取输出
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
                // 设置环境变量
                Environment = { ["PERL_UNICODE"] = "AS" },
            };
            return startInfo;
        }
        // 记录执行日志
        private static void LogExecution(string imagePath, string arguments, string output, string error, int exitCode)
        {
            string log = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] 执行命令: exiftool.exe {arguments}{Environment.NewLine}" +
                        $"文件: {imagePath}{Environment.NewLine}" +
                        $"退出代码: {exitCode}{Environment.NewLine}" +
                        $"输出: {(string.IsNullOrEmpty(output) ? "<空>" : output)}{Environment.NewLine}" +
                        $"错误: {(string.IsNullOrEmpty(error) ? "<空>" : error)}{Environment.NewLine}" +
                        new string('-', 80) + $"{Environment.NewLine}";
            Debug.WriteLine($"{log}");

           // File.AppendAllText("exiftool.log", log);
        }
    }
    #endregion

    #region 辅助类：ExifTool参数格式化和输出解析
    public class ExiftoolArgumentsFormat
    {
        /// <summary>
        /// 将经纬度字符串转换为 ExifTool 标准化参数
        /// </summary>
        /// <param name="coordinates">经纬度字符串（格式："纬度,经度"）</param>
        /// <returns>ExifTool 标准化参数字符串</returns>
        public static string ConvertCoordinatesToExifToolParams(string coordinates)
        {
            // 分割字符串获取纬度和经度
            string[] parts = coordinates.Split(',');
            if (parts.Length != 2)
            {
                throw new ArgumentException("无效的经纬度格式。请使用 '纬度,经度' 格式。", nameof(coordinates));
            }

            // 解析纬度和经度值
            if (!double.TryParse(parts[0].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out double latitude) ||
                !double.TryParse(parts[1].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out double longitude))
            {
                throw new ArgumentException("无法解析经纬度值。请确保使用有效的数字格式。", nameof(coordinates));
            }

            // 创建参数构建器
            var sb = new StringBuilder();

            // 处理纬度
            AppendCoordinate(sb, "GPSLatitude", Math.Abs(latitude), latitude < 0 ? "S" : "N");

            // 处理经度
            AppendCoordinate(sb, "GPSLongitude", Math.Abs(longitude), longitude < 0 ? "W" : "E");

            // 添加覆盖原始文件参数
            sb.AppendLine("-overwrite_original ");

            return sb.ToString().Replace(Environment.NewLine," ");
        }
        private static void AppendCoordinate(StringBuilder sb, string tagName, double value, string refValue)
        {
            // 优化精度：保留最多6位小数，但去除不必要的尾随零
            string formattedValue = value.ToString("0.######", CultureInfo.InvariantCulture);

            sb.AppendLine($"-{tagName}={formattedValue} ");
            sb.AppendLine($"-{tagName}Ref={refValue} ");
        }
    }

    /// <summary>
    /// exiftool输出数据的通用解析器，支持模糊匹配和大小写不敏感查询
    /// </summary>
    public class ExiftToolOutputParser
    {
        private readonly Dictionary<string, string> _exifData;
        private readonly Dictionary<string, string> _normalizedKeys;

        /// <summary>
        /// 初始化解析器
        /// </summary>
        /// <param name="exifToolOutput">ExifTool的原始输出文本</param>
        public ExiftToolOutputParser(string exifToolOutput)
        {
            _exifData = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _normalizedKeys = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            ParseExifToolOutput(exifToolOutput);
        }

        /// <summary>
        /// 解析ExifTool的原始输出
        /// </summary>
        private void ParseExifToolOutput(string output)
        {
            if (string.IsNullOrWhiteSpace(output))
                return;

            var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                // 使用冒号分隔键值对（最多分割成两部分）
                var separatorIndex = line.IndexOf(':');
                if (separatorIndex <= 0) continue;

                var key = line.Substring(0, separatorIndex).Trim();
                var value = line.Substring(separatorIndex + 1).Trim();

                if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                {
                    // 存储原始数据
                    _exifData[key] = value;

                    // 生成规范化的键用于模糊匹配
                    var normalizedKey = NormalizeKey(key);
                    _normalizedKeys[normalizedKey] = key;
                }
            }
        }

        /// <summary>
        /// 规范化键名用于模糊匹配
        /// </summary>
        private string NormalizeKey(string key)
        {
            if (string.IsNullOrEmpty(key))
                return key;

            // 移除特殊字符、多余空格，统一为小写
            return Regex.Replace(key, @"[^a-zA-Z0-9]", " ")
                       .ToLowerInvariant()
                       .Replace(" ", "")
                       .Trim();
        }

        /// <summary>
        /// 根据键名获取值（精确匹配）
        /// </summary>
        /// <param name="key">要查找的键名</param>
        /// <returns>对应的值，如果未找到返回null</returns>
        public string GetValue(string key)
        {
            if (_exifData.TryGetValue(key, out string value))
            {
                return value;
            }
            return null;
        }

        /// <summary>
        /// 使用模糊匹配查找值
        /// </summary>
        /// <param name="searchTerm">搜索词（支持模糊匹配）</param>
        /// <returns>匹配到的键值对</returns>
        public KeyValuePair<string, string>? FindValueFuzzy(string searchTerm)
        {
            //if (string.IsNullOrWhiteSpace(searchTerm))
            //    return null;

            //var normalizedSearch = NormalizeKey(searchTerm);

            //// 查找包含搜索词的所有键
            //var matches = _normalizedKeys.Where(kvp =>
            //    kvp.Key.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase))
            //    .ToList();

            //if (matches.Count == 1)
            //{
            //    var originalKey = matches[0].Value;
            //    return new KeyValuePair<string, string>(originalKey, _exifData[originalKey]);
            //}
            //else if (matches.Count > 1)
            //{
            //    // 如果有多个匹配，返回相似度最高的一个
            //    var bestMatch = matches.OrderByDescending(kvp =>
            //        GetSimilarityScore(kvp.Key, normalizedSearch)).First();
            //    var originalKey = bestMatch.Value;
            //    return new KeyValuePair<string, string>(originalKey, _exifData[originalKey]);
            //}

            return null;
        }

        /// <summary>
        /// 获取所有匹配模糊搜索的结果
        /// </summary>
        public Dictionary<string, string> FindAllValuesFuzzy(string searchTerm)
        {
            var results = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            //if (string.IsNullOrWhiteSpace(searchTerm))
            //    return results;

            //var normalizedSearch = NormalizeKey(searchTerm);

            //var matches = _normalizedKeys.Where(kvp =>
            //    kvp.Key.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase));

            //foreach (var match in matches)
            //{
            //    results[match.Value] = _exifData[match.Value];
            //}

            return results;
        }

        /// <summary>
        /// 计算字符串相似度得分（简单的重叠字符计数）
        /// </summary>
        private int GetSimilarityScore(string source, string target)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target))
                return 0;

            return source.Intersect(target).Count();
        }

        /// <summary>
        /// 尝试将GPS坐标从度分秒格式转换为十进制
        /// </summary>
        public double? TryParseGPSCoordinate(string gpsValue)
        {
            if (string.IsNullOrWhiteSpace(gpsValue))
                return null;

            try
            {
                // 匹配度分秒格式：12 deg 3' 40.55" S
                var match = Regex.Match(gpsValue,
                    @"(\d+)\s*deg\s*(\d+)\s*'\s*([\d.]+)\s*""\s*([NSEW])",
                    RegexOptions.IgnoreCase);

                if (match.Success)
                {
                    var degrees = double.Parse(match.Groups[1].Value);
                    var minutes = double.Parse(match.Groups[2].Value);
                    var seconds = double.Parse(match.Groups[3].Value);
                    var direction = match.Groups[4].Value.ToUpper();

                    var decimalDegrees = degrees + (minutes / 60.0) + (seconds / 3600.0);

                    // 根据方向调整正负号
                    if (direction == "S" || direction == "W")
                        decimalDegrees = -decimalDegrees;

                    return decimalDegrees;
                }

                // 尝试直接解析十进制格式
                if (double.TryParse(gpsValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
                {
                    return result;
                }
            }
            catch
            {
                // 解析失败返回null
            }

            return null;
        }

        /// <summary>
        /// 获取所有解析的EXIF数据
        /// </summary>
        public IReadOnlyDictionary<string, string> GetAllData()
        {
            return _exifData;
        }

        /// <summary>
        /// 检查是否包含特定键
        /// </summary>
        public bool ContainsKey(string key)
        {
            return _exifData.ContainsKey(key);
        }

        /// <summary>
        /// 获取特定日期时间格式的值
        /// </summary>
        public DateTime? GetDateTimeValue(string key)
        {
            var value = GetValue(key);
            if (string.IsNullOrWhiteSpace(value))
                return null;

            // 尝试解析EXIF日期时间格式：1981:01:01 17:46:22
            if (DateTime.TryParseExact(value, "yyyy:MM:dd HH:mm:ss",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
            {
                return result;
            }

            return null;
        }
    }
    #endregion
}