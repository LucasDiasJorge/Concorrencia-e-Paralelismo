using System;

namespace AtomicSequenceSafe;

public static class Program
{
    public static int Main(string[] args)
    {
        int threadsCountExplicit = 0;
        int incrementsPerThreadExplicit = 0;

        if (args.Length > 0 && int.TryParse(args[0], out int parsedThreads))
        {
            threadsCountExplicit = Math.Max(parsedThreads, 1);
        }
        else
        {
            threadsCountExplicit = Environment.ProcessorCount;
        }

        if (args.Length > 1 && int.TryParse(args[1], out int parsedIncs))
        {
            incrementsPerThreadExplicit = Math.Max(parsedIncs, 1);
        }
        else
        {
            incrementsPerThreadExplicit = 100_000;
        }

        long totalExpectedExplicit = (long)threadsCountExplicit * incrementsPerThreadExplicit;

        Console.WriteLine("--- Incremento thread-safe (SOLID) ---");
        Console.WriteLine($"Threads: {threadsCountExplicit}");
        Console.WriteLine($"Incrementos por thread: {incrementsPerThreadExplicit:N0}");
        Console.WriteLine($"Incrementos totais esperados: {totalExpectedExplicit:N0}\n");

        ICounter counter = new AtomicCounter();
        CounterRunner runner = new CounterRunner(counter, threadsCountExplicit, incrementsPerThreadExplicit);

        TimeSpan elapsed = runner.Run();

        Console.WriteLine($"Valor final do contador: {counter.Value:N0}");
        Console.WriteLine($"Total esperado: {totalExpectedExplicit:N0}");
        Console.WriteLine($"Execução levou {elapsed.TotalMilliseconds:N0} ms");

        if (counter.Value == totalExpectedExplicit)
        {
            Console.WriteLine("✅ Incremento concluído sem race condition.");
            return 0;
        }
        else
        {
            Console.WriteLine("⚠️ Resultado inesperado! Alguma race condition ocorreu.");
            return 2;
        }
    }
}
