using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace AsyncAwaitInternals.Examples;

/// <summary>
/// Demonstra como o compilador transforma async/await em máquina de estados
/// </summary>
public static class StateMachineDemo
{
    public static async Task RunAsync()
    {
        Console.WriteLine("\n=== DEMONSTRAÇÃO: MÁQUINA DE ESTADOS ===\n");

        Console.WriteLine("1. Executando método async original:");
        Stopwatch sw = Stopwatch.StartNew();
        string result = await OriginalAsyncMethod();
        sw.Stop();
        Console.WriteLine($"   Resultado: {result}");
        Console.WriteLine($"   Tempo: {sw.ElapsedMilliseconds}ms\n");

        Console.WriteLine("2. Executando versão manual da máquina de estados:");
        sw.Restart();
        result = await ManualStateMachineMethod();
        sw.Stop();
        Console.WriteLine($"   Resultado: {result}");
        Console.WriteLine($"   Tempo: {sw.ElapsedMilliseconds}ms\n");

        Console.WriteLine("3. Inspecionando a máquina de estados:");
        await InspectStateMachine();
    }

    // Método async original que o compilador transforma
    private static async Task<string> OriginalAsyncMethod()
    {
        Console.WriteLine($"   [Thread {Environment.CurrentManagedThreadId}] Início");
        
        await Task.Delay(50);
        Console.WriteLine($"   [Thread {Environment.CurrentManagedThreadId}] Após primeiro await");
        
        await Task.Delay(50);
        Console.WriteLine($"   [Thread {Environment.CurrentManagedThreadId}] Após segundo await");
        
        return "Completo";
    }

    // Versão manual aproximada do que o compilador gera
    private static Task<string> ManualStateMachineMethod()
    {
        ManualStateMachine stateMachine = new ManualStateMachine();
        stateMachine.builder = AsyncTaskMethodBuilder<string>.Create();
        stateMachine.state = -1;
        stateMachine.builder.Start(ref stateMachine);
        return stateMachine.builder.Task;
    }

    // Estrutura que simula a máquina de estados gerada pelo compilador
    private struct ManualStateMachine : IAsyncStateMachine
    {
        public int state;
        public AsyncTaskMethodBuilder<string> builder;
        private TaskAwaiter awaiter1;
        private TaskAwaiter awaiter2;

        public void MoveNext()
        {
            int currentState = state;
            string result;

            try
            {
                TaskAwaiter currentAwaiter;

                switch (currentState)
                {
                    case 0:
                        // Retomando após primeiro await
                        currentAwaiter = awaiter1;
                        awaiter1 = default;
                        state = -1;
                        goto AfterFirstAwait;
                    case 1:
                        // Retomando após segundo await
                        currentAwaiter = awaiter2;
                        awaiter2 = default;
                        state = -1;
                        goto AfterSecondAwait;
                }

                // Estado inicial
                Console.WriteLine($"   [Thread {Environment.CurrentManagedThreadId}] Início (manual)");

                // Primeiro await
                currentAwaiter = Task.Delay(50).GetAwaiter();
                if (!currentAwaiter.IsCompleted)
                {
                    state = 0;
                    awaiter1 = currentAwaiter;
                    builder.AwaitUnsafeOnCompleted(ref currentAwaiter, ref this);
                    return; // SUSPENDE aqui
                }

                AfterFirstAwait:
                currentAwaiter.GetResult();
                Console.WriteLine($"   [Thread {Environment.CurrentManagedThreadId}] Após primeiro await (manual)");

                // Segundo await
                currentAwaiter = Task.Delay(50).GetAwaiter();
                if (!currentAwaiter.IsCompleted)
                {
                    state = 1;
                    awaiter2 = currentAwaiter;
                    builder.AwaitUnsafeOnCompleted(ref currentAwaiter, ref this);
                    return; // SUSPENDE aqui
                }

                AfterSecondAwait:
                currentAwaiter.GetResult();
                Console.WriteLine($"   [Thread {Environment.CurrentManagedThreadId}] Após segundo await (manual)");

                result = "Completo (manual)";
            }
            catch (Exception ex)
            {
                state = -2;
                builder.SetException(ex);
                return;
            }

            state = -2;
            builder.SetResult(result);
        }

        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
            builder.SetStateMachine(stateMachine);
        }
    }

    private static async Task InspectStateMachine()
    {
        Console.WriteLine("   Estado -1: Executando");
        int state = -1;
        Console.WriteLine($"   Estado atual: {state}");

        Task<int> task = SlowOperationAsync();
        await Task.Delay(10); // Garantir que task ainda está pendente
        
        Console.WriteLine($"   Task.IsCompleted: {task.IsCompleted}");
        Console.WriteLine($"   Task.Status: {task.Status}");
        Console.WriteLine("   Estado 0: Aguardando primeiro await");

        int result = await task;
        
        Console.WriteLine($"   Task.IsCompleted: {task.IsCompleted}");
        Console.WriteLine($"   Task.Status: {task.Status}");
        Console.WriteLine($"   Resultado: {result}");
        Console.WriteLine("   Estado -2: Completo");
    }

    private static async Task<int> SlowOperationAsync()
    {
        await Task.Delay(100);
        return 42;
    }
}
