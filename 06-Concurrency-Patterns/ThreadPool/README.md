# 🏊 Thread Pool Pattern

Mantém um conjunto de threads reutilizáveis para executar tarefas, evitando o custo de criar e destruir threads constantemente.

---

## 📋 Problema

Criar threads é caro:
- Alocação de stack (~1MB por thread)
- Registro no SO
- Context switch overhead

```
Sem Pool:
Tarefa 1 → Cria Thread → Executa → Destrói Thread
Tarefa 2 → Cria Thread → Executa → Destrói Thread
...
1000 tarefas = 1000 criações/destruições!
```

## ✅ Solução

Reutilizar threads existentes:

```
Com Pool (4 threads):
┌──────────────────────────────────────────┐
│  Thread Pool                              │
│  ┌────────┐ ┌────────┐ ┌────────┐ ┌────┐ │
│  │ T1     │ │ T2     │ │ T3     │ │ T4 │ │
│  │ Tarefa │ │ Tarefa │ │ Tarefa │ │idle│ │
│  └────────┘ └────────┘ └────────┘ └────┘ │
└──────────────────────────────────────────┘
         ↑
    ┌─────────┐
    │  Fila   │ ← Tarefa 5, 6, 7, 8...
    │ Tarefas │
    └─────────┘

Threads são reutilizadas, fila absorve picos
```

---

## 💻 Implementações

### C# - ThreadPool / Task
```csharp
// Forma mais simples: Task (usa ThreadPool internamente)
await Task.Run(() => ProcessData());

// Acessando ThreadPool diretamente
ThreadPool.QueueUserWorkItem(_ => ProcessData());

// Configurando limites
ThreadPool.SetMinThreads(4, 4);
ThreadPool.SetMaxThreads(100, 100);
```

### C# - Custom Thread Pool
```csharp
public class SimpleThreadPool : IDisposable
{
    private readonly BlockingCollection<Action> _tasks = new();
    private readonly Thread[] _workers;

    public SimpleThreadPool(int workerCount)
    {
        _workers = new Thread[workerCount];
        for (int i = 0; i < workerCount; i++)
        {
            _workers[i] = new Thread(WorkerLoop) { IsBackground = true };
            _workers[i].Start();
        }
    }

    private void WorkerLoop()
    {
        foreach (var task in _tasks.GetConsumingEnumerable())
        {
            try { task(); }
            catch (Exception ex) { Console.WriteLine($"Task failed: {ex}"); }
        }
    }

    public void Submit(Action task) => _tasks.Add(task);

    public void Dispose()
    {
        _tasks.CompleteAdding();
        foreach (var worker in _workers)
            worker.Join();
    }
}
```

### Java - ExecutorService
```java
import java.util.concurrent.*;

// Pool de tamanho fixo
ExecutorService pool = Executors.newFixedThreadPool(4);

// Pool que cresce conforme demanda
ExecutorService cached = Executors.newCachedThreadPool();

// Pool com agendamento
ScheduledExecutorService scheduled = Executors.newScheduledThreadPool(2);

// Submeter tarefas
pool.submit(() -> processData());
Future<Integer> result = pool.submit(() -> compute());

// Shutdown gracioso
pool.shutdown();
pool.awaitTermination(60, TimeUnit.SECONDS);
```

### C++ - Custom Thread Pool
```cpp
#include <thread>
#include <queue>
#include <mutex>
#include <condition_variable>
#include <functional>
#include <vector>

class ThreadPool {
    std::vector<std::thread> workers;
    std::queue<std::function<void()>> tasks;
    std::mutex mutex;
    std::condition_variable cv;
    bool stop = false;

public:
    ThreadPool(size_t threads) {
        for (size_t i = 0; i < threads; ++i) {
            workers.emplace_back([this] {
                while (true) {
                    std::function<void()> task;
                    {
                        std::unique_lock<std::mutex> lock(mutex);
                        cv.wait(lock, [this] { return stop || !tasks.empty(); });
                        if (stop && tasks.empty()) return;
                        task = std::move(tasks.front());
                        tasks.pop();
                    }
                    task();
                }
            });
        }
    }

    void submit(std::function<void()> task) {
        {
            std::lock_guard<std::mutex> lock(mutex);
            tasks.push(std::move(task));
        }
        cv.notify_one();
    }

    ~ThreadPool() {
        {
            std::lock_guard<std::mutex> lock(mutex);
            stop = true;
        }
        cv.notify_all();
        for (auto& worker : workers)
            worker.join();
    }
};
```

