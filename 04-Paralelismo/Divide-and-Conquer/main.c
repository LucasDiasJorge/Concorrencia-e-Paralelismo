#include <stdio.h>
#include <stdlib.h>
#include <pthread.h>
#include <time.h>

#include <stdint.h>

#define NUM_THREADS 8
#define ARRAY_SIZE_DEFAULT 1000000
#define ELEMENT_TO_FIND 1981202369

int *array = NULL;

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

    printf("Array initializing at %ld\n", (long)time(NULL));

    int array_size = ARRAY_SIZE_DEFAULT;
    if (argc > 1) {
        long v = atol(argv[1]);
        if (v > 0) array_size = (int)v;
    }

    array = (int *)malloc(sizeof(int) * (size_t)array_size);
    if (!array) {
        fprintf(stderr, "Falha ao alocar array de tamanho %d\n", array_size);
        return 1;
    }

    // Inicializa o array de números aleatórios
    for (int i = 0; i < array_size; i++) {
        array[i] = rand() % 1000000000;
    }

    array[array_size - 1] = ELEMENT_TO_FIND;

    printf("Array initialized at %ld (size=%d)\n", (long)time(NULL), array_size);

    // Inicializa as variáveis compartilhadas entre as threads
    int result = -1;
    pthread_mutex_t mutex = PTHREAD_MUTEX_INITIALIZER;
    int done = 0;

    // Cria as threads
    int step = array_size / NUM_THREADS;
    for (t = 0; t < NUM_THREADS; t++) {
        td[t].thread_id = (int)t;
        td[t].start_index = (int)(t * step);
        td[t].end_index = (int)((t + 1) * step);
        td[t].result = &result;
        td[t].mutex = &mutex;
        td[t].done = &done;

        if (t == NUM_THREADS - 1) {
            td[t].end_index = array_size;
        }

        rc = pthread_create(&threads[t], NULL, linear_search, (void *) &td[t]);
        
        if (rc) {
            printf("ERROR; return code from pthread_create() is %d\n", rc);
            free(array);
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

    free(array);
    pthread_exit(NULL);
}
