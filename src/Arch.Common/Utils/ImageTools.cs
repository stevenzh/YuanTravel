using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;

namespace Arch.Common.Utils
{
    /// <summary>
    /// 处理图片的工具类
    /// </summary>
    public class ImageTools
    {
        /// <summary>
        /// Stream 和 byte[] 之间的转换 
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public byte[] StreamToBytes(Stream stream)
        {
            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);

            // 设置当前流的位置为流的开始 
            stream.Seek(0, SeekOrigin.Begin);
            return bytes;
        }

        /// <summary>
        /// 获取指定mimeType的ImageCodecInfo
        /// </summary>
        private ImageCodecInfo GetImageCodecInfo(string mimeType)
        {
            ImageCodecInfo[] codecInfo = ImageCodecInfo.GetImageEncoders();
            foreach (ImageCodecInfo ici in codecInfo)
            {
                if (ici.MimeType == mimeType) return ici;
            }
            return null;
        }

        /// <summary>
        ///  获取inputStream中的Bitmap对象
        /// </summary>
        public Bitmap GetBitmapFromStream(Stream inputStream)
        {
            Bitmap bitmap = new Bitmap(inputStream);
            return bitmap;
        }

        /// <summary>
        /// 将Bitmap对象压缩为JPG图片类型
        /// </summary>
        /// <param name="bmp">源bitmap对象</param>
        /// <param name="saveFilePath">目标图片的存储地址</param>
        /// <param name="quality">压缩质量，越大照片越清晰，推荐80</param>
        public bool ToCompressAsJPG(Bitmap bmp, string saveFilePath, int quality = 80)
        {
            try
            {
                EncoderParameter p = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality); ;
                EncoderParameters ps = new EncoderParameters(1);
                ps.Param[0] = p;
                bmp.Save(saveFilePath, GetImageCodecInfo("image/jpeg"), ps);
                bmp.Dispose();
            }
            catch (Exception)
            {
                return false;
            }

