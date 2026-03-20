# 🔏 Double-Checked Locking Pattern

Otimização para inicialização lazy thread-safe. Evita o custo do lock após a primeira inicialização.

---

## 📋 Problema

Singleton lazy precisa ser thread-safe, mas lock em toda chamada é caro:

```csharp
// ❌ Lock sempre - ineficiente
public static Singleton Instance
{
    get
    {
        lock (_lock)  // Lock em TODA chamada, mesmo já inicializado!
        {
            if (_instance == null)
                _instance = new Singleton();
            return _instance;
        }
    }
}
```

## ✅ Solução

Verificar duas vezes - uma sem lock, outra com:

```csharp
// ✅ Double-checked locking
public static Singleton Instance
{
    get
    {
        if (_instance == null)  // Primeiro check (sem lock)
        {
            lock (_lock)
            {
                if (_instance == null)  // Segundo check (com lock)
                    _instance = new Singleton();
            }
        }
        return _instance;  // Retorna sem lock
    }
}
```

---

## 💻 Implementações Corretas

### C# - Com volatile
```csharp
public sealed class Singleton
{
    private static volatile Singleton _instance;  // volatile é CRUCIAL!
    private static readonly object _lock = new object();

    private Singleton() { }

    public static Singleton Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                        _instance = new Singleton();
                }
            }
            return _instance;
        }
    }
}
```

### C# - Com Lazy<T> (RECOMENDADO)
```csharp
// Forma mais simples e segura!
public sealed class Singleton
{
    private static readonly Lazy<Singleton> _instance = 
        new Lazy<Singleton>(() => new Singleton());

    private Singleton() { }

    public static Singleton Instance => _instance.Value;
}
```

### Java - Com volatile
```java
public class Singleton {
    private static volatile Singleton instance;  // volatile obrigatório!

    private Singleton() {}

    public static Singleton getInstance() {
        if (instance == null) {
            synchronized (Singleton.class) {
                if (instance == null) {
                    instance = new Singleton();
                }
            }
        }
        return instance;
    }
}
```

### C++ - Com std::call_once (C++11)
```cpp
#include <mutex>

class Singleton {
    static std::unique_ptr<Singleton> instance;
    static std::once_flag flag;

    Singleton() {}

public:
    static Singleton& getInstance() {
        std::call_once(flag, []() {
            instance.reset(new Singleton());
        });
        return *instance;
    }
};
```

### C++ - Magic Static (C++11+) - MELHOR
```cpp
// Forma mais simples em C++11+
class Singleton {
    Singleton() {}

public:
    static Singleton& getInstance() {
        static Singleton instance;  // Thread-safe desde C++11!
        return instance;
    }
};
```

---

## ⚠️ Por que `volatile` é necessário?

Sem `volatile`, o compilador/CPU pode reordenar instruções:

```
// O que você escreve:
_instance = new Singleton();

// O que pode acontecer (reordenado):
1. Aloca memória
2. Atribui referência a _instance  ← Outro thread vê _instance != null
3. Executa construtor              ← Mas objeto ainda não está pronto!
```

`volatile` previne essa reordenação garantindo que a escrita só seja visível após o objeto estar completamente construído.

---

## ❌ Implementações QUEBRADAS

### Sem volatile (ERRADO!)
```java
// ❌ BROKEN - pode retornar objeto parcialmente construído
private static Singleton instance;  // Falta volatile!

public static Singleton getInstance() {
    if (instance == null) {
        synchronized (Singleton.class) {
            if (instance == null) {
                instance = new Singleton();
            }
        }
    }
    return instance;
}
```

### Sem segundo check (ERRADO!)
```java
// ❌ BROKEN - race condition entre threads
public static Singleton getInstance() {
    if (instance == null) {  // Thread A e B passam aqui
        synchronized (Singleton.class) {
            // Ambas criam instância!
            instance = new Singleton();
        }
    }
    return instance;
}
```

---

## 📊 Quando usar

| Cenário | Recomendação |
|---------|--------------|
| Singleton simples | Use `Lazy<T>` (C#) ou static holder (Java) |
| Cache lazy | Double-checked locking |
| Inicialização one-time | `std::call_once` (C++) ou `Lazy<T>` |
| Alto volume de leituras | Double-checked pode valer |

---

## 🔄 Alternativas Melhores

### C# - Lazy<T>
```csharp
private static readonly Lazy<ExpensiveResource> _resource = 
    new Lazy<ExpensiveResource>(() => LoadResource());

public ExpensiveResource Resource => _resource.Value;
```

### Java - Holder Class Idiom
```java
public class Singleton {
    private Singleton() {}

    private static class Holder {
        static final Singleton INSTANCE = new Singleton();
    }

    public static Singleton getInstance() {
        return Holder.INSTANCE;  // Lazy e thread-safe!
    }
}
```

---

## 🔗 Padrões Relacionados

- [Lock](../Lock/) - Usado dentro do double-check
- [Thread-Local Storage](../ThreadLocalStorage/) - Alternativa para alguns casos

---

## 📚 Referências

- [Double-checked locking - Wikipedia](https://en.wikipedia.org/wiki/Double-checked_locking)
- [The "Double-Checked Locking is Broken" Declaration](https://www.cs.umd.edu/~pugh/java/memoryModel/DoubleCheckedLocking.html)
- [C# and the CLR Memory Model](https://docs.microsoft.com/en-us/archive/msdn-magazine/2012/december/csharp-the-csharp-memory-model-in-theory-and-practice)
