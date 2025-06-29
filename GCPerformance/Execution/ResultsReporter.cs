using GCPerformance.Core;

namespace GCPerformance.Execution;

public static class ResultsReporter
{
    public static void DisplayResults(IEnumerable<BenchmarkResult> results)
    {
        var resultList = results.OrderBy(r => r.AverageTime).ToList();
        
        Console.WriteLine("\n" + new string('=', 60));
        Console.WriteLine("BENCHMARK RESULTS SUMMARY");
        Console.WriteLine(new string('=', 60));
        
        Console.WriteLine($"{"Strategy",-20} {"Total (ms)",-12} {"Avg (ms)",-12} {"Iterations",-10}");
        Console.WriteLine(new string('-', 60));
        
        foreach (var result in resultList)
        {
            Console.WriteLine($"{result.Strategy,-20} {result.TotalTime.TotalMilliseconds,-12:F2} " +
                              $"{result.AverageTime.TotalMilliseconds,-12:F2} {result.IterationCount,-10}");
        }
        
        Console.WriteLine(new string('=', 60));
        
        if (resultList.Count > 1)
        {
            var fastest = resultList.First();
            var slowest = resultList.Last();
            var speedup = slowest.AverageTime.TotalMilliseconds / fastest.AverageTime.TotalMilliseconds;
            
            Console.WriteLine($"Fastest: {fastest.Strategy}");
            Console.WriteLine($"Performance gain: {speedup:F2}x faster than slowest");
        }
    }
}