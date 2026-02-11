namespace AsyncAwaitInternals.Examples;

/// <summary>
/// Demonstra como continuações são registradas e disparadas
/// </summary>
public static class ContinuationDemo
{
    public static async Task RunAsync()
    {
        Console.WriteLine("\n=== DEMONSTRAÇÃO: CONTINUAÇÕES ===\n");

        Console.WriteLine("1. FLUXO DE CONTINUAÇÃO:");
        await DemonstrateContinuation();

        Console.WriteLine("\n2. MÚLTIPLAS CONTINUAÇÕES:");
        await MultipleContinuations();

        Console.WriteLine("\n3. CONTINUAÇÃO COM Task.ContinueWith:");
        await ExplicitContinuation();
    }

    private static async Task DemonstrateContinuation()
    {
        Console.WriteLine($"   [Thread {Environment.CurrentManagedThreadId}] Início do método");

        Task<string> task = SlowOperationAsync();
        Console.WriteLine($"   [Thread {Environment.CurrentManagedThreadId}] Task criada, Status: {task.Status}");

        // O await registra a continuação
        Console.WriteLine("   → Registrando continuação com AwaitUnsafeOnCompleted");
        Console.WriteLine("   → Thread será liberada agora...");

        string result = await task;

        Console.WriteLine($"   [Thread {Environment.CurrentManagedThreadId}] Continuação executada!");
        Console.WriteLine($"   Resultado: {result}");
        Console.WriteLine($"   Task.Status: {task.Status}");
    }

    private static async Task MultipleContinuations()
    {
        Task<int> sharedTask = ComputeAsync();

        // Múltiplas continuações da mesma task
        Task continuation1 = sharedTask.ContinueWith(t => 
        {
            Console.WriteLine($"   [Thread {Environment.CurrentManagedThreadId}] Continuação 1: resultado = {t.Result}");
        });

        Task continuation2 = sharedTask.ContinueWith(t => 
        {
            Console.WriteLine($"   [Thread {Environment.CurrentManagedThreadId}] Continuação 2: resultado = {t.Result}");
        });

        Task continuation3 = ProcessResultAsync(sharedTask);

        await Task.WhenAll(continuation1, continuation2, continuation3);
        
        Console.WriteLine("   ✅ Todas as continuações executadas");
    }

    private static async Task ProcessResultAsync(Task<int> task)
    {
        int result = await task;
        Console.WriteLine($"   [Thread {Environment.CurrentManagedThreadId}] Continuação async/await: resultado = {result}");
    }

    private static async Task ExplicitContinuation()
    {
        Console.WriteLine("   Criando task e continuações explícitas...");

        Task<string> task = FetchDataAsync();

        // ContinueWith é a API de baixo nível
        Task continuation = task.ContinueWith(t => 
        {
            Console.WriteLine($"   [Thread {Environment.CurrentManagedThreadId}] ContinueWith executado");
            Console.WriteLine($"   Task estava no estado: {t.Status}");
            Console.WriteLine($"   Resultado: {t.Result}");
        }, TaskScheduler.Default);

        await continuation;

        Console.WriteLine("\n   Nota: async/await usa ContinueWith internamente,");
        Console.WriteLine("   mas de forma mais eficiente via TaskAwaiter");
    }

    private static async Task<string> SlowOperationAsync()
    {
        Console.WriteLine($"   [Thread {Environment.CurrentManagedThreadId}] → Iniciando operação lenta");
        await Task.Delay(100);
        Console.WriteLine($"   [Thread {Environment.CurrentManagedThreadId}] ← Operação completa");
        return "Resultado da operação";
    }

    private static async Task<int> ComputeAsync()
    {
        Console.WriteLine($"   [Thread {Environment.CurrentManagedThreadId}] Computando...");
        await Task.Delay(50);
        return 42;
    }

    private static async Task<string> FetchDataAsync()
    {
        await Task.Delay(80);
        return "Dados obtidos";
    }
}
