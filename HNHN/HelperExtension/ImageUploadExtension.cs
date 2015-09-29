using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;

namespace FeedFormulation.HelperExtension
{
    public class ImageUploadExtension
    {
        // folder for the upload
        public static readonly string ItemUploadFolderPath = "~/Content/img/upload/";
        public static readonly string AvatarUploadFolderPath = "~/Content/img/avatar/";
        public static readonly string IconUploadFolderPath = "~/Content/img/icon/";
        public static readonly string ItemUploadSmallFolderPath = "~/Content/img/upload/small/";
        public static List<string> renameUploadFile(HttpPostedFileBase file, string mode, Int32 counter = 0)
        {
            var fileName = Path.GetFileName(file.FileName);

            string append = "image_";
            string finalFileName = append + ((counter).ToString()) + "_" + fileName;
            if (mode == "Resize")
            {
                if (System.IO.File.Exists
            (HttpContext.Current.Request.MapPath(ItemUploadFolderPath + finalFileName)))
                {
                    //file exists 
                    return renameUploadFile(file, mode, ++counter);
                }
            }
            if(mode == "Avatar")
            {
                if (System.IO.File.Exists
            (HttpContext.Current.Request.MapPath(AvatarUploadFolderPath + finalFileName)))
                {
                    //file exists 
                    return renameUploadFile(file, mode, ++counter);
                }
            }
            //file doesn't exist, upload item but validate first
            if (uploadFile(file, finalFileName, mode))
            {
                List<string> imagePaths = new List<string>();
                string imagePath = ItemUploadFolderPath + finalFileName;
                if (mode == "Avatar")
                {
                    imagePath = AvatarUploadFolderPath + finalFileName;
                }
                imagePaths.Add(imagePath);
                if (mode == "Resize")
                {
                    string imageSmallPath = ItemUploadSmallFolderPath + finalFileName;
                    imagePaths.Add(imageSmallPath);
                }
                return imagePaths;
            }
            else
            {
                return null;
            }
        }

