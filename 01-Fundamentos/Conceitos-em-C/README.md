# Paralelismo vs Concorrência

![image.png](image.png)
Esta pasta contém exemplos básicos (em C) que ilustram diferenças entre concorrência e paralelismo.

Conteúdo
- `concurrent.c` — exemplo simples de concorrência/threads
- `parallel.c` — exemplo orientado a paralelismo (divisão de trabalho)
- `image.png` — figura ilustrativa

Como compilar (Linux / WSL / MinGW):

```bash
gcc -std=c11 -O2 concurrent.c -pthread -o concurrent
gcc -std=c11 -O2 parallel.c -pthread -o parallel
```

Execução:

```bash
./concurrent
./parallel
```