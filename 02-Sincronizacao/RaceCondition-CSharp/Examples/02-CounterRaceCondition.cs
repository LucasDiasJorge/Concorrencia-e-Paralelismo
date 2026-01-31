using RaceCondition.Models;
using System.Diagnostics;

namespace RaceCondition.Examples;

/// <summary>
/// Demonstra race condition em operações de contador compartilhado.
/// Cenário: Incrementos perdidos em contador de analytics/métricas.
/// </summary>
public static class CounterRaceCondition
{
    /// <summary>
    /// Executa demonstração de race condition em contador compartilhado.
    /// </summary>
    public static void RunDemo()
    {
        Console.WriteLine("\n" + new string('=', 80));
        Console.WriteLine("EXEMPLO 2: RACE CONDITION EM CONTADOR COMPARTILHADO");
        Console.WriteLine(new string('=', 80));

        Console.WriteLine("\n📌 CENÁRIO:");
        Console.WriteLine("   - 20 threads incrementando o contador");
        Console.WriteLine("   - Cada thread faz 10.000 incrementos");
        Console.WriteLine("   - Total esperado: 200.000 incrementos");

        SharedCounter counter = new SharedCounter();

        // Teste 1: Versão INSEGURA
        Console.WriteLine("\n❌ TESTE 1: VERSÃO INSEGURA (counter++)");
        RunConcurrentIncrements(counter, CounterType.Unsafe);
        Console.WriteLine($"   Resultado: {counter.CounterUnsafe:N0}");
        Console.WriteLine($"   Incrementos perdidos: {200000 - counter.CounterUnsafe:N0}");
        Console.WriteLine($"   Taxa de perda: {((200000 - counter.CounterUnsafe) / 200000.0) * 100:F2}%");

        // Teste 2: Versão com LOCK
        Console.WriteLine("\n✅ TESTE 2: VERSÃO COM LOCK");
        counter.ResetAll();
        RunConcurrentIncrements(counter, CounterType.WithLock);
        Console.WriteLine($"   Resultado: {counter.CounterWithLock:N0}");
        Console.WriteLine($"   Incrementos perdidos: {200000 - counter.CounterWithLock:N0}");

        // Teste 3: Versão com INTERLOCKED (MAIS RÁPIDA!)
        Console.WriteLine("\n✅ TESTE 3: VERSÃO COM INTERLOCKED (RECOMENDADA)");
        counter.ResetAll();
        RunConcurrentIncrements(counter, CounterType.WithInterlocked);
        Console.WriteLine($"   Resultado: {counter.CounterWithInterlocked:N0}");
        Console.WriteLine($"   Incrementos perdidos: {200000 - counter.CounterWithInterlocked:N0}");

        // Comparação de Performance
        Console.WriteLine("\n📊 COMPARAÇÃO DE PERFORMANCE:");
        ComparePerformance();

        // Explicação técnica
        Console.WriteLine("\n📚 EXPLICAÇÃO TÉCNICA:");
        Console.WriteLine("\n   A operação counter++ em assembly:");
        Console.WriteLine("   MOV EAX, [counter]    ; Lê valor da memória");
        Console.WriteLine("   INC EAX               ; Incrementa no registrador");
        Console.WriteLine("   MOV [counter], EAX    ; Escreve de volta na memória");
        Console.WriteLine("\n   Se 2 threads executam simultaneamente:");
        Console.WriteLine("   Thread A: Lê 100 → Inc 101 → Escreve 101");
        Console.WriteLine("   Thread B: Lê 100 → Inc 101 → Escreve 101");
        Console.WriteLine("   Resultado: 101 (esperado: 102) ❌");
        Console.WriteLine("\n   SOLUÇÃO RECOMENDADA: Interlocked.Increment()");
        Console.WriteLine("   - Usa instrução LOCK XADD do processador");
        Console.WriteLine("   - Operação atômica em hardware");
        Console.WriteLine("   - ~5x mais rápido que lock { }");
    }

