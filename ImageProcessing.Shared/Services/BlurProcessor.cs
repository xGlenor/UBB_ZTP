using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ImageProcessing.Shared.Services;

public static class BlurProcessor
{

    public static Bitmap ApplyBlur(Bitmap source, int radius)
    {
        if (radius <= 0) return new Bitmap(source);
        
        var result = new Bitmap(source.Width, source.Height, source.PixelFormat);
        
        var sourceData = source.LockBits(new Rectangle(0, 0, source.Width, source.Height),
            ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        var resultData = result.LockBits(new Rectangle(0, 0, result.Width, result.Height),
            ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

        try
        {
            unsafe
            {
                byte* srcPtr = (byte*)sourceData.Scan0;
                byte* dstPtr = (byte*)resultData.Scan0;
                int stride = sourceData.Stride;
                int width = source.Width;
                int height = source.Height;
                
                IntPtr tempBufferPtr = Marshal.AllocHGlobal(stride * height);
                try
                {
                    byte* tempPtr = (byte*)tempBufferPtr;

                    Parallel.For(0, height, y =>
                    {
                        BoxBlurHorizontal(srcPtr, tempPtr, y, width, height, stride, radius);
                    });
                    
                    Parallel.For(0, width, x =>
                    {
                        BoxBlurVertical(tempPtr, dstPtr, x, width, height, stride, radius);
                    });
                }
                finally
                {
                    Marshal.FreeHGlobal(tempBufferPtr);
                }
            }
        }
        finally
        {
            source.UnlockBits(sourceData);
            result.UnlockBits(resultData);
        }

        return result;
    }

    private static unsafe void BoxBlurHorizontal(byte* src, byte* dst, int y, int width, int height, int stride, int radius)
    {
        byte* srcRow = src + y * stride;
        byte* dstRow = dst + y * stride;

        int diameter = radius * 2 + 1;

        for (int x = 0; x < width; x++)
        {
            int sumR = 0, sumG = 0, sumB = 0, sumA = 0;
            int count = 0;

            for (int kx = -radius; kx <= radius; kx++)
            {
                int px = Math.Max(0, Math.Min(width - 1, x + kx));
                byte* pixel = srcRow + px * 4;

                sumB += pixel[0];
                sumG += pixel[1];
                sumR += pixel[2];
                sumA += pixel[3];
                count++;
            }

            byte* dstPixel = dstRow + x * 4;
            dstPixel[0] = (byte)(sumB / count);
            dstPixel[1] = (byte)(sumG / count);
            dstPixel[2] = (byte)(sumR / count);
            dstPixel[3] = (byte)(sumA / count);
        }
    }

    private static unsafe void BoxBlurVertical(byte* src, byte* dst, int x, int width, int height, int stride, int radius)
    {
        for (int y = 0; y < height; y++)
        {
            int sumR = 0, sumG = 0, sumB = 0, sumA = 0;
            int count = 0;

            for (int ky = -radius; ky <= radius; ky++)
            {
                int py = Math.Max(0, Math.Min(height - 1, y + ky));
                byte* pixel = src + py * stride + x * 4;

                sumB += pixel[0];
                sumG += pixel[1];
                sumR += pixel[2];
                sumA += pixel[3];
                count++;
            }

            byte* dstPixel = dst + y * stride + x * 4;
            dstPixel[0] = (byte)(sumB / count);
            dstPixel[1] = (byte)(sumG / count);
            dstPixel[2] = (byte)(sumR / count);
            dstPixel[3] = (byte)(sumA / count);
        }
    }
}