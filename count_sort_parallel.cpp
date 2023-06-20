#include <iostream>
#include <vector>
#include <omp.h>

void countingSort(std::vector<int>& arr) {
    int n = arr.size();

    // Encontrar o maior elemento no array
    int maxElement = arr[0];
    for (int i = 1; i < n; i++) {
        if (arr[i] > maxElement) {
            maxElement = arr[i];
        }
    }

    // Criar um array auxiliar para contar o número de ocorrências de cada elemento
    std::vector<int> count(maxElement + 1, 0);

    // Contar o número de ocorrências de cada elemento (paralelizado)
    #pragma omp parallel for
    for (int i = 0; i < n; i++) {
        #pragma omp atomic
        count[arr[i]]++;
    }

    // Atualizar o array original com os elementos na ordem correta (sequencial)
    int index = 0;
    for (int i = 0; i <= maxElement; i++) {
        for (int j = 0; j < count[i]; j++) {
            arr[index++] = i;
        }
    }
}

int main() {
    std::vector<int> arr = {4, 2, 2, 8, 3, 3, 1};

    countingSort(arr);

    std::cout << "Array ordenado: ";
    for (int num : arr) {
        std::cout << num << " ";
    }
    std::cout << std::endl;

    return 0;
}