    /// <summary>
    /// Tipo de contador a ser testado.
    /// </summary>
    private enum CounterType
    {
        Unsafe,
        WithLock,
        WithInterlocked
    }

    /// <summary>
    /// Executa incrementos concorrentes no contador.
    /// </summary>
    private static void RunConcurrentIncrements(SharedCounter counter, CounterType type)
    {
        const int numberOfThreads = 20;
        const int incrementsPerThread = 10000;

        Thread[] threads = new Thread[numberOfThreads];
        Stopwatch stopwatch = Stopwatch.StartNew();

        for (int i = 0; i < numberOfThreads; i++)
        {
            threads[i] = new Thread(() =>
            {
                for (int j = 0; j < incrementsPerThread; j++)
                {
                    switch (type)
                    {
                        case CounterType.Unsafe:
                            counter.IncrementUnsafe();
                            break;
                        case CounterType.WithLock:
                            counter.IncrementWithLock();
                            break;
                        case CounterType.WithInterlocked:
                            counter.IncrementWithInterlocked();
                            break;
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
    /// Compara performance entre diferentes abordagens.
    /// </summary>
    private static void ComparePerformance()
    {
        const int iterations = 1000000;

        // Teste 1: Interlocked
        SharedCounter counter1 = new SharedCounter();
        Stopwatch sw1 = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            counter1.IncrementWithInterlocked();
        }
        sw1.Stop();

        // Teste 2: Lock
        SharedCounter counter2 = new SharedCounter();
        Stopwatch sw2 = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            counter2.IncrementWithLock();
        }
        sw2.Stop();

        Console.WriteLine($"   Interlocked: {sw1.ElapsedMilliseconds}ms ({iterations:N0} ops) - {(iterations / sw1.ElapsedMilliseconds):N0} ops/ms");
        Console.WriteLine($"   Lock:        {sw2.ElapsedMilliseconds}ms ({iterations:N0} ops) - {(iterations / sw2.ElapsedMilliseconds):N0} ops/ms");
        Console.WriteLine($"   Speedup:     {(double)sw2.ElapsedMilliseconds / sw1.ElapsedMilliseconds:F2}x mais rápido");
    }

    /// <summary>
    /// Demonstra operação Compare-And-Swap (CAS).
    /// </summary>
    public static void DemonstrateCompareExchange()
    {
        Console.WriteLine("\n" + new string('=', 80));
        Console.WriteLine("DEMONSTRAÇÃO: COMPARE-AND-SWAP (CAS)");
        Console.WriteLine(new string('=', 80));

        Console.WriteLine("\n📌 Compare-And-Swap é a operação fundamental em lock-free programming");
        Console.WriteLine("   Atomicamente: if (value == expected) { value = newValue; }");

        SharedCounter counter = new SharedCounter();
        counter.AddWithInterlocked(100);

        Console.WriteLine($"\n   Valor inicial: {counter.CounterWithInterlocked}");

        // Tenta trocar se o valor for 100
        int oldValue = counter.CompareExchangeWithInterlocked(100, 200);
        Console.WriteLine($"   CompareExchange(100, 200) → Valor antigo: {oldValue}, Novo: {counter.CounterWithInterlocked}");

        // Tenta trocar se o valor for 100 (vai falhar pois agora é 200)
        oldValue = counter.CompareExchangeWithInterlocked(100, 300);
        Console.WriteLine($"   CompareExchange(100, 300) → Valor antigo: {oldValue}, Novo: {counter.CounterWithInterlocked}");
        Console.WriteLine("   ⚠️  Operação falhou porque o valor não era 100!");

        Console.WriteLine("\n📚 USO PRÁTICO:");
        Console.WriteLine("   - Implementar estruturas lock-free");
        Console.WriteLine("   - Retry loops em operações concorrentes");
        Console.WriteLine("   - Algoritmos de sincronização avançados");
    }
}
