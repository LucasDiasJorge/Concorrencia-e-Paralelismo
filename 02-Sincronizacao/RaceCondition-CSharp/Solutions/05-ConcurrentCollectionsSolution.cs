using System.Collections.Concurrent;

namespace RaceCondition.Solutions;

/// <summary>
/// Demonstra o uso de Concurrent Collections para resolver race conditions.
/// Coleções thread-safe eliminam a necessidade de sincronização manual.
/// </summary>
public static class ConcurrentCollectionsSolution
{
    /// <summary>
    /// Demonstra todas as coleções concorrentes disponíveis em .NET.
    /// </summary>
    public static void RunDemo()
    {
        Console.WriteLine("\n" + new string('=', 80));
        Console.WriteLine("SOLUÇÃO 5: CONCURRENT COLLECTIONS");
        Console.WriteLine(new string('=', 80));

        Console.WriteLine("\n📚 O QUE SÃO CONCURRENT COLLECTIONS?");
        Console.WriteLine("   - Coleções thread-safe por design");
        Console.WriteLine("   - Implementam sincronização interna eficiente");
        Console.WriteLine("   - Lock-free ou fine-grained locking");
        Console.WriteLine("   - Namespace: System.Collections.Concurrent");

        Console.WriteLine("\n✅ VANTAGENS:");
        Console.WriteLine("   ✓ Sem necessidade de locks manuais");
        Console.WriteLine("   ✓ Menos propenso a erros");
        Console.WriteLine("   ✓ Performance otimizada");
        Console.WriteLine("   ✓ APIs seguras (TryAdd, TryTake, etc.)");

        Console.WriteLine("\n📦 COLEÇÕES DISPONÍVEIS:");
        Console.WriteLine("   • ConcurrentDictionary<K,V> - Dicionário thread-safe");
        Console.WriteLine("   • ConcurrentQueue<T> - Fila FIFO");
        Console.WriteLine("   • ConcurrentStack<T> - Pilha LIFO");
        Console.WriteLine("   • ConcurrentBag<T> - Bag desordenado");
        Console.WriteLine("   • BlockingCollection<T> - Producer/Consumer");

        DemonstrateConcurrentDictionary();
        DemonstrateConcurrentQueue();
        DemonstrateConcurrentStack();
        DemonstrateConcurrentBag();
        DemonstrateBlockingCollection();
    }

    /// <summary>
    /// Demonstra ConcurrentDictionary.
    /// </summary>
    private static void DemonstrateConcurrentDictionary()
    {
        Console.WriteLine("\n" + new string('-', 80));
        Console.WriteLine("1. CONCURRENTDICTIONARY<TKey, TValue>");
        Console.WriteLine(new string('-', 80));

        Console.WriteLine("\n   Operações atômicas especiais:");
        Console.WriteLine("   - TryAdd, TryGetValue, TryUpdate, TryRemove");
        Console.WriteLine("   - AddOrUpdate, GetOrAdd");
        Console.WriteLine("   - Todas são thread-safe e atômicas\n");

        ConcurrentDictionary<int, string> dict = new ConcurrentDictionary<int, string>();

        // TryAdd - Adiciona apenas se não existe
        bool added = dict.TryAdd(1, "Valor1");
        Console.WriteLine($"   TryAdd(1, 'Valor1'): {added}");

        added = dict.TryAdd(1, "OutroValor");
        Console.WriteLine($"   TryAdd(1, 'OutroValor'): {added} (já existe)");

        // GetOrAdd - Obtém ou cria atomicamente
        string value = dict.GetOrAdd(2, key => $"Valor{key}");
        Console.WriteLine($"\n   GetOrAdd(2, factory): '{value}'");

        // AddOrUpdate - Adiciona ou atualiza atomicamente
        string result = dict.AddOrUpdate(
            1,
            "NovoValor",
            (key, oldValue) => $"{oldValue}_Updated"
        );
        Console.WriteLine($"   AddOrUpdate(1): '{result}'");

        // TryUpdate - Atualiza apenas se valor atual é esperado (CAS)
        bool updated = dict.TryUpdate(1, "Valor1_Updated2", "Valor1_Updated");
        Console.WriteLine($"\n   TryUpdate(1, novo, esperado): {updated}");

        // Uso concorrente
        Console.WriteLine("\n   Teste concorrente: 10 threads adicionando 100 itens cada");
        
        ConcurrentDictionary<int, int> concurrentDict = new ConcurrentDictionary<int, int>();
        Thread[] threads = new Thread[10];

        for (int i = 0; i < 10; i++)
        {
            int threadId = i;
            threads[i] = new Thread(() =>
            {
                for (int j = 0; j < 100; j++)
                {
                    int key = threadId * 100 + j;
                    concurrentDict.TryAdd(key, key);
                }
            });
            threads[i].Start();
        }

        foreach (Thread thread in threads)
        {
            thread.Join();
        }

        Console.WriteLine($"   Itens no dicionário: {concurrentDict.Count:N0}");
        Console.WriteLine("   ✅ Todos os 1.000 itens adicionados com sucesso!");
    }

