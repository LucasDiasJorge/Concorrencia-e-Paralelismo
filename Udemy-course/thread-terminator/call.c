#include <stdio.h>
#include <pthread.h>

void *thread_function(void *arg) {
    while (1) {
        // Executar trabalho da thread
        printf("Thread running...\n");
    }
    pthread_exit(NULL);
}

int main() {
    pthread_t thread;
    pthread_create(&thread, NULL, thread_function, NULL);
    
    // Aguardar um pouco antes de cancelar a thread
    sleep(2);
    
    // Cancelar a thread
    pthread_cancel(thread);
    
    pthread_join(thread, NULL);
    printf("Main thread exiting...\n");
    return 0;
}
