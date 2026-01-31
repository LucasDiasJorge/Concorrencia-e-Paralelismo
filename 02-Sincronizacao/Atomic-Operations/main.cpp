#include <iostream>
#include <thread>
#include <vector>
#include <atomic>

class SequenceGenerator {
private:
    std::atomic<int> current{ 0 };

public:
    int getNext() {
        return ++current;
    }
};

int main() {
    SequenceGenerator generator;

    const int numThreads = 100;
    std::vector<std::thread> threads;

    for (int i = 0; i < numThreads; i++) {
        threads.emplace_back([&generator]() {
            int id = generator.getNext();
            std::cout << "Thread " << std::this_thread::get_id()
                << " -> ID gerado = " << id << "\n";
            });
    }

    for (auto& t : threads) {
        t.join();
    }

    return 0;
}
