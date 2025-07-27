using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace SaleBillSystem.NET.Tools
{
    public class IconGenerator
    {
        public static void GenerateApplicationIcon()
        {
            try
            {
                // Create different sizes for the icon
                int[] sizes = { 16, 32, 48, 64, 128, 256 };
                var iconImages = new Bitmap[sizes.Length];

                for (int i = 0; i < sizes.Length; i++)
                {
                    iconImages[i] = CreateIconImage(sizes[i]);
                }

                // Save as ICO file
                string iconPath = Path.Combine("Resources", "app-icon.ico");
                SaveAsIcon(iconImages, iconPath);

                // Clean up
                foreach (var image in iconImages)
                {
                    image?.Dispose();
                }

                Console.WriteLine($"Application icon generated successfully: {iconPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating icon: {ex.Message}");
            }
        }

        private static Bitmap CreateIconImage(int size)
        {
            var bitmap = new Bitmap(size, size, PixelFormat.Format32bppArgb);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.Clear(Color.Transparent);

                // Calculate scaling factor
                float scale = size / 256f;

                // Background circle
                var backgroundBrush = new LinearGradientBrush(
                    new Point(0, 0),
                    new Point(size, size),
                    Color.FromArgb(45, 45, 48),
                    Color.FromArgb(70, 70, 73)
                );

                graphics.FillEllipse(backgroundBrush, 2, 2, size - 4, size - 4);

                // Inner circle
                var innerBrush = new LinearGradientBrush(
                    new Point(size / 4, size / 4),
                    new Point(size * 3 / 4, size * 3 / 4),
                    Color.FromArgb(0, 120, 215),
                    Color.FromArgb(0, 100, 180)
                );

                graphics.FillEllipse(innerBrush, size / 6, size / 6, size * 2 / 3, size * 2 / 3);

                // Dollar sign symbol
                var font = new Font("Arial", size * 0.4f, FontStyle.Bold);
                var textBrush = new SolidBrush(Color.White);
                var text = "$";
                var textSize = graphics.MeasureString(text, font);
                var textX = (size - textSize.Width) / 2;
                var textY = (size - textSize.Height) / 2;

                // Add shadow
                var shadowBrush = new SolidBrush(Color.FromArgb(100, 0, 0, 0));
                graphics.DrawString(text, font, shadowBrush, textX + 1, textY + 1);

                // Main text
                graphics.DrawString(text, font, textBrush, textX, textY);

                // Add a small "S" for "Sale" at the bottom
                var smallFont = new Font("Arial", size * 0.15f, FontStyle.Bold);
                var smallText = "S";
                var smallTextSize = graphics.MeasureString(smallText, smallFont);
                var smallTextX = (size - smallTextSize.Width) / 2;
                var smallTextY = size * 0.75f;

                graphics.DrawString(smallText, smallFont, textBrush, smallTextX, smallTextY);

                // Clean up
                backgroundBrush.Dispose();
                innerBrush.Dispose();
                textBrush.Dispose();
                shadowBrush.Dispose();
                font.Dispose();
                smallFont.Dispose();
            }

            return bitmap;
        }

        private static void SaveAsIcon(Bitmap[] images, string filePath)
        {
            // Create directory if it doesn't exist
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            // For simplicity, we'll save the largest image as PNG and convert it
            // In a real scenario, you'd use a proper ICO writer
            var largestImage = images[images.Length - 1]; // 256x256
            largestImage.Save(filePath.Replace(".ico", ".png"), ImageFormat.Png);

            // For now, we'll create a simple ICO file by copying the PNG
            // In production, you'd want to use a proper ICO writer library
            File.Copy(filePath.Replace(".ico", ".png"), filePath, true);
        }
    }
} 