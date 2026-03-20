# 🚦 Semaphore Pattern

Controla o número de threads que podem acessar um recurso simultaneamente. Diferente do Lock (que permite apenas 1), o Semaphore permite N.

---

## 📋 Problema

Você tem um recurso limitado (ex: pool de conexões) e precisa limitar quantas threads podem usá-lo ao mesmo tempo.

```
Recurso: 3 conexões de banco
Threads: 10 querendo conectar

Sem controle: 10 threads tentam, 7 falham ou criam conexões demais
Com Semaphore(3): Apenas 3 executam, outras 7 esperam
```

## ✅ Solução

Semaphore mantém um contador que é decrementado ao entrar e incrementado ao sair.

```
Semaphore(3):  [3]

Thread A entra: [2]
Thread B entra: [1]
Thread C entra: [0]
Thread D tenta: [0] → BLOQUEIA (espera)

Thread A sai:   [1] → Thread D é liberada
Thread D entra: [0]
```

---

## 💻 Implementações

### C# - SemaphoreSlim
```csharp
// Permite 3 acessos simultâneos
private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(3);

public async Task AccessResourceAsync()
{
    await _semaphore.WaitAsync();  // Decrementa (ou espera)
    try
    {
        // Acessa recurso limitado
        await UseConnectionAsync();
    }
    finally
    {
        _semaphore.Release();  // Incrementa
    }
}
```

### C# - Semaphore (sistema)
```csharp
// Semaphore nomeado (cross-process)
using var semaphore = new Semaphore(3, 3, "MyAppSemaphore");

semaphore.WaitOne();  // Entra
try
{
    // Usa recurso
}
finally
{
    semaphore.Release();  // Sai
}
```

### Java - Semaphore
```java
import java.util.concurrent.Semaphore;

Semaphore semaphore = new Semaphore(3);

public void accessResource() throws InterruptedException {
    semaphore.acquire();  // Entra
    try {
        // Usa recurso
    } finally {
        semaphore.release();  // Sai
    }
}
```

### C++ - std::counting_semaphore (C++20)
```cpp
#include <semaphore>

std::counting_semaphore<3> semaphore(3);

void accessResource() {
    semaphore.acquire();  // Entra
    // Usa recurso
    semaphore.release();  // Sai
}
```

### C - POSIX sem_t
```c
#include <semaphore.h>

sem_t semaphore;
sem_init(&semaphore, 0, 3);  // Inicializa com 3

void accessResource() {
    sem_wait(&semaphore);    // Entra (decrementa)
    // Usa recurso
    sem_post(&semaphore);    // Sai (incrementa)
}
```

---

## 🎯 Casos de Uso

### 1. Connection Pool
```csharp
public class ConnectionPool
{
    private readonly SemaphoreSlim _pool;
    private readonly ConcurrentBag<Connection> _connections;

    public ConnectionPool(int maxConnections)
    {
        _pool = new SemaphoreSlim(maxConnections);
        _connections = new ConcurrentBag<Connection>();
        
        for (int i = 0; i < maxConnections; i++)
            _connections.Add(new Connection());
    }

    public async Task<Connection> GetConnectionAsync()
    {
        await _pool.WaitAsync();
        _connections.TryTake(out var conn);
        return conn;
    }

    public void ReturnConnection(Connection conn)
    {
        _connections.Add(conn);
        _pool.Release();
    }
}
```

### 2. Rate Limiting
```csharp
public class RateLimiter
{
    private readonly SemaphoreSlim _limiter;

    public RateLimiter(int maxRequestsPerSecond)
    {
        _limiter = new SemaphoreSlim(maxRequestsPerSecond);
        
        // Repõe tokens periodicamente
        Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(1000);
                int toRelease = maxRequestsPerSecond - _limiter.CurrentCount;
                if (toRelease > 0)
                    _limiter.Release(toRelease);
            }
        });
    }

    public async Task<bool> TryAcquireAsync()
    {
        return await _limiter.WaitAsync(0);  // Não bloqueia
    }
}
```

### 3. Throttling de Downloads
```csharp
private readonly SemaphoreSlim _downloadLimiter = new SemaphoreSlim(5);

public async Task DownloadFilesAsync(IEnumerable<string> urls)
{
    var tasks = urls.Select(async url =>
    {
        await _downloadLimiter.WaitAsync();
        try
        {
            await DownloadFileAsync(url);
        }
        finally
        {
            _downloadLimiter.Release();
        }
    });

    await Task.WhenAll(tasks);
}
```

---

## ⚠️ Cuidados

### 1. Sempre usar try-finally
```csharp
// ❌ Perigoso - se exceção ocorrer, semaphore nunca é liberado
_semaphore.Wait();
DoWork();  // Se lançar exceção...
_semaphore.Release();  // Nunca executa!

// ✅ Seguro
_semaphore.Wait();
try
{
    DoWork();
}
finally
{
    _semaphore.Release();  // Sempre executa
}
```

### 2. Não liberar mais do que adquiriu
```csharp
// ❌ Bug - libera sem ter adquirido
_semaphore.Release();  // Incrementa contador além do limite!

// O semaphore pode acabar permitindo mais acessos do que deveria
```

### 3. Binary Semaphore vs Mutex
```csharp
// Semaphore(1) parece mutex, mas não é igual!
var sem = new SemaphoreSlim(1);

// Diferença: qualquer thread pode Release()
// Mutex: apenas quem fez lock pode unlock

// Thread A
sem.Wait();

// Thread B
sem.Release();  // ✅ Funciona com Semaphore
                // ❌ Mutex lançaria exceção
```

---

## 🔄 Tipos de Semaphore

| Tipo | Descrição | Exemplo |
|------|-----------|---------|
| **Counting** | Permite N acessos | Pool de conexões |
| **Binary** | Permite 1 acesso (como mutex) | Exclusão mútua |
| **Named** | Compartilhado entre processos | Coordenação entre apps |

---

## 📊 Semaphore vs Mutex vs Lock

| Aspecto | Lock/Mutex | Semaphore |
|---------|-----------|-----------|
| Acessos simultâneos | 1 | N (configurável) |
| Owner | Thread específica | Nenhum owner |
| Release por outra thread | ❌ | ✅ |
| Reentrant | Pode ser | Não |
| Uso típico | Exclusão mútua | Limitar recursos |

---

## 🔗 Padrões Relacionados

- [Lock](../Lock/) - Caso especial: Semaphore(1)
- [Thread Pool](../ThreadPool/) - Usa semaphore internamente
- [Producer-Consumer](../ProducerConsumer/) - Pode usar semaphore para limite

---

## 📚 Referências

- [Semaphore (programming) - Wikipedia](https://en.wikipedia.org/wiki/Semaphore_(programming))
- [Dijkstra - Cooperating Sequential Processes (1965)](https://www.cs.utexas.edu/users/EWD/transcriptions/EWD01xx/EWD123.html)
- [SemaphoreSlim - Microsoft Docs](https://docs.microsoft.com/en-us/dotnet/api/system.threading.semaphoreslim)
