using System.Collections.Concurrent;

namespace RaceCondition.Examples;

/// <summary>
/// Demonstra race condition em coleções compartilhadas.
/// Cenário: List<T> não é thread-safe e causa exceções/corrupção.
/// </summary>
public static class ListRaceCondition
{
    /// <summary>
    /// Executa demonstração de race condition em List<T>.
    /// </summary>
    public static void RunDemo()
    {
        Console.WriteLine("\n" + new string('=', 80));
        Console.WriteLine("EXEMPLO 3: RACE CONDITION EM COLEÇÕES (LIST)");
        Console.WriteLine(new string('=', 80));

        Console.WriteLine("\n📌 CENÁRIO:");
        Console.WriteLine("   - 10 threads adicionando 1.000 itens cada");
        Console.WriteLine("   - Total esperado: 10.000 itens");
        Console.WriteLine("   - List<T> NÃO É THREAD-SAFE!");

        // Teste 1: List sem sincronização (pode lançar exceção)
        Console.WriteLine("\n❌ TESTE 1: LIST SEM SINCRONIZAÇÃO");
        try
        {
            List<int> unsafeList = new List<int>();
            RunConcurrentAdds(unsafeList, ListType.Unsafe);
            Console.WriteLine($"   Itens na lista: {unsafeList.Count:N0}");
            Console.WriteLine($"   Itens perdidos: {10000 - unsafeList.Count:N0}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ⚠️  EXCEÇÃO: {ex.GetType().Name}");
            Console.WriteLine($"   Mensagem: {ex.Message}");
            Console.WriteLine("   List<T> lançou exceção devido a modificação concorrente!");
        }

        // Teste 2: List com Lock
        Console.WriteLine("\n✅ TESTE 2: LIST COM LOCK");
        List<int> listWithLock = new List<int>();
        RunConcurrentAdds(listWithLock, ListType.WithLock);
        Console.WriteLine($"   Itens na lista: {listWithLock.Count:N0}");
        Console.WriteLine($"   Itens perdidos: {10000 - listWithLock.Count:N0}");

        // Teste 3: ConcurrentBag (RECOMENDADO)
        Console.WriteLine("\n✅ TESTE 3: CONCURRENTBAG (RECOMENDADO)");
        ConcurrentBag<int> concurrentBag = new ConcurrentBag<int>();
        RunConcurrentAdds(concurrentBag, ListType.ConcurrentBag);
        Console.WriteLine($"   Itens na bag: {concurrentBag.Count:N0}");
        Console.WriteLine($"   Itens perdidos: {10000 - concurrentBag.Count:N0}");

        // Explicação técnica
        Console.WriteLine("\n📚 EXPLICAÇÃO TÉCNICA:");
        Console.WriteLine("\n   List<T> internamente:");
        Console.WriteLine("   - Array interno que cresce dinamicamente");
        Console.WriteLine("   - Operação Add pode redimensionar o array");
        Console.WriteLine("   - Nenhuma sincronização interna");
        Console.WriteLine("\n   Problemas com concorrência:");
        Console.WriteLine("   1. IndexOutOfRangeException");
        Console.WriteLine("   2. NullReferenceException");
        Console.WriteLine("   3. Dados corrompidos");
        Console.WriteLine("   4. Perda de elementos");
        Console.WriteLine("\n   SOLUÇÕES:");
        Console.WriteLine("   ✓ ConcurrentBag<T> - Desordenado, alta performance");
        Console.WriteLine("   ✓ ConcurrentQueue<T> - FIFO, thread-safe");
        Console.WriteLine("   ✓ ConcurrentStack<T> - LIFO, thread-safe");
        Console.WriteLine("   ✓ BlockingCollection<T> - Producer/Consumer pattern");

        // Demonstra outras coleções concorrentes
        DemonstrateOtherCollections();
    }

    private enum ListType
    {
        Unsafe,
        WithLock,
        ConcurrentBag
    }

    /// <summary>
    /// Executa adições concorrentes em coleções.
    /// </summary>
    private static void RunConcurrentAdds(object collection, ListType type)
    {
        const int numberOfThreads = 10;
        const int itemsPerThread = 1000;
        object lockObject = new object();

        Thread[] threads = new Thread[numberOfThreads];

        for (int i = 0; i < numberOfThreads; i++)
        {
            int threadId = i;
            threads[i] = new Thread(() =>
            {
                for (int j = 0; j < itemsPerThread; j++)
                {
                    int value = threadId * itemsPerThread + j;

                    switch (type)
                    {
                        case ListType.Unsafe:
                            ((List<int>)collection).Add(value);
                            break;

                        case ListType.WithLock:
                            lock (lockObject)
                            {
                                ((List<int>)collection).Add(value);
                            }
                            break;

                        case ListType.ConcurrentBag:
                            ((ConcurrentBag<int>)collection).Add(value);
                            break;
                    }
                }
            });
            threads[i].Start();
        }

        foreach (Thread thread in threads)
        {
            thread.Join();
        }
    }

