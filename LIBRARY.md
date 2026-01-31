# 📚 Biblioteca de Referências - Concorrência e Paralelismo

Uma curadoria de recursos essenciais para aprofundar seus conhecimentos em programação concorrente e paralela.

---

## 📖 Livros

### Fundamentais

| Título | Autor(es) | Descrição |
|--------|-----------|-----------|
| **Java Concurrency in Practice** | Brian Goetz et al. | Considerado a "bíblia" da programação concorrente. Apesar do foco em Java, os conceitos são universais. |
| **The Art of Multiprocessor Programming** | Maurice Herlihy, Nir Shavit | Referência acadêmica completa sobre algoritmos concorrentes e estruturas de dados lock-free. |
| **C++ Concurrency in Action** | Anthony Williams | Guia prático e profundo sobre concorrência em C++ moderno (C++11/14/17/20). |
| **Programming with POSIX Threads** | David R. Butenhof | Clássico sobre programação com pthreads - essencial para entender threads em nível de sistema. |
| **Operating Systems: Three Easy Pieces** | Remzi & Andrea Arpaci-Dusseau | Excelente para entender concorrência do ponto de vista do SO. Disponível gratuitamente online. |

### Avançados

| Título | Autor(es) | Descrição |
|--------|-----------|-----------|
| **Is Parallel Programming Hard?** | Paul E. McKenney | Guia completo sobre programação paralela em Linux. Disponível gratuitamente. |
| **Parallel Programming: Techniques and Applications** | Barry Wilkinson, Michael Allen | Abordagem prática com MPI, OpenMP e mais. |
| **Structured Parallel Programming** | Michael McCool et al. | Padrões de design para programação paralela. |
| **The Little Book of Semaphores** | Allen B. Downey | Gratuito. Excelente para praticar problemas de sincronização. |

### Específicos por Linguagem

| Título | Autor(es) | Linguagem |
|--------|-----------|-----------|
| **Concurrent Programming in Java** | Doug Lea | Java |
| **Programming Rust** | Jim Blandy, Jason Orendorff | Rust (excelente cobertura de ownership e concorrência) |
| **Concurrency in C# Cookbook** | Stephen Cleary | C# |
| **Concurrency in Go** | Katherine Cox-Buday | Go |
| **Seven Concurrency Models in Seven Weeks** | Paul Butcher | Multi-linguagem |

---

## 📄 Artigos Acadêmicos

### Clássicos (Must Read)

| Título | Autor(es) | Ano | Tópico |
|--------|-----------|-----|--------|
| **Solution of a Problem in Concurrent Programming Control** | Edsger Dijkstra | 1965 | Mutex e exclusão mútua |
| **Cooperating Sequential Processes** | Edsger Dijkstra | 1965 | Semáforos |
| **The Dining Philosophers Problem** | Edsger Dijkstra | 1971 | Problema clássico de sincronização |
| **Monitors: An Operating System Structuring Concept** | C.A.R. Hoare | 1974 | Monitores |
| **Communicating Sequential Processes (CSP)** | C.A.R. Hoare | 1978 | Modelo de concorrência baseado em mensagens |
| **Time, Clocks, and the Ordering of Events in a Distributed System** | Leslie Lamport | 1978 | Relógios lógicos |

### Modernos

