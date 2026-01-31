# ⚡ Paralelismo

Enquanto concorrência é sobre **estrutura** do programa, paralelismo é sobre **performance**. Aqui você vai aprender a dividir trabalho entre múltiplos cores para acelerar computações.

---

## 📂 Conteúdo

### [Divide-and-Conquer/](Divide-and-Conquer/)
Implementação de busca linear paralela usando o paradigma **dividir para conquistar**.

**O que você encontra:**
- Busca linear paralelizada com pthreads
- Benchmarks comparando 1, 4, 8 e 16 threads
- Análise de overhead de criação de threads
- Discussão sobre quando paralelizar vale a pena

**Resultados de benchmark:**
| Threads | Tempo |
|---------|-------|
| 1 | 4.335s |
| 4 | 1.258s |
| 8 | 0.851s |
| 16 | 0.209s |

### [OpenMP/](OpenMP/)
Exemplos usando **OpenMP** para paralelização automática de loops.

**O que você encontra:**
- `parallel_search.cpp` — Busca paralela com diretivas OpenMP
- `count_sort_parallel.cpp` — Counting Sort paralelizado
- Discussão sobre limitações de paralelização em certos algoritmos

---

## 🎯 O que você vai aprender aqui

1. **Divide and Conquer** — Dividir problema em partes menores
2. **Data Parallelism** — Mesma operação em dados diferentes
3. **OpenMP** — Paralelização declarativa em C/C++
4. **Speedup e Eficiência** — Métricas de ganho
5. **Lei de Amdahl** — Limite teórico de paralelização

---

## 📊 Lei de Amdahl

Nem todo código pode ser paralelizado. A Lei de Amdahl define o limite:

```
Speedup máximo = 1 / (S + P/N)

S = fração sequencial (não paralelizável)
P = fração paralela
N = número de processadores
```

**Exemplo:**
- Se 90% do código é paralelo (P=0.9, S=0.1)
- Com 4 cores: Speedup = 1/(0.1 + 0.9/4) = **3.08x**
- Com infinitos cores: Speedup máximo = 1/0.1 = **10x**

> O gargalo sempre será a parte sequencial!

---

## 🔧 OpenMP Básico

```cpp
#include <omp.h>

// Paralelizar um loop simples
#pragma omp parallel for
for (int i = 0; i < N; i++) {
    process(data[i]);
}

// Definir número de threads
#pragma omp parallel for num_threads(4)
for (int i = 0; i < N; i++) {
    process(data[i]);
}

// Redução (soma paralela)
int sum = 0;
#pragma omp parallel for reduction(+:sum)
for (int i = 0; i < N; i++) {
    sum += data[i];
}
```

---

## ⚠️ Quando NÃO paralelizar

1. **Dados pequenos** — Overhead de threads > ganho
2. **Dependências entre iterações** — Resultado de uma afeta outra
3. **I/O bound** — Gargalo não é CPU
4. **Muita sincronização necessária** — Locks matam performance

```cpp
// ❌ NÃO pode ser paralelizado facilmente
for (int i = 1; i < N; i++) {
    data[i] = data[i-1] * 2;  // depende do anterior!
}

// ✅ PODE ser paralelizado
for (int i = 0; i < N; i++) {
    data[i] = data[i] * 2;  // independente!
}
```

---

## 📖 Ordem de estudo sugerida

1. Leia o README de [Divide-and-Conquer/](Divide-and-Conquer/) — entenda a motivação
2. Analise o código e os benchmarks
3. Experimente com diferentes números de threads
4. Estude os exemplos de [OpenMP/](OpenMP/)
5. Compile e teste variando `num_threads`

---

## 💡 Dica prática

> Sempre meça antes de otimizar. Use ferramentas de profiling para identificar se seu código é CPU-bound e qual parte consome mais tempo.

---

## ➡️ Próximo passo

Agora que você domina os conceitos, veja aplicações práticas em **[05-Estudos-de-Caso/](../05-Estudos-de-Caso/)** com cenários do mundo real.
