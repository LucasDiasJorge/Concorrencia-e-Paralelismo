
# Concorrência e Paralelismo 

Estudos em Concorrência e Paralelismo na linguagem C e desenvolvimento de algoritmos de alta eficiência.



## Material utilizado

 - [Concorrência e Paralelismo, by Fabio Akita pt 1](https://www.youtube.com/watch?v=cx1ULv4wYxM&ab_channel=FabioAkita)
 - [Concorrência e Paralelismo, by Fabio Akita pt 2](https://www.youtube.com/watch?v=gYJSWs-gp1g&ab_channel=FabioAkita)
 - [Documentação <pthread.h>](https://pubs.opengroup.org/onlinepubs/7908799/xsh/pthread.h.html)
  - [Utilização <pthread.h>](https://www.youtube.com/watch?v=uA8X5zNOGw8&ab_channel=JacobSorber)
  - [Mutex Synchronization](https://www.youtube.com/watch?v=GXXE42bkqQk&ab_channel=BrianFraser)
  - [Livro "Algoritmos - Teoria e Prática"](https://www.amazon.com.br/Algoritmos-Teoria-Pr%C3%A1tica-Thomas-Cormen/dp/8535236996/ref=asc_df_8535236996/?tag=googleshopp00-20&linkCode=df0&hvadid=379707181411&hvpos=&hvnetw=g&hvrand=6640580099952446125&hvpone=&hvptwo=&hvqmt=&hvdev=c&hvdvcmdl=&hvlocint=&hvlocphy=9074284&hvtargid=pla-1002925180312&psc=1)
  - [Mutex](https://www.geeksforgeeks.org/mutex-lock-for-linux-thread-synchronization/)




# Busca Linear paralela

A estrutura 'thread_data' é definida nesse código para armazenar os dados que serão passados como argumento para a função linear_search, que será executada por cada thread criada no programa.

Essa estrutura é utilizada para armazenar as seguintes informações:

- thread_id: identificador da thread;
- start_index: índice inicial do trecho do vetor que a thread irá pesquisar;
- end_index: índice final do trecho do vetor que a thread irá pesquisar.
- resut: índice que retorna o index do valor encontrado.
- mutex: índice que se comunica com as demais threads para interromper a busca caso o valor seja encontrado.


## Benchmarking 

Tempo de contrução do array de inteiros de tamanho "1410065408" em média de 27 a 31 segundos, já subtraidos no tempo de processamento.

O valor a ser encontrado está localizado na última posição do array, valor "1981202369", com finalidade de simular o pior cenário do algoritmo.

Complexidade linear, ou sejá, O(N).

Dados de uso de threads em buscas lineares:

| Threads | Tempo  |
| ------------- | ------------- |
| 1  | 4,335s  |
| 4  | 1,258s  |
| 8  | 0,851s  |
| 16  | 0,209s  |

Vale a pena ressaltar que são apenas tempos medios, e a criação excessiva de threads pode ter um impacto negativo no desempenho do sistema como um todo, pois cada thread usa recursos do sistema, como memória e CPU. No meu caso (Ryzen 7 2700 e 16gb de ram com Ubuntu 22.04).