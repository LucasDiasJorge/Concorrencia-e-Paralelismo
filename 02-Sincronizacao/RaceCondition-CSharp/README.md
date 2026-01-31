# Race Condition em C# - Guia Completo

## 📋 Índice

1. [O que é Race Condition?](#o-que-é-race-condition)
2. [Estrutura do Projeto](#estrutura-do-projeto)
3. [Exemplos Implementados](#exemplos-implementados)
4. [Como Executar](#como-executar)
5. [Análise de Performance](#análise-de-performance)

## 🎯 O que é Race Condition?

**Race Condition** (Condição de Corrida) ocorre quando múltiplas threads acessam e modificam dados compartilhados simultaneamente, resultando em comportamento imprevisível e incorreto.

### Características Principais:

- **Não-determinístico**: O resultado depende da ordem de execução das threads
- **Difícil de reproduzir**: Pode não ocorrer sempre, dificultando o debug
- **Silencioso**: Pode causar bugs sutis que só aparecem em produção
- **Perigoso**: Pode corromper dados e causar falhas no sistema

### Exemplo Visual:

```
Thread 1: Lê valor (100) → Incrementa (101) → Escreve (101)
Thread 2:              Lê valor (100) → Incrementa (101) → Escreve (101)

Resultado Esperado: 102
Resultado Real: 101 ❌
```

## 🏗️ Estrutura do Projeto

```
RaceCondition-CSharp/
├── README.md
├── RaceCondition.csproj
├── Program.cs (menu principal)
│
├── Examples/
│   ├── 01-BankAccountRaceCondition.cs
│   ├── 02-CounterRaceCondition.cs
│   ├── 03-ListRaceCondition.cs
│   └── 04-CacheRaceCondition.cs
│
├── Solutions/
│   ├── 01-LockSolution.cs
│   ├── 02-InterlockedSolution.cs
│   ├── 03-SemaphoreSolution.cs
│   ├── 04-ReaderWriterLockSolution.cs
│   ├── 05-ConcurrentCollectionsSolution.cs
│   └── 06-MonitorSolution.cs
│
├── Benchmarks/
│   ├── PerformanceComparison.cs
│   └── README.md
│
└── Models/
    ├── BankAccount.cs
    ├── SharedCounter.cs
    └── SharedCache.cs
```

## 🔬 Exemplos Implementados

### 1. **Bank Account Race Condition**
- **Problema**: Depósitos/saques simultâneos corrompem o saldo
- **Cenário Real**: Sistemas bancários, e-commerce
- **Soluções**: Lock, Interlocked, Monitor

### 2. **Counter Race Condition**
- **Problema**: Incrementos perdidos em contadores compartilhados
- **Cenário Real**: Analytics, métricas, contadores de visitas
- **Soluções**: Interlocked (mais eficiente)

### 3. **Collection Race Condition**
- **Problema**: Modificações simultâneas em listas causam exceções
- **Cenário Real**: Filas de processamento, cache
- **Soluções**: ConcurrentCollections

### 4. **Cache Race Condition**
- **Problema**: Leituras e escritas simultâneas no cache
- **Cenário Real**: Cache de aplicação, session storage
- **Soluções**: ReaderWriterLockSlim (otimizado para leitura)

## 🚀 Como Executar

### Pré-requisitos

- .NET 8.0 SDK ou superior
- Visual Studio 2022 / VS Code / Rider

### Comandos

```bash
# Navegar até o diretório
cd RaceCondition-CSharp

# Restaurar dependências
dotnet restore

# Executar o projeto
dotnet run

# Executar benchmarks
dotnet run --configuration Release
```

### Menu Interativo

O programa apresenta um menu onde você pode:

1. Ver demonstração de Race Conditions
2. Testar diferentes soluções
3. Comparar performance
4. Executar benchmarks detalhados

## 📊 Análise de Performance

### Comparativo de Soluções

| Solução | Throughput | Overhead | Uso Ideal |
|---------|-----------|----------|-----------|
| **Interlocked** | ⭐⭐⭐⭐⭐ | Muito Baixo | Operações atômicas simples |
| **Lock** | ⭐⭐⭐ | Médio | Operações críticas múltiplas |
| **Monitor** | ⭐⭐⭐ | Médio | Similar ao Lock, mais controle |
| **Semaphore** | ⭐⭐ | Alto | Limitar concorrência |
| **ReaderWriterLock** | ⭐⭐⭐⭐ | Médio | Muitas leituras, poucas escritas |
| **ConcurrentCollections** | ⭐⭐⭐⭐ | Baixo | Coleções compartilhadas |

### Recomendações

#### ✅ Use Interlocked quando:
- Operações simples (incremento, decremento, troca)
- Performance crítica
- Não há dependências entre operações

#### ✅ Use Lock quando:
- Múltiplas operações devem ser atômicas
- Código mais complexo na seção crítica
- Legibilidade é importante

#### ✅ Use ReaderWriterLockSlim quando:
- Muitas threads lendo
- Poucas threads escrevendo
- Dados não mudam frequentemente

#### ✅ Use ConcurrentCollections quando:
- Trabalhar com coleções compartilhadas
- Não precisa de sincronização manual
- Performance balanceada

## 🎓 Conceitos Importantes

### Atomicidade

Uma operação é **atômica** quando é executada completamente sem interrupção:

```csharp
// NÃO é atômico (3 operações)
counter = counter + 1;  // 1. Ler, 2. Incrementar, 3. Escrever

// É atômico (1 operação)
Interlocked.Increment(ref counter);
```

### Lock-Free Programming

Técnicas que evitam locks tradicionais:

- **Interlocked**: Operações atômicas de hardware
- **Concurrent Collections**: Estruturas otimizadas
- **Immutability**: Objetos imutáveis eliminam race conditions

### Deadlock vs Race Condition

- **Race Condition**: Múltiplas threads acessam dados sem sincronização
- **Deadlock**: Threads travadas esperando recursos umas das outras

## 🔍 Detectando Race Conditions

### Sinais de Alerta:

1. Valores inconsistentes em testes
2. Bugs que só aparecem sob carga
3. Resultados diferentes em execuções repetidas
4. Exceções intermitentes (InvalidOperationException)

### Ferramentas:

- **Thread Sanitizer**: Detecção automática
- **Concurrency Visualizer**: Visual Studio
- **dotTrace**: JetBrains
- **Tests de Stress**: Executar múltiplas vezes

## 📚 Referências

- [Microsoft Docs - Threading](https://docs.microsoft.com/en-us/dotnet/standard/threading/)
- [Interlocked Class](https://docs.microsoft.com/en-us/dotnet/api/system.threading.interlocked)
- [Concurrent Collections](https://docs.microsoft.com/en-us/dotnet/standard/collections/thread-safe/)
- [Threading Best Practices](https://docs.microsoft.com/en-us/dotnet/standard/threading/managed-threading-best-practices)

## 🤝 Contribuindo

Sinta-se à vontade para adicionar novos exemplos ou melhorias!

## 📝 Licença

Este projeto é educacional e de código aberto.
