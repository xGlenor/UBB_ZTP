using GCPerformance.Core;

namespace GCPerformance.Execution;

public class BenchmarkRunner
{
    private readonly List<IBenchmarkStrategy> _strategies = new();
    
    public BenchmarkRunner AddStrategy(string strategyName)
    {
        _strategies.Add(StrategySelector.GetStrategy(strategyName));
        return this;
    }

    public async Task<IEnumerable<BenchmarkResult>> RunBenchmarksAsync(
        string imagePath, 
        BenchmarkParameters parameters)
    {
        if (!File.Exists(imagePath))
            throw new FileNotFoundException($"Image file not found: {imagePath}");

        var results = new List<BenchmarkResult>();
        
        
        foreach (var strategy in _strategies)
        {
            
            Console.WriteLine($"Executing benchmark: {strategy.StrategyName}");
            
            var elapsed = await Task.Run(() => strategy.ExecuteBenchmark(imagePath, parameters));
            var average = TimeSpan.FromTicks(elapsed.Ticks / parameters.Iterations);
            
            results.Add(new BenchmarkResult(strategy.StrategyName, elapsed, average, parameters.Iterations));
            
            Console.WriteLine($"  Total: {elapsed.TotalMilliseconds:F2}ms | Average: {average.TotalMilliseconds:F2}ms");
            
        }

        return results;
    }
}