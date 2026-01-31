# 📦 Estruturas de Dados Concorrentes

Depois de entender os primitivos de sincronização, é hora de conhecer estruturas de dados que já foram projetadas para acesso concorrente. Essas estruturas abstraem a complexidade e evitam que você reinvente a roda.

---

## 📂 Conteúdo

### [ConcurrentQueue-CSharp/](ConcurrentQueue-CSharp/)
Demonstração do padrão **Producer-Consumer** usando:
- `ConcurrentQueue<T>` — Fila thread-safe sem bloqueio
- `BlockingCollection<T>` — Wrapper com semântica de bloqueio

**O que você encontra:**
- Múltiplos produtores enfileirando itens
- Múltiplos consumidores desenfileirando
- Comparação entre abordagem polling vs blocking
- Uso correto de `TryDequeue`, `TryPeek`, `ToArray`

---

## 🎯 O que você vai aprender aqui

1. **Producer-Consumer Pattern** — Um dos padrões mais importantes em concorrência
2. **ConcurrentQueue** — Fila lock-free para alta performance
3. **BlockingCollection** — Quando você quer bloqueio ao invés de polling
4. **ConcurrentDictionary** — Dicionário thread-safe
5. **ConcurrentBag** — Coleção não ordenada para alta concorrência

---

## 🔄 Producer-Consumer Pattern

Este é um dos padrões mais fundamentais em programação concorrente:

```
┌──────────┐     ┌─────────────────┐     ┌──────────┐
│ Producer │────▶│      Queue      │────▶│ Consumer │
│   (N)    │     │  (thread-safe)  │     │   (M)    │
└──────────┘     └─────────────────┘     └──────────┘
```

**Casos de uso:**
- Pipeline de processamento de dados
- Sistema de mensageria
- Pool de trabalho (work queue)
- Buffer entre camadas de diferentes velocidades

---

## 📊 Estruturas Disponíveis no .NET

| Estrutura | Descrição | Bloqueante? | Lock-free? |
|-----------|-----------|-------------|------------|
| `ConcurrentQueue<T>` | Fila FIFO | Não | Sim |
| `ConcurrentStack<T>` | Pilha LIFO | Não | Sim |
| `ConcurrentBag<T>` | Coleção não ordenada | Não | Parcial |
| `ConcurrentDictionary<K,V>` | Dicionário thread-safe | Não | Parcial |
| `BlockingCollection<T>` | Wrapper com bloqueio | Sim | Depende |

---

## 💡 Quando usar cada uma

### ConcurrentQueue
```csharp
// Use quando: alta performance, pode fazer polling
while (running)
{
    if (queue.TryDequeue(out var item))
        Process(item);
    else
        Thread.Sleep(1); // ou SpinWait
}
```

### BlockingCollection
```csharp
// Use quando: quer simplificar com bloqueio
foreach (var item in collection.GetConsumingEnumerable())
{
    Process(item); // bloqueia automaticamente quando vazio
}
```

---

## ⚠️ Cuidados importantes

1. **`Count` não é confiável** — Em alta concorrência, o valor pode mudar antes de você usar
2. **Prefira `TryDequeue` a verificar `Count`** — Padrão check-then-act é perigoso
3. **Use `ToArray()` para snapshot** — Se precisar de uma visão consistente

```csharp
// ❌ Errado - race condition
if (queue.Count > 0)
    queue.TryDequeue(out var item); // pode falhar!

// ✅ Correto - atômico
if (queue.TryDequeue(out var item))
    Process(item);
```

---

## 📖 Ordem de estudo sugerida

1. Leia sobre o padrão Producer-Consumer
2. Execute o exemplo em [ConcurrentQueue-CSharp/](ConcurrentQueue-CSharp/)
3. Compare o comportamento com e sem bloqueio
4. Experimente adicionar mais produtores/consumidores

---

## ➡️ Próximo passo

Estruturas concorrentes são ótimas, mas às vezes você precisa de **paralelismo de verdade** para performance. Vá para **[04-Paralelismo/](../04-Paralelismo/)** e aprenda sobre OpenMP e divisão de trabalho.
