using System.Diagnostics;
using System.Drawing;
using GCPerformance.Algorithms;
using GCPerformance.Core;

namespace GCPerformance.Strategies;

public class ManagedBenchmarkStrategy : IBenchmarkStrategy
{
    private readonly IPixelProcessor _processor;
    
    public ManagedBenchmarkStrategy() : this(new StandardPixelProcessor()) { }
    public ManagedBenchmarkStrategy(IPixelProcessor processor) => _processor = processor;
    
    public virtual string StrategyName => "Managed Memory";

    public TimeSpan ExecuteBenchmark(string imagePath, BenchmarkParameters parameters)
    {
        parameters.GcStrategy.Configure();
        
        var stopwatch = Stopwatch.StartNew();
        
        if (parameters.EnableParallelProcessing)
        {
            ExecuteParallel(imagePath, parameters);
        }
        else
        {
            ExecuteSequential(imagePath, parameters);
        }
        
        stopwatch.Stop();
        return stopwatch.Elapsed;
    }

    private void ExecuteParallel(string imagePath, BenchmarkParameters parameters)
    {
        Parallel.For(0, parameters.Iterations, _ => ProcessSingleIteration(imagePath, parameters));
    }

    private void ExecuteSequential(string imagePath, BenchmarkParameters parameters)
    {
        for (var i = 0; i < parameters.Iterations; i++)
        {
            ProcessSingleIteration(imagePath, parameters);
        }
    }

    private void ProcessSingleIteration(string imagePath, BenchmarkParameters parameters)
    {
        using var bitmap = new Bitmap(imagePath);
        var pixelData = ExtractPixelData(bitmap);
        var resultData = new byte[pixelData.Length];
        
        _processor.ProcessPixels(pixelData, resultData, bitmap.Width, bitmap.Height);
        
        if (!parameters.AutoDisposeResources)
        {
            // Simulate keeping resources alive
            GC.KeepAlive(resultData);
        }
        
        parameters.GcStrategy.ExecuteCleanup();
    }

    private static byte[] ExtractPixelData(Bitmap bitmap)
    {
        var width = bitmap.Width;
        var height = bitmap.Height;
        var pixels = new byte[width * height * 3];

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var color = bitmap.GetPixel(x, y);
                var index = (y * width + x) * 3;
                pixels[index] = color.R;
                pixels[index + 1] = color.G;
                pixels[index + 2] = color.B;
            }
        }

        return pixels;
    }
}