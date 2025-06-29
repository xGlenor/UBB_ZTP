namespace GCPerformance.Core;

public record BenchmarkParameters(
    int Iterations,
    bool EnableParallelProcessing,
    bool AutoDisposeResources,
    GarbageCollectionStrategy GcStrategy);