# 💼 Casos de Uso Reais - Race Condition em C#

## 🎯 Aplicações Práticas no Mundo Real

Este documento mostra como aplicar as técnicas aprendidas em cenários reais de desenvolvimento.

---

## 1. 🛒 E-Commerce: Controle de Estoque

### Problema:

```csharp
public class InventoryService
{
    private Dictionary<int, int> _stock = new();  // ProductId → Quantity

    // ❌ RACE CONDITION!
    public bool PurchaseProduct(int productId, int quantity)
    {
        if (_stock[productId] >= quantity)
        {
            // Outra thread pode comprar aqui!
            _stock[productId] -= quantity;
            return true;
        }
        return false;
    }
}
```

**Resultado:** Estoque negativo! Você vende mais produtos do que tem.

### Solução 1: Lock (Simples)

```csharp
public class InventoryServiceSafe
{
    private readonly Dictionary<int, int> _stock = new();
    private readonly object _lock = new object();

    public bool PurchaseProduct(int productId, int quantity)
    {
        lock (_lock)
        {
            if (_stock.TryGetValue(productId, out int available) && available >= quantity)
            {
                _stock[productId] = available - quantity;
                return true;
            }
            return false;
        }
    }
}
```

### Solução 2: ConcurrentDictionary (Otimizado)

```csharp
public class InventoryServiceOptimized
{
    private readonly ConcurrentDictionary<int, int> _stock = new();

    public bool PurchaseProduct(int productId, int quantity)
    {
        while (true)
        {
            if (!_stock.TryGetValue(productId, out int current))
                return false;

            if (current < quantity)
                return false;

            int newQuantity = current - quantity;
            
            // Tenta atualizar atomicamente
            if (_stock.TryUpdate(productId, newQuantity, current))
                return true;
            
            // Outra thread modificou, tenta novamente
        }
    }
}
```

**Uso em API:**

```csharp
[HttpPost("purchase")]
public IActionResult Purchase(int productId, int quantity)
{
    if (_inventory.PurchaseProduct(productId, quantity))
    {
        return Ok("Purchase successful");
    }
    return BadRequest("Insufficient stock");
}
```

---

## 2. 📊 Analytics: Contador de Visitas

### Problema:

```csharp
public class AnalyticsService
{
    private int _totalVisits = 0;
    private Dictionary<string, int> _pageViews = new();  // ❌ Race condition!

    public void RecordVisit(string page)
    {
        _totalVisits++;  // ❌ Não atômico
        
        if (!_pageViews.ContainsKey(page))
            _pageViews[page] = 0;
        
        _pageViews[page]++;  // ❌ Race condition
    }
}
```

### Solução Otimizada:

```csharp
public class AnalyticsServiceOptimized
{
    private int _totalVisits = 0;
    private readonly ConcurrentDictionary<string, int> _pageViews = new();

    public void RecordVisit(string page)
    {
        // Interlocked para contador simples
        Interlocked.Increment(ref _totalVisits);
        
        // AddOrUpdate atômico
        _pageViews.AddOrUpdate(page, 1, (_, current) => current + 1);
    }

    public int GetTotalVisits() => Volatile.Read(ref _totalVisits);
    
    public Dictionary<string, int> GetTopPages(int top)
    {
        return _pageViews
            .OrderByDescending(kvp => kvp.Value)
            .Take(top)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }
}
```

**Middleware ASP.NET:**

```csharp
public class AnalyticsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly AnalyticsServiceOptimized _analytics;

    public async Task InvokeAsync(HttpContext context)
    {
        _analytics.RecordVisit(context.Request.Path);
        await _next(context);
    }
}
```

---

## 3. 💾 Cache de Aplicação

### Problema:

```csharp
public class CacheService<TKey, TValue> where TKey : notnull
{
    private readonly Dictionary<TKey, TValue> _cache = new();  // ❌ Não thread-safe

    public TValue GetOrLoad(TKey key, Func<TKey, TValue> loader)
    {
        if (_cache.ContainsKey(key))
            return _cache[key];
        
        // Múltiplas threads podem carregar o mesmo valor!
        var value = loader(key);
        _cache[key] = value;
        return value;
    }
}
```

### Solução com Lazy Loading Thread-Safe:

