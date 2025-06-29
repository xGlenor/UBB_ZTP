namespace GCPerformance.Core;

public interface IBenchmarkStrategy
{
    string StrategyName { get; }
    TimeSpan ExecuteBenchmark(string imagePath, BenchmarkParameters parameters);
}