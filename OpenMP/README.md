# OpenMP implementation (in progress)

https://curc.readthedocs.io/en/latest/programming/OpenMP-C.html

g++ parallel_search.cpp -o parallel -fopenmp

g++ count_sort_parallel.cpp -o count_sort_parallel -fopenmp

O Counting Sort não é naturalmente paralelizável, pois a ordenação é baseada na contagem de ocorrências dos elementos. No entanto, podemos explorar técnicas de paralelização para melhorar o desempenho em algumas etapas do algoritmo.

Uma abordagem para paralelizar o Counting Sort é dividir o array em partes e realizar a contagem paralelamente para cada parte. No entanto, a etapa de combinar os resultados parciais em uma contagem global e atualizar o array original ainda precisa ser feita sequencialmente, o que limita o ganho de desempenho.