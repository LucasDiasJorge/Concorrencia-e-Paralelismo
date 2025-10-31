namespace RaceCondition.Solutions;

/// <summary>
/// Demonstra o uso de Semaphore e SemaphoreSlim para limitar concorrência.
/// Semáforos controlam o número de threads que podem acessar um recurso.
/// </summary>
public static class SemaphoreSolution
{
    /// <summary>
    /// Demonstra o uso de semáforos.
    /// </summary>
    public static void RunDemo()
    {
        Console.WriteLine("\n" + new string('=', 80));
        Console.WriteLine("SOLUÇÃO 3: SEMAPHORE E SEMAPHORESLIM");
        Console.WriteLine(new string('=', 80));

        Console.WriteLine("\n📚 O QUE É SEMAPHORE?");
        Console.WriteLine("   - Controla o número de threads que podem acessar um recurso");
        Console.WriteLine("   - Lock permite apenas 1 thread (semáforo binário)");
        Console.WriteLine("   - Semáforo permite N threads simultaneamente");
        Console.WriteLine("   - Útil para limitar concorrência (rate limiting)");

        Console.WriteLine("\n✅ QUANDO USAR:");
        Console.WriteLine("   ✓ Limitar acesso a recursos limitados (conexões, threads)");
        Console.WriteLine("   ✓ Rate limiting");
        Console.WriteLine("   ✓ Pool de recursos");
        Console.WriteLine("   ✓ Throttling");

        Console.WriteLine("\n❌ QUANDO NÃO USAR:");
        Console.WriteLine("   ✗ Proteção simples (use lock)");
        Console.WriteLine("   ✗ Operações atômicas (use Interlocked)");
        Console.WriteLine("   ✗ Não precisa limitar concorrência");

        DemonstrateSemaphoreSlim();
        DemonstrateConnectionPool();
        DemonstrateRateLimiting();
    }

    /// <summary>
    /// Demonstra uso básico de SemaphoreSlim.
    /// </summary>
    private static void DemonstrateSemaphoreSlim()
    {
        Console.WriteLine("\n" + new string('-', 80));
        Console.WriteLine("1. SEMAPHORESLIM BÁSICO");
        Console.WriteLine(new string('-', 80));

        Console.WriteLine("\n   SemaphoreSlim vs Semaphore:");
        Console.WriteLine("   - SemaphoreSlim: Leve, não atravessa processos, RECOMENDADO");
        Console.WriteLine("   - Semaphore: Pesado, atravessa processos, legacy");

        const int maxConcurrency = 3;
        Console.WriteLine($"\n   Permitindo apenas {maxConcurrency} threads simultâneas");
        Console.WriteLine("   10 threads tentarão acessar o recurso\n");

        using SemaphoreSlim semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);

        Thread[] threads = new Thread[10];

        for (int i = 0; i < 10; i++)
        {
            int threadId = i + 1;
            threads[i] = new Thread(() =>
            {
                Console.WriteLine($"   Thread {threadId}: Aguardando permissão...");
                
                semaphore.Wait(); // Bloqueia se já tiver 3 threads dentro
                try
                {
                    Console.WriteLine($"   Thread {threadId}: ✅ ENTROU (slots disponíveis: {semaphore.CurrentCount})");
                    Thread.Sleep(1000); // Simula trabalho
                    Console.WriteLine($"   Thread {threadId}: Saiu");
                }
                finally
                {
                    semaphore.Release(); // Libera um slot
                }
            });
            threads[i].Start();
            Thread.Sleep(100); // Pequeno delay para visualização
        }

        foreach (Thread thread in threads)
        {
            thread.Join();
        }