```csharp
public class CacheServiceOptimized<TKey, TValue> where TKey : notnull
{
    private readonly ConcurrentDictionary<TKey, Lazy<TValue>> _cache = new();

    public TValue GetOrLoad(TKey key, Func<TKey, TValue> loader)
    {
        // GetOrAdd garante que apenas uma thread cria o Lazy
        var lazy = _cache.GetOrAdd(key, k => new Lazy<TValue>(() => loader(k)));
        
        // Lazy<T> garante que o loader é executado apenas uma vez
        return lazy.Value;
    }

    public void Invalidate(TKey key)
    {
        _cache.TryRemove(key, out _);
    }

    public void Clear()
    {
        _cache.Clear();
    }
}
```

**Uso Real:**

```csharp
public class ProductService
{
    private readonly CacheServiceOptimized<int, Product> _cache = new();
    private readonly IProductRepository _repository;

    public async Task<Product> GetProductAsync(int id)
    {
        return _cache.GetOrLoad(id, _ => 
        {
            Console.WriteLine($"Loading product {id} from database...");
            return _repository.GetById(id);
        });
    }
}
```

---

## 4. 🔄 Worker Queue (Background Jobs)

### Problema:

```csharp
public class WorkerQueue
{
    private readonly Queue<Action> _tasks = new();  // ❌ Não thread-safe

    public void EnqueueTask(Action task)
    {
        _tasks.Enqueue(task);  // ❌ Race condition
    }

    public void ProcessTasks()
    {
        while (_tasks.Count > 0)
        {
            var task = _tasks.Dequeue();  // ❌ Pode lançar exceção
            task();
        }
    }
}
```

### Solução com BlockingCollection:

```csharp
public class WorkerQueueOptimized : IDisposable
{
    private readonly BlockingCollection<Action> _tasks;
    private readonly CancellationTokenSource _cts = new();
    private readonly Task[] _workers;

    public WorkerQueueOptimized(int workerCount = 4, int maxQueueSize = 100)
    {
        _tasks = new BlockingCollection<Action>(maxQueueSize);
        _workers = new Task[workerCount];

        // Inicia workers
        for (int i = 0; i < workerCount; i++)
        {
            int workerId = i;
            _workers[i] = Task.Run(() => ProcessTasks(workerId));
        }
    }

    public bool TryEnqueueTask(Action task, TimeSpan timeout)
    {
        return _tasks.TryAdd(task, timeout);
    }

    public void EnqueueTask(Action task)
    {
        _tasks.Add(task);
    }

    private void ProcessTasks(int workerId)
    {
        foreach (var task in _tasks.GetConsumingEnumerable(_cts.Token))
        {
            try
            {
                Console.WriteLine($"Worker {workerId} processing task...");
                task();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Worker {workerId} error: {ex.Message}");
            }
        }
    }

    public void Dispose()
    {
        _tasks.CompleteAdding();
        Task.WaitAll(_workers);
        _cts.Cancel();
        _tasks.Dispose();
        _cts.Dispose();
    }
}
```

**Uso:**

```csharp
using var queue = new WorkerQueueOptimized(workerCount: 4);

// Producer enfileira tarefas
for (int i = 0; i < 100; i++)
{
    int taskId = i;
    queue.EnqueueTask(() =>
    {
        Console.WriteLine($"Processing task {taskId}");
        Thread.Sleep(100);
    });
}

// Workers processam automaticamente
Console.ReadKey();
```

---

## 5. 🔐 Session Manager (Web Application)

### Problema:

```csharp
public class SessionManager
{
    private readonly Dictionary<string, UserSession> _sessions = new();  // ❌

    public void CreateSession(string sessionId, UserSession session)
    {
        _sessions[sessionId] = session;  // ❌ Race condition
    }

    public UserSession? GetSession(string sessionId)
    {
        return _sessions.TryGetValue(sessionId, out var session) ? session : null;
    }
}
```

### Solução Thread-Safe:

