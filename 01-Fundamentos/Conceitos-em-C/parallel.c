#include <stdio.h>
#include <stdlib.h>
#include <pthread.h>

#define THREADS_MAX_DEFAULT 4

void *function(void *param){
    int id = *((int *)(param));
    int i;
    int loops = 10;
    for (i = 0; i < loops; i++)
    {
        printf("thread %d: loop %d\n", id, i);
    }
    pthread_exit(NULL);
}

int main(int argc, char *argv[]){
    int threads_count = THREADS_MAX_DEFAULT;
    if (argc > 1) threads_count = atoi(argv[1]) > 0 ? atoi(argv[1]) : THREADS_MAX_DEFAULT;
    if (threads_count > 64) threads_count = 64;

    pthread_t threads[threads_count];
    int thread_args[threads_count];
    int i;
    printf("pre-execution (threads=%d)\n", threads_count);
    for (i = 0; i < threads_count; i++)
    {
        thread_args[i] = i;
        pthread_create(&threads[i], NULL, function, (void *)&thread_args[i]);
    }
    printf("mid-execution\n");

    for (i = 0; i < threads_count; i++)
    {
        pthread_join(threads[i], NULL);
    }
    printf("post-execution\n");
    return EXIT_SUCCESS;
}