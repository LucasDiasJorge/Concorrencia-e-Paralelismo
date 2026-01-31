namespace RaceCondition.Solutions;

/// <summary>
/// Demonstra o uso de Monitor para sincronização avançada.
/// Monitor oferece mais controle que lock, incluindo timeouts e Wait/Pulse.
/// </summary>
public static class MonitorSolution
{
    /// <summary>
    /// Demonstra recursos avançados do Monitor.
    /// </summary>
    public static void RunDemo()
    {
        Console.WriteLine("\n" + new string('=', 80));
        Console.WriteLine("SOLUÇÃO 6: MONITOR (AVANÇADO)");
        Console.WriteLine(new string('=', 80));

        Console.WriteLine("\n📚 O QUE É MONITOR?");
        Console.WriteLine("   - Classe que implementa exclusão mútua");
        Console.WriteLine("   - lock { } é syntactic sugar para Monitor.Enter/Exit");
        Console.WriteLine("   - Oferece recursos adicionais:");
        Console.WriteLine("     • Timeouts (TryEnter)");
        Console.WriteLine("     • Wait/Pulse (condition variables)");
        Console.WriteLine("     • Mais controle sobre o locking");

        Console.WriteLine("\n✅ QUANDO USAR:");
        Console.WriteLine("   ✓ Precisa de timeout no lock");
        Console.WriteLine("   ✓ Implementar condition variables (Wait/Pulse)");
        Console.WriteLine("   ✓ Producer/Consumer patterns");
        Console.WriteLine("   ✓ Mais controle que lock simples");

        Console.WriteLine("\n❌ QUANDO NÃO USAR:");
        Console.WriteLine("   ✗ Lock simples é suficiente");
        Console.WriteLine("   ✗ Operações atômicas (use Interlocked)");
        Console.WriteLine("   ✗ Não precisa de Wait/Pulse");

        DemonstrateMonitorBasics();
        DemonstrateMonitorTimeout();
        DemonstrateWaitPulse();
        DemonstrateProducerConsumer();
    }

    /// <summary>
    /// Demonstra equivalência entre lock e Monitor.
    /// </summary>
    private static void DemonstrateMonitorBasics()
    {
        Console.WriteLine("\n" + new string('-', 80));
        Console.WriteLine("1. MONITOR BÁSICO (EQUIVALENTE A LOCK)");
        Console.WriteLine(new string('-', 80));

        object lockObject = new object();
        int counter = 0;

        Console.WriteLine("\n   Estas duas formas são equivalentes:\n");

        Console.WriteLine("   // Forma 1: lock");
        Console.WriteLine("   lock(lockObject)");
        Console.WriteLine("   {");
        Console.WriteLine("       counter++;");
        Console.WriteLine("   }");

        Console.WriteLine("\n   // Forma 2: Monitor (o que lock faz internamente)");
        Console.WriteLine("   bool lockTaken = false;");
        Console.WriteLine("   try");
        Console.WriteLine("   {");
        Console.WriteLine("       Monitor.Enter(lockObject, ref lockTaken);");
        Console.WriteLine("       counter++;");
        Console.WriteLine("   }");
        Console.WriteLine("   finally");
        Console.WriteLine("   {");
        Console.WriteLine("       if (lockTaken) Monitor.Exit(lockObject);");
        Console.WriteLine("   }");

        // Demonstração prática
        Console.WriteLine("\n   Teste: 5 threads incrementando contador");

        Thread[] threads = new Thread[5];
        for (int i = 0; i < 5; i++)
        {
            threads[i] = new Thread(() =>
            {
                for (int j = 0; j < 1000; j++)
                {
                    bool lockTaken = false;
                    try
                    {
                        Monitor.Enter(lockObject, ref lockTaken);
                        counter++;
                    }
                    finally
                    {
                        if (lockTaken) Monitor.Exit(lockObject);
                    }
                }
            });
            threads[i].Start();
        }

        foreach (Thread thread in threads)
        {
            thread.Join();
        }

        Console.WriteLine($"   Resultado: {counter:N0}");
        Console.WriteLine($"   Esperado: 5.000");
        Console.WriteLine("   ✅ Perfeito!");
    }

    /// <summary>
    /// Demonstra Monitor.TryEnter com timeout.
    /// </summary>
    private static void DemonstrateMonitorTimeout()
    {
        Console.WriteLine("\n" + new string('-', 80));
        Console.WriteLine("2. MONITOR COM TIMEOUT");
        Console.WriteLine(new string('-', 80));

        Console.WriteLine("\n   TryEnter permite definir timeout para adquirir o lock");
        Console.WriteLine("   Útil para evitar deadlocks e implementar fallbacks\n");

        object lockObject = new object();

        Thread longTask = new Thread(() =>
        {
            Monitor.Enter(lockObject);
            try
            {
                Console.WriteLine("   Thread 1: Adquiriu lock, operação longa (3s)...");
                Thread.Sleep(3000);
                Console.WriteLine("   Thread 1: Completou");
            }
            finally
            {
                Monitor.Exit(lockObject);
            }
        });

        Thread timeoutTask = new Thread(() =>
        {
            Thread.Sleep(500); // Aguarda Thread 1 começar

            Console.WriteLine("   Thread 2: Tentando adquirir lock (timeout 1s)...");
            
            if (Monitor.TryEnter(lockObject, TimeSpan.FromSeconds(1)))
            {
                try
                {
                    Console.WriteLine("   Thread 2: ✅ Conseguiu o lock!");
                }
                finally
                {
                    Monitor.Exit(lockObject);
                }
            }
            else
            {
                Console.WriteLine("   Thread 2: ❌ TIMEOUT! Executando plano B...");
                Console.WriteLine("   Thread 2: Usando cache local ao invés de aguardar");
            }
        });

        longTask.Start();
        timeoutTask.Start();
        longTask.Join();
        timeoutTask.Join();
    }

