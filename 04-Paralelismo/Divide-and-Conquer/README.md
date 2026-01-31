# Dividir para Conquistar

Projeto criado com a intensão de aplicar o conceito de dividir para conquistar utilizando threads e assim performando de forma mais rápida o algoritmo linear search.

## Material utilizado

 - [Concorrência e Paralelismo, by Fabio Akita pt 1](https://www.youtube.com/watch?v=cx1ULv4wYxM&ab_channel=FabioAkita)
 - [Concorrência e Paralelismo, by Fabio Akita pt 2](https://www.youtube.com/watch?v=gYJSWs-gp1g&ab_channel=FabioAkita)
 - [Documentação <pthread.h>](https://pubs.opengroup.org/onlinepubs/7908799/xsh/pthread.h.html)
  - [Utilização <pthread.h>](https://www.youtube.com/watch?v=uA8X5zNOGw8&ab_channel=JacobSorber)
  - [Mutex Synchronization](https://www.youtube.com/watch?v=GXXE42bkqQk&ab_channel=BrianFraser)
  - [Livro "Algoritmos - Teoria e Prática"](https://www.amazon.com.br/Algoritmos-Teoria-Pr%C3%A1tica-Thomas-Cormen/dp/8535236996/ref=asc_df_8535236996/?tag=googleshopp00-20&linkCode=df0&hvadid=379707181411&hvpos=&hvnetw=g&hvrand=6640580099952446125&hvpone=&hvptwo=&hvqmt=&hvdev=c&hvdvcmdl=&hvlocint=&hvlocphy=9074284&hvtargid=pla-1002925180312&psc=1)
  - [Mutex](https://www.geeksforgeeks.org/mutex-lock-for-linux-thread-synchronization/)
  - [doc](https://stackoverflow.com/questions/7235934/pthreads-high-memory-usage)
  - [doc](https://www.ibm.com/docs/en/zos/2.2.0?topic=functions-pthread-attr-setstacksize-set-stacksize-attribute-object)

### Notas do autor:

Antes de abordarmos a busca linear de forma paralela, segue a definição de uma busca linear:

## Definição de Busca linear

A busca linear em um banco de dados é um método de pesquisa que envolve examinar cada registro sequencialmente até encontrar o item desejado. É uma técnica simples, mas pode ser ineficiente em grandes conjuntos de dados, pois requer uma quantidade significativa de tempo e recursos para percorrer todos os registros.

# Busca Linear paralela

Uma busca linear paralela é útil quando você precisa buscar por um elemento em grandes conjuntos de dados de maneira eficiente, aproveitando o poder da computação paralela. Ela distribui o trabalho entre várias threads, permitindo que várias partes do array sejam processadas simultaneamente, o que pode reduzir significativamente o tempo de execução, especialmente em ambientes com múltiplos núcleos de CPU.

## Casos de uso

### 1 - Processamento de Grandes Volumes de Dados

Em bancos de dados ou sistemas de armazenamento massivo, onde é necessário buscar um registro específico em conjuntos de dados grandes, a busca paralela pode acelerar o tempo de resposta.

**Exemplo:** Buscar por um usuário ou transação específica em uma tabela grande de um banco de dados distribuído ou em arquivos de logs massivos.

### 2 - Processamento de Logs e Arquivos Grandes

Ferramentas que analisam logs extensos em busca de um padrão específico podem dividir o arquivo em partes e realizar a busca paralelamente.

**Exemplo:** Ferramentas de monitoramento de segurança que buscam padrões maliciosos em logs de servidor ou em registros de sistema.

### 3 - Análise de Dados em Tempo Real

Em sistemas que precisam analisar dados em tempo real (como IoT ou sistemas de monitoramento financeiro), a busca paralela pode ser usada para identificar padrões, anomalias ou valores importantes em grandes fluxos de dados.

**Exemplo:** Identificar uma anomalia financeira em transações ou detectar um dispositivo IoT com falha em uma rede de sensores.

### Notas do autor:

No entanto, a busca linear paralela é mais vantajosa quando os dados a serem processados são grandes o suficiente para justificar a sobrecarga de gerenciamento de threads ou processos. Em datasets pequenos, o ganho de desempenho pode ser mínimo ou até negativo, devido à complexidade e sincronização das threads.


## Estrutura do projeto

A estrutura 'thread_data' é definida nesse código para armazenar os dados que serão passados como argumento para a função linear_search, que será executada por cada thread criada no programa.

Essa estrutura é utilizada para armazenar as seguintes informações:

- **thread_id:** identificador da thread;
- **start_index:** índice inicial do trecho do vetor que a thread irá pesquisar;
- **end_index:** índice final do trecho do vetor que a thread irá pesquisar.
- **resut:** índice que retorna o index do valor encontrado.
- **mutex:** índice que se comunica com as demais threads para interromper a busca caso o valor seja encontrado.

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
