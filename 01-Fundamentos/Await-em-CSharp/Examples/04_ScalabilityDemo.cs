using System.Diagnostics;

namespace AsyncAwaitInternals.Examples;

/// <summary>
/// Demonstra o impacto de async/await na escalabilidade sob carga
/// </summary>
public static class ScalabilityDemo
{
    public static async Task RunAsync()
    {
        Console.WriteLine("\n=== DEMONSTRAÇÃO: ESCALABILIDADE ===\n");

        int concurrentRequests = 50;
        int delayMs = 100;

        Console.WriteLine($"Simulando {concurrentRequests} requisições concorrentes");
        Console.WriteLine($"Cada requisição demora {delayMs}ms de I/O\n");

        // Versão Bloqueante
        Console.WriteLine("1. ABORDAGEM BLOQUEANTE:");
        await RunBlockingSimulation(concurrentRequests, delayMs);

        Console.WriteLine();

        // Versão Assíncrona
        Console.WriteLine("2. ABORDAGEM ASSÍNCRONA:");
        await RunAsyncSimulation(concurrentRequests, delayMs);

        Console.WriteLine("\n📊 ANÁLISE:");
        Console.WriteLine("   Bloqueante: Usa 1 thread por requisição (50 threads)");
        Console.WriteLine("   Assíncrono: Usa ~5-10 threads para todas requisições");
        Console.WriteLine("   Ganho de eficiência: 5-10x menos recursos");
    }

    private static async Task RunBlockingSimulation(int count, int delayMs)
    {
        Stopwatch sw = Stopwatch.StartNew();
        HashSet<int> threadsUsed = new HashSet<int>();

        Task[] tasks = new Task[count];

        for (int i = 0; i < count; i++)
        {
            int requestId = i;
            tasks[i] = Task.Run(() =>
            {
                threadsUsed.Add(Environment.CurrentManagedThreadId);
                
                // Simula I/O bloqueante
                Thread.Sleep(delayMs);
            });
        }

        await Task.WhenAll(tasks);
        sw.Stop();

        Console.WriteLine($"   Tempo total: {sw.ElapsedMilliseconds}ms");
        Console.WriteLine($"   Threads únicas usadas: {threadsUsed.Count}");
        Console.WriteLine($"   Threads bloqueadas durante I/O: {threadsUsed.Count}");
        Console.WriteLine($"   ❌ Alto uso de recursos");
    }

    private static async Task RunAsyncSimulation(int count, int delayMs)
    {
        Stopwatch sw = Stopwatch.StartNew();
        HashSet<int> threadsUsed = new HashSet<int>();
        object lockObj = new object();

        Task[] tasks = new Task[count];

        for (int i = 0; i < count; i++)
        {
            int requestId = i;
            tasks[i] = Task.Run(async () =>
            {
                lock (lockObj)
                {
                    threadsUsed.Add(Environment.CurrentManagedThreadId);
                }

                // Simula I/O assíncrono (thread é liberada)
                await Task.Delay(delayMs);

                lock (lockObj)
                {
                    threadsUsed.Add(Environment.CurrentManagedThreadId);
                }
            });
        }

        await Task.WhenAll(tasks);
        sw.Stop();

        Console.WriteLine($"   Tempo total: {sw.ElapsedMilliseconds}ms");
        Console.WriteLine($"   Threads únicas usadas: {threadsUsed.Count}");
        Console.WriteLine($"   Threads bloqueadas durante I/O: 0");
        Console.WriteLine($"   ✅ Uso eficiente de recursos");
    }
}
