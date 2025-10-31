# 🚀 Quick Start - Race Condition em C#

## ⚡ Início Rápido (5 minutos)

### 1. Clonar/Baixar o Projeto

```bash
cd RaceCondition-CSharp
```

### 2. Restaurar Dependências

```bash
dotnet restore
```

### 3. Executar o Projeto

```bash
dotnet run
```

### 4. Navegar pelo Menu

Use as teclas numéricas ou letras para selecionar os exemplos:
- **1-6**: Exemplos de race conditions
- **L, I, S, R, C, M**: Soluções
- **A**: Executar todos
- **H**: Ajuda
- **Q**: Sair

---

## 📝 Guia Rápido de Uso

### Exemplo 1: Ver um problema de Race Condition

1. Execute o programa
2. Pressione `1` (Conta Bancária)
3. Observe a diferença entre versão insegura e segura
4. Leia a explicação técnica

### Exemplo 2: Comparar Soluções

1. Pressione `L` (Lock)
2. Pressione `I` (Interlocked)
3. Compare a performance entre ambos

### Exemplo 3: Teste Interativo

1. Pressione `A` para executar todos os exemplos
2. Acompanhe cada demonstração
3. Pressione qualquer tecla para avançar

---

## 🎯 Estrutura dos Exemplos

Cada exemplo segue este formato:

```
📌 CENÁRIO
   Descrição do problema

❌ VERSÃO INSEGURA
   Demonstração com race condition
   
✅ VERSÃO SEGURA
   Solução correta

📚 EXPLICAÇÃO TÉCNICA
   Por que acontece e como resolver

📊 COMPARAÇÃO DE PERFORMANCE
   Medições práticas
```

---

## 💡 Conceitos-Chave (Resumo)

### 1. **Race Condition**
Múltiplas threads acessam dados compartilhados simultaneamente sem sincronização adequada.

```csharp
// ERRADO ❌
counter++;

// CERTO ✅
Interlocked.Increment(ref counter);
```

### 2. **Lock (Exclusão Mútua)**
Apenas uma thread executa o código por vez.

```csharp
lock(_lockObject)
{
    // Seção crítica
    balance += amount;
}
```

### 3. **Interlocked (Operações Atômicas)**
Operações atômicas em nível de hardware, muito mais rápidas que lock.

```csharp
Interlocked.Increment(ref counter);
Interlocked.Add(ref total, value);
Interlocked.CompareExchange(ref variable, newValue, expectedValue);
```

### 4. **Concurrent Collections**
Coleções thread-safe que eliminam necessidade de locks manuais.

```csharp
ConcurrentDictionary<int, string> dict = new();
dict.TryAdd(1, "Valor");

ConcurrentQueue<int> queue = new();
queue.Enqueue(42);
```

---

## 📊 Escolha Rápida de Solução

| Seu Caso | Use |
|----------|-----|
| Contador simples | `Interlocked.Increment` |
| Múltiplas operações atômicas | `lock { }` |
| Dicionário compartilhado | `ConcurrentDictionary` |
| Fila de tarefas | `ConcurrentQueue` |
| Cache (muitas leituras) | `ReaderWriterLockSlim` |
| Limitar concorrência | `SemaphoreSlim` |

---

## 🔍 Detectar Race Conditions no Seu Código

### Padrões Suspeitos:

```csharp
// ❌ PERIGO: Operação não-atômica
counter++;

// ❌ PERIGO: Check-then-act
if (balance >= amount)
    balance -= amount;

// ❌ PERIGO: List não é thread-safe
list.Add(item);

// ❌ PERIGO: Dictionary não é thread-safe
if (!dict.ContainsKey(key))
    dict.Add(key, value);
```

### Soluções Rápidas:

```csharp
// ✅ Use Interlocked
Interlocked.Increment(ref counter);

// ✅ Use lock
lock(_lock)
{
    if (balance >= amount)
        balance -= amount;
}

// ✅ Use ConcurrentBag
bag.Add(item);

// ✅ Use ConcurrentDictionary
dict.TryAdd(key, value);
```

---

## 🎓 Próximos Passos

1. **Execute todos os exemplos** (pressione `A`)
2. **Leia os READMEs detalhados**:
   - `README.md` - Visão geral do projeto
   - `CONCEPTS.md` - Conceitos fundamentais
   - `Benchmarks/README.md` - Análise de performance
3. **Experimente modificar o código**
4. **Aplique no seu projeto**

---

## 📚 Recursos Adicionais

### Dentro deste Projeto:
- `README.md` - Documentação principal
- `CONCEPTS.md` - Teoria e conceitos
- `Benchmarks/` - Performance comparisons
- Código fonte com comentários detalhados

### Links Úteis:
- [Microsoft Docs - Threading](https://docs.microsoft.com/en-us/dotnet/standard/threading/)
- [Interlocked Class](https://docs.microsoft.com/en-us/dotnet/api/system.threading.interlocked)
- [Concurrent Collections](https://docs.microsoft.com/en-us/dotnet/standard/collections/thread-safe/)

---

## ❓ FAQ Rápido

**Q: Quando devo usar Lock vs Interlocked?**
A: Use Interlocked para operações simples (contador, flags). Use Lock para lógica complexa.

**Q: ConcurrentDictionary é sempre mais rápido?**
A: Não. Para baixa contenção, Dictionary+Lock pode ser mais rápido. Sempre meça!

**Q: Como sei se tenho race condition?**
A: Resultados inconsistentes, bugs intermitentes, valores "impossíveis".

**Q: Lock tem overhead?**
A: Sim, ~25-50ns por operação. Interlocked tem ~5-10ns.

**Q: Posso usar lock(this)?**
A: ❌ Não! Use um objeto privado dedicado: `private readonly object _lock = new();`

---

## 🆘 Problemas Comuns

### "Meu contador está errado mesmo com lock"

Verifique se TODAS as operações estão dentro do lock:

```csharp
// ❌ ERRADO
int temp = counter;
lock(_lock)
{
    counter = temp + 1;
}

// ✅ CERTO
lock(_lock)
{
    counter++;
}
```

### "Interlocked não funciona com decimal"

Interlocked só funciona com int, long, float, double, IntPtr, object:

```csharp
// ❌ Não funciona
decimal value = 10m;
Interlocked.Increment(ref value); // Erro de compilação

// ✅ Use lock para decimal
lock(_lock)
{
    value++;
}
```

### "Meu programa trava (deadlock)"

Sempre adquira locks na mesma ordem:

```csharp
// ✅ CERTO: Ordem consistente
lock(resource1)
{
    lock(resource2)
    {
        // ...
    }
}
```

---

## 🎉 Pronto!

Você está pronto para explorar o mundo de race conditions em C#!

Comece com o exemplo de Conta Bancária (pressione `1`) e vá progredindo pelos demais.

**Boa sorte e código seguro! 🚀**
