# 🧵 Concorrência e Paralelismo

> Um repositório de estudos práticos sobre programação concorrente e paralela, com implementações em C, C++, Rust e C#.

Este repositório documenta minha jornada aprendendo sobre concorrência e paralelismo — desde os conceitos mais básicos até aplicações em cenários reais. Cada pasta contém código funcional, explicações detalhadas e, quando possível, benchmarks.

---

## 📁 Estrutura do Projeto

O repositório está organizado em uma progressão lógica de aprendizado:

```
📂 Concorrencia-e-Paralelismo/
│
├── 📘 01-Fundamentos/          # Conceitos básicos de threads
│   ├── Conceitos-em-C/         # Exemplos em C puro (pthreads)
│   ├── Conceitos-em-Rust/      # Mesmos conceitos em Rust
│   ├── Pthreads-Course/        # Exercícios do curso de pthreads
│   └── ASYNC-VS-MULTITHREADING.md
│
├── 🔒 02-Sincronizacao/        # Problemas e soluções de sync
│   ├── RaceCondition-CSharp/   # Projeto completo sobre race conditions
│   └── Atomic-Operations/      # Operações atômicas (C++ e C#)
│
├── 📦 03-Estruturas-Concorrentes/  # Data structures thread-safe
│   └── ConcurrentQueue-CSharp/     # Producer-Consumer pattern
│
├── ⚡ 04-Paralelismo/          # Performance com múltiplos cores
│   ├── Divide-and-Conquer/     # Busca paralela com benchmarks
│   └── OpenMP/                 # Paralelização declarativa
│
├── 🌍 05-Estudos-de-Caso/      # Aplicações do mundo real
│   └── Database-Atomicity/     # Atomicidade em banco de dados
│
├── 🎨 06-Concurrency-Patterns/ # Design Patterns de concorrência
│   ├── Lock/                   # Exclusão mútua básica
│   ├── Monitor/                # Lock + condições de espera
│   ├── Semaphore/              # Controle de acesso limitado
│   ├── ThreadPool/             # Reutilização de threads
│   ├── ProducerConsumer/       # Fila de trabalho
│   ├── Reactor/                # Event demultiplexing
│   ├── Barrier/                # Sincronização de grupo
│   └── DoubleCheckedLocking/   # Lazy init thread-safe
│
├── 📚 LIBRARY.md               # Curadoria de livros, artigos e cursos
└── 📖 README.md                # Este arquivo
```

---

## 🎯 Por onde começar?

### Se você é iniciante:
1. Comece por [01-Fundamentos/](01-Fundamentos/) — entenda threads e a diferença entre concorrência e paralelismo
2. Leia [LIBRARY.md](LIBRARY.md) para referências de estudo complementar

### Se você já conhece o básico:
1. Vá direto para [02-Sincronizacao/RaceCondition-CSharp/](02-Sincronizacao/RaceCondition-CSharp/) — projeto mais completo do repositório
2. Explore as [operações atômicas](02-Sincronizacao/Atomic-Operations/) para entender o hardware

### Se quer ver paralelismo na prática:
1. [04-Paralelismo/Divide-and-Conquer/](04-Paralelismo/Divide-and-Conquer/) tem benchmarks interessantes
2. [04-Paralelismo/OpenMP/](04-Paralelismo/OpenMP/) mostra como paralelizar com poucas linhas

---

## 💡 Conceitos Fundamentais

### Concorrência vs Paralelismo

| | Concorrência | Paralelismo |
|---|---|---|
| **O que é** | Lidar com várias coisas ao mesmo tempo | Fazer várias coisas ao mesmo tempo |
| **Execução** | Pode ser intercalada (1 core) | Simultânea (múltiplos cores) |
| **Foco** | Estrutura do programa | Performance |
| **Exemplo** | Servidor web atendendo requisições | Renderização de vídeo |

> "Concorrência é sobre lidar com muitas coisas ao mesmo tempo. Paralelismo é sobre fazer muitas coisas ao mesmo tempo." — Rob Pike

### Os problemas que surgem

Quando múltiplas threads acessam dados compartilhados, surgem problemas como:

- **Race Conditions** — resultado depende da ordem de execução
- **Deadlocks** — threads bloqueadas esperando uma pela outra
- **Starvation** — thread nunca consegue executar
- **Livelock** — threads mudam de estado mas não progridem

### As soluções

- **Mutex/Lock** — exclusão mútua
- **Semáforos** — controle de acesso limitado
- **Operações Atômicas** — instruções indivisíveis
- **Estruturas Thread-Safe** — abstrações prontas

---

## 🛠️ Tecnologias utilizadas

| Linguagem | Uso no projeto |
|-----------|----------------|
| **C** | Pthreads, exemplos de baixo nível |
| **C++** | OpenMP, std::atomic, std::thread |
| **Rust** | Concorrência com ownership |
| **C#** | ConcurrentCollections, async/await, Interlocked |

---

## 📊 Highlights

### Benchmark de Busca Paralela
Do projeto [Divide-and-Conquer](04-Paralelismo/Divide-and-Conquer/):

| Threads | Tempo | Speedup |
|---------|-------|---------|
| 1 | 4.335s | 1x |
| 4 | 1.258s | 3.4x |
| 8 | 0.851s | 5.1x |
| 16 | 0.209s | 20.7x |

### Projeto Destaque: Race Conditions em C#
O projeto [RaceCondition-CSharp](02-Sincronizacao/RaceCondition-CSharp/) inclui:
- ✅ 4 tipos de race conditions demonstradas
- ✅ 6 técnicas de sincronização comparadas
- ✅ Benchmarks de performance
- ✅ Cenários do mundo real

---

## 📚 Recursos de Estudo

Veja [LIBRARY.md](LIBRARY.md) para uma curadoria completa de:
- 📖 Livros recomendados
- 📄 Artigos acadêmicos clássicos
- 🎥 Cursos e vídeos
- 🔗 Blogs e documentação

---

## 🚀 Como usar este repositório

```bash
# Clone o repositório
git clone https://github.com/LucasDiasJorge/Concorrencia-e-Paralelismo.git

# Navegue para um projeto específico
cd Concorrencia-e-Paralelismo/02-Sincronizacao/RaceCondition-CSharp

# Siga as instruções do README local
dotnet run
```

Cada subpasta tem seu próprio README com instruções de compilação e execução.

---

## 📝 Notas pessoais

Este repositório é um trabalho em progresso. Conforme avanço nos estudos, novos exemplos e projetos são adicionados. Se você encontrar algum erro ou tiver sugestões, fique à vontade para abrir uma issue ou PR.

---

*Última atualização: Janeiro 2026*