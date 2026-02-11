using System.Diagnostics;

namespace AsyncAwaitInternals.Examples;

/// <summary>
/// Demonstra as diferenças entre await, .Result, .Wait() e GetAwaiter().GetResult()
/// </summary>
public static class AwaitVsBlockingDemo
{
    public static async Task RunAsync()
    {
        Console.WriteLine("\n=== DEMONSTRAÇÃO: await vs .Result vs .Wait() ===\n");

        Console.WriteLine("1. USANDO await (✅ CORRETO):");
        await DemoAwait();

        Console.WriteLine("\n2. USANDO .Result (❌ BLOQUEANTE):");
        DemoResult();

        Console.WriteLine("\n3. USANDO .Wait() (❌ BLOQUEANTE):");
        DemoWait();

        Console.WriteLine("\n4. USANDO GetAwaiter().GetResult() (⚠️ BLOQUEANTE):");
        DemoGetAwaiterGetResult();

        Console.WriteLine("\n5. TRATAMENTO DE EXCEÇÕES:");
        await DemoExceptionHandling();

        Console.WriteLine("\n📊 RESUMO:");
        Console.WriteLine("   await:                    Não bloqueia | Exceção direta | Usa SyncContext");
        Console.WriteLine("   .Result:                  BLOQUEIA     | AggregateException | Usa SyncContext | Deadlock risk");
        Console.WriteLine("   .Wait():                  BLOQUEIA     | AggregateException | Usa SyncContext | Deadlock risk");
        Console.WriteLine("   GetAwaiter().GetResult(): BLOQUEIA     | Exceção direta | Sem SyncContext");
    }

    private static async Task DemoAwait()
    {
        Stopwatch sw = Stopwatch.StartNew();
        int threadBefore = Environment.CurrentManagedThreadId;
        
        Console.WriteLine($"   [Thread {threadBefore}] Antes do await");
        
        string result = await GetDataAsync();
        
        int threadAfter = Environment.CurrentManagedThreadId;
        sw.Stop();
        
        Console.WriteLine($"   [Thread {threadAfter}] Depois do await");
        Console.WriteLine($"   Resultado: {result}");
        Console.WriteLine($"   Thread mudou: {threadBefore != threadAfter}");
        Console.WriteLine($"   Thread foi liberada durante I/O: ✅");
        Console.WriteLine($"   Tempo: {sw.ElapsedMilliseconds}ms");
    }

    private static void DemoResult()
    {
        Stopwatch sw = Stopwatch.StartNew();
        int thread = Environment.CurrentManagedThreadId;
        
        Console.WriteLine($"   [Thread {thread}] Antes do .Result");
        
        // BLOQUEIA a thread até task completar
        string result = GetDataAsync().Result;
        
        sw.Stop();
        
        Console.WriteLine($"   [Thread {thread}] Depois do .Result");
        Console.WriteLine($"   Resultado: {result}");
        Console.WriteLine($"   Thread ficou BLOQUEADA durante I/O: ❌");
        Console.WriteLine($"   Tempo: {sw.ElapsedMilliseconds}ms");
    }

    private static void DemoWait()
    {
        Stopwatch sw = Stopwatch.StartNew();
        int thread = Environment.CurrentManagedThreadId;
        
        Console.WriteLine($"   [Thread {thread}] Antes do .Wait()");
        
        Task<string> task = GetDataAsync();
        task.Wait(); // BLOQUEIA a thread até task completar
        string result = task.Result;
        
        sw.Stop();
        
        Console.WriteLine($"   [Thread {thread}] Depois do .Wait()");
        Console.WriteLine($"   Resultado: {result}");
        Console.WriteLine($"   Thread ficou BLOQUEADA durante I/O: ❌");
        Console.WriteLine($"   Tempo: {sw.ElapsedMilliseconds}ms");
    }

    private static void DemoGetAwaiterGetResult()
    {
        Stopwatch sw = Stopwatch.StartNew();
        int thread = Environment.CurrentManagedThreadId;
        
        Console.WriteLine($"   [Thread {thread}] Antes do GetAwaiter().GetResult()");
        
        // BLOQUEIA, mas não captura SynchronizationContext
        string result = GetDataAsync().GetAwaiter().GetResult();
        
        sw.Stop();
        
        Console.WriteLine($"   [Thread {thread}] Depois do GetAwaiter().GetResult()");
        Console.WriteLine($"   Resultado: {result}");
        Console.WriteLine($"   Thread ficou BLOQUEADA durante I/O: ❌");
        Console.WriteLine($"   Mas menos risco de deadlock (sem SyncContext)");
        Console.WriteLine($"   Tempo: {sw.ElapsedMilliseconds}ms");
    }

    private static async Task DemoExceptionHandling()
    {
        Console.WriteLine("\n   a) Exceção com await:");
        try
        {
            await ThrowsExceptionAsync();
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"      ✅ Capturou exceção direta: {ex.GetType().Name}");
            Console.WriteLine($"      Mensagem: {ex.Message}");
        }

        Console.WriteLine("\n   b) Exceção com .Result:");
        try
        {
            string result = ThrowsExceptionAsync().Result;
        }
        catch (AggregateException ex)
        {
            Console.WriteLine($"      ⚠️ Capturou AggregateException (wrapper)");
            Console.WriteLine($"      Exception interna: {ex.InnerException?.GetType().Name}");
            Console.WriteLine($"      Mensagem: {ex.InnerException?.Message}");
        }

        Console.WriteLine("\n   c) Exceção com GetAwaiter().GetResult():");
        try
        {
            string result = ThrowsExceptionAsync().GetAwaiter().GetResult();
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"      ✅ Capturou exceção direta: {ex.GetType().Name}");
            Console.WriteLine($"      Mensagem: {ex.Message}");
        }
    }

    private static async Task<string> GetDataAsync()
    {
        await Task.Delay(100);
        return "Dados carregados";
    }

    private static async Task<string> ThrowsExceptionAsync()
    {
        await Task.Delay(10);
        throw new InvalidOperationException("Erro simulado na operação");
    }
}
