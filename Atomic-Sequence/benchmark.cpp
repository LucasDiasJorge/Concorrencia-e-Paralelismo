// Benchmark simplificado: compara incremento com std::atomic vs std::mutex
// Uso:
//   g++ -std=c++17 -O2 -pthread simple_benchmark.cpp -o simple_bench
//   ./simple_bench [total_ops] [threads]
// Valores padrão: total_ops=500000, threads=8 (ou hardware_concurrency se menor)

#include <atomic>
#include <chrono>
#include <iostream>
#include <mutex>
#include <thread>
#include <vector>

long long run_atomic(long long total, int threads) {
    std::atomic<long long> counter{0};
    long long per = total / threads;
    std::vector<std::thread> v;
    v.reserve(threads);
    auto t0 = std::chrono::high_resolution_clock::now();
    for (int i = 0; i < threads; ++i) {
        v.emplace_back([&]() {
            for (long long k = 0; k < per; ++k) counter.fetch_add(1, std::memory_order_relaxed);
        });
    }
    for (auto &th : v) th.join();
    auto t1 = std::chrono::high_resolution_clock::now();
    return std::chrono::duration_cast<std::chrono::microseconds>(t1 - t0).count();
}

long long run_mutex(long long total, int threads) {
    long long counter = 0;
    std::mutex m;
    long long per = total / threads;
    std::vector<std::thread> v;
    v.reserve(threads);
    auto t0 = std::chrono::high_resolution_clock::now();
    for (int i = 0; i < threads; ++i) {
        v.emplace_back([&]() {
            for (long long k = 0; k < per; ++k) {
                std::lock_guard<std::mutex> lg(m);
                ++counter;
            }
        });
    }
    for (auto &th : v) th.join();
    auto t1 = std::chrono::high_resolution_clock::now();
    return std::chrono::duration_cast<std::chrono::microseconds>(t1 - t0).count();
}

int main(int argc, char *argv[]) {
    long long total = 500000;
    int threads = 8;
    int hw = std::thread::hardware_concurrency();
    if (hw > 0 && hw < threads) threads = hw;
    if (argc >= 2) total = std::stoll(argv[1]);
    if (argc >= 3) threads = std::stoi(argv[2]);
    if (threads < 1) threads = 1;
    if (total < threads) total = threads;

    std::cout << "Total ops: " << total << " | Threads: " << threads << "\n";
    auto us_atomic = run_atomic(total, threads);
    auto us_mutex = run_mutex(total, threads);

    double nsPerOpAtomic = (us_atomic * 1000.0) / total;
    double nsPerOpMutex = (us_mutex * 1000.0) / total;

    std::cout << "Atomic: " << us_atomic / 1000.0 << " ms  (" << nsPerOpAtomic << " ns/op)\n";
    std::cout << "Mutex : " << us_mutex / 1000.0 << " ms  (" << nsPerOpMutex << " ns/op)\n";
    double speedup = (double)us_mutex / (double)us_atomic;
    std::cout << "Speedup (mutex/atomic): " << speedup << "x\n";
    std::cout << "Obs: atomic usa memory_order_relaxed. Resultados variam conforme CPU e carga.\n";
    return 0;
}
