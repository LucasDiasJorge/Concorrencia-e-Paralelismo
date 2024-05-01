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
    
    // Main thread encerra imediatamente após criação da thread filha
    
    printf("Main thread exiting...\n");
    return 0;
}
