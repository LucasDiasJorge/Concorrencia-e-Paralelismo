#include <stdio.h>
#include <stdlib.h>
#include <pthread.h>

#define NUM_THREADS 2

// Global variable shared by all threads
int counter = 0;

// Mutex for protecting the shared variable
pthread_mutex_t mutex = PTHREAD_MUTEX_INITIALIZER;

// Function executed by each thread
void *thread_function(void *arg) {
    int *thread_id = (int *)arg;

    // Increment the shared variable in a loop
    for (int i = 0; i < 1000000; i++) {
        // Lock the mutex before accessing the shared variable
        pthread_mutex_lock(&mutex);
        
        counter++;
        
        // Unlock the mutex after accessing the shared variable
        pthread_mutex_unlock(&mutex);
    }

    printf("Thread %d finished\n", *thread_id);

    // Exit the thread
    pthread_exit(NULL);
}

int main() {
    pthread_t threads[NUM_THREADS];
    int thread_ids[NUM_THREADS];

    // Create threads
    for (int i = 0; i < NUM_THREADS; i++) {
        thread_ids[i] = i;
        pthread_create(&threads[i], NULL, thread_function, (void *)&thread_ids[i]);
    }

    // Wait for all threads to finish
    for (int i = 0; i < NUM_THREADS; i++) {
        pthread_join(threads[i], NULL);
    }

    // Print the final value of the shared variable
    printf("Final value of counter: %d\n", counter);

    return 0;
}
