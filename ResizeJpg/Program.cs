using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.Processing;

namespace ResizeJpg
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //壓縮目標路徑 務必要設定
            string inputPath = "GG:\\";
            //最長邊
            int maxDimension = 1200;
            //畫面品質
            int jpegQuality = 75;
            //DPI
            uint dpi = 72; // 將 DPI 設置為 72

            ResizeImagesInDirectory(inputPath, maxDimension, jpegQuality, dpi);
        }

        /// <summary>
        /// 撈出圖片
        /// </summary>
        /// <param name="directory">資料夾</param>
        /// <param name="maxDimension">最長邊</param>
        /// <param name="jpegQuality">圖片品質</param>
        /// <param name="dpi">dpi</param>
        static void ResizeImagesInDirectory(string directory, int maxDimension, int jpegQuality, uint dpi)
        {
            var files = Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories);
            Parallel.ForEach(files, file =>
            {
                if (file.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || file.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
                {
                    ResizeImage(file, maxDimension, jpegQuality, dpi);
                }
            });
        }

        /// <summary>
        /// 重新壓縮圖片大小 不改變長寬
        /// </summary>
        /// <param name="filePath">圖片路徑</param>
        /// <param name="jpegQuality">品質</param>
        /// <param name="dpi">dpi</param>
        static void CompressImage(string filePath, int jpegQuality, uint dpi)
        {
            using (var image = Image.Load(filePath))
            {
                // 設置 DPI
                image.Metadata.HorizontalResolution = dpi;
                image.Metadata.VerticalResolution = dpi;

                // 設置 Exif profile 中的 XResolution 和 YResolution
                var exifProfile = image.Metadata.ExifProfile ?? new ExifProfile();
                exifProfile.SetValue(ExifTag.XResolution, new Rational(dpi, 1));
                exifProfile.SetValue(ExifTag.YResolution, new Rational(dpi, 1));
                image.Metadata.ExifProfile = exifProfile;

                var jpegEncoder = new JpegEncoder
                {
                    Quality = jpegQuality
                };

                image.Save(filePath, jpegEncoder);
                Console.WriteLine($"壓縮 {filePath} with quality {jpegQuality} and dpi {dpi}.");
            }
        }

        /// <summary>
        /// 重設圖片大小
        /// </summary>
        /// <param name="filePath">圖片路徑</param>
        /// <param name="maxDimension">最長邊</param>
        /// <param name="jpegQuality">品質</param>
        /// <param name="dpi">dpi</param>
        static void ResizeImage(string filePath, int maxDimension, int jpegQuality, uint dpi)
        {
            using (var image = Image.Load(filePath))
            {
                int width = image.Width;
                int height = image.Height;

                // 檢查如果檔案長寬都小於設定的最大值，則略過
                if (width < maxDimension && height < maxDimension)
                {
                    //如果圖片解析度比較小就不改變長寬，但要降低jpg品質
                    CompressImage(filePath, jpegQuality, dpi);
                    //Console.WriteLine($"略過 {filePath}: 解析度為 {width}x{height} 小於要縮小的");
                    return;
                }

                float scale = Math.Min((float)maxDimension / width, (float)maxDimension / height);

                int newWidth = (int)(width * scale);
                int newHeight = (int)(height * scale);

                image.Mutate(x => x.Resize(newWidth, newHeight));

                // 設置 DPI
                image.Metadata.HorizontalResolution = dpi;
                image.Metadata.VerticalResolution = dpi;

                // 設置 Exif profile 中的 XResolution 和 YResolution
                var exifProfile = image.Metadata.ExifProfile ?? new ExifProfile();
                exifProfile.SetValue(ExifTag.XResolution, new Rational(dpi, 1));
                exifProfile.SetValue(ExifTag.YResolution, new Rational(dpi, 1));
                image.Metadata.ExifProfile = exifProfile;

                var jpegEncoder = new JpegEncoder
                {
                    Quality = jpegQuality
                };

                image.Save(filePath, jpegEncoder);
                Console.WriteLine($"調整大小 {filePath}: new dimensions {newWidth}x{newHeight}, dpi {dpi}.");
            }
        }
    }
}
