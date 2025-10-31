using RaceCondition.Models;
using System.Diagnostics;

namespace RaceCondition.Examples;

/// <summary>
/// Demonstra race condition em cache compartilhado.
/// Cenário: Cache com muitas leituras e poucas escritas.
/// </summary>
public static class CacheRaceCondition
{
    /// <summary>
    /// Executa demonstração de race condition em cache.
    /// </summary>
    public static void RunDemo()
    {
        Console.WriteLine("\n" + new string('=', 80));
        Console.WriteLine("EXEMPLO 4: RACE CONDITION EM CACHE");
        Console.WriteLine(new string('=', 80));

        Console.WriteLine("\n📌 CENÁRIO:");
        Console.WriteLine("   - Cache de produtos (cenário comum em aplicações web)");
        Console.WriteLine("   - 80% leituras, 20% escritas (padrão típico)");
        Console.WriteLine("   - 20 threads operando simultaneamente");

        SharedCache<int, string> cache = new SharedCache<int, string>();

        // Teste 1: Sem sincronização
        Console.WriteLine("\n❌ TESTE 1: DICTIONARY SEM SINCRONIZAÇÃO");
        try
        {
            RunCacheOperations(cache, CacheType.Unsafe);
            Console.WriteLine($"   Itens no cache: {cache.UnsafeCount}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ⚠️  EXCEÇÃO: {ex.GetType().Name}");
            Console.WriteLine($"   Mensagem: {ex.Message}");
        }

        cache.ClearAll();

        // Teste 2: Com Lock
        Console.WriteLine("\n✅ TESTE 2: COM LOCK (SIMPLES MAS LENTO)");
        RunCacheOperations(cache, CacheType.WithLock);
        Console.WriteLine($"   Itens no cache: {cache.LockCount}");

        cache.ClearAll();

        // Teste 3: ConcurrentDictionary
        Console.WriteLine("\n✅ TESTE 3: CONCURRENTDICTIONARY (RECOMENDADO)");
        RunCacheOperations(cache, CacheType.ConcurrentDictionary);
        Console.WriteLine($"   Itens no cache: {cache.ThreadSafeCount}");

        cache.ClearAll();

        // Teste 4: ReaderWriterLockSlim (otimizado para leitura)
        Console.WriteLine("\n✅ TESTE 4: READERWRITERLOCKSLIM (OTIMIZADO PARA LEITURA)");
        Console.WriteLine("   Ideal quando: 80%+ leituras, <20% escritas");
        RunCacheOperations(cache, CacheType.ReaderWriterLock);
        Console.WriteLine($"   Itens no cache: {cache.LockCount}");

        // Comparação de performance
        Console.WriteLine("\n📊 COMPARAÇÃO DE PERFORMANCE:");
        ComparePerformance();

        // Explicação técnica
        Console.WriteLine("\n📚 EXPLICAÇÃO TÉCNICA:");
        Console.WriteLine("\n   Dictionary<K,V> não é thread-safe:");
        Console.WriteLine("   - Add() pode causar redimensionamento do array interno");
        Console.WriteLine("   - Leituras simultâneas com escritas causam exceções");
        Console.WriteLine("   - Corrupção de dados invisível");
        Console.WriteLine("\n   SOLUÇÕES POR CENÁRIO:");
        Console.WriteLine("\n   ┌─────────────────────────┬──────────────────────────┐");
        Console.WriteLine("   │ Cenário                 │ Solução Recomendada      │");
        Console.WriteLine("   ├─────────────────────────┼──────────────────────────┤");
        Console.WriteLine("   │ Leitura/Escrita balanceado │ ConcurrentDictionary   │");
        Console.WriteLine("   │ Muitas leituras, poucas     │ ReaderWriterLockSlim  │");
        Console.WriteLine("   │ escritas (80/20)            │                        │");
        Console.WriteLine("   │ Operações simples           │ Lock simples          │");
        Console.WriteLine("   │ Baixa contenção             │ Lock simples          │");
        Console.WriteLine("   └─────────────────────────┴──────────────────────────┘");

        cache.Dispose();
    }

    private enum CacheType
    {
        Unsafe,
        WithLock,
        ConcurrentDictionary,
        ReaderWriterLock
    }

