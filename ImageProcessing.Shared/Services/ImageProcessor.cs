using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using ImageProcessing.Shared.Utils;

namespace ImageProcessing.Shared.Services;

public class ImageProcessor
{
    public static byte[] ApplyGrayscale(byte[] imageBytes)
    {
        for (var i = 0; i < imageBytes.Length; i += 3)
        {
            var blue = imageBytes[i];
            var green = imageBytes[i + 1];
            var red = imageBytes[i + 2];
            
            var gray = (byte)(0.299 * red + 0.587 * green + 0.114 * blue);

            imageBytes[i] = gray; 
            imageBytes[i + 1] = gray; 
            imageBytes[i + 2] = gray; 
        }

        return imageBytes;
    }

    public static unsafe byte[] ApplyGrayscaleUnsafe(byte[] pixels)
    {
        fixed (byte* ptr = pixels)
        {
            byte* p = ptr;
            byte* end = ptr + pixels.Length;

            while (p < end)
            {
                int b = p[0];
                int g = p[1];
                int r = p[2];

                int gray = (r * 299 + g * 587 + b * 114 + 500) / 1000;
                byte g8 = (byte)gray;

                p[0] = p[1] = p[2] = g8;
                p += 3;
            }
        }
        return pixels;
    }
    public static byte[] ApplyGrayscaleInPlace(byte[] pixels)
    {
        int len = pixels.Length;

        for (int i = 0; i < len; i += 3)
        {
            int b = pixels[i];
            int g = pixels[i + 1];
            int r = pixels[i + 2];
            
            int gray = (r * 299 + g * 587 + b * 114 + 500) / 1000;

            byte g8 = (byte)gray;
            pixels[i] = pixels[i + 1] = pixels[i + 2] = g8;
        }
        
        return pixels;
    }
    public static byte[] ApplyBlur(Bitmap image, int blurSize)
    {
        var rect = new Rectangle(0, 0, image.Width, image.Height);
        var bmpData = image.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
        int bytes = Math.Abs(bmpData.Stride) * bmpData.Height;
        byte[] buffer = new byte[bytes];
        Marshal.Copy(bmpData.Scan0, buffer, 0, bytes);
        image.UnlockBits(bmpData);

        int w = image.Width;
        int h = image.Height;
        int stride = bmpData.Stride;
        int bpp = 3;

        byte[] output = new byte[bytes];

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                int avgB = 0, avgG = 0, avgR = 0, count = 0;

                for (int yy = y; yy < y + blurSize && yy < h; yy++)
                {
                    for (int xx = x; xx < x + blurSize && xx < w; xx++)
                    {
                        int idx = yy * stride + xx * bpp;
                        avgB += buffer[idx];
                        avgG += buffer[idx + 1];
                        avgR += buffer[idx + 2];
                        count++;
                    }
                }

                int outIdx = y * stride + x * bpp;
                output[outIdx]     = (byte)(avgB / count);
                output[outIdx + 1] = (byte)(avgG / count);
                output[outIdx + 2] = (byte)(avgR / count);
            }
        }

        return output;
    }

    public static string BitmapToBase64(Bitmap bitmap)
    {
        using (var ms = new MemoryStream())
        {
            bitmap.Save(ms, ImageFormat.Png);
            return Convert.ToBase64String(ms.ToArray());
        }
    }

    public static Bitmap Base64ToBitmap(string base64String)
    {
        var imageBytes = Convert.FromBase64String(base64String);
        using (var ms = new MemoryStream(imageBytes))
        {
            return new Bitmap(ms);
        }
    }
}