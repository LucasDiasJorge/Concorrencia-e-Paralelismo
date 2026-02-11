using System.Diagnostics;

namespace AsyncAwaitInternals.Examples;

/// <summary>
/// Demonstra a diferença entre código síncrono bloqueante e assíncrono não-bloqueante
/// </summary>
public static class SyncVsAsyncDemo
{
    public static async Task RunAsync()
    {
        Console.WriteLine("\n=== DEMONSTRAÇÃO: SYNC vs ASYNC ===\n");

        Console.WriteLine("Cenário: Buscar dados de 3 APIs diferentes\n");

        // Versão Síncrona
        Console.WriteLine("1. VERSÃO SÍNCRONA (Bloqueante):");
        Stopwatch sw = Stopwatch.StartNew();
        FetchDataSync();
        sw.Stop();
        Console.WriteLine($"   Tempo total: {sw.ElapsedMilliseconds}ms");
        Console.WriteLine("   ❌ Thread ficou bloqueada durante todo o I/O\n");

        // Versão Assíncrona
        Console.WriteLine("2. VERSÃO ASSÍNCRONA (Não-bloqueante):");
        sw.Restart();
        await FetchDataAsync();
        sw.Stop();
        Console.WriteLine($"   Tempo total: {sw.ElapsedMilliseconds}ms");
        Console.WriteLine("   ✅ Thread foi liberada durante I/O\n");

        // Versão Assíncrona Paralela
        Console.WriteLine("3. VERSÃO ASSÍNCRONA PARALELA:");
        sw.Restart();
        await FetchDataParallelAsync();
        sw.Stop();
        Console.WriteLine($"   Tempo total: {sw.ElapsedMilliseconds}ms");
        Console.WriteLine("   ✅ Todas as requisições simultâneas\n");
    }

    private static void FetchDataSync()
    {
        Console.WriteLine($"   [Thread {Environment.CurrentManagedThreadId}] Iniciando API 1...");
        Thread.Sleep(100); // Simula I/O bloqueante
        Console.WriteLine($"   [Thread {Environment.CurrentManagedThreadId}] API 1 completa");

        Console.WriteLine($"   [Thread {Environment.CurrentManagedThreadId}] Iniciando API 2...");
        Thread.Sleep(100); // Simula I/O bloqueante
        Console.WriteLine($"   [Thread {Environment.CurrentManagedThreadId}] API 2 completa");

        Console.WriteLine($"   [Thread {Environment.CurrentManagedThreadId}] Iniciando API 3...");
        Thread.Sleep(100); // Simula I/O bloqueante
        Console.WriteLine($"   [Thread {Environment.CurrentManagedThreadId}] API 3 completa");
    }

    private static async Task FetchDataAsync()
    {
        Console.WriteLine($"   [Thread {Environment.CurrentManagedThreadId}] Iniciando API 1...");
        await Task.Delay(100); // Simula I/O assíncrono
        Console.WriteLine($"   [Thread {Environment.CurrentManagedThreadId}] API 1 completa");

        Console.WriteLine($"   [Thread {Environment.CurrentManagedThreadId}] Iniciando API 2...");
        await Task.Delay(100); // Simula I/O assíncrono
        Console.WriteLine($"   [Thread {Environment.CurrentManagedThreadId}] API 2 completa");

        Console.WriteLine($"   [Thread {Environment.CurrentManagedThreadId}] Iniciando API 3...");
        await Task.Delay(100); // Simula I/O assíncrono
        Console.WriteLine($"   [Thread {Environment.CurrentManagedThreadId}] API 3 completa");
    }

    private static async Task FetchDataParallelAsync()
    {
        Console.WriteLine($"   [Thread {Environment.CurrentManagedThreadId}] Iniciando todas as APIs...");

        Task task1 = FetchApi1Async();
        Task task2 = FetchApi2Async();
        Task task3 = FetchApi3Async();

        await Task.WhenAll(task1, task2, task3);

        Console.WriteLine($"   [Thread {Environment.CurrentManagedThreadId}] Todas completas");
    }

    private static async Task FetchApi1Async()
    {
        Console.WriteLine($"   [Thread {Environment.CurrentManagedThreadId}] → API 1 started");
        await Task.Delay(100);
        Console.WriteLine($"   [Thread {Environment.CurrentManagedThreadId}] ← API 1 done");
    }

    private static async Task FetchApi2Async()
    {
        Console.WriteLine($"   [Thread {Environment.CurrentManagedThreadId}] → API 2 started");
        await Task.Delay(100);
        Console.WriteLine($"   [Thread {Environment.CurrentManagedThreadId}] ← API 2 done");
    }

    private static async Task FetchApi3Async()
    {
        Console.WriteLine($"   [Thread {Environment.CurrentManagedThreadId}] → API 3 started");
        await Task.Delay(100);
        Console.WriteLine($"   [Thread {Environment.CurrentManagedThreadId}] ← API 3 done");
    }
}