| Título | Tópico | Link |
|--------|--------|------|
| **What Every Programmer Should Know About Memory** | Modelo de memória | [LWN.net](https://lwn.net/Articles/250967/) |
| **Memory Barriers: a Hardware View for Software Hackers** | Barreiras de memória | Paul McKenney |
| **Lock-Free Data Structures** | Estruturas sem lock | Andrei Alexandrescu |
| **The Problem with Threads** | Crítica ao modelo de threads | Edward A. Lee |

---

## 🎥 Vídeos e Cursos

### Cursos Online Gratuitos

| Curso | Plataforma | Descrição |
|-------|------------|-----------|
| **Parallel Programming** | Coursera (EPFL) | Curso em Scala sobre programação paralela |
| **Concurrent Programming in Java** | Coursera (Rice University) | Série de 3 cursos completos |
| **Parallel, Concurrent, and Distributed Programming in Java** | Coursera | Especialização completa |
| **CS140 - Operating Systems** | Stanford (YouTube) | Concorrência do ponto de vista de SO |

### Conferências e Talks

| Título | Palestrante | Evento | Link |
|--------|-------------|--------|------|
| **The Free Lunch Is Over** | Herb Sutter | - | [herbsutter.com](http://www.gotw.ca/publications/concurrency-ddj.htm) |
| **C++ and Beyond: Concurrency** | Herb Sutter | C++ and Beyond | YouTube |
| **Threads Cannot Be Implemented as a Library** | Hans Boehm | - | [Paper](https://www.hboehm.info/misc/threads-as-a-library.pdf) |
| **Race Conditions, Distribution, Interactions** | Martin Kleppmann | Strange Loop | YouTube |
| **Concurrency is not Parallelism** | Rob Pike | Heroku Waza | YouTube |

### Canais do YouTube

- **Computerphile** - Vídeos explicativos sobre conceitos de CS
- **MIT OpenCourseWare** - Cursos completos de sistemas operacionais
- **Jacob Sorber** - Excelente para C e sistemas
- **CodeOpinion** - Padrões de arquitetura e concorrência em .NET

---

## 🌐 Recursos Online

### Documentação Oficial

| Recurso | Descrição |
|---------|-----------|
| [POSIX Threads Programming](https://hpc-tutorials.llnl.gov/posix/) | Tutorial completo de pthreads |
| [OpenMP Official](https://www.openmp.org/resources/) | Especificação e tutoriais de OpenMP |
| [C++ Reference - Thread](https://en.cppreference.com/w/cpp/thread) | Documentação completa de threads em C++ |
| [Rust Book - Concurrency](https://doc.rust-lang.org/book/ch16-00-concurrency.html) | Capítulo oficial sobre concorrência em Rust |
| [.NET Threading](https://docs.microsoft.com/en-us/dotnet/standard/threading/) | Documentação oficial .NET |

### Blogs e Sites

| Site | Descrição |
|------|-----------|
| [Preshing on Programming](https://preshing.com/) | Artigos excelentes sobre lock-free e memória |
| [1024cores](http://www.1024cores.net/) | Dmitry Vyukov sobre lock-free |
| [Mechanical Sympathy](https://mechanical-sympathy.blogspot.com/) | Martin Thompson sobre performance |
| [Paul E. McKenney's Blog](https://paulmck.livejournal.com/) | Autor do RCU no Linux |
| [Bartosz Milewski's Blog](https://bartoszmilewski.com/) | Teoria de categorias e concorrência |

### Ferramentas

| Ferramenta | Uso |
|------------|-----|
| **ThreadSanitizer (TSan)** | Detecta race conditions em C/C++ |
| **Helgrind** | Detector de erros em threads (Valgrind) |
| **Intel Inspector** | Análise de threading e memória |
| **Go Race Detector** | Integrado ao Go |
| **CHESS** | Model checker da Microsoft |

---

## 🧩 Problemas Clássicos

Problemas essenciais para praticar:

1. **Produtor-Consumidor** (Producer-Consumer)
2. **Leitores-Escritores** (Readers-Writers)
3. **Filósofos Jantando** (Dining Philosophers)
4. **Barbeiro Dorminhoco** (Sleeping Barber)
5. **Fumantes de Cigarro** (Cigarette Smokers)
6. **Ponte de Mão Única** (One-Lane Bridge)
7. **Santa Claus Problem**
8. **H2O Building Problem**

> 💡 O livro **"The Little Book of Semaphores"** contém implementações e discussões de todos esses problemas.

---

## 📊 Modelos de Concorrência

| Modelo | Descrição | Linguagens/Frameworks |
|--------|-----------|----------------------|
| **Threads e Locks** | Modelo tradicional | Java, C++, C#, Python |
| **Actors** | Objetos que se comunicam via mensagens | Erlang, Akka (Scala/Java), Elixir |
| **CSP** | Processos comunicantes | Go (goroutines/channels), Clojure (core.async) |
| **STM** | Memória transacional | Haskell, Clojure |
| **Data Parallelism** | Operações paralelas em coleções | CUDA, OpenCL, SIMD |
| **Futures/Promises** | Computação assíncrona | JavaScript, Rust, C++ |
| **Reactive** | Streams assíncronos | RxJava, RxJS, Reactor |

---

## 🔗 Links Rápidos

### Cheat Sheets
- [POSIX Threads Cheat Sheet](https://hpc-tutorials.llnl.gov/posix/)
- [OpenMP Cheat Sheet](https://www.openmp.org/resources/refguides/)
- [C++ Memory Order Cheat Sheet](https://en.cppreference.com/w/cpp/atomic/memory_order)

### Visualizações Interativas
- [Deadlock Empire](https://deadlockempire.github.io/) - Jogo para aprender sobre concorrência
- [Thread Visualizer](https://github.com/nicklockwood/threading) - Visualização de execução de threads

---

## 📝 Papers Recomendados por Tópico

### Lock-Free Programming
- "Simple, Fast, and Practical Non-Blocking and Blocking Concurrent Queue Algorithms" - Michael & Scott
- "A Practical Multi-Word Compare-and-Swap Operation" - Harris et al.

### Memory Models
- "The Java Memory Model" - JSR-133
- "C++ Memory Model" - ISO/IEC 14882

### Distributed Systems
- "MapReduce: Simplified Data Processing on Large Clusters" - Dean & Ghemawat
- "Paxos Made Simple" - Leslie Lamport
- "In Search of an Understandable Consensus Algorithm (Raft)" - Ongaro & Ousterhout

---

## 🎯 Roadmap de Estudos Sugerido

```
1. Fundamentos
   ├── Processos vs Threads
   ├── Criação e gerenciamento de threads
   └── Contexto de execução

2. Sincronização Básica
   ├── Race Conditions
   ├── Mutex/Locks
   ├── Semáforos
   └── Variáveis de condição

3. Problemas Clássicos
   ├── Produtor-Consumidor
   ├── Leitores-Escritores
   └── Filósofos Jantando

4. Conceitos Avançados
   ├── Deadlock, Livelock, Starvation
   ├── Modelo de memória
   ├── Memory barriers
   └── Operações atômicas

5. Estruturas Lock-Free
   ├── Compare-and-Swap (CAS)
   ├── Filas lock-free
   └── ABA Problem

6. Paralelismo
   ├── SIMD
   ├── OpenMP
   ├── GPU Computing (CUDA/OpenCL)
   └── MapReduce

7. Modelos Alternativos
   ├── Actors
   ├── CSP
   ├── STM
   └── Async/Await
```

---

> **Última atualização:** Janeiro 2026
> 
> Contribuições são bem-vindas! Abra uma issue ou PR para sugerir novos recursos.
