namespace GCPerformance.Core;

public record BenchmarkResult(
    string Strategy,
    TimeSpan TotalTime,
    TimeSpan AverageTime,
    int IterationCount);