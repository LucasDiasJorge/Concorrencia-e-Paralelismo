using System.Diagnostics;

namespace AsyncAwaitInternals.Examples;

/// <summary>
/// Demonstra a diferença entre task já completada (sync) e task pendente (async)
/// </summary>
public static class SyncCompletionDemo
{
    public static async Task RunAsync()
    {
        Console.WriteLine("\n=== DEMONSTRAÇÃO: COMPLETAÇÃO SÍNCRONA vs ASSÍNCRONA ===\n");

        Console.WriteLine("1. TASK JÁ COMPLETADA (Fast Path):");
        await DemoSyncCompletion();

        Console.WriteLine("\n2. TASK PENDENTE (Slow Path):");
        await DemoAsyncCompletion();

        Console.WriteLine("\n3. IMPACTO NO DESEMPENHO:");
        await BenchmarkCompletion();
    }

    private static async Task DemoSyncCompletion()
    {
        Console.WriteLine("   Criando task já completada...");
        Task<int> completedTask = Task.FromResult(42);
        
        Console.WriteLine($"   Task.IsCompleted antes do await: {completedTask.IsCompleted}");
        Console.WriteLine($"   Task.Status: {completedTask.Status}");

        int threadBefore = Environment.CurrentManagedThreadId;
        Console.WriteLine($"   [Thread {threadBefore}] Antes do await");

        // awaiter.IsCompleted retorna true, execução continua sincronamente
        int result = await completedTask;

        int threadAfter = Environment.CurrentManagedThreadId;
        Console.WriteLine($"   [Thread {threadAfter}] Depois do await");
        Console.WriteLine($"   Resultado: {result}");
        Console.WriteLine($"   Thread não mudou: {threadBefore == threadAfter} ✅");
        Console.WriteLine("   → FAST PATH: Sem suspensão, sem continuação, sem overhead");
    }

    private static async Task DemoAsyncCompletion()
    {
        Console.WriteLine("   Criando task pendente...");
        Task<int> pendingTask = ComputeAsync();
        
        await Task.Delay(10); // Garantir que ainda está pendente
        
        Console.WriteLine($"   Task.IsCompleted antes do await: {pendingTask.IsCompleted}");
        Console.WriteLine($"   Task.Status: {pendingTask.Status}");

        int threadBefore = Environment.CurrentManagedThreadId;
        Console.WriteLine($"   [Thread {threadBefore}] Antes do await");

        // awaiter.IsCompleted retorna false, método suspende
        int result = await pendingTask;

        int threadAfter = Environment.CurrentManagedThreadId;
        Console.WriteLine($"   [Thread {threadAfter}] Depois do await");
        Console.WriteLine($"   Resultado: {result}");
        Console.WriteLine($"   Thread provavelmente mudou: {threadBefore != threadAfter}");
        Console.WriteLine("   → SLOW PATH: Suspensão, registro de continuação, overhead");
    }

    private static async Task BenchmarkCompletion()
    {
        const int iterations = 100000;

        // Benchmark: Task já completada
        Stopwatch sw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            await Task.FromResult(i);
        }
        sw.Stop();
        long syncTime = sw.ElapsedMilliseconds;

        Console.WriteLine($"   {iterations:N0} awaits em tasks já completadas: {syncTime}ms");

        // Benchmark: Task com delay mínimo
        sw.Restart();
        for (int i = 0; i < iterations; i++)
        {
            await Task.Yield(); // Força completação assíncrona
        }
        sw.Stop();
        long asyncTime = sw.ElapsedMilliseconds;

        Console.WriteLine($"   {iterations:N0} awaits em tasks pendentes: {asyncTime}ms");
        Console.WriteLine($"   Diferença: {asyncTime / Math.Max(syncTime, 1)}x mais lento (overhead de suspensão)");

        Console.WriteLine("\n   💡 Conclusão:");
        Console.WriteLine("   - IsCompleted permite otimização para tasks já completadas");
        Console.WriteLine("   - Fast path evita overhead de suspensão/continuação");
        Console.WriteLine("   - Por isso Task.FromResult e ValueTask são importantes para hot paths");
    }

    private static async Task<int> ComputeAsync()
    {
        await Task.Delay(50);
        return 42;
    }
}
