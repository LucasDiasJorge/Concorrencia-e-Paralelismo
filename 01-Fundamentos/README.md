# 📘 Fundamentos de Concorrência e Paralelismo

Esta seção contém os conceitos básicos que você precisa dominar antes de avançar para tópicos mais complexos. Aqui você vai entender **o que são threads**, como elas funcionam, e a diferença fundamental entre **concorrência** e **paralelismo**.

---

## 📂 Conteúdo

### [Conceitos-em-C/](Conceitos-em-C/)
Implementações básicas em C puro usando **pthreads**. Ideal para entender como threads funcionam no nível mais baixo, sem abstrações de linguagens de alto nível.

- `concurrent.c` — Demonstra múltiplas threads executando de forma intercalada
- `parallel.c` — Demonstra divisão de trabalho entre threads

### [Conceitos-em-Rust/](Conceitos-em-Rust/)
Os mesmos conceitos implementados em Rust, mostrando como a linguagem garante segurança de memória em tempo de compilação.

- `concurrent.rs` — Threads concorrentes com ownership
- `parallel.rs` — Paralelismo com divisão de trabalho

### [Pthreads-Course/](Pthreads-Course/)
Exercícios e exemplos do curso de Pthreads (Udemy). Contém:
- Exemplos básicos de criação de threads
- Mutex e sincronização
- Prioridade de threads
- Terminação de threads

### [ASYNC-VS-MULTITHREADING.md](ASYNC-VS-MULTITHREADING.md)
Documento teórico explicando a diferença entre programação **assíncrona** e **multithreading** — dois conceitos que frequentemente confundem iniciantes.

---

## 🎯 O que você vai aprender aqui

1. **Processo vs Thread** — Qual a diferença?
2. **Concorrência vs Paralelismo** — Não são a mesma coisa!
3. **Criação e gerenciamento de threads** — `pthread_create`, `pthread_join`
4. **Contexto de execução** — Stack, registradores, program counter
5. **Assíncrono vs Multithreading** — Quando usar cada um

---

## 🔄 Concorrência vs Paralelismo (Resumo)

| Aspecto | Concorrência | Paralelismo |
|---------|--------------|-------------|
| **Definição** | Lidar com várias coisas ao mesmo tempo | Fazer várias coisas ao mesmo tempo |
| **Execução** | Pode ser intercalada (1 core) | Simultânea (múltiplos cores) |
| **Foco** | Estrutura do programa | Performance |
| **Exemplo** | Servidor web atendendo requisições | Renderização de vídeo |

> "Concorrência é sobre **estrutura**, paralelismo é sobre **execução**." — Rob Pike

---

## 📖 Por onde começar

1. Leia o [ASYNC-VS-MULTITHREADING.md](ASYNC-VS-MULTITHREADING.md) para entender a teoria
2. Compile e execute os exemplos em [Conceitos-em-C/](Conceitos-em-C/)
3. Compare com as implementações em [Conceitos-em-Rust/](Conceitos-em-Rust/)
4. Explore os exercícios em [Pthreads-Course/](Pthreads-Course/)

---

## ➡️ Próximo passo

Depois de dominar os fundamentos, vá para **[02-Sincronizacao/](../02-Sincronizacao/)** para aprender sobre os problemas que surgem quando threads compartilham dados.
