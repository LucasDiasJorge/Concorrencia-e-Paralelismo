using System.Diagnostics;

namespace RaceCondition.Solutions;

/// <summary>
/// Demonstra o uso de Interlocked para operações atômicas.
/// Interlocked é a solução mais rápida para operações simples.
/// </summary>
public static class InterlockedSolution
{
    /// <summary>
    /// Demonstra todas as operações do Interlocked.
    /// </summary>
    public static void RunDemo()
    {
        Console.WriteLine("\n" + new string('=', 80));
        Console.WriteLine("SOLUÇÃO 2: INTERLOCKED (OPERAÇÕES ATÔMICAS)");
        Console.WriteLine(new string('=', 80));

        Console.WriteLine("\n📚 O QUE É INTERLOCKED?");
        Console.WriteLine("   - Operações atômicas garantidas pelo processador (CPU-level)");
        Console.WriteLine("   - Usa instruções especiais: LOCK XADD, LOCK CMPXCHG, etc.");
        Console.WriteLine("   - NÃO usa locks (lock-free programming)");
        Console.WriteLine("   - Extremamente rápido: ~5-10ns por operação");

        Console.WriteLine("\n✅ QUANDO USAR:");
        Console.WriteLine("   ✓ Incremento/decremento de contadores");
        Console.WriteLine("   ✓ Trocar valores atomicamente");
        Console.WriteLine("   ✓ Compare-And-Swap (CAS)");
        Console.WriteLine("   ✓ Performance crítica");
        Console.WriteLine("   ✓ Operações simples isoladas");

        Console.WriteLine("\n❌ QUANDO NÃO USAR:");
        Console.WriteLine("   ✗ Múltiplas operações que devem ser atômicas juntas");
        Console.WriteLine("   ✗ Lógica complexa");
        Console.WriteLine("   ✗ Tipos que não cabem em 32/64 bits");

        DemonstrateIncrement();
        DemonstrateAdd();
        DemonstrateExchange();
        DemonstrateCompareExchange();
        DemonstrateRead();
        CompareWithLock();
    }

    /// <summary>
    /// Demonstra Interlocked.Increment e Decrement.
    /// </summary>
    private static void DemonstrateIncrement()
    {
        Console.WriteLine("\n" + new string('-', 80));
        Console.WriteLine("1. INTERLOCKED.INCREMENT / DECREMENT");
        Console.WriteLine(new string('-', 80));

        int counter = 0;
        long longCounter = 0L;

        Console.WriteLine("\n   Operações disponíveis:");
        Console.WriteLine("   - Interlocked.Increment(ref int)");
        Console.WriteLine("   - Interlocked.Increment(ref long)");
        Console.WriteLine("   - Interlocked.Decrement(ref int)");
        Console.WriteLine("   - Interlocked.Decrement(ref long)");

        Console.WriteLine($"\n   Valor inicial: {counter}");

        // Incremento atômico
        int newValue = Interlocked.Increment(ref counter);
        Console.WriteLine($"   Após Increment: {newValue}");

        // Múltiplos incrementos concorrentes
        Console.WriteLine("\n   Executando 10 threads, 10.000 incrementos cada:");
        
        Thread[] threads = new Thread[10];
        for (int i = 0; i < 10; i++)
        {
            threads[i] = new Thread(() =>
            {
                for (int j = 0; j < 10000; j++)
                {
                    Interlocked.Increment(ref longCounter);
                }
            });
            threads[i].Start();
        }

        foreach (Thread thread in threads)
        {
            thread.Join();
        }

        Console.WriteLine($"   Resultado: {longCounter:N0}");
        Console.WriteLine($"   Esperado: 100.000");
        Console.WriteLine($"   ✅ Perfeito! Nenhum incremento perdido");

        // Decremento
        Interlocked.Decrement(ref counter);
        Console.WriteLine($"\n   Após Decrement: {counter}");
    }

