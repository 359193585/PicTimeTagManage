/*****************************************
 *
 * 项目名称： PicTimeTagManage  
 * 文 件 名:  GenerateThumbnail.cs
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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;
using Encoder = System.Drawing.Imaging.Encoder;

namespace PicTimeTagManage
{
    public class GenerateThumbnail
    {
        private bool _useOrientationProp { get; set; }  //是否使用图片的方向标记显示图片
        public GenerateThumbnail(bool useOrientationProp)
        {
            _useOrientationProp = useOrientationProp;
        }
        /// <summary>
        ///  生成高质量预览缩略图（可自动校正EXIF方向）
        /// </summary>
        /// <param name="imagePath">图片路径</param>
        /// <param name="maxWidth">最大宽度</param>
        /// <param name="maxHeight">最大高度</param>
        /// <returns>生成的缩略图</returns>
        public Image GetThumbnailImg(string imagePath, int maxWidth, int maxHeight)
        {
            try
            {
                using (Image originalImage = Image.FromFile(imagePath))
                {
                    Image thumbnail = GeneratePreviewThumbnail(originalImage, maxWidth, maxHeight);
                    return thumbnail;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载或校正图片失败: {ex.Message}");
                return null;
            }
        }
        public Image GeneratePreviewThumbnail(Image originalImage, int maxWidth, int maxHeight)
        {
            Image correctedImage = _useOrientationProp ? CorrectImageOrientation(originalImage) : originalImage;
            Size newSize = CalculateNewSize(correctedImage.Size, maxWidth, maxHeight);
            Bitmap thumbnail = new Bitmap(newSize.Width, newSize.Height);
            using (Graphics graphics = Graphics.FromImage(thumbnail))
            {
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.DrawImage(correctedImage, 0, 0, newSize.Width, newSize.Height);
            }
            // 重要：如果校正过程创建了新的图像副本，需要释放它
            if (correctedImage != originalImage)
            {
                correctedImage.Dispose();
            }
            return thumbnail;
        }
        /// <summary>
        /// 根据EXIF方向标记校正图像方向
        /// </summary>
        private Image CorrectImageOrientation(Image image)
        {
            try
            {
                // EXIF方向标记的ID是274（0x0112）
                if (Array.IndexOf(image.PropertyIdList, 274) > -1)
                {
                    PropertyItem orientationProp = image.GetPropertyItem(274);
                    byte orientationValue = orientationProp.Value[0];

                    // 根据方向值确定如何旋转和翻转
                    RotateFlipType rotateFlipType = GetRotateFlipType(orientationValue);
                    if (rotateFlipType != RotateFlipType.RotateNoneFlipNone)
                    {
                        // 创建图像副本并进行旋转校正
                        Bitmap correctedImage = new Bitmap(image);
                        correctedImage.RotateFlip(rotateFlipType);
                        
                        return correctedImage;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"校正图像方向时发生错误: {ex.Message}");
            }
            return image;// 无需校正，返回原图
        }
        /// <summary>
        /// 将EXIF方向值转换为对应的旋转翻转类型
        /// </summary>
        private  RotateFlipType GetRotateFlipType(byte orientationValue)
        {
            switch (orientationValue)
            {
                case 2: return RotateFlipType.RotateNoneFlipX;   // 水平翻转
                case 3: return RotateFlipType.Rotate180FlipNone; // 旋转180度
                case 4: return RotateFlipType.RotateNoneFlipY;   // 垂直翻转
                case 5: return RotateFlipType.Rotate90FlipX;     // 旋转90度并水平翻转
                case 6: return RotateFlipType.Rotate90FlipNone;  // 顺时针旋转90度
                case 7: return RotateFlipType.Rotate270FlipX;    // 旋转270度并水平翻转
                case 8: return RotateFlipType.Rotate270FlipNone; // 顺时针旋转270度
                default: return RotateFlipType.RotateNoneFlipNone; // 默认不旋转（方向值1或其他）
            }
        }
        /// <summary>
        /// 计算保持纵横比的新尺寸
        /// </summary>
        private Size CalculateNewSize(Size originalSize, int maxWidth, int maxHeight)
        {
            double widthRatio = (double)maxWidth / originalSize.Width;
            double heightRatio = (double)maxHeight / originalSize.Height;
            double ratio = Math.Min(widthRatio, heightRatio);

            return new Size(
                (int)(originalSize.Width * ratio),
                (int)(originalSize.Height * ratio)
            );
        }

        #region 如果图片有方向标记，就按照方向标记旋转图片并重新保存图片，方向标记修改为1
        public void ReSaveImage(string imagePath)
        {
            Image correctedImage = GetNewerImage(imagePath, out bool status);
            if (status == false) return;
            try
            {
                // 保存校正后的图像（保留高质量编码参数）
                SaveImageWithQuality(correctedImage, imagePath, 90L);
                Console.WriteLine("图像旋转并保存完成！");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"图像旋转保存失败:{ex.Message}");
            }
        }
        private Image GetNewerImage(string imagePath,out bool status)
        {
            using (Image originalImage = Image.FromFile(imagePath))
            {
                try
                {
                    Image correctedImage = CorrectImageOrientation(originalImage);
                    // 保留原始EXIF数据到correctedImage
                    PreserveExifData(originalImage, correctedImage);
                    // 更新方向标记为正常（1）
                    UpdateOrientationTag(correctedImage, 1);
                    status = true;
                    return correctedImage;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"处理图像时发生错误: {ex.Message}");
                    status = false;
                    return null;
                }
            }
        }
        /// <summary>
        /// 保留原始EXIF数据到destination
        /// </summary>
        private void PreserveExifData(Image source, Image destination)
        {
            try
            {
                foreach (PropertyItem propItem in source.PropertyItems)
                {
                    try
                    {
                        // 对于宽度和高度标记，使用方向调整后的实际尺寸
                        if (propItem.Id == 0x0100) // ImageWidth
                        {
                            propItem.Value = BitConverter.GetBytes(destination.Width);
                        }
                        else if (propItem.Id == 0x0101) // ImageHeight
                        {
                            propItem.Value = BitConverter.GetBytes(destination.Height);
                        }
                        destination.SetPropertyItem(propItem);
                    }
                    catch
                    {
                        // 忽略无法设置的属性
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"保留EXIF数据失败: {ex.Message}");
            }
        }
        /// <summary>
        /// 保留所有EXIF数据，并更新尺寸信息
        /// </summary>
        private void PreserveAllExifData(Image source, Bitmap destination, int originalOrientation)
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
        /// <summary>
        /// 更新图像的方向标记
        /// </summary>
        private void UpdateOrientationTag(Image image, byte orientationValue)
        {
            try
            {
                if (Array.IndexOf(image.PropertyIdList, 274) > -1)
                {
                    PropertyItem orientationProp = image.GetPropertyItem(274);
                    orientationProp.Value[0] = orientationValue;
                    image.SetPropertyItem(orientationProp);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"更新方向标记失败: {ex.Message}");
            }
        }
        /// <summary>
        /// 高质量保存图像
        /// </summary>
        private void SaveImageWithQuality(Image image, string path, long quality)
        {
            if (quality < 1L || quality > 100L)
                quality = 90L;

            // 设置编码参数（JPEG质量）
            EncoderParameters encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, quality);

            // 获取JPEG编码器
            ImageCodecInfo jpegCodec = GetEncoderInfo("image/jpeg");

            if (jpegCodec != null)
            {
                image.Save(path, jpegCodec, encoderParams);
            }
            else
            {
                // 回退到默认保存方式
                image.Save(path, ImageFormat.Jpeg);
            }
        }
        /// <summary>
        /// 获取指定MIME类型的图像编码器
        /// </summary>
        private ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            return codecs.FirstOrDefault(codec => codec.MimeType == mimeType);
        }
        #endregion
    }

}
