// Exemplo simples que demonstra race condition: incremento sem sincronização
#include <stdio.h>
#include <pthread.h>

int shared_variable = 0;

void *thread_function(void *arg) {
    for (int i = 0; i < 1000000; i++) {
        shared_variable++;
    }
    pthread_exit(NULL);
}

int main(void) {
    pthread_t thread1, thread2;

    pthread_create(&thread1, NULL, thread_function, NULL);
    pthread_create(&thread2, NULL, thread_function, NULL);

    pthread_join(thread1, NULL);
    pthread_join(thread2, NULL);

    printf("Final value of shared_variable: %d\n", shared_variable);

    // Observe que o valor final frequentemente será menor que 2000000 devido a race conditions.
    // Compare com a versão em "mutex/main.c" que usa um mutex para proteger o incremento.

    return 0;
}
