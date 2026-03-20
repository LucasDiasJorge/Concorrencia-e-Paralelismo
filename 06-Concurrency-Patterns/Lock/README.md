# 🔐 Lock Pattern

O padrão mais fundamental de sincronização. Garante que apenas uma thread por vez acesse uma seção crítica do código.

---

## 📋 Problema

Múltiplas threads tentam modificar o mesmo recurso simultaneamente, causando race conditions.

```
Thread A: lê valor (10) → incrementa → escreve (11)
Thread B:        lê valor (10) → incrementa → escreve (11)

Resultado: 11 (deveria ser 12!)
```

## ✅ Solução

Usar um lock para garantir acesso exclusivo:

```
Thread A: [LOCK] → lê (10) → incrementa → escreve (11) → [UNLOCK]
Thread B:                                                  [LOCK] → lê (11) → incrementa → escreve (12) → [UNLOCK]

Resultado: 12 ✓
```

---

## 💻 Implementações

### C# - lock statement
```csharp
private readonly object _lockObj = new object();
private int _counter = 0;

public void Increment()
{
    lock (_lockObj)
    {
        _counter++;
    }
}
```

### C++ - std::mutex
```cpp
#include <mutex>

std::mutex mtx;
int counter = 0;

void increment() {
    std::lock_guard<std::mutex> lock(mtx);
    counter++;
}
```

### C - pthread_mutex
```c
#include <pthread.h>

pthread_mutex_t mutex = PTHREAD_MUTEX_INITIALIZER;
int counter = 0;

void increment() {
    pthread_mutex_lock(&mutex);
    counter++;
    pthread_mutex_unlock(&mutex);
}
```

### Java - synchronized
```java
private final Object lock = new Object();
private int counter = 0;

public void increment() {
    synchronized (lock) {
        counter++;
    }
}
```

### Rust - Mutex
```rust
use std::sync::Mutex;

let counter = Mutex::new(0);

{
    let mut num = counter.lock().unwrap();
    *num += 1;
} // lock é liberado automaticamente aqui
```

---

## ⚠️ Cuidados

### 1. Deadlock
```csharp
// Thread A
lock (lockA) {
    lock (lockB) { /* ... */ }  // espera lockB
}

// Thread B
lock (lockB) {
    lock (lockA) { /* ... */ }  // espera lockA
}
// DEADLOCK! Ambas esperam eternamente
```

**Solução:** Sempre adquirir locks na mesma ordem.

### 2. Lock muito amplo
```csharp
// ❌ Ruim - lock por muito tempo
lock (_lock) {
    var data = FetchFromDatabase();  // I/O lento!
    ProcessData(data);
    SaveToDatabase(data);
}

// ✅ Melhor - lock apenas no necessário
var data = FetchFromDatabase();
var processed = ProcessData(data);
lock (_lock) {
    _sharedState = processed;
}
SaveToDatabase(processed);
```

### 3. Não usar lock em tipos valor
```csharp
// ❌ NUNCA faça isso
lock (42) { }           // boxing cria objeto diferente cada vez
lock ("string") { }     // strings são internadas, pode bloquear código externo

// ✅ Sempre use objeto dedicado
private readonly object _lock = new object();
lock (_lock) { }
```

---

## 🔄 Variações

| Variação | Descrição | Quando usar |
|----------|-----------|-------------|
| **Mutex** | Lock básico | Propósito geral |
| **SpinLock** | Busy-wait loop | Seções muito curtas |
| **ReentrantLock** | Permite reentrada | Chamadas recursivas |
| **ReadWriteLock** | Separa leitura/escrita | Muitas leituras |

---

## 📊 Performance

| Operação | Custo aproximado |
|----------|------------------|
| Lock sem contenção | ~20 ns |
| Lock com contenção | ~1-10 μs (depende da espera) |
| Context switch | ~1-10 μs |

> Para operações simples (++, --), considere usar [operações atômicas](../../02-Sincronizacao/Atomic-Operations/) que são mais rápidas.

---

## 🔗 Padrões Relacionados

- [Monitor](../Monitor/) - Lock + condições de espera
- [Semaphore](../Semaphore/) - Múltiplos acessos simultâneos
- [Double-Checked Locking](../DoubleCheckedLocking/) - Otimização para lazy init

---

## 📚 Referências

- [Monitor (synchronization) - Wikipedia](https://en.wikipedia.org/wiki/Lock_(computer_science))
- [C# lock statement](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/statements/lock)
- [std::mutex - cppreference](https://en.cppreference.com/w/cpp/thread/mutex)
