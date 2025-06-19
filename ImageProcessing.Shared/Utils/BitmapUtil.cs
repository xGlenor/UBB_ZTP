using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using ImageProcessing.Shared.Models;

namespace ImageProcessing.Shared.Utils;

public static class BitmapUtil
{
    public static byte[] BitmapToByteArray(Bitmap bitmap)
    {
        var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
            ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

        var imageBytes = new byte[Math.Abs(bitmapData.Stride) * bitmap.Height];
        Marshal.Copy(bitmapData.Scan0, imageBytes, 0, imageBytes.Length);
        bitmap.UnlockBits(bitmapData);

        return imageBytes;
    }

    public static void SaveProcessedImage(byte[] processedImage, string filename, int width, int height)
    {

        try
        {
            var bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            Marshal.Copy(processedImage, 0, bitmapData.Scan0, processedImage.Length);
            bitmap.UnlockBits(bitmapData);
            
            var directory = Path.GetDirectoryName(filename);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory)) Directory.CreateDirectory(directory);

            bitmap.Save(filename, ImageFormat.Png);
            bitmap.Dispose();

            Console.WriteLine($"  Saved: {filename}");
        }catch (Exception ex)
        {
            Console.WriteLine($"Error saving {filename}: {ex.Message}");
        }
    }
    
    public static string GenerateOutputPath(string inputPath, string id)
    {
        var directory = Path.GetDirectoryName(inputPath) ?? "";
        var fileNameWithoutExt = Path.GetFileNameWithoutExtension(inputPath);
        return Path.Combine(directory, $"{fileNameWithoutExt}_{id}.png");
    }
    public static string? GetImagePath()
    {
        while (true)
        {
            Console.Write("Enter path to image file: ");
            var path = Console.ReadLine()?.Trim('"');

            if (string.IsNullOrEmpty(path))
            {
                Console.WriteLine("Path cannot be empty!");
                continue;
            }

            if (!File.Exists(path))
            {
                Console.WriteLine("File does not exist!");
                continue;
            }

            var extension = Path.GetExtension(path).ToLower();
            if (!IsImageFile(extension))
            {
                Console.WriteLine("Unsupported file format! Supported: .bmp, .jpg, .jpeg, .png, .gif, .tiff");
                continue;
            }

            return path;
        }
    }
    
    private static bool IsImageFile(string extension)
    {
        var supportedExtensions = new[] { ".bmp", ".jpg", ".jpeg", ".png", ".gif", ".tiff", ".tif" };
        return supportedExtensions.Contains(extension);
    }
    
}