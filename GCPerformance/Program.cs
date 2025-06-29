using GCPerformance.Configuration;
using GCPerformance.Core;
using GCPerformance.Execution;
using GCPerformance.Strategies;

class Program
{
    private static readonly Dictionary<string, BenchmarkParameters> ProfileMap = new()
    {
        ["baseline"] = BenchmarkProfiles.Baseline,
        ["performance"] = BenchmarkProfiles.HighPerformance,
        ["lowlatency"] = BenchmarkProfiles.LowLatency,
        ["memory"] = BenchmarkProfiles.MemoryOptimized,
        ["cleanup"] = BenchmarkProfiles.AggressiveCleanup
    };

    private static readonly List<string> BenchmarkStrategyNames =
    [
        "managed",
        "unsafe",
        "pooled",
        "vectorized",
    ];
    
    static async Task Main(string[] args)
    {
        try
        {
            Console.WriteLine("=== Image Processing Benchmark Suite ===\n");
            
            var profileName = GetProfileSelection();
            var imagePath = args.Length > 0 ? args[0] : "./image.jpg";
            
            if (!ProfileMap.TryGetValue(profileName, out var parameters))
            {
                Console.WriteLine($"Unknown profile: {profileName}");
                return;
            }
            
            var strategyName = GetStrategySelection();

            DisplayConfiguration(profileName, parameters, imagePath, strategyName);
            
            var runner = new BenchmarkRunner()
                .AddStrategy(strategyName);

            var results = await runner.RunBenchmarksAsync(imagePath, parameters);
            
            ResultsReporter.DisplayResults(results);
            
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private static string GetProfileSelection()
    {
        Console.WriteLine("Available profiles:");
        foreach (var profile in ProfileMap.Keys)
        {
            Console.WriteLine($"  - {profile}");
        }
        
        Console.Write("\nSelect profile (default: baseline): ");
        var input = Console.ReadLine()?.Trim().ToLower();
        return string.IsNullOrEmpty(input) ? "baseline" : input;
    }

    private static string GetStrategySelection()
    {
        Console.WriteLine("Available strategies:");
        foreach (var strategy in BenchmarkStrategyNames)
        {
            Console.WriteLine($"  - {strategy}");
        }
        
        Console.Write("\nSelect strategy (default: managed): ");
        var input = Console.ReadLine()?.Trim().ToLower();
        return string.IsNullOrEmpty(input) ? "managed" : input;
    }

    private static void DisplayConfiguration(string profile, BenchmarkParameters parameters, string imagePath, string strategy)
    {
        Console.WriteLine($"\nConfiguration:");
        Console.WriteLine($"  Profile: {profile}");
        Console.WriteLine($"  Strategy: {strategy}");
        Console.WriteLine($"  Image: {imagePath}");
        Console.WriteLine($"  Iterations: {parameters.Iterations}");
        Console.WriteLine($"  Parallel: {parameters.EnableParallelProcessing}");
        Console.WriteLine($"  Auto-dispose: {parameters.AutoDisposeResources}");
        Console.WriteLine($"  GC Strategy: {parameters.GcStrategy.GetType().Name}");
        Console.WriteLine();
    }
}