            return true;

        }

        /// <summary>
        /// 将inputStream中的对象压缩为JPG图片类型
        /// </summary>
        /// <param name="inputStream">源Stream对象</param>
        /// <param name="saveFilePath">目标图片的存储地址</param>
        /// <param name="quality">压缩质量，越大照片越清晰，推荐80</param>
        public bool ToCompressAsJPG(Stream inputStream, string saveFilePath, int quality = 80)
        {
            Bitmap bmp = GetBitmapFromStream(inputStream);
            return ToCompressAsJPG(bmp, saveFilePath, quality);
        }


        /// <summary>
        /// 生成缩略图（JPG 格式）
        /// </summary>
        /// <param name="inputStream">包含图片的Stream</param>
        /// <param name="saveFilePath">目标图片的存储地址</param>
        /// <param name="width">缩略图的宽</param>
        /// <param name="height">缩略图的高</param>
        public bool ToThumbAsJPG(Stream inputStream, string saveFilePath, int width, int height)
        {
            Bitmap bmp = GetBitmapFromStream(inputStream);
            return ToThumbAsJPG(bmp, saveFilePath, width, height);
        }


        public bool ToThumbAsJPG(Bitmap image, string saveFilePath, int width, int height)
        {

            if (image.Width == width && image.Height == height)
            {
                return ToCompressAsJPG(image, saveFilePath, 80);
            }
            int tWidth, tHeight, tLeft, tTop;
            double fScale = (double)height / (double)width;
            if (((double)image.Width * fScale) > (double)image.Height)
            {
                tWidth = width;
                tHeight = (int)((double)image.Height * (double)tWidth / (double)image.Width);
                tLeft = 0;
                tTop = (height - tHeight) / 2;
            }
            else
            {
                tHeight = height;
                tWidth = (int)((double)image.Width * (double)tHeight / (double)image.Height);
                tLeft = (width - tWidth) / 2;
                tTop = 0;
            }
            if (tLeft < 0) tLeft = 0;
            if (tTop < 0) tTop = 0;

            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            Graphics graphics = Graphics.FromImage(bitmap);

            //可以在这里设置填充背景颜色
            graphics.Clear(Color.White);
            graphics.DrawImage(image, new Rectangle(tLeft, tTop, tWidth, tHeight));
            image.Dispose();

            bool flag = false;
            try
            {
                flag = ToCompressAsJPG(bitmap, saveFilePath, 80);
            }
            catch
            {
                ;
            }
            finally
            {
                bitmap.Dispose();
                graphics.Dispose();
            }

            return flag;
        }

        /// <summary>
        /// 将Bitmap对象裁剪为指定JPG文件
        /// </summary>
        /// <param name="bmp">源bmp对象</param>
        /// <param name="saveFilePath">目标图片的存储地址</param>
        /// <param name="x">开始坐标x，单位：像素</param>
        /// <param name="y">开始坐标y，单位：像素</param>
        /// <param name="width">宽度：像素</param>
        /// <param name="height">高度：像素</param>
        public bool ToCutAsJPG(Bitmap bmp, string saveFilePath, int width, int height, int x = 0, int y = 0)
        {
            try
            {
                int bmpW = bmp.Width;
                int bmpH = bmp.Height;

                if (x >= bmpW || y >= bmpH)
                {
                    return ToCompressAsJPG(bmp, saveFilePath, 80);
                }

                if (x + width > bmpW)
                {
                    width = bmpW - x;
                }

                if (y + height > bmpH)
                {
                    height = bmpH - y;
                }


                Bitmap bmpOut = new Bitmap(width, height, PixelFormat.Format24bppRgb);
                Graphics g = Graphics.FromImage(bmpOut);
                g.DrawImage(bmp, new Rectangle(0, 0, width, height), new Rectangle(x, y, width, height), GraphicsUnit.Pixel);
                g.Dispose();
                bmp.Dispose();
                return ToCompressAsJPG(bmpOut, saveFilePath, 80);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 将Stream中的对象裁剪为指定JPG文件
        /// </summary>
        /// <param name="inputStream">源bmp对象</param>
        /// <param name="saveFilePath">目标图片的存储地址</param>
        /// <param name="x">开始坐标x，单位：像素</param>
        /// <param name="y">开始坐标y，单位：像素</param>
        /// <param name="width">宽度：像素</param>
        /// <param name="height">高度：像素</param>
        public bool ToCutAsJPG(Stream inputStream, string saveFilePath, int width, int height, int x = 0, int y = 0)
        {
            Bitmap bmp = GetBitmapFromStream(inputStream);
            bmp = ToShrink(bmp, width, height);

            return ToCutAsJPG(bmp, saveFilePath, width, height, x, y);
        }


        #region 图片水印操作

        /// <summary>
        /// 给图片添加图片水印
        /// </summary>
        /// <param name="inputStream">包含要源图片的流</param>
        /// <param name="watermarkPath">水印图片的物理地址</param>
        /// <param name="saveFilePath">目标图片的存储地址</param>
        /// <param name="mp">水印位置</param>
        public bool AddPicWatermarkAsJPG(Stream inputStream, string watermarkPath, string saveFilePath, MarkPosition mp)
        {

            Image image = Image.FromStream(inputStream);
            Bitmap b = new Bitmap(image.Width, image.Height, PixelFormat.Format24bppRgb);
            Graphics g = Graphics.FromImage(b);
            g.Clear(Color.White);
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.High;
            g.DrawImage(image, 0, 0, image.Width, image.Height);

            AddWatermarkImage(g, watermarkPath, mp, image.Width, image.Height);

            try
            {
                return ToCompressAsJPG(b, saveFilePath, 80);
            }
            catch {; }
            finally
            {
                b.Dispose();
                image.Dispose();
            }
            return false;
        }


        /// <summary>
        /// 给图片添加图片水印
        /// </summary>
        /// <param name="sourcePath">源图片的存储地址</param>
        /// <param name="watermarkPath">水印图片的物理地址</param>
        /// <param name="saveFilePath">目标图片的存储地址</param>
        /// <param name="mp">水印位置</param>
        public void AddPicWatermarkAsJPG(string sourcePath, string watermarkPath, string saveFilePath, MarkPosition mp)
        {
            if (File.Exists(sourcePath))
            {
                using (StreamReader sr = new StreamReader(sourcePath))
                {
                    AddPicWatermarkAsJPG(sr.BaseStream, watermarkPath, saveFilePath, mp);
                }
            }
        }

        /// <summary>
        /// 给图片添加文字水印
        /// </summary>
        /// <param name="inputStream">包含要源图片的流</param>
        /// <param name="text">水印文字</param>
        /// <param name="saveFilePath">目标图片的存储地址</param>
        /// <param name="mp">水印位置</param>
        public void AddTextWatermarkAsJPG(Stream inputStream, string text, string saveFilePath, MarkPosition mp)
        {

            Image image = Image.FromStream(inputStream);
            Bitmap b = new Bitmap(image.Width, image.Height, PixelFormat.Format24bppRgb);
            Graphics g = Graphics.FromImage(b);
            g.Clear(Color.White);
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.High;
            g.DrawImage(image, 0, 0, image.Width, image.Height);

            AddWatermarkText(g, text, mp, image.Width, image.Height);

            try
            {
                ToCompressAsJPG(b, saveFilePath, 80);
            }
            catch {; }
            finally
            {
                b.Dispose();
                image.Dispose();
            }
        }

        /// <summary>
        /// 给图片添加文字水印
        /// </summary>
        /// <param name="sourcePath">源图片的存储地址</param>
        /// <param name="text">水印文字</param>
        /// <param name="saveFilePath">目标图片的存储地址</param>
        /// <param name="mp">水印位置</param>
        public void AddTextWatermarkAsJPG(string sourcePath, string text, string saveFilePath, MarkPosition mp)
        {
            if (File.Exists(sourcePath))
            {
                using (StreamReader sr = new StreamReader(sourcePath))
                {
                    AddTextWatermarkAsJPG(sr.BaseStream, text, saveFilePath, mp);
                }
            }
        }

        /// <summary>
        /// 添加文字水印
        /// </summary>
        /// <param name="picture">要加水印的原图像</param>
        /// <param name="text">水印文字</param>
        /// <param name="mp">添加的位置</param>
        /// <param name="width">原图像的宽度</param>
        /// <param name="height">原图像的高度</param>
        private void AddWatermarkText(Graphics picture, string text, MarkPosition mp, int width, int height)
        {
            int[] sizes = new int[] { 16, 14, 12, 10, 8, 6, 4 };
            Font crFont = null;
            SizeF crSize = new SizeF();
            for (int i = 0; i < 7; i++)
            {
                crFont = new Font("Arial", sizes[i], FontStyle.Bold);
                crSize = picture.MeasureString(text, crFont);

                if ((ushort)crSize.Width < (ushort)width)
                    break;
            }

            float xpos = 0;
            float ypos = 0;

            switch (mp)
            {
                case MarkPosition.MP_Left_Top:
                    xpos = ((float)width * (float).01) + (crSize.Width / 2);
                    ypos = (float)height * (float).01;
                    break;
                case MarkPosition.MP_Right_Top:
                    xpos = ((float)width * (float).99) - (crSize.Width / 2);
                    ypos = (float)height * (float).01;
                    break;
                case MarkPosition.MP_Right_Bottom:
                    xpos = ((float)width * (float).99) - (crSize.Width / 2);
                    ypos = ((float)height * (float).99) - crSize.Height;
                    break;
                case MarkPosition.MP_Left_Bottom:
                    xpos = ((float)width * (float).01) + (crSize.Width / 2);
                    ypos = ((float)height * (float).99) - crSize.Height;
                    break;
            }

            StringFormat StrFormat = new StringFormat();
            StrFormat.Alignment = StringAlignment.Center;

            SolidBrush semiTransBrush2 = new SolidBrush(Color.FromArgb(153, 0, 0, 0));
            picture.DrawString(text, crFont, semiTransBrush2, xpos + 1, ypos + 1, StrFormat);

            SolidBrush semiTransBrush = new SolidBrush(Color.FromArgb(153, 255, 255, 255));
            picture.DrawString(text, crFont, semiTransBrush, xpos, ypos, StrFormat);

            semiTransBrush2.Dispose();
            semiTransBrush.Dispose();

        }

        /// <summary>
        /// 添加图片水印
        /// </summary>
        /// <param name="picture">要加水印的原图像</param>
        /// <param name="waterMarkPath">水印文件的物理地址</param>
        /// <param name="mp">添加的位置</param>
        /// <param name="width">原图像的宽度</param>
        /// <param name="height">原图像的高度</param>
        private void AddWatermarkImage(Graphics picture, string waterMarkPath, MarkPosition mp, int width, int height)
        {
            Image watermark = new Bitmap(waterMarkPath);

            ImageAttributes imageAttributes = new ImageAttributes();
            ColorMap colorMap = new ColorMap();

            colorMap.OldColor = Color.FromArgb(255, 0, 255, 0);
            colorMap.NewColor = Color.FromArgb(0, 0, 0, 0);
            ColorMap[] remapTable = { colorMap };

            imageAttributes.SetRemapTable(remapTable, ColorAdjustType.Bitmap);

            float[][] colorMatrixElements = {
                                              new float[] {1.0f,  0.0f,  0.0f,  0.0f, 0.0f},
                                              new float[] {0.0f,  1.0f,  0.0f,  0.0f, 0.0f},
                                              new float[] {0.0f,  0.0f,  1.0f,  0.0f, 0.0f},
                                              new float[] {0.0f,  0.0f,  0.0f,  0.3f, 0.0f},
                                              new float[] {0.0f,  0.0f,  0.0f,  0.0f, 1.0f}
                                          };

            ColorMatrix colorMatrix = new ColorMatrix(colorMatrixElements);

            imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            int xpos = 0;
            int ypos = 0;
            int WatermarkWidth = 0;
            int WatermarkHeight = 0;
            double bl = 1d;
            if ((width > watermark.Width * 4) && (height > watermark.Height * 4))
            {
                bl = 1;
            }
            else if ((width > watermark.Width * 4) && (height < watermark.Height * 4))
            {
                bl = Convert.ToDouble(height / 4) / Convert.ToDouble(watermark.Height);

            }
            else

                if ((width < watermark.Width * 4) && (height > watermark.Height * 4))
            {
                bl = Convert.ToDouble(width / 4) / Convert.ToDouble(watermark.Width);
            }
            else
            {
                if ((width * watermark.Height) > (height * watermark.Width))
                {
                    bl = Convert.ToDouble(height / 4) / Convert.ToDouble(watermark.Height);

                }
                else
                {
                    bl = Convert.ToDouble(width / 4) / Convert.ToDouble(watermark.Width);

                }

            }

            WatermarkWidth = Convert.ToInt32(watermark.Width * bl);
            WatermarkHeight = Convert.ToInt32(watermark.Height * bl);


            switch (mp)
            {
                case MarkPosition.MP_Left_Top:
                    xpos = 10;
                    ypos = 10;
                    break;
                case MarkPosition.MP_Right_Top:
                    xpos = width - WatermarkWidth - 10;
                    ypos = 10;
                    break;
                case MarkPosition.MP_Right_Bottom:
                    xpos = width - WatermarkWidth - 30;
                    ypos = height - WatermarkHeight - 20;
                    break;
                case MarkPosition.MP_Left_Bottom:
                    xpos = 10;
                    ypos = height - WatermarkHeight - 10;
                    break;
            }

            picture.DrawImage(watermark, new Rectangle(xpos, ypos, WatermarkWidth, WatermarkHeight), 0, 0, watermark.Width, watermark.Height, GraphicsUnit.Pixel, imageAttributes);


            watermark.Dispose();
            imageAttributes.Dispose();
        }

        /// <summary>
        /// 水印的位置
        /// </summary>
        public enum MarkPosition
        {
            /// <summary>
            /// 左上角
            /// </summary>
            MP_Left_Top,

            /// <summary>
            /// 左下角
            /// </summary>
            MP_Left_Bottom,

            /// <summary>
            /// 右上角
            /// </summary>
            MP_Right_Top,

            /// <summary>
            /// 右下角
            /// </summary>
            MP_Right_Bottom
        }


        #endregion


        #region 缩放等比图片

        /// <summary>
        /// 将Stream中的对象裁剪为指定JPG文件
        /// </summary>
        /// <param name="inputStream">源bmp对象</param>
        /// <param name="saveFilePath">目标图片的存储地址</param>
        /// <param name="width">宽度：像素 width=0 ,等高 </param>
        /// <param name="height">高度：像素  height=0，等宽</param>
        public bool ToShrinkAsJPG(Stream inputStream, string saveFilePath, int width = 0, int height = 0)
        {
            Bitmap bmp = GetBitmapFromStream(inputStream);


            float scale = 0;
            if (height == 0)
                scale = ((float)bmp.Width / (float)width);

            if (width == 0)
                scale = ((float)bmp.Height / (float)height);


            float currentWidth = bmp.Width / scale;
            float currentHeight = bmp.Height / scale;

            return ToThumbAsJPG(bmp, saveFilePath, (int)currentWidth, (int)currentHeight);
        }



        private Bitmap ToThumb(Bitmap image, int width, int height)
        {

            int tWidth, tHeight, tLeft, tTop;
            double fScale = (double)height / (double)width;
            if (((double)image.Width * fScale) > (double)image.Height)
            {
                tWidth = width;
                tHeight = (int)((double)image.Height * (double)tWidth / (double)image.Width);
                tLeft = 0;
                tTop = (height - tHeight) / 2;
            }
            else
            {
                tHeight = height;
                tWidth = (int)((double)image.Width * (double)tHeight / (double)image.Height);
                tLeft = (width - tWidth) / 2;
                tTop = 0;
            }
            if (tLeft < 0) tLeft = 0;
            if (tTop < 0) tTop = 0;

            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            Graphics graphics = Graphics.FromImage(bitmap);

            //可以在这里设置填充背景颜色
            graphics.Clear(Color.White);
            graphics.DrawImage(image, new Rectangle(tLeft, tTop, tWidth, tHeight));
            image.Dispose();

            //todo: maybe memory out

            return bitmap;
        }

        /// <summary>
        /// 缩放缩小
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        private Bitmap ToShrink(Bitmap bmp, int width = 0, int height = 0)
        {
            try
            {
                int maxSize = 0;
                // 先判断
                if (width > height)
                    maxSize = width;
                else
                    maxSize = height;


                float wScale = ((float)bmp.Width / (float)width);
                float hScale = ((float)bmp.Height / (float)height);


                float minScale = 0;
                if (wScale <= hScale)
                    minScale = wScale;
                else
                    minScale = hScale;


                float currentWidth = bmp.Width / minScale;
                float currentHeight = bmp.Height / minScale;

                return ToThumb(bmp, (int)currentWidth, (int)currentHeight);
                //bitmap.Dispose();

            }
            catch (Exception)
            {
                return null;
            }


        }
        /// <summary>
        /// 放大
        /// </summary>
        /// <returns></returns>
        private bool ToZoom(Bitmap bmp, int width, int height)
        {
            try
            {

            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        #endregion



        #region 图片缩放
        /// <summary>
        /// 图片缩放
        /// </summary>
        /// <param name="originalImagePath">源图路径（物理路径）</param>
        /// <param name="thumbnailPath">缩略图路径（物理路径）</param>
        /// <param name="width">缩略图宽度</param>
        /// <param name="height">缩略图高度</param>
        /// <param name="TypeMode">方式，W指定宽，高按比例，H指定高，宽按比例，Cut指定高宽裁减（不变形），HW指定高宽缩放（可能变形），Auto,</param>
        public static Stream GreateMiniImage(Stream originalImagePath, int width, int height, string TypeMode)
        {
            string mode = TypeMode;
            Image originalImage = Image.FromStream(originalImagePath);

            int towidth = width;
            int toheight = height;

            int x = 0;
            int y = 0;
            int ow = originalImage.Width;
            int oh = originalImage.Height;

            switch (mode)
            {
                case "Auto":
                    {
                        if (ow > oh)
                        {
                            toheight = originalImage.Height * width / originalImage.Width;
                        }
                        else
                        {
                            towidth = originalImage.Width * height / originalImage.Height;
                        }
                        break;
                    }
                case "HW"://指定高宽缩放（可能变形）  
                    {
                        break;
                    }
                case "W"://指定宽，高按比例  
                    {
                        toheight = originalImage.Height * width / originalImage.Width;
                        break;
                    }
                case "H"://指定高，宽按比例
                    {
                        towidth = originalImage.Width * height / originalImage.Height;
                        break;
                    }
                case "Cut"://指定高宽裁减（不变形）    
                    {
                        if ((double)originalImage.Width / (double)originalImage.Height > (double)towidth / (double)toheight)
                        {
                            oh = originalImage.Height;
                            ow = originalImage.Height * towidth / toheight;
                            y = 0;
                            x = (originalImage.Width - ow) / 2;
                        }
                        else
                        {
                            ow = originalImage.Width;
                            oh = originalImage.Width * height / towidth;
                            x = 0;
                            y = (originalImage.Height - oh) / 2;
                        }
                        break;
                    }
                default:
                    {
                        break;
                    }
            }


            //新建一个bmp图片
            System.Drawing.Image bitmap = new System.Drawing.Bitmap(towidth, toheight);

            //新建一个画板
            Graphics g = System.Drawing.Graphics.FromImage(bitmap);

            //设置高质量插值法
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;

            //设置高质量,低速度呈现平滑程度
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            //清空画布并以透明背景色填充
            g.Clear(Color.Transparent);

            //在指定位置并且按指定大小绘制原图片的指定部分
            g.DrawImage(originalImage, new Rectangle(0, 0, towidth, toheight),
                new Rectangle(x, y, ow, oh),
                GraphicsUnit.Pixel);
            try
            {
                MemoryStream imageStream = new MemoryStream();
                //将图片的实例保存到流中   
                bitmap.Save(imageStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                imageStream.Position = 0;

                //Stream stream = ImageToByteArray(bitmap);
                //Image images = Image.FromStream(stream);
                //images.Save("D:/Workspace/Src/Web/CCT.Web.Pay.Site/Images/77.jpg", ImageFormat.Jpeg);
                //bitmap.Save(stream, ImageFormat.Jpeg);
                //stream.Close();
                return imageStream;

            }
            catch (System.Exception e)
            {
                throw e;
            }
            finally
            {
                originalImage.Dispose();
                bitmap.Dispose();
                g.Dispose();
            }
        }
        #endregion

    }

}
