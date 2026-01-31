# 🔒 Sincronização e Race Conditions

Quando múltiplas threads acessam dados compartilhados, as coisas podem dar muito errado. Esta seção aborda os **problemas de sincronização** e as diversas **técnicas para resolvê-los**.

---

## 📂 Conteúdo

### [RaceCondition-CSharp/](RaceCondition-CSharp/)
Projeto educacional completo sobre race conditions em C#. Este é provavelmente o projeto mais didático do repositório.

**O que você encontra:**
- 4 tipos diferentes de race conditions com exemplos práticos
- 6 técnicas de sincronização implementadas e comparadas:
  - `lock` (Monitor)
  - `Interlocked` (operações atômicas)
  - `Semaphore`
  - `ReaderWriterLockSlim`
  - `ConcurrentCollections`
  - `Monitor.Wait/Pulse`
- Benchmarks de performance
- Cenários do mundo real (banco, cache, analytics)

### [Atomic-Operations/](Atomic-Operations/)
Demonstrações de operações atômicas em C++ e C#.

**O que você encontra:**
- `std::atomic<int>` em C++ para contadores thread-safe
- `Interlocked.Increment` em C# como equivalente
- Explicação detalhada de como funciona no nível de hardware (instruções `LOCK XADD`, CAS)
- Protocolo de coerência de cache (MESI)

---

## 🎯 O que você vai aprender aqui

1. **Race Conditions** — O que são e por que acontecem
2. **Critical Sections** — Identificando regiões perigosas do código
3. **Mutex/Lock** — A solução clássica
4. **Semáforos** — Controlando acesso limitado
5. **Operações Atômicas** — A alternativa de alta performance
6. **ReaderWriterLock** — Otimizando para leitura
7. **Coleções Thread-Safe** — Abstrações prontas para uso

---

## ⚠️ O Problema

```
Valor inicial: counter = 0
Thread A: lê 0 → incrementa → escreve 1
Thread B: lê 0 → incrementa → escreve 1  (executou antes de A escrever!)

Resultado: counter = 1 (deveria ser 2!)
```

Isso é uma **race condition** — o resultado depende da ordem de execução das threads, que é imprevisível.

---

## 🛠️ Soluções Comparadas

| Técnica | Uso Ideal | Performance | Complexidade |
|---------|-----------|-------------|--------------|
| `lock` | Propósito geral | Média | Baixa |
| `Interlocked` | Operações simples (++, --) | Alta | Baixa |
| `Semaphore` | Limitar acessos simultâneos | Média | Média |
| `ReaderWriterLock` | Muitas leituras, poucas escritas | Alta* | Média |
| `ConcurrentCollections` | Coleções compartilhadas | Alta | Baixa |

*Alta performance para cenários de leitura intensiva

---

## 📖 Ordem de estudo sugerida

1. **Entenda o problema**: Execute os exemplos de race condition em [RaceCondition-CSharp/Examples/](RaceCondition-CSharp/Examples/)
2. **Aprenda as soluções**: Estude [RaceCondition-CSharp/Solutions/](RaceCondition-CSharp/Solutions/)
3. **Vá mais fundo**: Leia sobre operações atômicas em [Atomic-Operations/](Atomic-Operations/)
4. **Compare performance**: Execute os benchmarks

---

## 💡 Dica importante

> Sempre prefira a solução mais simples que resolva seu problema. `lock` é suficiente para 90% dos casos. Só migre para soluções mais complexas quando tiver evidência de que precisa.

---

## ➡️ Próximo passo

Com os fundamentos de sincronização dominados, avance para **[03-Estruturas-Concorrentes/](../03-Estruturas-Concorrentes/)** e aprenda sobre estruturas de dados projetadas para acesso concorrente.
