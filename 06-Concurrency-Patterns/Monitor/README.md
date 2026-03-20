# 🖥️ Monitor Pattern

Extensão do Lock que adiciona a capacidade de threads **esperarem por condições** antes de continuar a execução.

---

## 📋 Problema

Às vezes não basta ter acesso exclusivo - você precisa **esperar** que uma condição seja verdadeira.

```
Thread Consumidor: quer consumir item, mas fila está vazia
                   → não pode simplesmente sair e voltar (busy wait)
                   → precisa esperar de forma eficiente
```

## ✅ Solução

Monitor = Lock + Condition Variables

```
Consumidor: [LOCK] → fila vazia? → [WAIT] (libera lock e dorme)
Produtor:   [LOCK] → adiciona item → [SIGNAL] (acorda consumidor) → [UNLOCK]
Consumidor: (acordado) → [LOCK] → consome item → [UNLOCK]
```

---

## 💻 Implementações

### C# - Monitor.Wait / Monitor.Pulse
```csharp
private readonly object _lock = new object();
private Queue<int> _queue = new Queue<int>();

// Produtor
public void Produce(int item)
{
    lock (_lock)
    {
        _queue.Enqueue(item);
        Monitor.Pulse(_lock);  // Acorda UM consumidor esperando
    }
}

// Consumidor
public int Consume()
{
    lock (_lock)
    {
        while (_queue.Count == 0)
        {
            Monitor.Wait(_lock);  // Libera lock e espera
        }
        return _queue.Dequeue();
    }
}
```

### Java - wait / notify
```java
private final Object lock = new Object();
private Queue<Integer> queue = new LinkedList<>();

// Produtor
public void produce(int item) {
    synchronized (lock) {
        queue.add(item);
        lock.notify();  // Acorda um consumidor
    }
}

// Consumidor
public int consume() throws InterruptedException {
    synchronized (lock) {
        while (queue.isEmpty()) {
            lock.wait();  // Libera lock e espera
        }
        return queue.poll();
    }
}
```

### C++ - std::condition_variable
```cpp
#include <mutex>
#include <condition_variable>
#include <queue>

std::mutex mtx;
std::condition_variable cv;
std::queue<int> queue;

// Produtor
void produce(int item) {
    {
        std::lock_guard<std::mutex> lock(mtx);
        queue.push(item);
    }
    cv.notify_one();  // Acorda um consumidor
}

// Consumidor
int consume() {
    std::unique_lock<std::mutex> lock(mtx);
    cv.wait(lock, []{ return !queue.empty(); });  // Espera com predicado
    int item = queue.front();
    queue.pop();
    return item;
}
```

### C - pthread_cond
```c
#include <pthread.h>

pthread_mutex_t mutex = PTHREAD_MUTEX_INITIALIZER;
pthread_cond_t cond = PTHREAD_COND_INITIALIZER;
int queue[100];
int count = 0;

// Produtor
void produce(int item) {
    pthread_mutex_lock(&mutex);
    queue[count++] = item;
    pthread_cond_signal(&cond);
    pthread_mutex_unlock(&mutex);
}

// Consumidor
int consume() {
    pthread_mutex_lock(&mutex);
    while (count == 0) {
        pthread_cond_wait(&cond, &mutex);
    }
    int item = queue[--count];
    pthread_mutex_unlock(&mutex);
    return item;
}
```

---

## ⚠️ Cuidados

### 1. Sempre usar WHILE, nunca IF
```csharp
// ❌ ERRADO - pode ter spurious wakeup
if (_queue.Count == 0)
    Monitor.Wait(_lock);

// ✅ CORRETO - verifica novamente após acordar
while (_queue.Count == 0)
    Monitor.Wait(_lock);
```

**Por quê?** Podem ocorrer:
- **Spurious wakeups**: Thread acorda sem ninguém ter chamado Signal
- **Stolen wakeups**: Outra thread consumiu o item antes de você

### 2. Signal vs SignalAll (Pulse vs PulseAll)
```csharp
// Acorda UMA thread esperando
Monitor.Pulse(_lock);

// Acorda TODAS as threads esperando
Monitor.PulseAll(_lock);
```

Use `PulseAll` quando múltiplas threads podem estar interessadas na mudança.

### 3. Ordem importa
```csharp
// ❌ Signal fora do lock pode perder notificação
lock (_lock) {
    _queue.Enqueue(item);
}
Monitor.Pulse(_lock);  // Thread pode não ver!

// ✅ Signal dentro do lock
lock (_lock) {
    _queue.Enqueue(item);
    Monitor.Pulse(_lock);
}
```

---

## 🔄 Como funciona internamente

```
┌─────────────────────────────────────────────────┐
│                   MONITOR                        │
├─────────────────────────────────────────────────┤
│  ┌─────────────┐     ┌──────────────────────┐   │
│  │   LOCK      │     │   WAIT QUEUE         │   │
│  │             │     │                      │   │
│  │  Thread A   │     │  Thread B (waiting)  │   │
│  │  (owner)    │     │  Thread C (waiting)  │   │
│  └─────────────┘     └──────────────────────┘   │
│                                                  │
│  Wait():  Move owner → wait queue, libera lock  │
│  Pulse(): Move uma thread wait → ready          │
│  PulseAll(): Move todas wait → ready            │
└─────────────────────────────────────────────────┘
```

---

## 📊 Comparativo Wait vs Sleep

| Aspecto | Monitor.Wait() | Thread.Sleep() |
|---------|---------------|----------------|
| Libera lock | ✅ Sim | ❌ Não |
| Pode ser acordado | ✅ Por Pulse | ❌ Só por timeout |
| Uso | Esperar condição | Delay fixo |
| Recurso | Eficiente | Bloqueia recursos |

---

## 🔗 Padrões Relacionados

- [Lock](../Lock/) - Versão mais simples sem condições
- [Guarded Suspension](../GuardedSuspension/) - Usa Monitor internamente
- [Producer-Consumer](../ProducerConsumer/) - Caso de uso clássico

---

## 📚 Referências

- [Monitor (synchronization) - Wikipedia](https://en.wikipedia.org/wiki/Monitor_(synchronization))
- [C.A.R. Hoare - Monitors: An Operating System Structuring Concept (1974)](https://dl.acm.org/doi/10.1145/355620.361161)
- [Threading in C# - Albahari](http://www.albahari.com/threading/part4.aspx)
