using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

// Programa exemplo: ConcurrentQueue<T> com múltiplos produtores e consumidores
// Também mostra TryPeek, ToArray e uma alternativa com BlockingCollection<T>

class Program
{
    static async Task<int> Main(string[] args)
    {
        Console.WriteLine("ConcurrentQueue Example - RaceCondition-CSharp/Examples/ConcurrentQueueExample\n");

        // Se passado "blocking" como argumento, roda a versão com BlockingCollection
        bool useBlocking = args.Length > 0 && args[0].Equals("blocking", StringComparison.OrdinalIgnoreCase);

        if (useBlocking)
        {
            RunBlockingCollectionExample();
            return 0;
        }

        await RunConcurrentQueueExample();
        return 0;
    }

    static async Task RunConcurrentQueueExample()
    {
        var queue = new ConcurrentQueue<int>();

        const int totalItems = 100;
        const int producers = 4;
        const int consumers = 3;

        int processedCount = 0; // incrementado com Interlocked

        // Criar produtores
        Task[] producerTasks = new Task[producers];
        int itemsPerProducer = totalItems / producers;

        for (int p = 0; p < producers; p++)
        {
            int producerId = p;
            int start = producerId * itemsPerProducer;
            int end = (producerId == producers - 1) ? totalItems : start + itemsPerProducer;

            producerTasks[p] = Task.Run(() =>
            {
                for (int i = start; i < end; i++)
                {
                    queue.Enqueue(i);
                    Console.WriteLine($"[Producer {producerId}] Enqueued {i}");
                    Thread.Sleep(5); // simula variação de produção
                }
            });
        }

        // Criar consumidores
        Task[] consumerTasks = new Task[consumers];
        for (int c = 0; c < consumers; c++)
        {
            int consumerId = c;
            consumerTasks[c] = Task.Run(() =>
            {
                while (Volatile.Read(ref processedCount) < totalItems)
                {
                    if (queue.TryDequeue(out int item))
                    {
                        int done = Interlocked.Increment(ref processedCount);
                        Console.WriteLine($"    [Consumer {consumerId}] Dequeued {item} (processed {done}/{totalItems})");
                        Thread.Sleep(20); // simula processamento
                    }
                    else
                    {
                        // Fila vazia temporariamente
                        Thread.Yield();
                    }
                }
            });
        }

        // Espera produtores terminarem
        await Task.WhenAll(producerTasks);

        // Opcional: demonstrar TryPeek e ToArray após produtores terminarem
        if (queue.TryPeek(out int front))
        {
            Console.WriteLine($"\nTryPeek: próximo item na fila (sem remover) = {front}");
        }

        int[] snapshot = queue.ToArray();
        Console.WriteLine($"Snapshot: {snapshot.Length} itens restantes na fila (ToArray)");

        // Espera consumidores terminarem
        await Task.WhenAll(consumerTasks);

        Console.WriteLine("\nTodos os itens processados com ConcurrentQueue.");
    }

    static void RunBlockingCollectionExample()
    {
        Console.WriteLine("BlockingCollection<T> + ConcurrentQueue<T> example\n");

        using var blocking = new BlockingCollection<int>(new ConcurrentQueue<int>(), boundedCapacity: 50);

        const int totalItems = 100;
        const int producers = 3;
        const int consumers = 2;

        // Produtores
        Task[] prod = new Task[producers];
        int itemsPerProducer = totalItems / producers;
        for (int p = 0; p < producers; p++)
        {
            int pid = p;
            int start = pid * itemsPerProducer;
            int end = (pid == producers - 1) ? totalItems : start + itemsPerProducer;
            prod[p] = Task.Run(() =>
            {
                for (int i = start; i < end; i++)
                {
                    blocking.Add(i);
                    Console.WriteLine($"[Producer {pid}] Added {i}");
                    Thread.Sleep(10);
                }
            });
        }

        // Consumidores usam GetConsumingEnumerable (bloqueia até item disponível, termina quando CompleteAdding())
        Task[] cons = new Task[consumers];
        for (int c = 0; c < consumers; c++)
        {
            int cid = c;
            cons[c] = Task.Run(() =>
            {
                foreach (var item in blocking.GetConsumingEnumerable())
                {
                    Console.WriteLine($"    [Consumer {cid}] Took {item}");
                    Thread.Sleep(30);
                }
            });
        }

        Task.WaitAll(prod);
        // Indica que não haverá mais itens
        blocking.CompleteAdding();

        Task.WaitAll(cons);
        Console.WriteLine("\nTodos os itens processados com BlockingCollection.");
    }
}