    /// <summary>
    /// Executa operações concorrentes no cache.
    /// </summary>
    private static void RunCacheOperations(SharedCache<int, string> cache, CacheType type)
    {
        const int numberOfThreads = 20;
        const int operationsPerThread = 100;
        const double writePercentage = 0.2; // 20% escritas, 80% leituras

        Thread[] threads = new Thread[numberOfThreads];
        Stopwatch stopwatch = Stopwatch.StartNew();

        for (int i = 0; i < numberOfThreads; i++)
        {
            int threadId = i;
            threads[i] = new Thread(() =>
            {
                Random random = new Random(threadId);

                for (int j = 0; j < operationsPerThread; j++)
                {
                    int key = random.Next(0, 50);
                    string value = $"Product-{key}";

                    // 20% escritas, 80% leituras
                    if (random.NextDouble() < writePercentage)
                    {
                        // Escrita
                        switch (type)
                        {
                            case CacheType.Unsafe:
                                cache.AddUnsafe(key, value);
                                break;
                            case CacheType.WithLock:
                                cache.AddWithLock(key, value);
                                break;
                            case CacheType.ConcurrentDictionary:
                                cache.AddThreadSafe(key, value);
                                break;
                            case CacheType.ReaderWriterLock:
                                cache.AddWithReaderWriterLock(key, value);
                                break;
                        }
                    }
                    else
                    {
                        // Leitura
                        switch (type)
                        {
                            case CacheType.Unsafe:
                                _ = cache.GetUnsafe(key);
                                break;
                            case CacheType.WithLock:
                                _ = cache.GetWithLock(key);
                                break;
                            case CacheType.ConcurrentDictionary:
                                _ = cache.GetThreadSafe(key);
                                break;
                            case CacheType.ReaderWriterLock:
                                _ = cache.GetWithReaderWriterLock(key);
                                break;
                        }
                    }
                }
            });
            threads[i].Start();
        }

        foreach (Thread thread in threads)
        {
            thread.Join();
        }

        stopwatch.Stop();
        Console.WriteLine($"   Tempo de execução: {stopwatch.ElapsedMilliseconds}ms");
    }

    /// <summary>
    /// Compara performance entre diferentes implementações de cache.
    /// </summary>
    private static void ComparePerformance()
    {
        const int iterations = 10000;
        const double readPercentage = 0.8;

        Console.WriteLine("\n   Cenário: 80% leituras, 20% escritas");
        Console.WriteLine($"   Operações: {iterations:N0}\n");

        // Lock simples
        SharedCache<int, string> cache1 = new SharedCache<int, string>();
        Stopwatch sw1 = Stopwatch.StartNew();
        RunSingleThreadedCache(cache1, iterations, readPercentage, "Lock");
        sw1.Stop();
        Console.WriteLine($"   Lock:                  {sw1.ElapsedMilliseconds,5}ms");

        // ConcurrentDictionary
        SharedCache<int, string> cache2 = new SharedCache<int, string>();
        Stopwatch sw2 = Stopwatch.StartNew();
        RunSingleThreadedCache(cache2, iterations, readPercentage, "ConcurrentDictionary");
        sw2.Stop();
        Console.WriteLine($"   ConcurrentDictionary:  {sw2.ElapsedMilliseconds,5}ms");

        // ReaderWriterLock
        SharedCache<int, string> cache3 = new SharedCache<int, string>();
        Stopwatch sw3 = Stopwatch.StartNew();
        RunSingleThreadedCache(cache3, iterations, readPercentage, "ReaderWriterLock");
        sw3.Stop();
        Console.WriteLine($"   ReaderWriterLock:      {sw3.ElapsedMilliseconds,5}ms");

        Console.WriteLine("\n   🏆 VENCEDOR:");
        long min = Math.Min(sw1.ElapsedMilliseconds, Math.Min(sw2.ElapsedMilliseconds, sw3.ElapsedMilliseconds));
        
        if (min == sw1.ElapsedMilliseconds)
            Console.WriteLine("   Lock (melhor para baixa contenção)");
        else if (min == sw2.ElapsedMilliseconds)
            Console.WriteLine("   ConcurrentDictionary (melhor uso geral)");
        else
            Console.WriteLine("   ReaderWriterLock (melhor para muitas leituras)");

        cache1.Dispose();
        cache2.Dispose();
        cache3.Dispose();
    }

    /// <summary>
    /// Executa operações de cache em uma única thread para benchmark.
    /// </summary>
    private static void RunSingleThreadedCache(SharedCache<int, string> cache, int iterations, 
        double readPercentage, string type)
    {
        Random random = new Random(42);

        for (int i = 0; i < iterations; i++)
        {
            int key = random.Next(0, 100);
            string value = $"Value-{key}";

            if (random.NextDouble() < readPercentage)
            {
                // Leitura
                switch (type)
                {
                    case "Lock":
                        _ = cache.GetWithLock(key);
                        break;
                    case "ConcurrentDictionary":
                        _ = cache.GetThreadSafe(key);
                        break;
                    case "ReaderWriterLock":
                        _ = cache.GetWithReaderWriterLock(key);
                        break;
                }
            }
            else
            {
                // Escrita
                switch (type)
                {
                    case "Lock":
                        cache.AddWithLock(key, value);
                        break;
                    case "ConcurrentDictionary":
                        cache.AddThreadSafe(key, value);
                        break;
                    case "ReaderWriterLock":
                        cache.AddWithReaderWriterLock(key, value);
                        break;
                }
            }
        }
    }
}
