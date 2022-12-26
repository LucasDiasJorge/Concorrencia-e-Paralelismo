#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <pthread.h>

char l[5] = "false";

void *myThreadFun2(void *vargp){

    strcpy(l, "true");

    printf("Second Thread, l = %s\n", l);

    return NULL;

}

void *myThreadFun(void *vargp){

    printf("First Thread, l = %s\n", l);

    pthread_t thread_id_2;
    pthread_create(&thread_id_2, NULL, myThreadFun2, NULL);
    pthread_join(thread_id_2, NULL);

    return NULL;
}

int main(){

    printf("Starting ...\n");
    sleep(3);
    pthread_t thread_id;
    printf("Before Thread\n");
    pthread_create(&thread_id, NULL, myThreadFun, NULL);
    pthread_join(thread_id, NULL);
    printf("After Thread\n");
    printf("l = %s\n", l);
    printf("... Done");
    exit(0);

}
