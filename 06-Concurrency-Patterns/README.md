# 🎨 Concurrency Patterns (Padrões de Concorrência)

Padrões de projeto específicos para lidar com processamento concorrente. Estes padrões são soluções comprovadas para problemas recorrentes em sistemas multi-threaded.

> **Referência:** [Wikipedia - Concurrency Pattern](https://en.wikipedia.org/wiki/Concurrency_pattern)

---

## 📂 Padrões Documentados

### Padrões de Sincronização
| Padrão | Descrição | Status |
|--------|-----------|--------|
| [Lock](Lock/) | Controle de acesso exclusivo a recursos | 📝 Documentado |
| [Monitor](Monitor/) | Sincronização com condições de espera | 📝 Documentado |
| [Semaphore](Semaphore/) | Controle de acesso limitado | 📝 Documentado |
| [Readers-Writer Lock](ReadersWriterLock/) | Otimização para leitura intensiva | 📝 Documentado |
| [Double-Checked Locking](DoubleCheckedLocking/) | Lazy initialization thread-safe | 📝 Documentado |

### Padrões de Execução
| Padrão | Descrição | Status |
|--------|-----------|--------|
| [Thread Pool](ThreadPool/) | Reutilização de threads | 📝 Documentado |
| [Active Object](ActiveObject/) | Desacoplamento de execução | 📝 Documentado |
| [Scheduler](Scheduler/) | Controle de ordem de execução | 📝 Documentado |

### Padrões de Comunicação
| Padrão | Descrição | Status |
|--------|-----------|--------|
| [Producer-Consumer](ProducerConsumer/) | Fila de trabalho entre threads | 📝 Documentado |
| [Reactor](Reactor/) | Event demultiplexing | 📝 Documentado |
| [Proactor](Proactor/) | Async I/O com callbacks | 📝 Documentado |

### Padrões de Estado
| Padrão | Descrição | Status |
|--------|-----------|--------|
| [Thread-Local Storage](ThreadLocalStorage/) | Dados isolados por thread | 📝 Documentado |
| [Guarded Suspension](GuardedSuspension/) | Espera por condição | 📝 Documentado |
| [Balking](Balking/) | Rejeitar se estado inválido | 📝 Documentado |
| [Barrier](Barrier/) | Sincronização de grupo | 📝 Documentado |

---

## 🗺️ Mapa de Decisão

```
Preciso controlar acesso a recurso compartilhado?
├── Sim, acesso exclusivo → Lock / Monitor
├── Sim, limitar quantidade → Semaphore
├── Sim, muitas leituras → Readers-Writer Lock
└── Não

Preciso gerenciar execução de tarefas?
├── Sim, muitas tarefas curtas → Thread Pool
├── Sim, tarefas assíncronas → Active Object
├── Sim, ordem específica → Scheduler
└── Não

Preciso comunicação entre threads?
├── Sim, produtor/consumidor → Producer-Consumer
├── Sim, eventos de I/O → Reactor / Proactor
└── Não

Preciso gerenciar estado por thread?
├── Sim, dados isolados → Thread-Local Storage
├── Sim, esperar condição → Guarded Suspension
├── Sim, sincronizar grupo → Barrier
└── Não
```

---

## 📊 Comparativo de Padrões

### Por Complexidade

| Nível | Padrões |
|-------|---------|
| 🟢 Básico | Lock, Semaphore, Thread-Local Storage |
| 🟡 Intermediário | Monitor, Producer-Consumer, Thread Pool, Barrier |
| 🔴 Avançado | Active Object, Reactor, Proactor, Double-Checked Locking |

### Por Caso de Uso

| Caso de Uso | Padrões Recomendados |
|-------------|---------------------|
| Web Server | Thread Pool, Reactor |
| Database Connection Pool | Semaphore, Object Pool |
| Cache | Readers-Writer Lock, Double-Checked Locking |
| Message Queue | Producer-Consumer |
| GUI Event Loop | Reactor, Active Object |
| Batch Processing | Thread Pool, Barrier |
| Lazy Singleton | Double-Checked Locking |

---

## 📖 Ordem de Estudo Sugerida

1. **Fundamentos** (se ainda não domina)
   - Vá para [01-Fundamentos/](../01-Fundamentos/) primeiro

2. **Padrões Básicos**
   - Lock → Monitor → Semaphore

3. **Padrões de Produção**
   - Thread Pool → Producer-Consumer

4. **Padrões Avançados**
   - Reactor → Proactor → Active Object

---

## 📚 Referências

### Livros
- **"Pattern-Oriented Software Architecture, Volume 2"** - Douglas C. Schmidt et al.
  - A referência definitiva para padrões de concorrência
- **"Java Concurrency in Practice"** - Brian Goetz
  - Excelente cobertura prática dos padrões

### Papers
- **"Active Object"** - R. Greg Lavender, Douglas C. Schmidt (1995)
- **"Reactor: An Object Behavioral Pattern"** - Douglas C. Schmidt

### Links
- [ScaleConf: Concurrency Patterns](http://shairosenfeld.com/concurrency.html)
- [GopherCon: Rethinking Classical Concurrency Patterns](https://www.youtube.com/watch?v=5zXAHh5tJqQ)
- [Software Engineering Radio: Concurrency Series](http://www.se-radio.net/)

---

## 🔗 Conexão com Outras Seções

| Seção | Relação |
|-------|---------|
| [02-Sincronizacao/](../02-Sincronizacao/) | Implementações de Lock, Monitor, Semaphore |
| [03-Estruturas-Concorrentes/](../03-Estruturas-Concorrentes/) | Producer-Consumer com ConcurrentQueue |
| [04-Paralelismo/](../04-Paralelismo/) | Thread Pool aplicado a paralelismo |

---

> 💡 **Dica:** Não tente usar todos os padrões. Escolha o mais simples que resolva seu problema. Complexidade adicional = mais bugs potenciais.
