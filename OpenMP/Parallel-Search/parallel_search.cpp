#include <iostream>
#include <vector>
#include <omp.h>

int parallelSearch(const std::vector<int>& vec, int target) {
    int size = static_cast<int>(vec.size());
    int foundIndex = -1;  // Índice onde o valor é encontrado (inicializado com -1)

    #pragma omp parallel for shared(foundIndex)
    for (int i = 0; i < size; i++) {
        if (vec[i] == target) {
            #pragma omp critical
            {
                std::cout << "Encontrado pela thread: " << omp_get_thread_num() << "\n";
                if (foundIndex == -1) foundIndex = i;  // armazena o primeiro índice encontrado
            }
        }
    }

    return foundIndex;
}

int main(int argc, char* argv[]) {
    std::vector<int> vec = {5, 3, 8, 2, 9, 1, 4, 7, 6};
    int target = 6;
    if (argc > 1) target = std::atoi(argv[1]);

    int result = parallelSearch(vec, target);

    if (result != -1) {
        std::cout << "Valor " << target << " encontrado no índice " << result << std::endl;
    } else {
        std::cout << "Valor " << target << " não encontrado no vetor" << std::endl;
    }

    return 0;
}