    /// <summary>
    /// Demonstra ConcurrentQueue (FIFO).
    /// </summary>
    private static void DemonstrateConcurrentQueue()
    {
        Console.WriteLine("\n" + new string('-', 80));
        Console.WriteLine("2. CONCURRENTQUEUE<T> (FIFO - First In, First Out)");
        Console.WriteLine(new string('-', 80));

        Console.WriteLine("\n   Ideal para: Filas de trabalho, message queues, task pipelines");
        Console.WriteLine("   Operações: Enqueue, TryDequeue, TryPeek\n");

        ConcurrentQueue<string> queue = new ConcurrentQueue<string>();

        // Producer-Consumer pattern
        Console.WriteLine("   Cenário: Producer enfileira, Consumer desenfileira");

        Thread producer = new Thread(() =>
        {
            for (int i = 1; i <= 5; i++)
            {
                string item = $"Task-{i}";
                queue.Enqueue(item);
                Console.WriteLine($"   Producer: ➕ Enfileirou {item}");
                Thread.Sleep(100);
            }
        });

        Thread consumer = new Thread(() =>
        {
            int processed = 0;
            while (processed < 5)
            {
                if (queue.TryDequeue(out string? item))
                {
                    Console.WriteLine($"   Consumer: ➖ Processou {item}");
                    processed++;
                }
                Thread.Sleep(150);
            }
        });

        producer.Start();
        consumer.Start();
        producer.Join();
        consumer.Join();

        Console.WriteLine("\n   ✅ Fila processada com sucesso!");
    }

    /// <summary>
    /// Demonstra ConcurrentStack (LIFO).
    /// </summary>
    private static void DemonstrateConcurrentStack()
    {
        Console.WriteLine("\n" + new string('-', 80));
        Console.WriteLine("3. CONCURRENTSTACK<T> (LIFO - Last In, First Out)");
        Console.WriteLine(new string('-', 80));

        Console.WriteLine("\n   Ideal para: Undo/Redo, recursion tracking, pools");
        Console.WriteLine("   Operações: Push, TryPop, TryPeek, PushRange, TryPopRange\n");

        ConcurrentStack<int> stack = new ConcurrentStack<int>();

        // Push individual
        stack.Push(1);
        stack.Push(2);
        stack.Push(3);
        Console.WriteLine("   Push: 1, 2, 3");

        // TryPop
        if (stack.TryPop(out int item))
        {
            Console.WriteLine($"   Pop: {item} (último adicionado)");
        }

        // PushRange - Adiciona múltiplos itens atomicamente
        int[] items = { 10, 20, 30, 40, 50 };
        stack.PushRange(items);
        Console.WriteLine($"\n   PushRange: {string.Join(", ", items)}");

        // TryPopRange - Remove múltiplos itens atomicamente
        int[] popped = new int[3];
        int count = stack.TryPopRange(popped);
        Console.WriteLine($"   TryPopRange(3): {string.Join(", ", popped.Take(count))}");

        Console.WriteLine($"\n   Itens restantes na pilha: {stack.Count}");
    }

