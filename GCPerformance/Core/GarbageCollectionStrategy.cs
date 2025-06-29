using System.Runtime;

namespace GCPerformance.Core;

public abstract class GarbageCollectionStrategy
{
    public abstract void Configure();
    public abstract void ExecuteCleanup();
    
    public static GarbageCollectionStrategy None => new NoGcStrategy();
    public static GarbageCollectionStrategy Aggressive => new AggressiveGcStrategy();
    public static GarbageCollectionStrategy Optimized => new OptimizedGcStrategy();
    public static GarbageCollectionStrategy LowLatency => new LowLatencyGcStrategy();
}

file class NoGcStrategy : GarbageCollectionStrategy
{
    public override void Configure() { }
    public override void ExecuteCleanup() { }
}

file class AggressiveGcStrategy : GarbageCollectionStrategy
{
    public override void Configure() { }
    public override void ExecuteCleanup()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }
}

file class OptimizedGcStrategy : GarbageCollectionStrategy
{
    public override void Configure()
    {
        GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
    }
    
    public override void ExecuteCleanup()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }
}

file class LowLatencyGcStrategy : GarbageCollectionStrategy
{
    public override void Configure()
    {
        GCSettings.LatencyMode = GCLatencyMode.LowLatency;
    }
    
    public override void ExecuteCleanup() { }
}