### Go - Worker Pool com Goroutines
```go
func workerPool(jobs <-chan int, results chan<- int, workers int) {
    var wg sync.WaitGroup
    
    for i := 0; i < workers; i++ {
        wg.Add(1)
        go func() {
            defer wg.Done()
            for job := range jobs {
                results <- process(job)
            }
        }()
    }
    
    wg.Wait()
    close(results)
}
```

---

## 🎯 Casos de Uso

### 1. Web Server
```csharp
// Cada requisição é processada por uma thread do pool
app.MapGet("/api/data", async () =>
{
    // Executa no ThreadPool automaticamente
    var data = await db.QueryAsync();
    return data;
});
```

### 2. Processamento em Lote
```csharp
var items = GetItemsToProcess();  // 10.000 items

// Processa em paralelo usando ThreadPool
await Parallel.ForEachAsync(items, 
    new ParallelOptions { MaxDegreeOfParallelism = 8 },
    async (item, _) => await ProcessItem(item));
```

### 3. Download Paralelo
```csharp
var urls = GetUrls();

var downloadTasks = urls.Select(url => 
    Task.Run(() => DownloadFile(url)));  // Usa ThreadPool

await Task.WhenAll(downloadTasks);
```

---

## ⚠️ Cuidados

### 1. Não bloquear threads do pool
```csharp
// ❌ Bloqueia thread do pool com I/O síncrono
Task.Run(() => {
    var data = File.ReadAllText("file.txt");  // BLOQUEIA!
});

// ✅ Use async para I/O
Task.Run(async () => {
    var data = await File.ReadAllTextAsync("file.txt");
});
```

### 2. Thread Starvation
```csharp
// ❌ Todas threads esperando umas pelas outras
Parallel.For(0, 1000, i => {
    var result = Task.Run(() => Compute()).Result;  // DEADLOCK POTENCIAL!
});

// ✅ Use async all the way
await Parallel.ForEachAsync(Enumerable.Range(0, 1000), async (i, _) => {
    var result = await Task.Run(() => Compute());
});
```

### 3. Tarefas muito pequenas
```csharp
// ❌ Overhead maior que o trabalho
Parallel.For(0, 1000000, i => {
    sum += i;  // Operação trivial
});

// ✅ Agrupe trabalho
Parallel.For(0, 100, chunk => {
    int localSum = 0;
    for (int i = chunk * 10000; i < (chunk + 1) * 10000; i++)
        localSum += i;
    Interlocked.Add(ref sum, localSum);
});
```

---

## 📊 Dimensionamento do Pool

### Regras gerais:
- **CPU-bound:** `threads = número de cores`
- **I/O-bound:** `threads = cores * (1 + wait_time/compute_time)`
- **Misto:** começar com `cores * 2` e ajustar

### Exemplo de cálculo:
```
CPU: 8 cores
Tarefa: 80% esperando I/O, 20% computando

threads = 8 * (1 + 0.8/0.2) = 8 * 5 = 40 threads
```

---

## 🔄 Arquitetura

```
┌─────────────────────────────────────────────────────────┐
│                     THREAD POOL                          │
├─────────────────────────────────────────────────────────┤
│                                                          │
│  ┌──────────────────────────────────────────────────┐   │
│  │                   TASK QUEUE                      │   │
│  │  [Task1] [Task2] [Task3] [Task4] [Task5] ...     │   │
│  └──────────────────────────────────────────────────┘   │
│                         │                                │
│                         ▼                                │
│  ┌────────┐ ┌────────┐ ┌────────┐ ┌────────┐            │
│  │Worker 1│ │Worker 2│ │Worker 3│ │Worker 4│            │
│  │        │ │        │ │        │ │        │            │
│  │ Task1  │ │ Task2  │ │ Task3  │ │ idle   │            │
│  └────────┘ └────────┘ └────────┘ └────────┘            │
│                                                          │
└─────────────────────────────────────────────────────────┘
```

---

## 🔗 Padrões Relacionados

- [Producer-Consumer](../ProducerConsumer/) - Thread Pool é um caso especial
- [Active Object](../ActiveObject/) - Combina com Thread Pool
- [Reactor](../Reactor/) - Usa pool para handlers

---

## 📚 Referências

- [Thread pool - Wikipedia](https://en.wikipedia.org/wiki/Thread_pool)
- [ThreadPool Class - Microsoft Docs](https://docs.microsoft.com/en-us/dotnet/api/system.threading.threadpool)
- [Java ExecutorService](https://docs.oracle.com/javase/8/docs/api/java/util/concurrent/ExecutorService.html)
