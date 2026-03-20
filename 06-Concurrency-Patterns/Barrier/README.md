# 🚧 Barrier Pattern

Ponto de sincronização onde múltiplas threads esperam umas pelas outras antes de continuar. Todas chegam → todas prosseguem.

---

## 📋 Problema

Você tem trabalho dividido em fases, e todas as threads precisam completar uma fase antes de iniciar a próxima.

```
Sem Barrier:
Thread A: [Fase 1] ────────→ [Fase 2] → ...
Thread B: [Fase 1] ──→ [Fase 2] → ...  ❌ Começou antes de A terminar!
Thread C: [Fase 1] ─────────────→ [Fase 2] → ...
```

## ✅ Solução

Barrier sincroniza todas as threads no final de cada fase:

```
Com Barrier:
Thread A: [Fase 1] ────────→ │ → [Fase 2]
Thread B: [Fase 1] ──→ wait  │ → [Fase 2]
Thread C: [Fase 1] ─────────→│ → [Fase 2]
                           Barrier
                     (todas esperaram)
```

---

## 💻 Implementações

### C# - Barrier
```csharp
public class ParallelComputation
{
    private readonly Barrier _barrier;

    public ParallelComputation(int threadCount)
    {
        _barrier = new Barrier(threadCount, barrier =>
        {
            // Executa após todos chegarem, antes de liberar
            Console.WriteLine($"Fase {barrier.CurrentPhaseNumber} completa!");
        });
    }

    public void RunWorker(int workerId)
    {
        for (int phase = 0; phase < 3; phase++)
        {
            // Fase 1: Processamento
            DoWork(workerId, phase);
            
            // Espera todos terminarem
            _barrier.SignalAndWait();
            
            // Fase 2: Próximo passo (todos sincronizados)
        }
    }
}

// Uso
var computation = new ParallelComputation(4);
Parallel.For(0, 4, i => computation.RunWorker(i));
```

### Java - CyclicBarrier
```java
import java.util.concurrent.CyclicBarrier;

public class ParallelComputation {
    private final CyclicBarrier barrier;

    public ParallelComputation(int threads) {
        barrier = new CyclicBarrier(threads, () -> {
            System.out.println("Fase completa!");
        });
    }

    public void runWorker(int workerId) throws Exception {
        for (int phase = 0; phase < 3; phase++) {
            doWork(workerId, phase);
            barrier.await();  // Espera todos
        }
    }
}
```

### C++ - std::barrier (C++20)
```cpp
#include <barrier>
#include <thread>
#include <vector>

std::barrier sync_point(4, []() noexcept {
    std::cout << "Phase complete!\n";
});

void worker(int id) {
    for (int phase = 0; phase < 3; ++phase) {
        do_work(id, phase);
        sync_point.arrive_and_wait();  // Espera todos
    }
}

int main() {
    std::vector<std::thread> threads;
    for (int i = 0; i < 4; ++i)
        threads.emplace_back(worker, i);
    for (auto& t : threads)
        t.join();
}
```

### C - pthread_barrier
```c
#include <pthread.h>

pthread_barrier_t barrier;

void* worker(void* arg) {
    int id = *(int*)arg;
    
    for (int phase = 0; phase < 3; phase++) {
        do_work(id, phase);
        pthread_barrier_wait(&barrier);  // Espera todos
    }
    return NULL;
}

int main() {
    pthread_barrier_init(&barrier, NULL, 4);  // 4 threads
    
    pthread_t threads[4];
    int ids[4] = {0, 1, 2, 3};
    
    for (int i = 0; i < 4; i++)
        pthread_create(&threads[i], NULL, worker, &ids[i]);
    
    for (int i = 0; i < 4; i++)
        pthread_join(threads[i], NULL);
    
    pthread_barrier_destroy(&barrier);
}
```

### Go - sync.WaitGroup (similar)
```go
import "sync"

func main() {
    var wg sync.WaitGroup
    phases := 3
    workers := 4

    for phase := 0; phase < phases; phase++ {
        wg.Add(workers)
        
        for i := 0; i < workers; i++ {
            go func(id int) {
                doWork(id, phase)
                wg.Done()
            }(i)
        }
        
        wg.Wait()  // Espera todos terminarem a fase
        fmt.Printf("Phase %d complete!\n", phase)
    }
}
```

