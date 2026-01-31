#include <stdio.h>
#include <stdlib.h>
#include <pthread.h>

#define NUM_THREADS 5

// Função para calcular a soma dos números de 1 a N
void *calculate_sum(void *arg) {
    int N = *((int *)arg);
    long long sum = 0;
    for (int i = 1; i <= N; i++) {
        sum += i;
    }
    printf("Soma dos números de 1 a %d: %lld\n", N, sum);
    pthread_exit(NULL);
}

int main() {
    pthread_t threads[NUM_THREADS];
    int thread_args[NUM_THREADS] = {1000000, 2000000, 3000000, 4000000, 5000000};
    int priorities[NUM_THREADS] = {1, 2, 3, 4, 5};
    int i, rc;

    pthread_attr_t attr;
    struct sched_param param;

    pthread_attr_init(&attr);
    pthread_attr_setinheritsched(&attr, PTHREAD_EXPLICIT_SCHED);
    pthread_attr_setschedpolicy(&attr, SCHED_FIFO);

    for (i = 0; i < NUM_THREADS; i++) {
        param.sched_priority = priorities[i]; // Normally, 0 is the default value. Vary of 0 to 99
        pthread_attr_setschedparam(&attr, &param);
        rc = pthread_create(&threads[i], &attr, calculate_sum, (void *)&thread_args[i]);
        if (rc) {
            printf("Erro ao criar a thread: %d\n", rc);
            exit(-1);
        }
    }

    pthread_attr_destroy(&attr);

    for (i = 0; i < NUM_THREADS; i++) {
        rc = pthread_join(threads[i], NULL);
        if (rc) {
            printf("Erro ao esperar pela thread: %d\n", rc);
            exit(-1);
        }
    }

    printf("Todas as threads terminaram a execução.\n");

    return 0;
}
