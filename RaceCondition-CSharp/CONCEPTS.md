# Conceitos Fundamentais de Race Condition

## 🎓 O que é Race Condition?

**Race Condition** (Condição de Corrida) é um tipo de bug que ocorre quando o comportamento de um programa depende da ordem ou timing de eventos que não podem ser controlados, especialmente em ambientes com múltiplas threads.

## 🔍 Anatomia de uma Race Condition

### Exemplo Clássico: Incremento de Contador

```csharp
// Código com race condition
int counter = 0;

void IncrementCounter()
{
    counter++;  // Esta operação NÃO é atômica!
}
```

### O que acontece no nível de assembly:

```assembly
MOV EAX, [counter]    ; 1. Lê o valor da memória para registrador
INC EAX               ; 2. Incrementa o valor no registrador
MOV [counter], EAX    ; 3. Escreve o valor de volta na memória
```

### Cenário de Race Condition:

```
Valor inicial: counter = 100

Thread A               Thread B               counter
--------               --------               -------
LÊ 100                                       100
                       LÊ 100                100
INC → 101                                    100
                       INC → 101             100
ESCREVE 101                                  101
                       ESCREVE 101           101

Resultado: 101 (esperado: 102) ❌
```

## 🎯 Tipos de Race Conditions

### 1. **Read-Modify-Write**

Operação de leitura, modificação e escrita não atômica.

```csharp
// Race condition
counter++;

// Solução
Interlocked.Increment(ref counter);
```

### 2. **Check-Then-Act (TOCTOU)**

Time-Of-Check to Time-Of-Use vulnerability.

```csharp
// Race condition
if (balance >= amount)
{
    // Outra thread pode modificar balance aqui!
    balance -= amount;
}

// Solução
lock(_lock)
{
    if (balance >= amount)
    {
        balance -= amount;
    }
}
```

### 3. **Double-Checked Locking**

Padrão que pode causar race condition se implementado incorretamente.

```csharp
// Race condition (singleton incorreto)
if (_instance == null)
{
    _instance = new Singleton(); // Duas threads podem entrar aqui!
}

// Solução
if (_instance == null)
{
    lock(_lock)
    {
        if (_instance == null)
        {
            _instance = new Singleton();
        }
    }
}
```

### 4. **Compound Actions**

Múltiplas operações que devem ser atômicas juntas.

```csharp
// Race condition
if (!dictionary.ContainsKey(key))
{
    dictionary.Add(key, value); // Outra thread pode adicionar entre o if e o Add!
}

// Solução
lock(_lock)
{
    if (!dictionary.ContainsKey(key))
    {
        dictionary.Add(key, value);
    }
}

// Ou melhor ainda
concurrentDict.TryAdd(key, value);
```

## 🚨 Sintomas de Race Condition

### Como Identificar:

1. **Resultados inconsistentes**: Valores diferentes em execuções repetidas
2. **Bugs intermitentes**: Funciona 99% das vezes, falha 1%
3. **Heisenbugs**: Desaparecem quando você tenta debugar
4. **Comportamento dependente de carga**: Funciona com pouca carga, falha com muita
5. **Exceções estranhas**: `IndexOutOfRangeException`, `NullReferenceException`

### Exemplos de Manifestação:

```csharp
// 1. Dados corrompidos
Account balance = 1000;
// Após múltiplas transações simultâneas
// Esperado: 1500, Real: 1342 ❌

// 2. Contador incorreto
int visitors = 0;
// Após 1000 visitas simultâneas
// Esperado: 1000, Real: 847 ❌

// 3. Exceções
List<int> list = new List<int>();
// InvalidOperationException: Collection was modified ❌
```

## 🛠️ Ferramentas para Detectar

### 1. **Testes de Stress**

```csharp
[Test]
public void StressTest_RaceCondition()
{
    const int threads = 100;
    const int iterations = 1000;
    int counter = 0;

    Parallel.For(0, threads, i =>
    {
        for (int j = 0; j < iterations; j++)
        {
            counter++; // Race condition!
        }
    });

    // Se counter != threads * iterations, há race condition
    Assert.AreEqual(threads * iterations, counter);
}
```

### 2. **Thread Sanitizer**

