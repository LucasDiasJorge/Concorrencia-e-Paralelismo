namespace RaceCondition.Solutions;

/// <summary>
/// Demonstra o uso de Lock para resolver race conditions.
/// Lock é a solução mais simples e comum para proteger seções críticas.
/// </summary>
public static class LockSolution
{
    /// <summary>
    /// Demonstra diferentes formas de usar lock.
    /// </summary>
    public static void RunDemo()
    {
        Console.WriteLine("\n" + new string('=', 80));
        Console.WriteLine("SOLUÇÃO 1: LOCK (MONITOR)");
        Console.WriteLine(new string('=', 80));

        Console.WriteLine("\n📚 O QUE É LOCK?");
        Console.WriteLine("   - Palavra-chave C# que garante exclusão mútua");
        Console.WriteLine("   - Apenas uma thread pode executar o código dentro do lock");
        Console.WriteLine("   - Syntactic sugar para Monitor.Enter/Monitor.Exit");
        Console.WriteLine("   - Bloqueante: outras threads aguardam");

        Console.WriteLine("\n✅ QUANDO USAR:");
        Console.WriteLine("   ✓ Proteger seções críticas curtas");
        Console.WriteLine("   ✓ Garantir atomicidade de múltiplas operações");
        Console.WriteLine("   ✓ Código legível e simples");
        Console.WriteLine("   ✓ Baixa contenção (poucas threads competindo)");

        Console.WriteLine("\n❌ QUANDO NÃO USAR:");
        Console.WriteLine("   ✗ Operações longas (I/O, Network)");
        Console.WriteLine("   ✗ Alta contenção (muitas threads)");
        Console.WriteLine("   ✗ Operações simples (use Interlocked)");
        Console.WriteLine("   ✗ Risco de deadlock");

        DemonstrateLockBasics();
        DemonstrateLockTimeout();
        DemonstrateDeadlock();
        DemonstrateBestPractices();
    }

    /// <summary>
    /// Demonstra uso básico de lock.
    /// </summary>
    private static void DemonstrateLockBasics()
    {
        Console.WriteLine("\n" + new string('-', 80));
        Console.WriteLine("1. USO BÁSICO DO LOCK");
        Console.WriteLine(new string('-', 80));

        BankAccountExample account = new BankAccountExample(1000m);

        Console.WriteLine($"\n   Saldo inicial: R$ {account.Balance:N2}");

        // 5 threads fazendo depósitos simultaneamente
        Thread[] threads = new Thread[5];
        for (int i = 0; i < 5; i++)
        {
            int threadId = i + 1;
            threads[i] = new Thread(() =>
            {
                for (int j = 0; j < 100; j++)
                {
                    account.Deposit(10m);
                }
                Console.WriteLine($"   Thread {threadId} completou 100 depósitos");
            });
            threads[i].Start();
        }

        foreach (Thread thread in threads)
        {
            thread.Join();
        }

        Console.WriteLine($"\n   Saldo final: R$ {account.Balance:N2}");
        Console.WriteLine($"   Esperado: R$ {1000m + (5 * 100 * 10m):N2}");
        Console.WriteLine($"   ✅ Nenhuma perda de dados!");
    }

    /// <summary>
    /// Demonstra lock com timeout usando Monitor.TryEnter.
    /// </summary>
    private static void DemonstrateLockTimeout()
    {
        Console.WriteLine("\n" + new string('-', 80));
        Console.WriteLine("2. LOCK COM TIMEOUT");
        Console.WriteLine(new string('-', 80));

        Console.WriteLine("\n   Evita deadlocks ao definir um tempo máximo de espera");

        object lockObject = new object();
        bool acquired = false;

        Thread longTask = new Thread(() =>
        {
            lock (lockObject)
            {
                Console.WriteLine("   Thread 1: Adquiriu o lock, simulando operação longa...");
                Thread.Sleep(3000);
                Console.WriteLine("   Thread 1: Liberou o lock");
            }
        });

        Thread timeoutTask = new Thread(() =>
        {
            Thread.Sleep(500); // Aguarda Thread 1 adquirir o lock

            acquired = Monitor.TryEnter(lockObject, TimeSpan.FromSeconds(1));
            try
            {
                if (acquired)
                {
                    Console.WriteLine("   Thread 2: Conseguiu adquirir o lock!");
                }
                else
                {
                    Console.WriteLine("   Thread 2: TIMEOUT! Não conseguiu o lock em 1 segundo");
                    Console.WriteLine("   Thread 2: Pode fazer fallback ou retry");
                }
            }
            finally
            {
                if (acquired)
                {
                    Monitor.Exit(lockObject);
                }
            }
        });

        longTask.Start();
        timeoutTask.Start();
        longTask.Join();
        timeoutTask.Join();
    }

