#include <stdio.h>
#include <stdlib.h>
#include <pthread.h>
#include <time.h>

#define NUM_THREADS 16
#define ARRAY_SIZE 1410065408
#define ELEMENT_TO_FIND 1981202369

int array[ARRAY_SIZE];

struct thread_data {
    int thread_id;
    int start_index;
    int end_index;
    int *result;
    pthread_mutex_t *mutex;
    int *done;
};

void *linear_search(void *threadarg) {

    struct thread_data *my_data;
    my_data = (struct thread_data *) threadarg;
    int i;
    
    for (i = my_data->start_index; i < my_data->end_index; i++) {
        if (array[i] == ELEMENT_TO_FIND) {
            pthread_mutex_lock(my_data->mutex);
            if (!*my_data->done) {
                *my_data->result = i;
                *my_data->done = 1;
            }
            pthread_mutex_unlock(my_data->mutex);
            pthread_exit(NULL);
        }
    }
    
    pthread_exit(NULL);
}

int main() {

    pthread_t threads[NUM_THREADS];
    int rc;
    long t;
    struct thread_data td[NUM_THREADS];

    printf("Array initializing at %ld\n",time(NULL));

    // Inicializa o array de números aleatórios
    for (int i = 0; i < ARRAY_SIZE; i++) {
        array[i] = rand() % 1000000000;
    }

    array[ARRAY_SIZE - 1] = 1981202369;

    printf("Array initialized at %ld\n",time(NULL));

    // Inicializa as variáveis compartilhadas entre as threads
    int result = -1;
    pthread_mutex_t mutex = PTHREAD_MUTEX_INITIALIZER;
    int done = 0;

    // Cria as threads
    int step = ARRAY_SIZE / NUM_THREADS;
    for (t = 0; t < NUM_THREADS; t++) {
        td[t].thread_id = t;
        td[t].start_index = t * step;
        td[t].end_index = (t + 1) * step;
        td[t].result = &result;
        td[t].mutex = &mutex;
        td[t].done = &done;

        if (t == NUM_THREADS - 1) {
            td[t].end_index = ARRAY_SIZE;
        }

        rc = pthread_create(&threads[t], NULL, linear_search, (void *) &td[t]);
        
        if (rc) {
            printf("ERROR; return code from pthread_create() is %d\n", rc);
            exit(-1);
        }
    }

    // Aguarda as threads terminarem
    for (t = 0; t < NUM_THREADS; t++) {
        pthread_join(threads[t], NULL);
    }

    // Exibe o resultado da busca
    if (result != -1) {
        printf("Element found at index %d\n", result);
    } else {
        printf("Element not found\n");
    }

    pthread_exit(NULL);
}