        private static bool uploadFile(HttpPostedFileBase file, string fileName, string mode)
        {
            var path =
          Path.Combine(HttpContext.Current.Request.MapPath(ItemUploadFolderPath), fileName);
            if (mode == "Avatar")
            {
                path =
          Path.Combine(HttpContext.Current.Request.MapPath(AvatarUploadFolderPath), fileName);
            }
            string extension = Path.GetExtension(file.FileName);
            //make sure the file is valid
            if (!validateExtension(extension))
            {
                return false;
            }
            try
            {
                file.SaveAs(path);

                Image imgOriginal = Image.FromFile(path);
                //pass max size to resize
                int size = 1000;
                if (imgOriginal.Width <= size && imgOriginal.Height <= size && file.ContentLength < 148576)
                {
                    Image imgActual = ScaleBySize(imgOriginal, size, mode);
                    if (mode == "Resize")
                    {
                        var smallPath = Path.Combine(HttpContext.Current.Request.MapPath(ItemUploadSmallFolderPath), fileName);
                        Image imgSmall = Crop(imgOriginal, 65, 65, "Center", "Center");
                        imgSmall.Save(smallPath, ImageFormat.Jpeg);
                        imgSmall.Dispose();
                    }
                    imgOriginal.Dispose();
                    imgActual.Save(path, ImageFormat.Jpeg);
                    imgActual.Dispose();
                }
                else
                {
                    if (mode == "Resize")
                    {
                        var smallPath = Path.Combine(HttpContext.Current.Request.MapPath(ItemUploadSmallFolderPath), fileName);
                        Image imgSmall = Crop(imgOriginal, 65, 65, "Center", "Center");
                        imgSmall.Save(smallPath, ImageFormat.Jpeg);
                        imgSmall.Dispose();
                    }
                    imgOriginal.Dispose();
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool validateExtension(string extension)
        {
            extension = extension.ToLower();
            switch (extension)
            {
                case ".jpg":
                    return true;
                case ".png":
                    return true;
                case ".gif":
                    return true;
                case ".jpeg":
                    return true;
                default:
                    return false;
            }
        }


        public static Image ScaleBySize(Image imgPhoto, int size, string mode)
        {
            float sourceWidth = imgPhoto.Width;
            float sourceHeight = imgPhoto.Height;
            float destHeight = 0;
            float destWidth = 0;
            int sourceX = 0;
            int sourceY = 0;
            int destX = 0;
            int destY = 0;

            if (mode.Equals("Resize"))
            {
                // To preserve the aspect ratio
                float ratioX = (float)size / (float)sourceWidth;
                float ratioY = (float)size / (float)sourceHeight;
                float ratio = Math.Min(ratioX, ratioY);

                // New width and height based on aspect ratio
                destWidth = (int)(sourceWidth * ratio);
                destHeight = (int)(sourceHeight * ratio);
            }
            else
            {
                size = 200;
                // Resize Image to have the height = width
                destWidth = size;
                destHeight = size;
            }

            Bitmap bmPhoto = new Bitmap((int)destWidth, (int)destHeight, PixelFormat.Format24bppRgb);
            bmPhoto.SetResolution(imgPhoto.HorizontalResolution, imgPhoto.VerticalResolution);

            Graphics grPhoto = Graphics.FromImage(bmPhoto);
            grPhoto.Clear(Color.White);
            grPhoto.SmoothingMode = SmoothingMode.AntiAlias;
            grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;

            grPhoto.DrawImage(imgPhoto,
                new Rectangle(destX, destY, (int)destWidth, (int)destHeight),
                new Rectangle(sourceX, sourceY, (int)sourceWidth, (int)sourceHeight),
                GraphicsUnit.Pixel);

            grPhoto.Dispose();

            return bmPhoto;
        }

        public static bool removeUploadedFile(string imgPath)
        {
            string realPath = imgPath;
            if (System.IO.File.Exists
        (HttpContext.Current.Request.MapPath(realPath)))
            {
                //file exists
                FileInfo file = new FileInfo(HttpContext.Current.Request.MapPath(realPath));
                file.Delete();
                return true;
            }
            return false;
        }

        public static Image Crop(Image imgPhoto, int Width, int Height, string vertical, string horizontal)
        {
            int sourceWidth = imgPhoto.Width;
            int sourceHeight = imgPhoto.Height;
            int sourceX = 0;
            int sourceY = 0;
            int destX = 0;
            int destY = 0;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;

            nPercentW = ((float)Width / (float)sourceWidth);
            nPercentH = ((float)Height / (float)sourceHeight);

            if (nPercentH < nPercentW)
            {
                nPercent = nPercentW;
                switch (vertical)
                {
                    case "Top":
                        destY = 0;
                        break;
                    case "Bottom":
                        destY = (int)(Height - (sourceHeight * nPercent));
                        break;
                    default:
                        destY = (int)((Height - (sourceHeight * nPercent)) / 2);
                        break;
                }
            }
            else
            {
                nPercent = nPercentH;
                switch (horizontal)
                {
                    case "Left":
                        destX = 0;
                        break;
                    case "Right":
                        destX = (int)(Width - (sourceWidth * nPercent));
                        break;
                    default:
                        destX = (int)((Width - (sourceWidth * nPercent)) / 2);
                        break;
                }
            }

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            Bitmap bmPhoto = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);
            bmPhoto.SetResolution(imgPhoto.HorizontalResolution, imgPhoto.VerticalResolution);

            Graphics grPhoto = Graphics.FromImage(bmPhoto);
            grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;

            grPhoto.DrawImage(imgPhoto,
                new Rectangle(destX, destY, destWidth, destHeight),
                new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
                GraphicsUnit.Pixel);

            grPhoto.Dispose();
            return bmPhoto;
        }

        public static string renameUploadIcon(HttpPostedFileBase file, Int32 counter = 0)
        {
            var fileName = Path.GetFileName(file.FileName);

            string append = "icon_";
            string finalFileName = append + ((counter).ToString()) + "_" + fileName;
            if (System.IO.File.Exists
        (HttpContext.Current.Request.MapPath(IconUploadFolderPath + finalFileName)))
            {
                //file exists 
                return renameUploadIcon(file, ++counter);
            }
            var path = Path.Combine(HttpContext.Current.Request.MapPath(IconUploadFolderPath), finalFileName);
            file.SaveAs(path);
            return IconUploadFolderPath + finalFileName;
        }
    }
}