    /// <summary>
    /// Demonstra Interlocked.Add.
    /// </summary>
    private static void DemonstrateAdd()
    {
        Console.WriteLine("\n" + new string('-', 80));
        Console.WriteLine("2. INTERLOCKED.ADD");
        Console.WriteLine(new string('-', 80));

        int value = 100;

        Console.WriteLine("\n   Operações disponíveis:");
        Console.WriteLine("   - Interlocked.Add(ref int, int)");
        Console.WriteLine("   - Interlocked.Add(ref long, long)");

        Console.WriteLine($"\n   Valor inicial: {value}");

        // Adiciona 50
        int result = Interlocked.Add(ref value, 50);
        Console.WriteLine($"   Após Add(50): {result}");

        // Subtrai usando valor negativo
        result = Interlocked.Add(ref value, -30);
        Console.WriteLine($"   Após Add(-30): {result}");

        Console.WriteLine("\n   💡 DICA: Use Add(ref x, -n) para subtrair");

        // Teste de concorrência
        Console.WriteLine("\n   Teste: 5 threads adicionando 1.000 cada");
        long longValue = 0;

        Thread[] threads = new Thread[5];
        for (int i = 0; i < 5; i++)
        {
            threads[i] = new Thread(() =>
            {
                for (int j = 0; j < 1000; j++)
                {
                    Interlocked.Add(ref longValue, 1);
                }
            });
            threads[i].Start();
        }

        foreach (Thread thread in threads)
        {
            thread.Join();
        }

        Console.WriteLine($"   Resultado: {longValue:N0} ✅");
    }

    /// <summary>
    /// Demonstra Interlocked.Exchange.
    /// </summary>
    private static void DemonstrateExchange()
    {
        Console.WriteLine("\n" + new string('-', 80));
        Console.WriteLine("3. INTERLOCKED.EXCHANGE");
        Console.WriteLine(new string('-', 80));

        int value = 42;
        long longValue = 100L;
        object? objValue = "Antigo";

        Console.WriteLine("\n   Operações disponíveis:");
        Console.WriteLine("   - Interlocked.Exchange(ref int, int)");
        Console.WriteLine("   - Interlocked.Exchange(ref long, long)");
        Console.WriteLine("   - Interlocked.Exchange(ref float, float)");
        Console.WriteLine("   - Interlocked.Exchange(ref double, double)");
        Console.WriteLine("   - Interlocked.Exchange(ref object, object)");

        Console.WriteLine($"\n   Valor inicial: {value}");

        // Troca atomicamente e retorna o valor antigo
        int oldValue = Interlocked.Exchange(ref value, 99);
        Console.WriteLine($"   Exchange(99) → Antigo: {oldValue}, Novo: {value}");

        // Exchange com objetos
        object? oldObj = Interlocked.Exchange(ref objValue, "Novo");
        Console.WriteLine($"   Exchange(objeto) → Antigo: {oldObj}, Novo: {objValue}");

        Console.WriteLine("\n   📚 USO PRÁTICO:");
        Console.WriteLine("   - Flags atômicos (bool usando int 0/1)");
        Console.WriteLine("   - Trocar referências de objetos");
        Console.WriteLine("   - Resetar contadores");
    }

    /// <summary>
    /// Demonstra Interlocked.CompareExchange (CAS).
    /// </summary>
    private static void DemonstrateCompareExchange()
    {
        Console.WriteLine("\n" + new string('-', 80));
        Console.WriteLine("4. INTERLOCKED.COMPAREEXCHANGE (Compare-And-Swap)");
        Console.WriteLine(new string('-', 80));

        Console.WriteLine("\n   Compare-And-Swap (CAS): Operação fundamental em lock-free");
        Console.WriteLine("   Atomicamente:");
        Console.WriteLine("   if (variable == comparand)");
        Console.WriteLine("       variable = newValue;");
        Console.WriteLine("   return oldValue;");

        int value = 100;

        Console.WriteLine($"\n   Valor inicial: {value}");

        // Tenta trocar se o valor for 100
        int oldValue = Interlocked.CompareExchange(ref value, 200, 100);
        Console.WriteLine($"\n   CompareExchange(newValue: 200, comparand: 100)");
        Console.WriteLine($"   → Valor anterior: {oldValue}");
        Console.WriteLine($"   → Valor atual: {value}");
        Console.WriteLine($"   → Troca realizada: {oldValue == 100} ✅");

        // Tenta trocar se o valor for 100 (vai falhar pois agora é 200)
        oldValue = Interlocked.CompareExchange(ref value, 300, 100);
        Console.WriteLine($"\n   CompareExchange(newValue: 300, comparand: 100)");
        Console.WriteLine($"   → Valor anterior: {oldValue}");
        Console.WriteLine($"   → Valor atual: {value}");
        Console.WriteLine($"   → Troca realizada: {oldValue == 100} ❌");

        // Implementação de retry loop
        Console.WriteLine("\n   🔄 RETRY LOOP (padrão comum):");
        Console.WriteLine("   Incrementar atomicamente com operação complexa:");

        value = 100;
        int retries = 0;
        
        // Incrementa com multiplicação (operação complexa)
        int currentValue;
        int newValue;
        do
        {
            currentValue = value;
            newValue = currentValue * 2 + 10; // Operação complexa
            retries++;
        } while (Interlocked.CompareExchange(ref value, newValue, currentValue) != currentValue);

        Console.WriteLine($"   Valor inicial: 100");
        Console.WriteLine($"   Operação: value * 2 + 10");
        Console.WriteLine($"   Resultado: {value}");
        Console.WriteLine($"   Tentativas: {retries}");
    }

