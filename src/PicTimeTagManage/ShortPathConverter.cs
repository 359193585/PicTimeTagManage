/*****************************************
 *
 * 项目名称： PicTimeTagManage  
 * 文 件 名:  ShortPathConverter.cs
 * 命名空间： %Namespace%
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
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

public class ShortPathConverter
{
    // 导入 Windows API 函数
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern uint GetShortPathName(
        [MarshalAs(UnmanagedType.LPTStr)] string longPath,
        [MarshalAs(UnmanagedType.LPTStr)] StringBuilder shortPath,
        uint bufferSize);

    /// <summary>
    /// 获取文件的 8.3 格式短路径
    /// </summary>
    /// <param name="longPath">原始长路径</param>
    /// <returns>8.3 格式的短路径</returns>
    public static string GetShortPath(string longPath)
    {
        // 验证路径是否存在
        if (!File.Exists(longPath) && !Directory.Exists(longPath))
            throw new FileNotFoundException("文件或目录不存在", longPath);

        // 初始化缓冲区
        uint bufferSize = 256;
        var shortPathBuilder = new StringBuilder((int)bufferSize);

        // 调用 API 获取短路径
        uint result = GetShortPathName(longPath, shortPathBuilder, bufferSize);

        // 处理缓冲区不足的情况
        if (result == 0)
            throw new Exception($"获取短路径失败，错误代码: {Marshal.GetLastWin32Error()}");

        if (result > bufferSize)
        {
            bufferSize = result;
            shortPathBuilder = new StringBuilder((int)bufferSize);
            result = GetShortPathName(longPath, shortPathBuilder, bufferSize);
            if (result == 0 || result > bufferSize)
                throw new Exception($"获取短路径失败，错误代码: {Marshal.GetLastWin32Error()}");
        }

        return shortPathBuilder.ToString();
    }
}