    /// <summary>
    /// Demonstra Wait e Pulse (condition variables).
    /// </summary>
    private static void DemonstrateWaitPulse()
    {
        Console.WriteLine("\n" + new string('-', 80));
        Console.WriteLine("3. WAIT E PULSE (CONDITION VARIABLES)");
        Console.WriteLine(new string('-', 80));

        Console.WriteLine("\n   Wait/Pulse permite coordenação entre threads:");
        Console.WriteLine("   - Wait: Libera o lock e aguarda um sinal");
        Console.WriteLine("   - Pulse: Sinaliza uma thread aguardando");
        Console.WriteLine("   - PulseAll: Sinaliza todas as threads aguardando\n");

        Console.WriteLine("   Cenário: Thread aguarda condição específica");

        object lockObject = new object();
        bool dataReady = false;

        Thread waiter = new Thread(() =>
        {
            lock (lockObject)
            {
                Console.WriteLine("   Waiter: Aguardando dados ficarem prontos...");
                
                while (!dataReady)
                {
                    Monitor.Wait(lockObject); // Libera lock e aguarda
                }
                
                Console.WriteLine("   Waiter: ✅ Dados prontos! Processando...");
            }
        });

        Thread producer = new Thread(() =>
        {
            Thread.Sleep(1000); // Simula preparação
            
            lock (lockObject)
            {
                Console.WriteLine("   Producer: Preparando dados...");
                Thread.Sleep(500);
                dataReady = true;
                Console.WriteLine("   Producer: Dados prontos! Sinalizando...");
                Monitor.Pulse(lockObject); // Acorda o waiter
            }
        });

        waiter.Start();
        Thread.Sleep(100); // Garante que waiter comece primeiro
        producer.Start();
        
        waiter.Join();
        producer.Join();

        Console.WriteLine("\n   ✅ Coordenação bem-sucedida!");
    }

    /// <summary>
    /// Demonstra Producer/Consumer usando Monitor.Wait/Pulse.
    /// </summary>
    private static void DemonstrateProducerConsumer()
    {
        Console.WriteLine("\n" + new string('-', 80));
        Console.WriteLine("4. PRODUCER/CONSUMER COM MONITOR");
        Console.WriteLine(new string('-', 80));

        Console.WriteLine("\n   Implementação clássica de buffer limitado\n");

        BoundedBuffer<int> buffer = new BoundedBuffer<int>(capacity: 5);

        // Producer
        Thread producer = new Thread(() =>
        {
            for (int i = 1; i <= 10; i++)
            {
                buffer.Put(i);
                Console.WriteLine($"   Producer: ➕ Produziu {i} (buffer: {buffer.Count}/5)");
                Thread.Sleep(50);
            }
        });

        // Consumer
        Thread consumer = new Thread(() =>
        {
            for (int i = 1; i <= 10; i++)
            {
                int item = buffer.Take();
                Console.WriteLine($"   Consumer: ➖ Consumiu {item}");
                Thread.Sleep(150); // Consumer mais lento
            }
        });

        producer.Start();
        consumer.Start();
        producer.Join();
        consumer.Join();

        Console.WriteLine("\n   ✅ Producer/Consumer completado!");
        Console.WriteLine("   💡 Producer bloqueou quando buffer estava cheio");
        Console.WriteLine("   💡 Consumer bloqueou quando buffer estava vazio");
    }

    /// <summary>
    /// Buffer limitado thread-safe usando Monitor.
    /// </summary>
    private class BoundedBuffer<T>
    {
        private readonly Queue<T> _queue = new Queue<T>();
        private readonly int _capacity;
        private readonly object _lock = new object();

        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _queue.Count;
                }
            }
        }

        public BoundedBuffer(int capacity)
        {
            _capacity = capacity;
        }

        public void Put(T item)
        {
            lock (_lock)
            {
                // Aguarda enquanto buffer está cheio
                while (_queue.Count >= _capacity)
                {
                    Monitor.Wait(_lock);
                }

                _queue.Enqueue(item);
                
                // Sinaliza consumers que podem ter novo item
                Monitor.PulseAll(_lock);
            }
        }

        public T Take()
        {
            lock (_lock)
            {
                // Aguarda enquanto buffer está vazio
                while (_queue.Count == 0)
                {
                    Monitor.Wait(_lock);
                }

                T item = _queue.Dequeue();
                
                // Sinaliza producers que podem ter espaço
                Monitor.PulseAll(_lock);
                
                return item;
            }
        }
    }
}