---

## 🎯 Casos de Uso

### 1. Simulação em Fases
```csharp
// Simulação de física onde cada frame depende do anterior
public void SimulationStep()
{
    // Fase 1: Calcular forças (paralelo)
    Parallel.ForEach(particles, p => p.CalculateForces());
    _barrier.SignalAndWait();
    
    // Fase 2: Atualizar posições (paralelo)
    Parallel.ForEach(particles, p => p.UpdatePosition());
    _barrier.SignalAndWait();
    
    // Fase 3: Detectar colisões (paralelo)
    Parallel.ForEach(particles, p => p.DetectCollisions());
    _barrier.SignalAndWait();
}
```

### 2. MapReduce Local
```csharp
// Map: todos processam em paralelo
// Barrier
// Reduce: combina resultados
public int[] ParallelSort(int[] data)
{
    var chunks = SplitIntoChunks(data, _threadCount);
    
    // Map phase: cada thread ordena seu chunk
    Parallel.For(0, _threadCount, i => {
        Array.Sort(chunks[i]);
        _barrier.SignalAndWait();
    });
    
    // Reduce phase: merge dos chunks ordenados
    return MergeSorted(chunks);
}
```

### 3. Testes de Carga
```csharp
// Todas as threads começam ao mesmo tempo
public async Task LoadTest()
{
    var threads = 100;
    var barrier = new Barrier(threads);
    
    var tasks = Enumerable.Range(0, threads).Select(_ => Task.Run(() =>
    {
        // Prepara
        var client = new HttpClient();
        
        // Espera todos estarem prontos
        barrier.SignalAndWait();
        
        // AGORA! Todos disparam juntos
        return client.GetAsync("http://api/endpoint");
    }));
    
    await Task.WhenAll(tasks);
}
```

---

## ⚠️ Cuidados

### 1. Deadlock se thread não chegar
```csharp
var barrier = new Barrier(4);

// Se apenas 3 threads chegarem, DEADLOCK!
// Todas esperam para sempre pela 4ª
```

**Solução:** Use timeout ou cancellation
```csharp
if (!_barrier.SignalAndWait(TimeSpan.FromSeconds(30)))
{
    throw new TimeoutException("Barrier timeout!");
}
```

### 2. Exceção em uma thread
```csharp
// ❌ Se uma thread lança exceção, outras esperam para sempre
try {
    DoWork();
    _barrier.SignalAndWait();
} catch {
    // Outras threads ainda esperando!
}

// ✅ Usar RemoveParticipant ou redesenhar
try {
    DoWork();
    _barrier.SignalAndWait();
} catch {
    _barrier.RemoveParticipant();  // Libera as outras
    throw;
}
```

---

## 📊 Barrier vs Outras Primitivas

| Primitiva | Uso | Reutilizável? |
|-----------|-----|---------------|
| **Barrier** | Sincronizar N threads em fases | ✅ Sim |
| **CountdownEvent** | Esperar N sinais | ❌ Reset manual |
| **ManualResetEvent** | Sinalizar evento para múltiplos | ❌ Não conta |
| **Semaphore** | Limitar acesso | ✅ Sim |

---

## 🔄 Variações

### Sense-Reversing Barrier
Otimização que evita reset explícito alternando "sense" a cada fase.

### Tree Barrier
Para grande número de threads, organiza em árvore para reduzir contenção.

---

## 🔗 Padrões Relacionados

- [Monitor](../Monitor/) - Pode implementar barrier com wait/pulse
- [Semaphore](../Semaphore/) - Alternativa para alguns casos
- [Thread Pool](../ThreadPool/) - Frequentemente usado junto

---

## 📚 Referências

- [Barrier (computer science) - Wikipedia](https://en.wikipedia.org/wiki/Barrier_(computer_science))
- [System.Threading.Barrier](https://docs.microsoft.com/en-us/dotnet/api/system.threading.barrier)
- [CyclicBarrier - Java](https://docs.oracle.com/javase/8/docs/api/java/util/concurrent/CyclicBarrier.html)
