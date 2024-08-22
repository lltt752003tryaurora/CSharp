using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Net;

namespace Utilities
{
    public class ImageHelper
    {
        public static string StringToBase64Image(string inputString)
        {
            Image i = ConvertTextToImage(inputString, "Bookman Old Style", 32, Color.White, Color.Black, 200, 50);
            return ImageToBase64(i, ImageFormat.Png);
        }
        public static Bitmap ConvertTextToImage(string txt, string fontname, int fontsize, Color bgcolor, Color fcolor, int width, int Height)
        {
            Bitmap bmp = new Bitmap(width, Height);
            using (Graphics graphics = Graphics.FromImage(bmp))
            {

                Font font = new Font(fontname, fontsize);
                graphics.FillRectangle(new SolidBrush(bgcolor), 0, 0, bmp.Width, bmp.Height);
                graphics.DrawString(txt, font, new SolidBrush(fcolor), 0, 0);
                graphics.Flush();
                font.Dispose();
                graphics.Dispose();

            }
            return bmp;
        }
        public static string ImageToBase64(Image image, System.Drawing.Imaging.ImageFormat format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // Convert Image to byte[]
                image.Save(ms, format);
                byte[] imageBytes = ms.ToArray();

                // Convert byte[] to Base64 String
                string base64String = Convert.ToBase64String(imageBytes);
                return base64String;
            }
        }
        public static byte[] ImageToByteArray(System.Drawing.Image imageIn, System.Drawing.Imaging.ImageFormat imageFormat)
        {
            byte[] rs = null;
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, imageFormat);
            rs = ms.ToArray();
            imageIn.Dispose();
            ms.Position = 0; // Not actually needed, SetLength(0) will reset the Position anyway
            ms.SetLength(0);
            ms.Dispose();
            return rs;
        }
        public static void Resize(string fileName, int width, int height, string thumbPrefix = "")
        {
            FileInfo f = new FileInfo(fileName);
            var fileBase = Path.GetFileNameWithoutExtension(fileName);
            var ext = Path.GetExtension(fileName);
            var folder = Path.GetDirectoryName(fileName);
            string thumpFile = Path.Combine(folder, thumbPrefix + fileBase + ext);
            if (f.Length <= 0)
            {
                return;
            }
            Bitmap bmp = new Bitmap(fileName);

            if (bmp.Width <= width && bmp.Height <= height)
            {
                if (!string.IsNullOrEmpty(thumbPrefix))
                {
                    bmp.Save(thumpFile);
                }
                return;
            }

            if (bmp.Width < width)
            {
                width = bmp.Width;
            }
            double aspectRatio = (double)bmp.Width / (double)bmp.Height;
            int newHeight = (int)Math.Round(width / aspectRatio);

            //if (newHeight < 700)
            //{
            //    height = newHeight;
            //}
            Size newSize = new Size(width, newHeight);

            using (Bitmap thumb = new Bitmap((System.Drawing.Image)bmp, newSize))
            {
                System.Drawing.Imaging.ImageCodecInfo codec = System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders()[1];
                System.Drawing.Imaging.EncoderParameters eParams = new System.Drawing.Imaging.EncoderParameters(1);
                eParams.Param[0] = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 100L);
                using (Graphics g = Graphics.FromImage(thumb)) // Create Graphics object from original Image
                {
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                    g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;


                    g.DrawImage(bmp, new Rectangle(0, 0, thumb.Width, thumb.Height));
                    //Set Image codec of JPEG type, the index of JPEG codec is "1"

                    if (newHeight > height)
                    {
                        Bitmap thumb1;
                        if ((float)newHeight / height > 1.2)
                        {
                            thumb1 = thumb.Clone(new Rectangle(0, newHeight * 5 / 100, thumb.Width, height), System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                        }
                        else
                        {
                            thumb1 = thumb.Clone(new Rectangle(0, 0, thumb.Width, height), System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                        }
                        bmp.Dispose();
                        thumb.Dispose();
                        thumb1.Save(thumpFile, codec, eParams);
                    }
                    else
                    {
                        Bitmap m = thumb.Clone(new Rectangle(0, 0, thumb.Width, thumb.Height), System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                        thumb.Dispose();
                        bmp.Dispose();
                        m.Save(thumpFile);
                    }
                    //g.Dispose();
                }

            }

        }

        //Scale the image to a percentage of its actual size.

        public static Image ScaleByPercentage(Image img, double percent)

        {

            double fractionalPercentage = (percent / 100.0);

            int outputWidth = (int)(img.Width * fractionalPercentage);

            int outputHeight = (int)(img.Height * fractionalPercentage);

            return ImageHelper.ScaleImage(img, outputWidth, outputHeight);

        }

        //Scale down the image till it fits the given size.

        public static Image ScaleDownTillFits(Image img, Size size)

        {

            Image ret = img;

            bool bFound = false;

            if ((img.Width > size.Width) || (img.Height > size.Height))

            {

                for (double percent = 100; percent > 0; percent--)

                {

                    double fractionalPercentage = (percent / 100.0);

                    int outputWidth = (int)(img.Width * fractionalPercentage);

                    int outputHeight = (int)(img.Height * fractionalPercentage);

                    if ((outputWidth < size.Width) && (outputHeight < size.Height))

                    {

                        bFound = true;

                        ret = ImageHelper.ScaleImage(img, outputWidth, outputHeight);

                        break;

                    }

                }

                if (!bFound)

                {

                    ret = ImageHelper.ScaleImage(img, size.Width, size.Height);

                }

            }

            return ret;

        }

        //Scale an image by a set width. The height will be set proportionally.

        public static Image ScaleByWidth(Image img, int width)

        {

            double fractionalPercentage = ((double)width / (double)img.Width);

            int outputWidth = width;

            int outputHeight = (int)(img.Height * fractionalPercentage);

            return ImageHelper.ScaleImage(img, outputWidth, outputHeight);

        }

        //Scale an image by a set height. The width will be set proportionally.

        public static Image ScaleByHeight(Image img, int height)

        {

            double fractionalPercentage = ((double)height / (double)img.Height);

            int outputWidth = (int)(img.Width * fractionalPercentage);

            int outputHeight = height;

            return ImageHelper.ScaleImage(img, outputWidth, outputHeight);

        }

        //Scale an image to a given width and height.

        public static Image ScaleImage(Image img, int outputWidth, int outputHeight)

        {

            Bitmap outputImage = new Bitmap(outputWidth, outputHeight, img.PixelFormat);

            outputImage.SetResolution(img.HorizontalResolution, img.VerticalResolution);

            Graphics graphics = Graphics.FromImage(outputImage);

            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

            graphics.DrawImage(img, new Rectangle(0, 0, outputWidth, outputHeight),

            new Rectangle(0, 0, img.Width, img.Height), GraphicsUnit.Pixel);

            graphics.Dispose();

            return outputImage;

        }

        public static Image CropImage(Image img, System.Drawing.Rectangle cropArea)
        {
            Bitmap bmpImage = new Bitmap(img);
            return bmpImage.Clone(cropArea, bmpImage.PixelFormat);
        }
    }
}