    /// <summary>
    /// Demonstra uso de outras coleções concorrentes.
    /// </summary>
    private static void DemonstrateOtherCollections()
    {
        Console.WriteLine("\n" + new string('=', 80));
        Console.WriteLine("DEMONSTRAÇÃO: OUTRAS COLEÇÕES CONCORRENTES");
        Console.WriteLine(new string('=', 80));

        // ConcurrentQueue
        Console.WriteLine("\n1️⃣  CONCURRENTQUEUE<T> (FIFO - First In, First Out)");
        ConcurrentQueue<string> queue = new ConcurrentQueue<string>();
        
        Thread producer = new Thread(() =>
        {
            for (int i = 1; i <= 5; i++)
            {
                queue.Enqueue($"Item-{i}");
                Console.WriteLine($"   ➕ Produziu: Item-{i}");
                Thread.Sleep(100);
            }
        });

        Thread consumer = new Thread(() =>
        {
            Thread.Sleep(50); // Aguarda um pouco
            for (int i = 1; i <= 5; i++)
            {
                if (queue.TryDequeue(out string? item))
                {
                    Console.WriteLine($"   ➖ Consumiu: {item}");
                }
                Thread.Sleep(150);
            }
        });

        producer.Start();
        consumer.Start();
        producer.Join();
        consumer.Join();

        // ConcurrentStack
        Console.WriteLine("\n2️⃣  CONCURRENTSTACK<T> (LIFO - Last In, First Out)");
        ConcurrentStack<string> stack = new ConcurrentStack<string>();
        stack.Push("Primeiro");
        stack.Push("Segundo");
        stack.Push("Terceiro");

        while (stack.TryPop(out string? item))
        {
            Console.WriteLine($"   Pop: {item}");
        }

        // ConcurrentDictionary
        Console.WriteLine("\n3️⃣  CONCURRENTDICTIONARY<TKey, TValue>");
        ConcurrentDictionary<int, string> dict = new ConcurrentDictionary<int, string>();

        // AddOrUpdate é thread-safe
        dict.AddOrUpdate(1, "Valor1", (key, oldValue) => "ValorAtualizado1");
        Console.WriteLine($"   Chave 1: {dict[1]}");

        // GetOrAdd é thread-safe
        string value = dict.GetOrAdd(2, key => $"Valor{key}");
        Console.WriteLine($"   Chave 2: {value}");

        // TryRemove é thread-safe
        if (dict.TryRemove(1, out string? removed))
        {
            Console.WriteLine($"   Removido: {removed}");
        }

        // BlockingCollection
        Console.WriteLine("\n4️⃣  BLOCKINGCOLLECTION<T> (Producer/Consumer Pattern)");
        Console.WriteLine("   - Bloqueia quando está vazia (consumidor aguarda)");
        Console.WriteLine("   - Bloqueia quando está cheia (produtor aguarda)");
        Console.WriteLine("   - Ideal para pipelines de processamento");

        using BlockingCollection<int> blockingCollection = new BlockingCollection<int>(boundedCapacity: 3);

        Task producerTask = Task.Run(() =>
        {
            for (int i = 1; i <= 5; i++)
            {
                blockingCollection.Add(i);
                Console.WriteLine($"   ➕ Adicionou: {i} (Count: {blockingCollection.Count})");
                Thread.Sleep(100);
            }
            blockingCollection.CompleteAdding();
        });

        Task consumerTask = Task.Run(() =>
        {
            foreach (int item in blockingCollection.GetConsumingEnumerable())
            {
                Console.WriteLine($"   ➖ Processou: {item}");
                Thread.Sleep(200);
            }
        });

        Task.WaitAll(producerTask, consumerTask);

        Console.WriteLine("\n📚 RESUMO:");
        Console.WriteLine("   ConcurrentQueue → Fila (FIFO)");
        Console.WriteLine("   ConcurrentStack → Pilha (LIFO)");
        Console.WriteLine("   ConcurrentBag → Coleção desordenada, alta performance");
        Console.WriteLine("   ConcurrentDictionary → Dicionário thread-safe");
        Console.WriteLine("   BlockingCollection → Producer/Consumer com bloqueio");
    }
}
