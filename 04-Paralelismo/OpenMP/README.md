# OpenMP examples

Referências:
- https://curc.readthedocs.io/en/latest/programming/OpenMP-C.html

Como compilar (g++):

```bash
g++ parallel_search.cpp -o parallel -fopenmp
g++ count_sort_parallel.cpp -o count_sort_parallel -fopenmp
```

Observação sobre Counting Sort:

O Counting Sort não é naturalmente paralelizável em todas as etapas porque depende de uma contagem global de ocorrências. Uma estratégia é dividir o array em blocos, fazer a contagem local em cada bloco (paralela) e depois reduzir/combinar as contagens locais em uma contagem global antes de reconstruir o array. A etapa de combinação pode limitar o ganho final.