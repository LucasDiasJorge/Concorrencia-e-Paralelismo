# 📦 Producer-Consumer Pattern

Um dos padrões mais fundamentais em concorrência. Desacopla a produção de dados do consumo através de uma fila compartilhada.

---

## 📋 Problema

Você tem componentes que **produzem** dados em ritmo diferente dos que **consomem**.

```
Sem buffer:
Produtor RÁPIDO ──────────────────→ Consumidor LENTO
         ^                                 ^
         └── Precisa esperar ──────────────┘
             (acoplamento temporal)
```

## ✅ Solução

Adicionar uma fila entre produtor e consumidor:

```
Com buffer:
Produtor ──→ [Fila] ──→ Consumidor
             buffer
             absorve
             diferença

Produtor pode continuar mesmo se consumidor está ocupado
```

---

## 💻 Implementações

### C# - BlockingCollection
```csharp
public class ProducerConsumer
{
    private readonly BlockingCollection<int> _queue = new(boundedCapacity: 100);

    public void StartProducer()
    {
        Task.Run(() =>
        {
            for (int i = 0; i < 1000; i++)
            {
                _queue.Add(i);  // Bloqueia se fila cheia
                Console.WriteLine($"Produced: {i}");
            }
            _queue.CompleteAdding();  // Sinaliza fim
        });
    }

    public void StartConsumer()
    {
        Task.Run(() =>
        {
            foreach (var item in _queue.GetConsumingEnumerable())
            {
                Process(item);  // Bloqueia se fila vazia
                Console.WriteLine($"Consumed: {item}");
            }
        });
    }
}
```

### C# - Channel (mais moderno)
```csharp
public class ProducerConsumerChannel
{
    private readonly Channel<int> _channel = Channel.CreateBounded<int>(100);

    public async Task ProduceAsync()
    {
        for (int i = 0; i < 1000; i++)
        {
            await _channel.Writer.WriteAsync(i);
        }
        _channel.Writer.Complete();
    }

    public async Task ConsumeAsync()
    {
        await foreach (var item in _channel.Reader.ReadAllAsync())
        {
            await ProcessAsync(item);
        }
    }
}
```

### Java - BlockingQueue
```java
import java.util.concurrent.*;

public class ProducerConsumer {
    private final BlockingQueue<Integer> queue = new ArrayBlockingQueue<>(100);

    public void produce() throws InterruptedException {
        for (int i = 0; i < 1000; i++) {
            queue.put(i);  // Bloqueia se cheio
        }
        queue.put(-1);  // Sentinel para parar
    }

    public void consume() throws InterruptedException {
        while (true) {
            int item = queue.take();  // Bloqueia se vazio
            if (item == -1) break;
            process(item);
        }
    }
}
```

### Go - Channels
```go
func producer(ch chan<- int) {
    for i := 0; i < 1000; i++ {
        ch <- i  // Bloqueia se buffer cheio
    }
    close(ch)  // Sinaliza fim
}

func consumer(ch <-chan int) {
    for item := range ch {  // Bloqueia se vazio, termina quando fechado
        process(item)
    }
}

func main() {
    ch := make(chan int, 100)  // Buffer de 100
    
    go producer(ch)
    consumer(ch)
}
```

### C++ - Com condition_variable
```cpp
#include <queue>
#include <mutex>
#include <condition_variable>

template<typename T>
class BlockingQueue {
    std::queue<T> queue;
    std::mutex mutex;
    std::condition_variable cv;
    size_t capacity;

public:
    BlockingQueue(size_t cap) : capacity(cap) {}

    void push(T item) {
        std::unique_lock<std::mutex> lock(mutex);
        cv.wait(lock, [this] { return queue.size() < capacity; });
        queue.push(std::move(item));
        cv.notify_one();
    }

    T pop() {
        std::unique_lock<std::mutex> lock(mutex);
        cv.wait(lock, [this] { return !queue.empty(); });
        T item = std::move(queue.front());
        queue.pop();
        cv.notify_one();
        return item;
    }
};
```

---

## 🎯 Variações

