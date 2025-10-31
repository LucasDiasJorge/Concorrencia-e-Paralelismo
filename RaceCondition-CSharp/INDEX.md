# 📑 Índice do Projeto - Race Condition em C#

## 🎯 Navegação Rápida

### 📖 Documentação

| Arquivo | Descrição | Tempo de Leitura |
|---------|-----------|------------------|
| [README.md](README.md) | Visão geral completa do projeto | 15 min |
| [QUICKSTART.md](QUICKSTART.md) | Guia de início rápido | 5 min |
| [CONCEPTS.md](CONCEPTS.md) | Conceitos fundamentais e teoria | 20 min |
| [SUMMARY.md](SUMMARY.md) | Resumo executivo | 10 min |
| [REAL-WORLD-EXAMPLES.md](REAL-WORLD-EXAMPLES.md) | Casos de uso reais | 15 min |

---

## 🔍 Exemplos de Race Conditions

| Arquivo | Problema | Solução | Cenário Real |
|---------|----------|---------|--------------|
| [01-BankAccountRaceCondition.cs](Examples/01-BankAccountRaceCondition.cs) | Depósitos/saques simultâneos | Lock | Sistema bancário |
| [02-CounterRaceCondition.cs](Examples/02-CounterRaceCondition.cs) | Incrementos perdidos | Interlocked | Analytics, métricas |
| [03-ListRaceCondition.cs](Examples/03-ListRaceCondition.cs) | Coleções não thread-safe | ConcurrentCollections | Filas, processamento |
| [04-CacheRaceCondition.cs](Examples/04-CacheRaceCondition.cs) | Leitura/escrita simultânea | ReaderWriterLock | Cache de aplicação |

---

## ✅ Soluções e Técnicas

| Arquivo | Técnica | Performance | Complexidade | Melhor Para |
|---------|---------|-------------|--------------|-------------|
| [01-LockSolution.cs](Solutions/01-LockSolution.cs) | Lock/Monitor | ⭐⭐⭐ | ⭐⭐ | Caso geral |
| [02-InterlockedSolution.cs](Solutions/02-InterlockedSolution.cs) | Interlocked | ⭐⭐⭐⭐⭐ | ⭐ | Contadores, flags |
| [03-SemaphoreSolution.cs](Solutions/03-SemaphoreSolution.cs) | Semaphore | ⭐⭐ | ⭐⭐⭐ | Rate limiting |
| [04-ReaderWriterLockSolution.cs](Solutions/04-ReaderWriterLockSolution.cs) | ReaderWriterLock | ⭐⭐⭐⭐ | ⭐⭐⭐⭐ | Cache read-heavy |
| [05-ConcurrentCollectionsSolution.cs](Solutions/05-ConcurrentCollectionsSolution.cs) | Concurrent Collections | ⭐⭐⭐⭐ | ⭐⭐ | Coleções |
| [06-MonitorSolution.cs](Solutions/06-MonitorSolution.cs) | Monitor Wait/Pulse | ⭐⭐⭐ | ⭐⭐⭐⭐ | Producer/Consumer |

---

## 📦 Modelos Base

| Arquivo | Descrição | Recursos |
|---------|-----------|----------|
| [BankAccount.cs](Models/BankAccount.cs) | Conta bancária com versões safe/unsafe | Depósitos, saques |
| [SharedCounter.cs](Models/SharedCounter.cs) | Contador com múltiplas implementações | Incremento, CAS |
| [SharedCache.cs](Models/SharedCache.cs) | Cache com diferentes sincronizações | Get, Set, Lock types |

---

## 📊 Benchmarks

| Arquivo | Conteúdo |
|---------|----------|
| [README.md](Benchmarks/README.md) | Análise detalhada de performance |
| [PerformanceComparison.cs](Benchmarks/PerformanceComparison.cs) | Benchmarks com BenchmarkDotNet |

---

## 🎓 Guias de Aprendizado

### Iniciante (Comece aqui!)

1. ✅ Leia [QUICKSTART.md](QUICKSTART.md) (5 min)
2. ✅ Execute o programa: `dotnet run`
3. ✅ Veja exemplo 1 (Conta Bancária)
4. ✅ Teste a solução com Lock

### Intermediário

1. ✅ Leia [CONCEPTS.md](CONCEPTS.md)
2. ✅ Execute todos os exemplos
3. ✅ Compare Lock vs Interlocked
4. ✅ Explore ConcurrentCollections

### Avançado

1. ✅ Leia [REAL-WORLD-EXAMPLES.md](REAL-WORLD-EXAMPLES.md)
2. ✅ Execute benchmarks (modo Release)
3. ✅ Implemente seu próprio exemplo
4. ✅ Otimize para seu caso de uso

---

## 🔗 Links Rápidos por Tópico

### Performance

- [Benchmarks/README.md](Benchmarks/README.md) - Comparações detalhadas
- [02-InterlockedSolution.cs](Solutions/02-InterlockedSolution.cs) - Técnica mais rápida
- [04-ReaderWriterLockSolution.cs](Solutions/04-ReaderWriterLockSolution.cs) - Otimizado para leitura

