using System.Diagnostics;

namespace RaceCondition.Solutions;

/// <summary>
/// Demonstra o uso de ReaderWriterLockSlim para cenários read-heavy.
/// Otimizado para cenários com muitas leituras e poucas escritas.
/// </summary>
public static class ReaderWriterLockSolution
{
    /// <summary>
    /// Demonstra o uso de ReaderWriterLockSlim.
    /// </summary>
    public static void RunDemo()
    {
        Console.WriteLine("\n" + new string('=', 80));
        Console.WriteLine("SOLUÇÃO 4: READERWRITERLOCKSLIM");
        Console.WriteLine(new string('=', 80));

        Console.WriteLine("\n📚 O QUE É READERWRITERLOCKSLIM?");
        Console.WriteLine("   - Permite múltiplas leituras simultâneas");
        Console.WriteLine("   - Escrita requer exclusividade (bloqueia leituras)");
        Console.WriteLine("   - Otimizado para cenários read-heavy");
        Console.WriteLine("   - Mais complexo que lock, mas mais performático");

        Console.WriteLine("\n✅ QUANDO USAR:");
        Console.WriteLine("   ✓ Muitas leituras, poucas escritas (>80% leituras)");
        Console.WriteLine("   ✓ Cache, dicionários compartilhados");
        Console.WriteLine("   ✓ Configurações que raramente mudam");
        Console.WriteLine("   ✓ Dados que são lidos frequentemente");

        Console.WriteLine("\n❌ QUANDO NÃO USAR:");
        Console.WriteLine("   ✗ Proporção balanceada de leitura/escrita");
        Console.WriteLine("   ✗ Maioria escritas");
        Console.WriteLine("   ✗ Operações muito rápidas (overhead não compensa)");

        DemonstrateBasicUsage();
        DemonstrateUpgradeableLock();
        ComparePerformance();
    }

    /// <summary>
    /// Demonstra uso básico de ReaderWriterLockSlim.
    /// </summary>
    private static void DemonstrateBasicUsage()
    {
        Console.WriteLine("\n" + new string('-', 80));
        Console.WriteLine("1. USO BÁSICO");
        Console.WriteLine(new string('-', 80));

        SharedResource resource = new SharedResource();

        Console.WriteLine("\n   Cenário: 8 leitores, 2 escritores");
        Console.WriteLine("   Observe como múltiplos leitores executam simultaneamente\n");

        Thread[] threads = new Thread[10];

        // 8 threads de leitura
        for (int i = 0; i < 8; i++)
        {
            int readerId = i + 1;
            threads[i] = new Thread(() =>
            {
                string value = resource.Read();
                Console.WriteLine($"   Reader {readerId}: Leu '{value}' às {DateTime.Now:HH:mm:ss.fff}");
                Thread.Sleep(500); // Simula processamento
            });
        }

        // 2 threads de escrita
        for (int i = 8; i < 10; i++)
        {
            int writerId = i - 7;
            threads[i] = new Thread(() =>
            {
                Thread.Sleep(200); // Aguarda leitores iniciarem
                resource.Write($"Valor{writerId}");
                Console.WriteLine($"   Writer {writerId}: ✍️  Escreveu 'Valor{writerId}' às {DateTime.Now:HH:mm:ss.fff}");
            });
        }

        // Inicia todas as threads
        foreach (Thread thread in threads)
        {
            thread.Start();
        }

        foreach (Thread thread in threads)
        {
            thread.Join();
        }

        Console.WriteLine("\n   ✅ Leitores executaram simultaneamente!");
        Console.WriteLine("   ✅ Escritores aguardaram exclusividade!");

        resource.Dispose();
    }

    /// <summary>
    /// Demonstra lock upgradeable (leitura que pode virar escrita).
    /// </summary>
    private static void DemonstrateUpgradeableLock()
    {
        Console.WriteLine("\n" + new string('-', 80));
        Console.WriteLine("2. UPGRADEABLE LOCK (Read → Write)");
        Console.WriteLine(new string('-', 80));

        Console.WriteLine("\n   Cenário: Lazy loading de cache");
        Console.WriteLine("   1. Tenta ler do cache (read lock)");
        Console.WriteLine("   2. Se não existe, promove para write lock");
        Console.WriteLine("   3. Adiciona no cache");
        Console.WriteLine("   4. Volta para read lock\n");

        LazyCache cache = new LazyCache();

        Thread[] threads = new Thread[5];

        for (int i = 0; i < 5; i++)
        {
            int threadId = i + 1;
            threads[i] = new Thread(() =>
            {
                string value = cache.GetOrAdd("chave1", () =>
                {
                    Console.WriteLine($"   Thread {threadId}: Cache miss! Carregando dados...");
                    Thread.Sleep(1000); // Simula carga de BD
                    return "Dados carregados";
                });

                Console.WriteLine($"   Thread {threadId}: Obteve '{value}'");
            });
            threads[i].Start();
        }

        foreach (Thread thread in threads)
        {
            thread.Join();
        }

        Console.WriteLine("\n   ✅ Apenas UMA thread carregou os dados!");
        Console.WriteLine("   ✅ Outras threads aguardaram e reutilizaram o cache!");

        cache.Dispose();
    }

