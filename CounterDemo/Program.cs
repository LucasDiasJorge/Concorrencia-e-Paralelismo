using CounterDemo.Demo;
using CounterDemo.Repositories;

namespace CounterDemo;

class Program
{
    static async Task Main(string[] args)
    {
        // ⚠️ IMPORTANTE: Ajuste a connection string para seu ambiente
        // Formato: "Server=localhost;Database=counter_demo;User=root;Password=sua_senha;"
        const string connectionString = "Server=localhost;Port=3307;Database=counter_demo;User=root;Password=myrootdevpass;";

        Console.WriteLine("Verificando conexão com o banco de dados...\n");

        try
        {
            // Cria as instâncias dos repositories
            var atomicRepository = new AtomicProductRepository(connectionString);
            var nonAtomicRepository = new NonAtomicProductRepository(connectionString);

            // Verifica se o produto existe
            try
            {
                await atomicRepository.GetProductAsync(1);
            }
            catch
            {
                Console.WriteLine("⚠️ ERRO: Produto com ID 1 não encontrado no banco de dados.");
                Console.WriteLine("Por favor, execute o script setup.sql antes de rodar este programa.\n");
                Console.WriteLine("Pressione qualquer tecla para sair...");
                Console.ReadKey();
                return;
            }

            // Configuração do teste
            int productId = 1;
            int initialStock = 0;
            int threadCount = 50;
            int incrementsPerThread = 5;

            // Permite customização via argumentos de linha de comando
            if (args.Length >= 1 && int.TryParse(args[0], out int customThreadCount))
            {
                threadCount = customThreadCount;
            }

            if (args.Length >= 2 && int.TryParse(args[1], out int customIncrements))
            {
                incrementsPerThread = customIncrements;
            }

            // Cria e executa a demonstração
            var demo = new StockConcurrencyDemo(
                atomicRepository,
                nonAtomicRepository,
                productId,
                initialStock,
                threadCount,
                incrementsPerThread);

            await demo.RunDemoAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ ERRO: {ex.Message}");
            Console.WriteLine("\nVerifique se:");
            Console.WriteLine("1. O MySQL está rodando");
            Console.WriteLine("2. A connection string está correta");
            Console.WriteLine("3. O banco de dados 'counter_demo' existe");
            Console.WriteLine("4. A tabela 'products' existe e contém o produto com ID 1\n");
        }

        Console.WriteLine("Pressione qualquer tecla para sair...");
        Console.ReadKey();
    }
}
