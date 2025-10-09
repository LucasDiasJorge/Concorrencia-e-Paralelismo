using CounterDemo.Interfaces;
using System.Diagnostics;

namespace CounterDemo.Demo;

public class StockConcurrencyDemo
{
    private readonly IProductRepository _atomicRepository;
    private readonly IProductRepository _nonAtomicRepository;
    private readonly int _productId;
    private readonly int _initialStock;
    private readonly int _threadCount;
    private readonly int _incrementsPerThread;

    public StockConcurrencyDemo(
        IProductRepository atomicRepository,
        IProductRepository nonAtomicRepository,
        int productId = 1,
        int initialStock = 0,
        int threadCount = 10,
        int incrementsPerThread = 100)
    {
        _atomicRepository = atomicRepository;
        _nonAtomicRepository = nonAtomicRepository;
        _productId = productId;
        _initialStock = initialStock;
        _threadCount = threadCount;
        _incrementsPerThread = incrementsPerThread;
    }

    public async Task RunDemoAsync()
    {
        Console.WriteLine("==========================================================");
        Console.WriteLine("DEMONSTRAÇÃO: Incremento Atômico vs Não-Atômico");
        Console.WriteLine("==========================================================\n");

        Console.WriteLine($"Configuração do teste:");
        Console.WriteLine($"  - Número de threads: {_threadCount}");
        Console.WriteLine($"  - Incrementos por thread: {_incrementsPerThread}");
        Console.WriteLine($"  - Total de incrementos esperados: {_threadCount * _incrementsPerThread}");
        Console.WriteLine($"  - Estoque inicial: {_initialStock}\n");

        // Testa implementação atômica
        Console.WriteLine("----------------------------------------------------------");
        Console.WriteLine("Testando método ATÔMICO...");
        Console.WriteLine("----------------------------------------------------------");
        await _atomicRepository.ResetDemoAsync(_productId, _initialStock);
        var atomicResult = await RunTestAsync(_atomicRepository, "ATÔMICO");

        Console.WriteLine("\n----------------------------------------------------------");
        Console.WriteLine("Testando método NÃO-ATÔMICO...");
        Console.WriteLine("----------------------------------------------------------");
        await _nonAtomicRepository.ResetDemoAsync(_productId, _initialStock);
        var nonAtomicResult = await RunTestAsync(_nonAtomicRepository, "NÃO-ATÔMICO");

        // Exibe resultados comparativos
        Console.WriteLine("\n==========================================================");
        Console.WriteLine("RESULTADOS");
        Console.WriteLine("==========================================================\n");

        int expectedFinal = _initialStock + (_threadCount * _incrementsPerThread);
        Console.WriteLine($"Estoque esperado: {expectedFinal}");
        Console.WriteLine();

        Console.WriteLine($"Método ATÔMICO:");
        Console.WriteLine($"  - Estoque final: {atomicResult.FinalStock}");
        Console.WriteLine($"  - Tempo de execução: {atomicResult.ElapsedTime.TotalSeconds:F2}s");
        var atomicDiff = expectedFinal - atomicResult.FinalStock;
        Console.WriteLine($"  - Incrementos perdidos: {atomicDiff}");
        Console.WriteLine($"  - Taxa de sucesso: {(1 - (double)atomicDiff / (_threadCount * _incrementsPerThread)) * 100:F2}%");
        Console.WriteLine();

        Console.WriteLine($"Método NÃO-ATÔMICO:");
        Console.WriteLine($"  - Estoque final: {nonAtomicResult.FinalStock}");
        Console.WriteLine($"  - Tempo de execução: {nonAtomicResult.ElapsedTime.TotalSeconds:F2}s");
        var nonAtomicDiff = expectedFinal - nonAtomicResult.FinalStock;
        Console.WriteLine($"  - Incrementos perdidos: {nonAtomicDiff}");
        Console.WriteLine($"  - Taxa de sucesso: {(1 - (double)nonAtomicDiff / (_threadCount * _incrementsPerThread)) * 100:F2}%");
        Console.WriteLine();

        Console.WriteLine("==========================================================");
        Console.WriteLine("CONCLUSÃO");
        Console.WriteLine("==========================================================");
        Console.WriteLine();

        if (atomicDiff == 0 && nonAtomicDiff > 0)
        {
            Console.WriteLine("✓ O método ATÔMICO manteve consistência perfeita!");
            Console.WriteLine($"✗ O método NÃO-ATÔMICO perdeu {nonAtomicDiff} incrementos devido a race conditions.");
            Console.WriteLine("\nIsso demonstra a importância de operações atômicas em ambientes concorrentes.");
        }
        else if (atomicDiff == 0 && nonAtomicDiff == 0)
        {
            Console.WriteLine("⚠ Ambos os métodos tiveram sucesso total.");
            Console.WriteLine("  Isso pode indicar que a concorrência não foi suficiente.");
            Console.WriteLine("  Tente aumentar o número de threads ou incrementos.");
        }
        else
        {
            Console.WriteLine("⚠ Resultados inesperados. Verifique a conexão com o banco de dados.");
        }

        Console.WriteLine("\n==========================================================\n");
    }

    private async Task<TestResult> RunTestAsync(IProductRepository repository, string methodName)
    {
        var stopwatch = Stopwatch.StartNew();

        var tasks = new List<Task>();
        for (int i = 0; i < _threadCount; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                for (int j = 0; j < _incrementsPerThread; j++)
                {
                    await repository.IncrementStockAsync(_productId, 1);
                }
            }));
        }

        await Task.WhenAll(tasks);
        stopwatch.Stop();

        var product = await repository.GetProductAsync(_productId);

        Console.WriteLine($"Concluído! Estoque final: {product.StockQuantity} | Tempo: {stopwatch.Elapsed.TotalSeconds:F2}s");

        return new TestResult
        {
            FinalStock = product.StockQuantity,
            ElapsedTime = stopwatch.Elapsed
        };
    }

    private class TestResult
    {
        public int FinalStock { get; set; }
        public TimeSpan ElapsedTime { get; set; }
    }
}
