using GCPerformance.Algorithms;

namespace GCPerformance.Strategies;

public class UnsafeBenchmarkStrategy() : ManagedBenchmarkStrategy(new UnsafePixelProcessor())
{
    public override string StrategyName => "Unsafe Pointers";
}