    /// <summary>
    /// Demonstra ConcurrentBag (desordenado).
    /// </summary>
    private static void DemonstrateConcurrentBag()
    {
        Console.WriteLine("\n" + new string('-', 80));
        Console.WriteLine("4. CONCURRENTBAG<T> (Desordenado)");
        Console.WriteLine(new string('-', 80));

        Console.WriteLine("\n   Ideal para: Quando ordem não importa, alta performance");
        Console.WriteLine("   Características:");
        Console.WriteLine("   - Otimizado para thread-local storage");
        Console.WriteLine("   - Cada thread tem sua própria lista interna");
        Console.WriteLine("   - Mínima contenção entre threads");
        Console.WriteLine("   Operações: Add, TryTake, TryPeek\n");

        ConcurrentBag<int> bag = new ConcurrentBag<int>();

        Console.WriteLine("   Teste: 5 threads adicionando 100 itens cada");

        Thread[] threads = new Thread[5];
        for (int i = 0; i < 5; i++)
        {
            int threadId = i;
            threads[i] = new Thread(() =>
            {
                for (int j = 0; j < 100; j++)
                {
                    bag.Add(threadId * 100 + j);
                }
            });
            threads[i].Start();
        }

        foreach (Thread thread in threads)
        {
            thread.Join();
        }

        Console.WriteLine($"   Total de itens: {bag.Count:N0}");

        // Remover alguns itens
        int taken = 0;
        while (taken < 5 && bag.TryTake(out int item))
        {
            Console.WriteLine($"   TryTake: {item}");
            taken++;
        }

        Console.WriteLine("\n   ⚠️  NOTA: Ordem dos itens não é garantida!");
    }

    /// <summary>
    /// Demonstra BlockingCollection (Producer/Consumer pattern).
    /// </summary>
    private static void DemonstrateBlockingCollection()
    {
        Console.WriteLine("\n" + new string('-', 80));
        Console.WriteLine("5. BLOCKINGCOLLECTION<T> (Producer/Consumer Pattern)");
        Console.WriteLine(new string('-', 80));

        Console.WriteLine("\n   Características:");
        Console.WriteLine("   - Bloqueia quando vazia (consumer aguarda)");
        Console.WriteLine("   - Bloqueia quando cheia (producer aguarda)");
        Console.WriteLine("   - Suporta capacidade limitada");
        Console.WriteLine("   - CompleteAdding() sinaliza fim da produção");
        Console.WriteLine("   Ideal para: Pipelines de processamento, worker threads\n");

        using BlockingCollection<string> collection = new BlockingCollection<string>(boundedCapacity: 5);

        Console.WriteLine("   Cenário: Producer (rápido) → Queue (máx 5) → Consumer (lento)");
        Console.WriteLine("   Producer será bloqueado quando fila estiver cheia\n");

        Task producer = Task.Run(() =>
        {
            for (int i = 1; i <= 10; i++)
            {
                string item = $"Item-{i}";
                Console.WriteLine($"   Producer: Tentando adicionar {item}...");
                collection.Add(item); // Bloqueia se cheio
                Console.WriteLine($"   Producer: ✅ Adicionou {item} (Count: {collection.Count})");
                Thread.Sleep(50);
            }
            collection.CompleteAdding();
            Console.WriteLine("   Producer: Finalizou produção");
        });

        Task consumer = Task.Run(() =>
        {
            Console.WriteLine("   Consumer: Aguardando itens...\n");
            
            // GetConsumingEnumerable bloqueia até ter itens
            foreach (string item in collection.GetConsumingEnumerable())
            {
                Console.WriteLine($"   Consumer: ⚙️  Processando {item}...");
                Thread.Sleep(200); // Processamento lento
                Console.WriteLine($"   Consumer: ✅ Completou {item}");
            }
            
            Console.WriteLine("   Consumer: Finalizou consumo");
        });

        Task.WaitAll(producer, consumer);

        Console.WriteLine("\n   ✅ Pipeline completado com sucesso!");
        Console.WriteLine("   💡 Producer foi automaticamente throttled pela fila limitada");
    }
}
