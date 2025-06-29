using GCPerformance.Algorithms;

namespace GCPerformance.Strategies;

public class VectorizedBenchmarkStrategy() : ManagedBenchmarkStrategy(new VectorizedPixelProcessor())
{
   
    public override string StrategyName => "SIMD Vectorized";
}