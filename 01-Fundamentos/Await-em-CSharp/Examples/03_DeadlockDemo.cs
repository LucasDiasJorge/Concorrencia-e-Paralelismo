namespace AsyncAwaitInternals.Examples;

/// <summary>
/// Demonstra cenários de deadlock e como evitá-los
/// </summary>
public static class DeadlockDemo
{
    public static async Task RunAsync()
    {
        Console.WriteLine("\n=== DEMONSTRAÇÃO: DEADLOCK ===\n");

        Console.WriteLine("1. CENÁRIO SEM DEADLOCK (uso correto de await):");
        await CorrectUsageAsync();
        Console.WriteLine("   ✅ Executado com sucesso\n");

        Console.WriteLine("2. CENÁRIO COM DEADLOCK SIMULADO:");
        Console.WriteLine("   (em WinForms/WPF isso causaria deadlock real)");
        Console.WriteLine("   (em Console/ASP.NET Core não há SynchronizationContext)");
        
        // Simula o cenário de deadlock
        SimulateWinFormsDeadlock();

        Console.WriteLine("\n3. SOLUÇÃO 1: ConfigureAwait(false):");
        await SolutionWithConfigureAwait();
        Console.WriteLine("   ✅ ConfigureAwait(false) evita captura de contexto\n");

        Console.WriteLine("4. SOLUÇÃO 2: Totalmente assíncrono:");
        await SolutionFullyAsync();
        Console.WriteLine("   ✅ Nunca bloquear com .Result ou .Wait()\n");
    }

    private static async Task CorrectUsageAsync()
    {
        Console.WriteLine($"   [Thread {Environment.CurrentManagedThreadId}] Antes do await");
        string result = await LongRunningOperationAsync();
        Console.WriteLine($"   [Thread {Environment.CurrentManagedThreadId}] Depois do await: {result}");
    }

    private static void SimulateWinFormsDeadlock()
    {
        Console.WriteLine("   Código que causaria deadlock em WinForms:");
        Console.WriteLine("   ");
        Console.WriteLine("   private void Button_Click(object sender, EventArgs e)");
        Console.WriteLine("   {");
        Console.WriteLine("       // UI Thread bloqueia aqui esperando task");
        Console.WriteLine("       string result = GetDataAsync().Result; ❌");
        Console.WriteLine("   }");
        Console.WriteLine("   ");
        Console.WriteLine("   private async Task<string> GetDataAsync()");
        Console.WriteLine("   {");
        Console.WriteLine("       await Task.Delay(1000);");
        Console.WriteLine("       // Tenta voltar para UI thread via SyncContext");
        Console.WriteLine("       // MAS UI thread está bloqueada esperando!");
        Console.WriteLine("       return \"data\"; // 💀 DEADLOCK");
        Console.WriteLine("   }");
        Console.WriteLine();
        Console.WriteLine("   Por quê?");
        Console.WriteLine("   - UI thread chama .Result (BLOQUEIA)");
        Console.WriteLine("   - Task completa em ThreadPool");
        Console.WriteLine("   - Continuação tenta voltar para UI thread (SyncContext)");
        Console.WriteLine("   - UI thread está bloqueada esperando .Result");
        Console.WriteLine("   - 💀 DEADLOCK perpétuo");
    }

    private static async Task SolutionWithConfigureAwait()
    {
        Console.WriteLine($"   [Thread {Environment.CurrentManagedThreadId}] Chamando operação...");
        
        // ConfigureAwait(false) não tenta capturar SynchronizationContext
        string result = await LongRunningOperationAsync().ConfigureAwait(false);
        
        Console.WriteLine($"   [Thread {Environment.CurrentManagedThreadId}] Resultado: {result}");
        Console.WriteLine("   Continuação executou no ThreadPool, não voltou para contexto original");
    }

    private static async Task SolutionFullyAsync()
    {
        Console.WriteLine($"   [Thread {Environment.CurrentManagedThreadId}] Início");
        
        // Nunca usar .Result ou .Wait(), sempre await
        string result = await LongRunningOperationAsync();
        
        Console.WriteLine($"   [Thread {Environment.CurrentManagedThreadId}] Fim: {result}");
        Console.WriteLine("   Sem bloqueio de thread = sem deadlock");
    }

    private static async Task<string> LongRunningOperationAsync()
    {
        await Task.Delay(100);
        return "Dados carregados";
    }
}
