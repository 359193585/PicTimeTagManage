/*****************************************
 *
 * 项目名称： PicTimeTagManage  
 * 文 件 名:  GpsCoordinateValidator.cs
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
using System.Globalization;
using System.Text.RegularExpressions;

public static class GpsCoordinateValidator
{
    /// <summary>
    /// 验证GPS坐标格式是否有效
    /// </summary>
    /// <param name="coordinateString">GPS坐标字符串，格式为"纬度,经度"</param>
    /// <returns>验证结果和错误信息（如果无效）</returns>
    public static (bool isValid, string message) ValidateGpsCoordinate(string coordinateString)
    {
        if (string.IsNullOrWhiteSpace(coordinateString))
            return (false, "坐标字符串不能为空");

        // 按逗号分割坐标部分
        string[] parts = coordinateString.Split(',');
        if (parts.Length != 2)
            return (false, "坐标格式不正确，应为'纬度,经度'格式");

        // 去除空格
        string latString = parts[0].Trim();
        string lonString = parts[1].Trim();

        // 验证纬度
        if (!double.TryParse(latString, NumberStyles.Float, CultureInfo.InvariantCulture, out double latitude))
            return (false, "纬度格式无效，必须是有效的数字");

        // 验证经度
        if (!double.TryParse(lonString, NumberStyles.Float, CultureInfo.InvariantCulture, out double longitude))
            return (false, "经度格式无效，必须是有效的数字");

        // 检查纬度范围（-90° 到 +90°）
        if (latitude < -90.0 || latitude > 90.0)
            return (false, $"纬度值 {latitude} 超出有效范围（-90° 到 +90°）");

        // 检查经度范围（-180° 到 +180°）
        if (longitude < -180.0 || longitude > 180.0)
            return (false, $"经度值 {longitude} 超出有效范围（-180° 到 +180°）");

        return (true, "坐标格式有效");
    }
    /// <summary>
    /// 返回解析后的坐标值
    /// </summary>
    /// <param name="coordinateString">格式：逗号分割的纬度，精度字符串，如“32.010203,120.102030”</param>
    /// <returns></returns>
    public static (bool isValid, string message, double latitude, double longitude)
        ValidateAndParseGpsCoordinate(string coordinateString)
    {
        var result = ValidateGpsCoordinate(coordinateString);

        if (result.isValid)
        {
            string[] parts = coordinateString.Split(',');
            double lat = double.Parse(parts[0].Trim(), CultureInfo.InvariantCulture);
            double lon = double.Parse(parts[1].Trim(), CultureInfo.InvariantCulture);

            return (true, result.message, lat, lon);
        }

        return (false, result.message, 0, 0);
    }
    public static void GetMapUrl(string gpsStr, out string url)
    {
        string urlHead = "https://www.bing.com/maps?lvl=15.0&cp=";
        var detailedResult = GpsCoordinateValidator.ValidateAndParseGpsCoordinate(gpsStr);
        if (detailedResult.isValid)
        {
            double NewLat = detailedResult.latitude - 0.0022;
            double NewLong = detailedResult.longitude - 0.0022;

            url = $"{urlHead}{NewLat:F6}~{NewLong:F6}";
        }
        else
        {
            //MessageBox.Show($"你输入的坐标位置可能错误:{detailedResult}", "提醒");
            url = $"{urlHead}32.066247%7E118.75298";
            return;
        }
    }
}

public static class GpsConverter
{
    /// <summary>
    /// 将度分秒格式的GPS坐标转换为十进制格式
    /// </summary>
    /// <param name="dms">度分秒格式的字符串（例如：32 deg 3' 40.55"）</param>
    /// <returns>十进制坐标值</returns>
    public static double ConvertDmsToDecimal(string dms)
    {
        // 使用正则表达式提取度、分、秒
        var match = Regex.Match(dms, @"(\d+)\s*deg\s*(\d+)'\s*([\d.]+)""");

        if (!match.Success || match.Groups.Count < 4)
        {
            throw new ArgumentException("无效的GPS坐标格式", nameof(dms));
        }

        // 解析各部分数值
        double degrees = double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
        double minutes = double.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
        double seconds = double.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture);

        // 计算十进制坐标
        double _rtn =  degrees + (minutes / 60.0) + (seconds / 3600.0);
        return _rtn;
    }
    /// <summary>
    /// 将ExifTool输出的GPS经纬度转换为十进制格式
    /// </summary>
    /// <param name="latitude">纬度字符串（例如：32 deg 3' 40.55"）</param>
    /// <param name="longitude">经度字符串（例如：118 deg 50' 53.63"）</param>
    /// <returns>包含十进制纬度和经度的元组</returns>
    public static (double Latitude, double Longitude) ConvertGpsCoordinates(string latitude, string longitude)
    {
        return (
            ConvertDmsToDecimal(latitude),
            ConvertDmsToDecimal(longitude)
        );
    }

    /// <summary>
    /// 将 ExifTool 输出的 GPS 信息转换为带符号的十进制格式
    /// </summary>
    /// <param name="latitude">纬度字符串（如 "32 deg 3' 40.55\""）</param>
    /// <param name="latitudeRef">纬度参考（如 "N" 或 "S"）</param>
    /// <param name="longitude">经度字符串（如 "118 deg 50' 53.63\""）</param>
    /// <param name="longitudeRef">经度参考（如 "E" 或 "W"）</param>
    /// <returns>带符号的十进制坐标元组（纬度, 经度）</returns>
    public static (double Latitude, double Longitude) ConvertToSignedDecimal(
        string latitude, string latitudeRef,
        string longitude, string longitudeRef)

    {
        if (string.IsNullOrEmpty(latitude)) return (0.0, 0.0);
        return (
            ConvertDmsToDecimal(latitude, latitudeRef),
            ConvertDmsToDecimal(longitude, longitudeRef)
        );
    }

    /// <summary>
    /// 将单个坐标值转换为带符号的十进制格式
    /// </summary>
    private static double ConvertDmsToDecimal(string dms, string reference)
    {
        // 解析度分秒格式
        double value = ConvertDmsToDecimal(dms);

        // 根据参考值添加符号
        return ApplyReferenceSign(value, reference);
    }

    /// <summary>
    /// 应用参考值的符号
    /// </summary>
    private static double ApplyReferenceSign(double value, string reference)
    {
        if (string.IsNullOrWhiteSpace(reference))
            return value;

        // 取参考值的第一个字符（支持 "N" 和 "North" 等格式）
        char refChar = reference.Trim().ToUpper()[0];
        if(refChar =='S' || refChar == 'W')// 南纬或西经为负值
        {
            return -value;
        }
        else
        {
            return value;
        }
    }
    
}

