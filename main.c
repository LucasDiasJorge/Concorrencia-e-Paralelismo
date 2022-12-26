#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <pthread.h>

void* myTurn(void *arg){

    while(1){
        sleep(1);
        printf("My Turn!\n");
    }

    return NULL;

}

void yourTurn(){

    while(1){
        sleep(1);
        printf("Your Turn!\n");
    }
}

int main(){

    printf("Starting ...\n");

    sleep(3);

    printf("Before Thread\n");

    pthread_t thread_id;

    pthread_create(&thread_id, NULL, myTurn, NULL);

    yourTurn();

    exit(0);

}