### Segurança

- [01-LockSolution.cs](Solutions/01-LockSolution.cs) - Prevenção de deadlock
- [CONCEPTS.md](CONCEPTS.md) - Detectar race conditions

### Casos Práticos

- [REAL-WORLD-EXAMPLES.md](REAL-WORLD-EXAMPLES.md) - E-commerce, cache, etc.
- [05-ConcurrentCollectionsSolution.cs](Solutions/05-ConcurrentCollectionsSolution.cs) - Coleções thread-safe

### Patterns Avançados

- [06-MonitorSolution.cs](Solutions/06-MonitorSolution.cs) - Wait/Pulse
- [04-ReaderWriterLockSolution.cs](Solutions/04-ReaderWriterLockSolution.cs) - Upgradeable locks

---

## 📝 Cheatsheet Rápido

### Escolha a Solução Certa

```
Contador simples      → Interlocked.Increment
Seção crítica        → lock { }
Coleção compartilhada → ConcurrentDictionary
Cache (80% reads)    → ReaderWriterLockSlim
Rate limiting        → SemaphoreSlim
Worker queue         → BlockingCollection
```

### Padrões Comuns

```csharp
// Lock básico
lock (_lock) { /* código */ }

// Interlocked
Interlocked.Increment(ref counter);

// ConcurrentDictionary
dict.TryAdd(key, value);
dict.GetOrAdd(key, k => CreateValue(k));

// ReaderWriterLock
_rwLock.EnterReadLock();
try { /* read */ }
finally { _rwLock.ExitReadLock(); }

// Semaphore
await semaphore.WaitAsync();
try { /* trabalho */ }
finally { semaphore.Release(); }
```

---

## 🎯 Por Caso de Uso

| Preciso... | Veja... |
|------------|---------|
| Incrementar um contador | [02-InterlockedSolution.cs](Solutions/02-InterlockedSolution.cs) |
| Proteger seção crítica | [01-LockSolution.cs](Solutions/01-LockSolution.cs) |
| Dicionário compartilhado | [05-ConcurrentCollectionsSolution.cs](Solutions/05-ConcurrentCollectionsSolution.cs) |
| Cache de aplicação | [04-CacheRaceCondition.cs](Examples/04-CacheRaceCondition.cs) |
| Fila de trabalho | [05-ConcurrentCollectionsSolution.cs](Solutions/05-ConcurrentCollectionsSolution.cs) |
| Limitar requests | [03-SemaphoreSolution.cs](Solutions/03-SemaphoreSolution.cs) |
| Controle de estoque | [REAL-WORLD-EXAMPLES.md](REAL-WORLD-EXAMPLES.md#1--e-commerce-controle-de-estoque) |
| Analytics/métricas | [REAL-WORLD-EXAMPLES.md](REAL-WORLD-EXAMPLES.md#2--analytics-contador-de-visitas) |

---

## 📚 Ordem de Leitura Recomendada

### Para Estudantes
1. QUICKSTART.md
2. Executar exemplos 1 e 2
3. CONCEPTS.md
4. Executar todos os exemplos
5. REAL-WORLD-EXAMPLES.md

### Para Desenvolvedores
1. SUMMARY.md
2. Executar exemplo relevante ao seu problema
3. REAL-WORLD-EXAMPLES.md
4. Benchmarks/README.md
5. Implementar solução

### Para Professores
1. README.md completo
2. CONCEPTS.md
3. Todos os exemplos
4. Preparar laboratório baseado nos exemplos

---

## 🆘 Problemas Comuns

| Problema | Solução | Arquivo |
|----------|---------|---------|
| Contador incorreto | Use Interlocked | [02-CounterRaceCondition.cs](Examples/02-CounterRaceCondition.cs) |
| Saldo negativo | Use Lock | [01-BankAccountRaceCondition.cs](Examples/01-BankAccountRaceCondition.cs) |
| Exception em List | Use ConcurrentBag | [03-ListRaceCondition.cs](Examples/03-ListRaceCondition.cs) |
| Cache lento | Use ReaderWriterLock | [04-CacheRaceCondition.cs](Examples/04-CacheRaceCondition.cs) |

---

## 🎓 Certificado de Conclusão

Você completou o projeto quando:

- ✅ Executou todos os 4 exemplos
- ✅ Testou todas as 6 soluções
- ✅ Entendeu Lock vs Interlocked
- ✅ Conhece ConcurrentCollections
- ✅ Leu CONCEPTS.md
- ✅ Aplicou em um projeto real

**Parabéns! Você domina race conditions em C#! 🏆**

---

## 📞 Recursos Adicionais

- 📖 [Microsoft Docs - Threading](https://docs.microsoft.com/en-us/dotnet/standard/threading/)
- ⚡ [Interlocked Class](https://docs.microsoft.com/en-us/dotnet/api/system.threading.interlocked)
- 📦 [Concurrent Collections](https://docs.microsoft.com/en-us/dotnet/standard/collections/thread-safe/)
- 🔬 [BenchmarkDotNet](https://benchmarkdotnet.org/)

---

**Criado com ❤️ para aprendizado de programação concorrente em C#**
