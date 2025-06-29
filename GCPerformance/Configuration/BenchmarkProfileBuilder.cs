using GCPerformance.Core;

namespace GCPerformance.Configuration;

public class BenchmarkProfileBuilder
{
    private string _imagePath = "./image.jpg";
    private int _iterations = 5;
    private bool _enableParallel = false;
    private bool _autoDispose = true;
    private GarbageCollectionStrategy _gcStrategy = GarbageCollectionStrategy.None;

    public BenchmarkProfileBuilder WithImagePath(string path)
    {
        _imagePath = path;
        return this;
    }

    public BenchmarkProfileBuilder WithIterations(int count)
    {
        _iterations = count;
        return this;
    }

    public BenchmarkProfileBuilder EnableParallelProcessing(bool enable = true)
    {
        _enableParallel = enable;
        return this;
    }

    public BenchmarkProfileBuilder AutoDisposeResources(bool dispose = true)
    {
        _autoDispose = dispose;
        return this;
    }

    public BenchmarkProfileBuilder WithGarbageCollection(GarbageCollectionStrategy strategy)
    {
        _gcStrategy = strategy;
        return this;
    }

    public BenchmarkParameters Build() => new(_iterations, _enableParallel, _autoDispose, _gcStrategy);
}