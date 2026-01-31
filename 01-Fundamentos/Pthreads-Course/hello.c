#include <stdio.h>
#include <stdlib.h>
#include <pthread.h>
#include <unistd.h>

static void *thread_fn_callback(void *thread_input)
{

    char *input = (char *)thread_input;
    int a = 0;

    while (a < 10)
    {
        printf("Thread input: %s\n", input);
        sleep(1);
        if (a == 5)
        {
            pthread_exit(0);
        }
        a++;
    }
}

void thread1_create()
{

    pthread_t pthread1; // thread hadler

    static char *thread_input1 = "I am thread no 1";

    int rc = pthread_create(&pthread1, NULL, thread_fn_callback, (void *)thread_input1);
    // &pthread1 = pointer to value of thread id
    // NULL pointer to a structure that is userd to define thread attributes, NULL means default attributes
    // thread_fn_callback is a pointer to a function that thread will run
    // (void *)thread function input

    if (rc != 0)
    {
        printf("Error occurred, thread could not be created, errno = %d\n", rc);
        free(thread_input1);
        exit(EXIT_FAILURE);
    }
}

int main()
{

    thread1_create(); // create a "fork"
    sleep(10);
    printf("main fn paused\n");
    // pause();
    pthread_exit(0);

    return 0;
}