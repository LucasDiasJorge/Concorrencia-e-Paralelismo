#include <stdio.h>
#include <stdlib.h>
#include <pthread.h>

#define THREADS_MAX 100

void *function(void *param) {
    int id = *((int *)param);
    int i, loops = 100;
    for (i = 0; i < loops; i++) {
        printf("thread %d: loop %d\n", id, i);
    }
    pthread_exit(NULL);
}

int main(void) {
    pthread_t threads[THREADS_MAX];
    int thread_args[THREADS_MAX];
    int i;
    
    printf("pre-execution\n");

    // Create threads
    for (i = 0; i < THREADS_MAX; i++) {
        thread_args[i] = i;
        if (pthread_create(&threads[i], NULL, function, (void *)&thread_args[i]) != 0) {
            perror("Failed to create thread");
            exit(EXIT_FAILURE);
        }
    }

    printf("mid-execution\n");

    // Wait for threads to finish
    for (i = 0; i < THREADS_MAX; i++) {
        if (pthread_join(threads[i], NULL) != 0) {
            perror("Failed to join thread");
            exit(EXIT_FAILURE);
        }
    }

    printf("post-execution\n");
    return EXIT_SUCCESS;
}
