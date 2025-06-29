using GCPerformance.Strategies;

namespace GCPerformance.Core;

public static class StrategySelector
{
    public static IBenchmarkStrategy GetStrategy(string strategyName)
    {
        return strategyName switch
        {
            "managed" => new ManagedBenchmarkStrategy(),
            "unsafe" => new UnsafeBenchmarkStrategy(),
            "vectorized" => new VectorizedBenchmarkStrategy(),
            "pooled" => new PooledBenchmarkStrategy(),
            _ => new ManagedBenchmarkStrategy()
        };
    }
    
}