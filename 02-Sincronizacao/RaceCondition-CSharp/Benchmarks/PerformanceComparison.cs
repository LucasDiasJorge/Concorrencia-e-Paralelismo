using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace RaceCondition.Benchmarks;

/// <summary>
/// Benchmarks comparando diferentes técnicas de sincronização.
/// Execute com: dotnet run -c Release
/// </summary>
[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, iterationCount: 10)]
public class SynchronizationBenchmarks
{
    private int _counter;
    private readonly object _lock = new object();
    private const int Iterations = 10000;

    [GlobalSetup]
    public void Setup()
    {
        _counter = 0;
    }

    /// <summary>
    /// Baseline: Incremento sem sincronização (INCORRETO, mas mostra overhead).
    /// </summary>
    [Benchmark(Baseline = true)]
    public void Unsafe_Increment()
    {
        for (int i = 0; i < Iterations; i++)
        {
            _counter++;
        }
    }

    /// <summary>
    /// Incremento com Interlocked.Increment.
    /// </summary>
    [Benchmark]
    public void Interlocked_Increment()
    {
        for (int i = 0; i < Iterations; i++)
        {
            Interlocked.Increment(ref _counter);
        }
    }

    /// <summary>
    /// Incremento com Lock.
    /// </summary>
    [Benchmark]
    public void Lock_Increment()
    {
        for (int i = 0; i < Iterations; i++)
        {
            lock (_lock)
            {
                _counter++;
            }
        }
    }

    /// <summary>
    /// Incremento com Monitor.
    /// </summary>
    [Benchmark]
    public void Monitor_Increment()
    {
        for (int i = 0; i < Iterations; i++)
        {
            Monitor.Enter(_lock);
            try
            {
                _counter++;
            }
            finally
            {
                Monitor.Exit(_lock);
            }
        }
    }
}

/// <summary>
/// Benchmarks para operações de cache.
/// </summary>
[MemoryDiagnoser]
public class CacheBenchmarks
{
    private readonly Dictionary<int, string> _dictWithLock = new Dictionary<int, string>();
    private readonly System.Collections.Concurrent.ConcurrentDictionary<int, string> _concurrentDict = new System.Collections.Concurrent.ConcurrentDictionary<int, string>();
    private readonly object _lock = new object();
    private readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();

    [GlobalSetup]
    public void Setup()
    {
        // Pré-popula os dicionários
        for (int i = 0; i < 1000; i++)
        {
            _dictWithLock[i] = $"Value{i}";
            _concurrentDict[i] = $"Value{i}";
        }
    }

    [Benchmark(Baseline = true)]
    public void Dictionary_WithLock_Read()
    {
        for (int i = 0; i < 1000; i++)
        {
            lock (_lock)
            {
                _ = _dictWithLock.TryGetValue(i, out string? value);
            }
        }
    }

    [Benchmark]
    public void ConcurrentDictionary_Read()
    {
        for (int i = 0; i < 1000; i++)
        {
            _ = _concurrentDict.TryGetValue(i, out string? value);
        }
    }

    [Benchmark]
    public void ReaderWriterLockSlim_Read()
    {
        for (int i = 0; i < 1000; i++)
        {
            _rwLock.EnterReadLock();
            try
            {
                _ = _dictWithLock.TryGetValue(i, out string? value);
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
        }
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _rwLock?.Dispose();
    }
}

/// <summary>
/// Classe para executar os benchmarks.
/// </summary>
public static class BenchmarkRunner
{
    /// <summary>
    /// Executa todos os benchmarks.
    /// </summary>
    public static void RunAllBenchmarks()
    {
        Console.WriteLine("\n" + new string('=', 80));
        Console.WriteLine("🔬 EXECUTANDO BENCHMARKS COM BENCHMARKDOTNET");
        Console.WriteLine(new string('=', 80));

        Console.WriteLine("\n⚠️  IMPORTANTE:");
        Console.WriteLine("   - Benchmarks requerem compilação Release");
        Console.WriteLine("   - Podem levar vários minutos para completar");
        Console.WriteLine("   - Feche outros programas para resultados precisos");

        Console.WriteLine("\n📊 BENCHMARKS DISPONÍVEIS:");
        Console.WriteLine("   1. Synchronization Benchmarks (Interlocked vs Lock)");
        Console.WriteLine("   2. Cache Benchmarks (Dictionary vs ConcurrentDictionary)");

        Console.Write("\nExecutar benchmarks? (s/n): ");
        string? response = Console.ReadLine();

        if (response?.ToLower() == "s")
        {
            var summary1 = BenchmarkDotNet.Running.BenchmarkRunner.Run<SynchronizationBenchmarks>();
            var summary2 = BenchmarkDotNet.Running.BenchmarkRunner.Run<CacheBenchmarks>();
        }
        else
        {
            Console.WriteLine("\n❌ Benchmarks cancelados.");
        }
    }
}