    /// <summary>
    /// Compara performance entre Lock e ReaderWriterLockSlim.
    /// </summary>
    private static void ComparePerformance()
    {
        Console.WriteLine("\n" + new string('-', 80));
        Console.WriteLine("3. COMPARAÇÃO DE PERFORMANCE");
        Console.WriteLine(new string('-', 80));

        const int iterations = 100000;
        const double readPercentage = 0.9; // 90% leituras

        Console.WriteLine($"\n   Cenário: {iterations:N0} operações");
        Console.WriteLine($"   Proporção: {readPercentage * 100}% leituras, {(1 - readPercentage) * 100}% escritas\n");

        // Teste com Lock
        SharedResourceWithLock resourceLock = new SharedResourceWithLock();
        Stopwatch sw1 = Stopwatch.StartNew();
        RunOperations(resourceLock, iterations, readPercentage);
        sw1.Stop();

        // Teste com ReaderWriterLockSlim
        SharedResourceWithRWLock resourceRW = new SharedResourceWithRWLock();
        Stopwatch sw2 = Stopwatch.StartNew();
        RunOperations(resourceRW, iterations, readPercentage);
        sw2.Stop();

        Console.WriteLine($"   Lock:                {sw1.ElapsedMilliseconds,5}ms");
        Console.WriteLine($"   ReaderWriterLock:    {sw2.ElapsedMilliseconds,5}ms");
        
        if (sw2.ElapsedMilliseconds < sw1.ElapsedMilliseconds)
        {
            double improvement = ((double)sw1.ElapsedMilliseconds / sw2.ElapsedMilliseconds);
            Console.WriteLine($"\n   🏆 ReaderWriterLock é {improvement:F2}x mais rápido!");
        }
        else
        {
            Console.WriteLine("\n   ⚠️  Lock foi mais rápido (baixa contenção ou poucas leituras)");
        }

        Console.WriteLine("\n   📊 REGRA GERAL:");
        Console.WriteLine("   - <70% leituras → Use Lock");
        Console.WriteLine("   - >80% leituras → Use ReaderWriterLockSlim");
        Console.WriteLine("   - 70-80% → Teste ambos e meça!");

        resourceLock.Dispose();
        resourceRW.Dispose();
    }

    private static void RunOperations(ISharedResource resource, int iterations, double readPercentage)
    {
        Random random = new Random(42);

        for (int i = 0; i < iterations; i++)
        {
            if (random.NextDouble() < readPercentage)
            {
                _ = resource.Read();
            }
            else
            {
                resource.Write($"Value{i}");
            }
        }
    }

    private interface ISharedResource : IDisposable
    {
        string Read();
        void Write(string value);
    }

    private class SharedResource : IDisposable
    {
        private string _value = "Inicial";
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        public string Read()
        {
            _lock.EnterReadLock();
            try
            {
                return _value;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void Write(string value)
        {
            _lock.EnterWriteLock();
            try
            {
                _value = value;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Dispose()
        {
            _lock?.Dispose();
        }
    }

    private class LazyCache : IDisposable
    {
        private readonly Dictionary<string, string> _cache = new Dictionary<string, string>();
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        public string GetOrAdd(string key, Func<string> valueFactory)
        {
            // Tenta ler primeiro
            _lock.EnterUpgradeableReadLock();
            try
            {
                if (_cache.TryGetValue(key, out string? value))
                {
                    return value;
                }

                // Não existe, promove para write lock
                _lock.EnterWriteLock();
                try
                {
                    // Double-check (outra thread pode ter adicionado)
                    if (_cache.TryGetValue(key, out value))
                    {
                        return value;
                    }

                    // Cria o valor
                    value = valueFactory();
                    _cache[key] = value;
                    return value;
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        public void Dispose()
        {
            _lock?.Dispose();
        }
    }

    private class SharedResourceWithLock : ISharedResource
    {
        private string _value = "Initial";
        private readonly object _lock = new object();

        public string Read()
        {
            lock (_lock)
            {
                return _value;
            }
        }

        public void Write(string value)
        {
            lock (_lock)
            {
                _value = value;
            }
        }

        public void Dispose() { }
    }

    private class SharedResourceWithRWLock : ISharedResource
    {
        private string _value = "Initial";
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        public string Read()
        {
            _lock.EnterReadLock();
            try
            {
                return _value;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void Write(string value)
        {
            _lock.EnterWriteLock();
            try
            {
                _value = value;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Dispose()
        {
            _lock?.Dispose();
        }
    }
}
