using AsyncAwaitInternals.Examples;

namespace AsyncAwaitInternals;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== ASYNC/AWAIT INTERNALS - DEMOS ===\n");

        while (true)
        {
            Console.WriteLine("\nEscolha um exemplo:");
            Console.WriteLine("1 - Máquina de Estados (State Machine)");
            Console.WriteLine("2 - Sync vs Async Comparison");
            Console.WriteLine("3 - Deadlock Example (WinForms simulation)");
            Console.WriteLine("4 - Escalabilidade e Thread Usage");
            Console.WriteLine("5 - Continuation Registration");
            Console.WriteLine("6 - await vs .Result vs .Wait()");
            Console.WriteLine("7 - Completação Síncrona vs Assíncrona");
            Console.WriteLine("0 - Sair");
            Console.Write("\nOpção: ");

            string? input = Console.ReadLine();

            try
            {
                switch (input)
                {
                    case "1":
                        await StateMachineDemo.RunAsync();
                        break;
                    case "2":
                        await SyncVsAsyncDemo.RunAsync();
                        break;
                    case "3":
                        await DeadlockDemo.RunAsync();
                        break;
                    case "4":
                        await ScalabilityDemo.RunAsync();
                        break;
                    case "5":
                        await ContinuationDemo.RunAsync();
                        break;
                    case "6":
                        await AwaitVsBlockingDemo.RunAsync();
                        break;
                    case "7":
                        await SyncCompletionDemo.RunAsync();
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Opção inválida!");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Erro: {ex.Message}");
            }

            Console.WriteLine("\n" + new string('─', 80));
        }
    }
}