### 1. Múltiplos Produtores, Múltiplos Consumidores (MPMC)
```csharp
var channel = Channel.CreateBounded<WorkItem>(100);

// N Produtores
for (int i = 0; i < 3; i++)
{
    _ = Task.Run(async () =>
    {
        while (hasWork)
            await channel.Writer.WriteAsync(GetWork());
    });
}

// M Consumidores
for (int i = 0; i < 5; i++)
{
    _ = Task.Run(async () =>
    {
        await foreach (var item in channel.Reader.ReadAllAsync())
            await ProcessAsync(item);
    });
}
```

### 2. Pipeline (cadeia de producer-consumers)
```csharp
// Stage 1: Download
var downloadChannel = Channel.CreateBounded<string>(10);
// Stage 2: Parse
var parseChannel = Channel.CreateBounded<Document>(10);
// Stage 3: Save

_ = Task.Run(async () => // Downloader (Producer)
{
    foreach (var url in urls)
    {
        var content = await DownloadAsync(url);
        await downloadChannel.Writer.WriteAsync(content);
    }
    downloadChannel.Writer.Complete();
});

_ = Task.Run(async () => // Parser (Consumer/Producer)
{
    await foreach (var content in downloadChannel.Reader.ReadAllAsync())
    {
        var doc = Parse(content);
        await parseChannel.Writer.WriteAsync(doc);
    }
    parseChannel.Writer.Complete();
});

_ = Task.Run(async () => // Saver (Consumer)
{
    await foreach (var doc in parseChannel.Reader.ReadAllAsync())
    {
        await SaveAsync(doc);
    }
});
```

### 3. Fan-out / Fan-in
```
Fan-out: 1 produtor → N consumidores
         ┌→ Consumer 1
Producer ┼→ Consumer 2
         └→ Consumer 3

Fan-in: N produtores → 1 consumidor
Producer 1 ─┐
Producer 2 ─┼→ Consumer
Producer 3 ─┘
```

---

## ⚠️ Cuidados

### 1. Bounded vs Unbounded Queue
```csharp
// ❌ Unbounded pode causar OutOfMemoryException
var queue = new BlockingCollection<int>();  // Sem limite!

// ✅ Bounded limita memória e pressiona produtor
var queue = new BlockingCollection<int>(100);  // Max 100 items
```

### 2. Graceful Shutdown
```csharp
// ❌ Sem sinalização de fim
// Consumidor fica esperando para sempre

// ✅ Sinaliza quando terminar
_channel.Writer.Complete();

// Ou use cancellation token
await _channel.Writer.WriteAsync(item, cancellationToken);
```

### 3. Exception Handling
```csharp
try
{
    await foreach (var item in channel.Reader.ReadAllAsync())
    {
        try
        {
            await ProcessAsync(item);
        }
        catch (Exception ex)
        {
            // Log e continua, ou dead-letter queue
            await deadLetterChannel.Writer.WriteAsync((item, ex));
        }
    }
}
catch (ChannelClosedException)
{
    // Canal fechado, termina normalmente
}
```

---

## 📊 Escolhendo o Tamanho do Buffer

| Cenário | Tamanho | Motivo |
|---------|---------|--------|
| Produtor mais rápido | Maior | Absorve picos |
| Consumidor mais rápido | Menor | Não precisa buffer grande |
| Latência crítica | Menor | Menos tempo na fila |
| Throughput crítico | Maior | Menos bloqueios |
| Memória limitada | Menor | Controle de uso |

---

## 🔗 Padrões Relacionados

- [Thread Pool](../ThreadPool/) - Consumidores são workers do pool
- [Monitor](../Monitor/) - Usado para implementar blocking queue
- [Reactor](../Reactor/) - Event queue é producer-consumer

---

## 📚 Referências

- [Producer-consumer problem - Wikipedia](https://en.wikipedia.org/wiki/Producer%E2%80%93consumer_problem)
- [System.Threading.Channels](https://docs.microsoft.com/en-us/dotnet/core/extensions/channels)
- [Go Concurrency Patterns](https://go.dev/blog/pipelines)
