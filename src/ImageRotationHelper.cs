/*****************************************
 *
 * 项目名称： PicTimeTagManage  
 * 文 件 名:  ImageRotationHelper.cs
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
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

public class ImageRotationHelper
{
    /// <summary>
    /// 根据EXIF方向标记实际旋转图像，并保留所有元数据
    /// </summary>
    public static Bitmap RotateImageAccordingToExif(Image originalImage)
    {
        // 创建原图像的副本，避免修改原图
        Bitmap rotatedImage = new Bitmap(originalImage);

        // 获取EXIF方向标记
        int orientation = GetExifOrientation(originalImage);

        // 根据方向值进行相应的旋转
        RotateFlipType rotateFlipType = GetRotateFlipType(orientation);
        if (rotateFlipType != RotateFlipType.RotateNoneFlipNone)
        {
            rotatedImage.RotateFlip(rotateFlipType);
        }

        // 保留所有EXIF元数据（包括更新尺寸信息）
        PreserveAllExifData(originalImage, rotatedImage, orientation);

        return rotatedImage;
    }

    /// <summary>
    /// 获取图像的EXIF方向值
    /// </summary>
    private static int GetExifOrientation(Image image)
    {
        try
        {
            // EXIF方向标记的ID是274（0x0112）
            if (Array.IndexOf(image.PropertyIdList, 274) > -1)
            {
                PropertyItem prop = image.GetPropertyItem(274);
                return prop.Value[0];
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"读取EXIF方向信息失败: {ex.Message}");
        }

        return 1; // 默认值，表示不需要旋转
    }

    /// <summary>
    /// 将EXIF方向值转换为RotateFlipType
    /// </summary>
    private static RotateFlipType GetRotateFlipType(int orientation)
    {
        switch (orientation)
        {
            case 2: return RotateFlipType.RotateNoneFlipX;
            case 3: return RotateFlipType.Rotate180FlipNone;
            case 4: return RotateFlipType.RotateNoneFlipY;
            case 5: return RotateFlipType.Rotate90FlipX;
            case 6: return RotateFlipType.Rotate90FlipNone; // 需要顺时针旋转90度
            case 7: return RotateFlipType.Rotate270FlipX;
            case 8: return RotateFlipType.Rotate270FlipNone;
            default: return RotateFlipType.RotateNoneFlipNone;
        }
    }

    /// <summary>
    /// 保留所有EXIF数据，并更新尺寸信息
    /// </summary>
    private static void PreserveAllExifData(Image source, Bitmap destination, int originalOrientation)
    {
        try
        {
            // 复制所有属性
            foreach (PropertyItem propItem in source.PropertyItems)
            {
                try
                {
                    // 对于方向标记，旋转后应重置为1（正常方向）
                    if (propItem.Id == 274)
                    {
                        propItem.Value[0] = 1; // 设置为正常方向
                    }
                    // 更新宽度和高度信息（如果旋转改变了尺寸）
                    else if (propItem.Id == 0x0100) // ImageWidth
                    {
                        // 只有旋转90或270度时宽度和高度会交换
                        if (originalOrientation == 5 || originalOrientation == 6 ||
                            originalOrientation == 7 || originalOrientation == 8)
                        {
                            propItem.Value = BitConverter.GetBytes(destination.Height);
                        }
                        else
                        {
                            propItem.Value = BitConverter.GetBytes(destination.Width);
                        }
                    }
                    else if (propItem.Id == 0x0101) // ImageHeight
                    {
                        if (originalOrientation == 5 || originalOrientation == 6 ||
                            originalOrientation == 7 || originalOrientation == 8)
                        {
                            propItem.Value = BitConverter.GetBytes(destination.Width);
                        }
                        else
                        {
                            propItem.Value = BitConverter.GetBytes(destination.Height);
                        }
                    }

                    destination.SetPropertyItem(propItem);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"复制属性ID {propItem.Id} 失败: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"保留EXIF数据时发生错误: {ex.Message}");
        }
    }
}