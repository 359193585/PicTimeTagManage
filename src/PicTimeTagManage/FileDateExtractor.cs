/*****************************************
 *
 * 项目名称： PicTimeTagManage  
 * 文 件 名:  FileDateExtractor.cs
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
using System.Text.RegularExpressions;

namespace PicTimeTagManage
{
    /// <summary>
    /// 从可能含有日期时间信息的字符串获取DateTime
    /// </summary>
    public class StringDateExtractor
    {
        /// <summary>
        /// 从字符中提取日期信息和有用文本
        /// </summary>
        /// <param name="SourceStr">不包含路径和扩展名</param>
        /// <returns>包含提取信息的StringDateInfo对象</returns>
        public static StringDateInfo ExtractDateInfo(string SourceStr)
        {
            // 常见日期模式列表（按优先级排序）
            // 定义支持的各种日期时间格式模式（优先级从具体到一般）
            var patterns = new[]
            {
            // 格式: yyyyMMdd_HHmmss (紧凑格式带下划线)
            new
            {
                Pattern = @"(\d{4})(\d{2})(\d{2})_(\d{2})(\d{2})(\d{2})",
                Groups = 6
            },
            // 格式: yyyyMMdd.HHmmss (紧凑格式带下划线)
            new
            {
                Pattern = @"(\d{4})(\d{2})(\d{2})\.(\d{2})(\d{2})(\d{2})",
                Groups = 6
            },
            // 日期横线/下划线，空格/横线/下划线后面接时间紧凑格式)
            // yyyy-MM-dd HHmmss,
            // yyyy-MM-dd-HHmmss,
            // yyyy-MM-dd_HHmmss,
            // yyyy_MM_dd HHmmss,
            // yyyy_MM_dd-HHmmss
            // yyyy_MM_dd_HHmmss
            new
            {
                Pattern = @"(\d{4})[-_](\d{2})[-_](\d{2})[-_\s](\d{2})(\d{2})(\d{2})",
                Groups = 6
            },
            // 格式: yyyy_MM_dd_HH_mm_ss 或 yyyy-MM-dd HHmmss
            new
            {
                Pattern = @"(\d{4})[-_](\d{2})[-_](\d{2})[-_\s](\d{2})[-_](\d{2})[-_](\d{2})",
                Groups = 6
            },
            // 格式: yyyy:MM:dd HH:mm:ss
            new
            {
                Pattern = @"(\d{4})[:](\d{2})[:](\d{2})[-_\s](\d{2})[:](\d{2})[:](\d{2})",
                Groups = 6
            },
            // 格式: yyyy_MM_dd_HH_mm 或 yyyy-MM-dd HHmm
            new
            {
                Pattern = @"(\d{4})[-_](\d{2})[-_](\d{2})[-_\s](\d{2})[-_](\d{2})",
                Groups = 5
            },
            // 格式: yyyyMMddHHmmss (紧凑格式)
            new
            {
                Pattern = @"(\d{4})(\d{2})(\d{2})(\d{2})(\d{2})(\d{2})",
                Groups = 6
            },
            // 格式: yyyyMMddHHmm (紧凑格式)
            new
            {
                Pattern = @"(\d{4})(\d{2})(\d{2})(\d{2})(\d{2})",
                Groups = 5
            },
            // 格式: yyyy_MM_dd 或 yyyy-MM-dd
            new
            {
                Pattern = @"(\d{4})[-_](\d{2})[-_](\d{2})",
                Groups = 3
            },
            // 格式: yyyyMMdd (紧凑格式)
            new
            {
                Pattern = @"(\d{4})(\d{2})(\d{2})",
                Groups = 3
            }
            };


            // 尝试匹配每种模式
            foreach (var patternInfo in patterns)
            {
                Match match = Regex.Match(SourceStr, patternInfo.Pattern);
                if (match.Success)
                {
                    try
                    {
                        DateTime dateTime;
                        string dateFormat;
                        string dateString;
                        switch (patternInfo.Groups)
                        {
                            case 6: // 有6个捕获组: 年月日时分秒
                                dateTime = new DateTime(
                                    int.Parse(match.Groups[1].Value),
                                    int.Parse(match.Groups[2].Value),
                                    int.Parse(match.Groups[3].Value),
                                    int.Parse(match.Groups[4].Value),
                                    int.Parse(match.Groups[5].Value),
                                    int.Parse(match.Groups[6].Value));
                                dateFormat = "yyyy-MM-dd_HHmmss";
                                dateString = dateTime.ToString("yyyy-MM-dd_HHmmss");
                                break;
                            case 5: // 有5个捕获组: 年月日时分
                                dateTime = new DateTime(
                                    int.Parse(match.Groups[1].Value),
                                    int.Parse(match.Groups[2].Value),
                                    int.Parse(match.Groups[3].Value),
                                    int.Parse(match.Groups[4].Value),
                                    int.Parse(match.Groups[5].Value),
                                    0); // 秒数设为0
                                dateFormat = "yyyy-MM-dd_HHmm";
                                dateString = dateTime.ToString("yyyy-MM-dd_HHmm");
                                break;
                            case 3: // 有3个捕获组: 年月日
                                dateTime = new DateTime(
                                    int.Parse(match.Groups[1].Value),
                                    int.Parse(match.Groups[2].Value),
                                    int.Parse(match.Groups[3].Value));
                                dateFormat = "yyyy-MM-dd";
                                dateString = dateTime.ToString("yyyy-MM-dd");
                                break;
                            default:
                                continue; // 不匹配任何已知格式，尝试下一个模式
                        }

                        // 提取剩余文本（去除日期部分）
                        string remainingText = SourceStr.Replace(match.Value, "").Trim();

                        // 清理剩余文本中的多余分隔符，但保留有意义的文本
                        remainingText = Regex.Replace(remainingText, @"^[-_\s]+|[-_\s]+$", ""); // 去除首尾分隔符
                        remainingText = Regex.Replace(remainingText, @"[-_\s]+", " "); // 将中间的多余分隔符替换为单个空格
                        remainingText = remainingText.Trim();

                        return new StringDateInfo
                        {
                            OriginalString = SourceStr,
                            DateTime = dateTime,
                            DateFormat = dateFormat,
                            DateString = dateString,
                            RemainingText = string.IsNullOrEmpty(remainingText) ? null : remainingText,
                            Extension = ""
                        };
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        // 日期值超出范围（如2月30日），尝试下一个模式
                        continue;
                    }
                }
            }

            // 未找到日期模式
            return new StringDateInfo
            {
                OriginalString = SourceStr,
                DateTime = null,
                DateFormat = null,
                DateString = null,
                RemainingText = SourceStr,
                Extension = ""
            };
        }
    }
    /// <summary>
    /// 包含文件名日期提取结果的对象
    /// </summary>
    public class StringDateInfo
    {
        public string OriginalString { get; set; }
        public DateTime? DateTime { get; set; }
        public string DateFormat { get; set; }
        public string DateString { get; set; }
        public string RemainingText { get; set; }
        public string Extension { get; set; }
    }

}

