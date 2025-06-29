using GCPerformance.Core;

namespace GCPerformance.Configuration;


public static class BenchmarkProfiles
{
    public static BenchmarkParameters Baseline =>
        new BenchmarkProfileBuilder()
            .WithIterations(5)
            .AutoDisposeResources(true)
            .EnableParallelProcessing(false)
            .WithGarbageCollection(GarbageCollectionStrategy.None)
            .Build();

    public static BenchmarkParameters HighPerformance =>
        new BenchmarkProfileBuilder()
            .WithIterations(5)
            .AutoDisposeResources(false)
            .EnableParallelProcessing(true)
            .WithGarbageCollection(GarbageCollectionStrategy.Optimized)
            .Build();

    public static BenchmarkParameters LowLatency =>
        new BenchmarkProfileBuilder()
            .WithIterations(5)
            .AutoDisposeResources(true)
            .EnableParallelProcessing(false)
            .WithGarbageCollection(GarbageCollectionStrategy.LowLatency)
            .Build();

    public static BenchmarkParameters MemoryOptimized =>
        new BenchmarkProfileBuilder()
            .WithIterations(5)
            .AutoDisposeResources(false)
            .EnableParallelProcessing(false)
            .WithGarbageCollection(GarbageCollectionStrategy.None)
            .Build();

    public static BenchmarkParameters AggressiveCleanup =>
        new BenchmarkProfileBuilder()
            .WithIterations(5)
            .AutoDisposeResources(true)
            .EnableParallelProcessing(false)
            .WithGarbageCollection(GarbageCollectionStrategy.Aggressive)
            .Build();
}