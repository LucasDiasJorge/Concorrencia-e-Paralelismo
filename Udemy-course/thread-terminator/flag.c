#include <stdio.h>
#include <pthread.h>

volatile int should_terminate = 0; // Flag de controle

void *thread_function(void *arg) {
    while (!should_terminate) {
        // Executar trabalho da thread
        printf("Thread running...\n");
    }
    printf("Thread terminating...\n");
    pthread_exit(NULL);
}

int main() {
    pthread_t thread;
    pthread_create(&thread, NULL, thread_function, NULL);
    
    // Aguardar um pouco antes de definir a flag para terminar
    sleep(2);
    
    // Definir a flag para terminar
    should_terminate = 1;
    
    pthread_join(thread, NULL);
    printf("Main thread exiting...\n");
    return 0;
}