        Console.WriteLine("\n   ✅ Todas as threads completaram!");
        Console.WriteLine($"   Máximo de {maxConcurrency} threads rodando simultaneamente");
    }

    /// <summary>
    /// Demonstra uso de semáforo para pool de conexões.
    /// </summary>
    private static void DemonstrateConnectionPool()
    {
        Console.WriteLine("\n" + new string('-', 80));
        Console.WriteLine("2. POOL DE CONEXÕES");
        Console.WriteLine(new string('-', 80));

        Console.WriteLine("\n   Cenário: Pool com 5 conexões de banco de dados");
        Console.WriteLine("   10 clientes precisam de uma conexão\n");

        DatabaseConnectionPool pool = new DatabaseConnectionPool(maxConnections: 5);

        Thread[] clients = new Thread[10];

        for (int i = 0; i < 10; i++)
        {
            int clientId = i + 1;
            clients[i] = new Thread(() =>
            {
                Console.WriteLine($"   Cliente {clientId}: Solicitando conexão...");
                
                using (DatabaseConnection? connection = pool.AcquireConnection())
                {
                    if (connection != null)
                    {
                        Console.WriteLine($"   Cliente {clientId}: ✅ Conexão {connection.Id} adquirida");
                        Thread.Sleep(500); // Simula query
                        Console.WriteLine($"   Cliente {clientId}: Query executada");
                    }
                } // Conexão liberada automaticamente (Dispose)
            });
            clients[i].Start();
            Thread.Sleep(50);
        }

        foreach (Thread client in clients)
        {
            client.Join();
        }

        Console.WriteLine("\n   ✅ Pool gerenciou conexões eficientemente!");
        pool.Dispose();
    }

    /// <summary>
    /// Demonstra rate limiting com semáforo.
    /// </summary>
    private static void DemonstrateRateLimiting()
    {
        Console.WriteLine("\n" + new string('-', 80));
        Console.WriteLine("3. RATE LIMITING");
        Console.WriteLine(new string('-', 80));

        Console.WriteLine("\n   Cenário: API com limite de 5 requests por segundo");
        Console.WriteLine("   10 clientes fazendo requisições\n");

        RateLimiter limiter = new RateLimiter(maxRequestsPerSecond: 5);

        Thread[] requests = new Thread[10];

        for (int i = 0; i < 10; i++)
        {
            int requestId = i + 1;
            requests[i] = new Thread(() =>
            {
                bool allowed = limiter.TryAcquire(timeout: TimeSpan.FromSeconds(3));
                
                if (allowed)
                {
                    Console.WriteLine($"   Request {requestId}: ✅ PERMITIDO às {DateTime.Now:HH:mm:ss.fff}");
                    Thread.Sleep(200); // Simula processamento
                }
                else
                {
                    Console.WriteLine($"   Request {requestId}: ❌ REJEITADO (rate limit)");
                }
            });
            requests[i].Start();
        }

        foreach (Thread request in requests)
        {
            request.Join();
        }

        Console.WriteLine("\n   ✅ Rate limiter funcionou corretamente!");
        limiter.Dispose();
    }

    /// <summary>
    /// Pool de conexões de banco de dados.
    /// </summary>
    private class DatabaseConnectionPool : IDisposable
    {
        private readonly SemaphoreSlim _semaphore;
        private readonly int _maxConnections;
        private int _nextConnectionId = 1;

        public DatabaseConnectionPool(int maxConnections)
        {
            _maxConnections = maxConnections;
            _semaphore = new SemaphoreSlim(maxConnections, maxConnections);
        }

        public DatabaseConnection? AcquireConnection()
        {
            _semaphore.Wait();
            
            int connectionId = Interlocked.Increment(ref _nextConnectionId) - 1;
            return new DatabaseConnection(connectionId, this);
        }

        public void ReleaseConnection()
        {
            _semaphore.Release();
        }

        public void Dispose()
        {
            _semaphore?.Dispose();
        }
    }

    /// <summary>
    /// Representa uma conexão de banco de dados.
    /// </summary>
    private class DatabaseConnection : IDisposable
    {
        public int Id { get; }
        private readonly DatabaseConnectionPool _pool;

        public DatabaseConnection(int id, DatabaseConnectionPool pool)
        {
            Id = id;
            _pool = pool;
        }

        public void Dispose()
        {
            _pool.ReleaseConnection();
        }
    }

    /// <summary>
    /// Rate limiter usando semáforo.
    /// </summary>
    private class RateLimiter : IDisposable
    {
        private readonly SemaphoreSlim _semaphore;
        private readonly Timer _resetTimer;
        private readonly int _maxRequests;

        public RateLimiter(int maxRequestsPerSecond)
        {
            _maxRequests = maxRequestsPerSecond;
            _semaphore = new SemaphoreSlim(maxRequestsPerSecond, maxRequestsPerSecond);
            
            // Reseta o semáforo a cada segundo
            _resetTimer = new Timer(_ =>
            {
                // Libera todos os slots
                int currentCount = _semaphore.CurrentCount;
                if (currentCount < _maxRequests)
                {
                    _semaphore.Release(_maxRequests - currentCount);
                }
            }, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        }

        public bool TryAcquire(TimeSpan timeout)
        {
            return _semaphore.Wait(timeout);
        }

        public void Dispose()
        {
            _resetTimer?.Dispose();
            _semaphore?.Dispose();
        }
    }
}
