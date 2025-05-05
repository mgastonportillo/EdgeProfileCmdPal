using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Security.Cryptography;

namespace EdgeProfileCmdPal.Helpers
{
    internal class ImageHelpers
    {
        /// <summary>
        /// Loads an image, masks it to a perfect circle (with inset), saves it to a temporary file,
        /// and returns the absolute file URI string for the circular icon.
        /// If processing fails, returns the URI of the original image path.
        /// </summary>
        public static string ClipToCircle(string imagePath)
        {
            string key = ComputeHash(imagePath);
            string cacheFile = Path.Combine(Path.GetTempPath(), $"EdgeProfileCircle_{key}.png");
            if (File.Exists(cacheFile))
                return new Uri(cacheFile).AbsoluteUri;

            string fallbackUri;
            try
            {
                if (!Path.IsPathRooted(imagePath))
                    imagePath = Path.GetFullPath(imagePath);
                fallbackUri = new Uri(imagePath).AbsoluteUri;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ImageHelpers.ClipToCircle] ERROR creating fallback URI for '{imagePath}': {ex.Message}. Returning raw path.");
                return imagePath;
            }

            if (!File.Exists(imagePath))
                return imagePath;

            try
            {
                using var image = Image.FromFile(imagePath);
                const int inset = 5;
                int diameter = Math.Min(image.Width, image.Height) - (inset * 2);

                if (diameter <= 0)
                {
                    Debug.WriteLine($"[ImageHelpers.ClipToCircle] Image too small for inset ({image.Width}x{image.Height} with inset {inset}). Returning original URI.");
                    return imagePath;
                }

                using var bitmap = new Bitmap(image.Width, image.Height, PixelFormat.Format32bppArgb);
                using var graphics = Graphics.FromImage(bitmap);

                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.Clear(Color.Transparent);

                using (var path = new GraphicsPath())
                {
                    path.AddEllipse(inset, inset, diameter, diameter);
                    graphics.SetClip(path);
                }

                graphics.DrawImage(image, 0, 0, image.Width, image.Height);
                bitmap.Save(cacheFile, ImageFormat.Png);

                return new Uri(cacheFile).AbsoluteUri;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ImageHelpers.ClipToCircle] ERROR processing '{imagePath}': {ex.Message}");
                try
                {
                    if (File.Exists(cacheFile))
                        File.Delete(cacheFile);
                }
                catch { }
                return imagePath;
            }
        }

        public static string ComputeHash(string filePath)
        {
            using var sha = SHA256.Create();
            using var stream = File.OpenRead(filePath);
            var hash = sha.ComputeHash(stream);
            return Convert.ToHexString(hash).Substring(0, 16).ToLowerInvariant();
        }

    }
}