    /// <summary>
    /// Demonstra como deadlocks podem ocorrer e como evitá-los.
    /// </summary>
    private static void DemonstrateDeadlock()
    {
        Console.WriteLine("\n" + new string('-', 80));
        Console.WriteLine("3. PREVENÇÃO DE DEADLOCK");
        Console.WriteLine(new string('-', 80));

        Console.WriteLine("\n   ⚠️  DEADLOCK: Duas threads esperando uma pela outra");
        Console.WriteLine("\n   Cenário de deadlock:");
        Console.WriteLine("   Thread A: lock(resource1) → aguarda lock(resource2)");
        Console.WriteLine("   Thread B: lock(resource2) → aguarda lock(resource1)");
        Console.WriteLine("   Resultado: Ambas travam para sempre!");

        Console.WriteLine("\n   ✅ SOLUÇÃO: Sempre adquirir locks na mesma ordem");

        object resource1 = new object();
        object resource2 = new object();

        Console.WriteLine("\n   Executando transferência segura entre contas...");

        Thread thread1 = new Thread(() =>
        {
            // Sempre bloqueia resource1 primeiro, depois resource2
            lock (resource1)
            {
                Console.WriteLine("   Thread A: Adquiriu resource1");
                Thread.Sleep(100);
                lock (resource2)
                {
                    Console.WriteLine("   Thread A: Adquiriu resource2");
                    Console.WriteLine("   Thread A: Transferência completa!");
                }
            }
        });

        Thread thread2 = new Thread(() =>
        {
            // MESMA ORDEM: resource1 primeiro, depois resource2
            lock (resource1)
            {
                Console.WriteLine("   Thread B: Adquiriu resource1");
                Thread.Sleep(100);
                lock (resource2)
                {
                    Console.WriteLine("   Thread B: Adquiriu resource2");
                    Console.WriteLine("   Thread B: Transferência completa!");
                }
            }
        });

        thread1.Start();
        thread2.Start();
        thread1.Join();
        thread2.Join();

        Console.WriteLine("\n   ✅ Sem deadlock! Ambas threads completaram");
    }

    /// <summary>
    /// Demonstra melhores práticas ao usar lock.
    /// </summary>
    private static void DemonstrateBestPractices()
    {
        Console.WriteLine("\n" + new string('-', 80));
        Console.WriteLine("4. MELHORES PRÁTICAS");
        Console.WriteLine(new string('-', 80));

        Console.WriteLine("\n   ✅ FAÇA:");
        Console.WriteLine("   1. Use objeto privado dedicado para lock:");
        Console.WriteLine("      private readonly object _lockObject = new object();");
        Console.WriteLine("\n   2. Mantenha a seção crítica pequena:");
        Console.WriteLine("      lock(_lock) { /* apenas código essencial */ }");
        Console.WriteLine("\n   3. Nunca retorne o objeto de lock:");
        Console.WriteLine("      Encapsule completamente o lock");
        Console.WriteLine("\n   4. Use nomes descritivos:");
        Console.WriteLine("      _accountLock, _cacheLock, etc.");
        Console.WriteLine("\n   5. Documente locks complexos");

        Console.WriteLine("\n   ❌ NÃO FAÇA:");
        Console.WriteLine("   1. lock(this) - Outros podem bloquear sua instância!");
        Console.WriteLine("   2. lock(typeof(MyClass)) - Lock em tipo é perigoso!");
        Console.WriteLine("   3. lock(string) - Strings são interned, lock compartilhado!");
        Console.WriteLine("   4. lock com I/O - Operações longas travam outras threads");
        Console.WriteLine("   5. lock aninhado sem ordem - Risco de deadlock!");

        Console.WriteLine("\n   📊 COMPARAÇÃO:");
        Console.WriteLine("\n   ❌ RUIM:");
        Console.WriteLine("   lock(this)");
        Console.WriteLine("   {");
        Console.WriteLine("       // Qualquer código externo pode bloquear");
        Console.WriteLine("   }");

        Console.WriteLine("\n   ✅ BOM:");
        Console.WriteLine("   private readonly object _lockObject = new object();");
        Console.WriteLine("   lock(_lockObject)");
        Console.WriteLine("   {");
        Console.WriteLine("       // Somente seu código pode bloquear");
        Console.WriteLine("   }");
    }

    /// <summary>
    /// Exemplo de conta bancária com lock correto.
    /// </summary>
    private class BankAccountExample
    {
        private decimal _balance;
        private readonly object _lockObject = new object(); // ✅ Objeto privado dedicado

        public decimal Balance
        {
            get
            {
                lock (_lockObject)
                {
                    return _balance;
                }
            }
        }

        public BankAccountExample(decimal initialBalance)
        {
            _balance = initialBalance;
        }

        public void Deposit(decimal amount)
        {
            lock (_lockObject) // ✅ Lock simples e claro
            {
                _balance += amount; // Seção crítica mínima
            }
        }

        public bool Withdraw(decimal amount)
        {
            lock (_lockObject)
            {
                if (_balance >= amount) // Check-Then-Act dentro do lock
                {
                    _balance -= amount;
                    return true;
                }
                return false;
            }
        }
    }
}
