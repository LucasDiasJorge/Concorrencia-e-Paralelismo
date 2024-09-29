#include <iostream>
#include <vector>
#include <omp.h>

int parallelSearch(const std::vector<int>& vec, int target) {
    int size = vec.size();
    int foundIndex = -1;  // Índice onde o valor é encontrado (inicializado com -1)

    #pragma omp parallel for
    for (int i = 0; i < size; i++) {
        if (vec[i] == target) {
            #pragma omp critical
            {
                printf("Encontrado pelo processo: %d\n", omp_get_thread_num());
                foundIndex = i;  // Armazena o índice encontrado (somente uma thread pode atualizar a variável)
            }
        }
    }

    return foundIndex;
}

int main() {
    std::vector<int> vec = {5, 3, 8, 2, 9, 1, 4, 7, 6};

    int target = 6;
    int result = parallelSearch(vec, target);

    if (result != -1) {
        std::cout << "Valor " << target << " encontrado no índice " << result << std::endl;
    } else {
        std::cout << "Valor " << target << " não encontrado no vetor" << std::endl;
    }

    return 0;
}