- Ferramenta que detecta automaticamente race conditions
- Disponível em C++, Rust, Go
- Para .NET: Use profilers como dotTrace

### 3. **Code Review**

Procure por padrões suspeitos:

- ❌ Operações em variáveis compartilhadas sem sincronização
- ❌ Check-then-act patterns
- ❌ Uso de coleções não thread-safe
- ❌ Acesso a campos sem volatile/locks

## 🎨 Padrões de Sincronização

### 1. **Mutex/Lock**

```csharp
private readonly object _lock = new object();

public void Method()
{
    lock (_lock)
    {
        // Seção crítica
    }
}
```

**Quando usar**: Seções críticas gerais

### 2. **Atomic Operations**

```csharp
int counter = 0;
Interlocked.Increment(ref counter);
```

**Quando usar**: Operações simples isoladas

### 3. **Reader-Writer Lock**

```csharp
private readonly ReaderWriterLockSlim _rwLock = new();

public void Read()
{
    _rwLock.EnterReadLock();
    try { /* read */ }
    finally { _rwLock.ExitReadLock(); }
}

public void Write()
{
    _rwLock.EnterWriteLock();
    try { /* write */ }
    finally { _rwLock.ExitWriteLock(); }
}
```

**Quando usar**: Muitas leituras, poucas escritas

### 4. **Lock-Free Data Structures**

```csharp
ConcurrentQueue<int> queue = new ConcurrentQueue<int>();
queue.Enqueue(42);
```

**Quando usar**: Alta concorrência, performance crítica

## 📊 Comparação de Soluções

| Solução | Complexidade | Performance | Uso Ideal |
|---------|--------------|-------------|-----------|
| Interlocked | ⭐ Baixa | ⭐⭐⭐⭐⭐ | Operações atômicas simples |
| Lock | ⭐⭐ Média | ⭐⭐⭐ | Seções críticas gerais |
| Monitor | ⭐⭐⭐ Média | ⭐⭐⭐ | Lock com timeouts |
| Semaphore | ⭐⭐⭐ Alta | ⭐⭐ | Rate limiting |
| ReaderWriterLock | ⭐⭐⭐⭐ Alta | ⭐⭐⭐⭐ | Read-heavy workloads |
| Lock-Free | ⭐⭐⭐⭐⭐ Muito Alta | ⭐⭐⭐⭐⭐ | Alta concorrência |

## 💡 Boas Práticas

### DO (Faça):

✅ Use sincronização apropriada para dados compartilhados
✅ Mantenha seções críticas pequenas
✅ Use tipos thread-safe quando disponíveis
✅ Documente requisitos de threading
✅ Teste com múltiplas threads
✅ Use ferramentas de análise estática

### DON'T (Não Faça):

❌ lock(this) ou lock(typeof(T))
❌ Operações longas dentro de locks
❌ Locks aninhados sem ordem definida
❌ Assumir que operações simples são atômicas
❌ Ignorar avisos do compilador
❌ Compartilhar estado mutável sem proteção

## 🔬 Conceitos Avançados

### Memory Barriers

```csharp
// Garante que writes sejam visíveis para outras threads
Thread.MemoryBarrier();
```

### Volatile

```csharp
// Previne otimizações que poderiam reordenar acessos
private volatile bool _flag;
```

### False Sharing

```csharp
// Ruim: Variáveis próximas causam contenção de cache
int counter1;
int counter2;

// Bom: Padding evita false sharing
struct PaddedCounter
{
    public int Value;
    private long _pad1, _pad2, _pad3, _pad4, _pad5, _pad6, _pad7;
}
```

## 📚 Leitura Recomendada

- **C# in Depth** - Jon Skeet
- **CLR via C#** - Jeffrey Richter
- **Concurrency in C# Cookbook** - Stephen Cleary
- **The Art of Multiprocessor Programming** - Herlihy & Shavit

## 🎯 Conclusão

Race conditions são bugs sutis e perigosos, mas compreendendo os conceitos e usando as ferramentas certas, você pode escrever código concorrente seguro e eficiente.

**Lembre-se**: 
- Simplifique primeiro (use locks)
- Otimize depois (perfil e benchmark)
- Teste sempre (stress tests)