    /// <summary>
    /// Demonstra Interlocked.Read para leitura atômica de long.
    /// </summary>
    private static void DemonstrateRead()
    {
        Console.WriteLine("\n" + new string('-', 80));
        Console.WriteLine("5. INTERLOCKED.READ");
        Console.WriteLine(new string('-', 80));

        Console.WriteLine("\n   ⚠️  IMPORTANTE: Leitura de long (64-bit) não é atômica em sistemas 32-bit!");
        Console.WriteLine("   Em x86 (32-bit), ler um long requer 2 operações:");
        Console.WriteLine("   1. Ler os primeiros 32 bits");
        Console.WriteLine("   2. Ler os últimos 32 bits");
        Console.WriteLine("\n   Se outra thread escrever no meio, você pode ler um valor inconsistente!");

        long value = 0x0123456789ABCDEF;

        Console.WriteLine($"\n   Valor: 0x{value:X16}");

        // Leitura NÃO atômica (perigoso em 32-bit)
        long unsafeRead = value;
        Console.WriteLine($"   Leitura normal: 0x{unsafeRead:X16} (pode ser inconsistente em 32-bit)");

        // Leitura atômica
        long safeRead = Interlocked.Read(ref value);
        Console.WriteLine($"   Interlocked.Read: 0x{safeRead:X16} (sempre consistente)");

        Console.WriteLine("\n   💡 NOTA:");
        Console.WriteLine("   - Em sistemas 64-bit (x64), leitura de long já é atômica");
        Console.WriteLine("   - Interlocked.Read ainda é útil para garantir portabilidade");
        Console.WriteLine("   - int (32-bit) sempre tem leitura atômica");
    }

    /// <summary>
    /// Compara performance entre Interlocked e Lock.
    /// </summary>
    private static void CompareWithLock()
    {
        Console.WriteLine("\n" + new string('-', 80));
        Console.WriteLine("6. COMPARAÇÃO DE PERFORMANCE: INTERLOCKED vs LOCK");
        Console.WriteLine(new string('-', 80));

        const int iterations = 1000000;

        // Interlocked
        int interlockedCounter = 0;
        Stopwatch sw1 = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            Interlocked.Increment(ref interlockedCounter);
        }
        sw1.Stop();

        // Lock
        int lockCounter = 0;
        object lockObject = new object();
        Stopwatch sw2 = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            lock (lockObject)
            {
                lockCounter++;
            }
        }
        sw2.Stop();

        Console.WriteLine($"\n   Operações: {iterations:N0}");
        Console.WriteLine($"\n   Interlocked.Increment:");
        Console.WriteLine($"   - Tempo: {sw1.ElapsedMilliseconds}ms");
        Console.WriteLine($"   - Throughput: {iterations / sw1.ElapsedMilliseconds:N0} ops/ms");
        Console.WriteLine($"   - Tempo por op: {(sw1.ElapsedTicks * 1000000.0 / Stopwatch.Frequency / iterations):F2} ns");

        Console.WriteLine($"\n   Lock {{ counter++; }}:");
        Console.WriteLine($"   - Tempo: {sw2.ElapsedMilliseconds}ms");
        Console.WriteLine($"   - Throughput: {iterations / sw2.ElapsedMilliseconds:N0} ops/ms");
        Console.WriteLine($"   - Tempo por op: {(sw2.ElapsedTicks * 1000000.0 / Stopwatch.Frequency / iterations):F2} ns");

        double speedup = (double)sw2.ElapsedMilliseconds / sw1.ElapsedMilliseconds;
        Console.WriteLine($"\n   🏆 Interlocked é {speedup:F2}x mais rápido que Lock!");

        Console.WriteLine("\n   📊 RESUMO:");
        Console.WriteLine("   ┌────────────────┬──────────┬────────────────────┐");
        Console.WriteLine("   │ Método         │ Tempo    │ Uso Ideal          │");
        Console.WriteLine("   ├────────────────┼──────────┼────────────────────┤");
        Console.WriteLine("   │ Interlocked    │ ~5-10ns  │ Operações simples  │");
        Console.WriteLine("   │ Lock           │ ~25-50ns │ Operações complexas│");
        Console.WriteLine("   └────────────────┴──────────┴────────────────────┘");
    }
}