```csharp
public class SessionManagerOptimized
{
    private readonly ConcurrentDictionary<string, SessionData> _sessions = new();
    private readonly Timer _cleanupTimer;

    public SessionManagerOptimized()
    {
        // Limpeza automática a cada minuto
        _cleanupTimer = new Timer(CleanupExpiredSessions, null, 
            TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
    }

    public string CreateSession(string userId, TimeSpan expiration)
    {
        string sessionId = Guid.NewGuid().ToString();
        var session = new SessionData
        {
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.Add(expiration)
        };

        _sessions.TryAdd(sessionId, session);
        return sessionId;
    }

    public bool ValidateSession(string sessionId, out string? userId)
    {
        userId = null;
        
        if (_sessions.TryGetValue(sessionId, out var session))
        {
            if (DateTime.UtcNow < session.ExpiresAt)
            {
                // Renova expiração
                session.ExpiresAt = DateTime.UtcNow.AddMinutes(30);
                userId = session.UserId;
                return true;
            }
            else
            {
                // Remove sessão expirada
                _sessions.TryRemove(sessionId, out _);
            }
        }

        return false;
    }

    public void InvalidateSession(string sessionId)
    {
        _sessions.TryRemove(sessionId, out _);
    }

    private void CleanupExpiredSessions(object? state)
    {
        var now = DateTime.UtcNow;
        var expired = _sessions
            .Where(kvp => kvp.Value.ExpiresAt < now)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var sessionId in expired)
        {
            _sessions.TryRemove(sessionId, out _);
        }

        if (expired.Count > 0)
        {
            Console.WriteLine($"Cleaned up {expired.Count} expired sessions");
        }
    }

    public class SessionData
    {
        public string UserId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
```

---

## 6. 📞 Rate Limiter para API

### Solução Completa:

```csharp
public class RateLimiter
{
    private readonly ConcurrentDictionary<string, RequestWindow> _windows = new();
    private readonly int _maxRequests;
    private readonly TimeSpan _windowSize;

    public RateLimiter(int maxRequests, TimeSpan windowSize)
    {
        _maxRequests = maxRequests;
        _windowSize = windowSize;
    }

    public bool AllowRequest(string clientId)
    {
        var now = DateTime.UtcNow;
        
        var window = _windows.GetOrAdd(clientId, _ => new RequestWindow
        {
            StartTime = now,
            RequestCount = 0
        });

        lock (window)
        {
            // Reseta janela se expirou
            if (now - window.StartTime > _windowSize)
            {
                window.StartTime = now;
                window.RequestCount = 0;
            }

            if (window.RequestCount < _maxRequests)
            {
                window.RequestCount++;
                return true;
            }

            return false;
        }
    }

    private class RequestWindow
    {
        public DateTime StartTime { get; set; }
        public int RequestCount { get; set; }
    }
}
```

**Middleware ASP.NET:**

```csharp
public class RateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly RateLimiter _limiter;

    public RateLimitMiddleware(RequestDelegate next)
    {
        _next = next;
        _limiter = new RateLimiter(maxRequests: 100, TimeSpan.FromMinutes(1));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientId = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        if (_limiter.AllowRequest(clientId))
        {
            await _next(context);
        }
        else
        {
            context.Response.StatusCode = 429;  // Too Many Requests
            await context.Response.WriteAsync("Rate limit exceeded");
        }
    }
}
```

---

## 🎯 Resumo de Boas Práticas

### ✅ Para Contadores e Flags:
```csharp
Interlocked.Increment(ref counter);
```

### ✅ Para Coleções:
```csharp
ConcurrentDictionary<K, V> dict = new();
ConcurrentQueue<T> queue = new();
```

### ✅ Para Cache:
```csharp
ConcurrentDictionary<K, Lazy<V>> cache = new();
```

### ✅ Para Worker Queues:
```csharp
BlockingCollection<T> queue = new(capacity);
```

### ✅ Para Rate Limiting:
```csharp
SemaphoreSlim semaphore = new(maxConcurrent);
```

### ✅ Para Read-Heavy:
```csharp
ReaderWriterLockSlim rwLock = new();
```

---

## 💡 Lembre-se:

1. **Simplifique primeiro**, otimize depois
2. **Meça sempre** - use profilers e benchmarks
3. **Teste sob carga** - race conditions aparecem com concorrência
4. **Prefira coleções thread-safe** - menos erros
5. **Use Interlocked** para operações simples - muito mais rápido

**Código seguro E performático é possível! 🚀**
