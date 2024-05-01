#include <stdio.h>
#include <pthread.h>

int shared_variable = 0;

void *thread_function(void *arg) {
    for (int i = 0; i < 1000000; i++) {
        shared_variable++;
    }
    pthread_exit(NULL);
}

int main() {
    pthread_t thread1, thread2;

    pthread_create(&thread1, NULL, thread_function, NULL);
    pthread_create(&thread2, NULL, thread_function, NULL);

    pthread_join(thread1, NULL);
    pthread_join(thread2, NULL);

    printf("Final value of shared_variable: %d\n", shared_variable);

    return 0;
}
