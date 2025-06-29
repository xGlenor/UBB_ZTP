using System.Buffers;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using GCPerformance.Algorithms;
using GCPerformance.Core;

namespace GCPerformance.Strategies;

public class PooledBenchmarkStrategy : IBenchmarkStrategy
{
    public string StrategyName => "Memory Pooling";
    public unsafe TimeSpan ExecuteBenchmark(string imagePath, BenchmarkParameters parameters)
    {
        parameters.GcStrategy.Configure();
        
        var stopwatch = Stopwatch.StartNew();
        
        if (parameters.EnableParallelProcessing)
        {
            Parallel.For(0, parameters.Iterations, _ => ProcessWithPooling(imagePath, parameters));
        }
        else
        {
            for (var i = 0; i < parameters.Iterations; i++)
            {
                ProcessWithPooling(imagePath, parameters);
            }
        }
        
        stopwatch.Stop();
        return stopwatch.Elapsed;
    }

    private unsafe void ProcessWithPooling(string imagePath, BenchmarkParameters parameters)
    {
        using var sourceBitmap = new Bitmap(imagePath);
        using var resultBitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height);

        var sourceData = sourceBitmap.LockBits(
            new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height),
            ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

        var resultData = resultBitmap.LockBits(
            new Rectangle(0, 0, resultBitmap.Width, resultBitmap.Height),
            ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

        var kernelBuffer = ArrayPool<int>.Shared.Rent(25); // 5x5 kernel
        
        try
        {
            CopyKernelToBuffer(kernelBuffer);
            ProcessBitmapData((byte*)sourceData.Scan0, (byte*)resultData.Scan0, 
                            kernelBuffer, sourceBitmap.Width, sourceBitmap.Height, sourceData.Stride);
        }
        finally
        {
            ArrayPool<int>.Shared.Return(kernelBuffer);
            sourceBitmap.UnlockBits(sourceData);
            resultBitmap.UnlockBits(resultData);
            
            if (!parameters.AutoDisposeResources)
            {
                GC.KeepAlive(resultBitmap);
            }
            
            parameters.GcStrategy.ExecuteCleanup();
        }
    }

    private static void CopyKernelToBuffer(int[] buffer)
    {
        var kernel = EdgeDetectionKernel.LaplacianMatrix;
        var index = 0;
        
        for (var i = 0; i < 5; i++)
        {
            for (var j = 0; j < 5; j++)
            {
                buffer[index++] = kernel[i, j];
            }
        }
    }

    private static unsafe void ProcessBitmapData(
        byte* source, byte* result, int[] kernel, 
        int width, int height, int stride)
    {
        fixed (int* kernelPtr = kernel)
        {
            for (var y = 2; y < height - 2; y++)
            {
                for (var x = 2; x < width - 2; x++)
                {
                    int r = 0, g = 0, b = 0;

                    for (var ky = -2; ky <= 2; ky++)
                    {
                        for (var kx = -2; kx <= 2; kx++)
                        {
                            var pixelPtr = source + ((y + ky) * stride) + ((x + kx) * 3);
                            var kernelValue = kernelPtr[(ky + 2) * 5 + (kx + 2)];
                            
                            b += pixelPtr[0] * kernelValue;
                            g += pixelPtr[1] * kernelValue;
                            r += pixelPtr[2] * kernelValue;
                        }
                    }

                    var outputPtr = result + (y * stride) + (x * 3);
                    outputPtr[0] = (byte)Math.Clamp(b, 0, 255);
                    outputPtr[1] = (byte)Math.Clamp(g, 0, 255);
                    outputPtr[2] = (byte)Math.Clamp(r, 0, 255);
                }
            }
        }
    }
}