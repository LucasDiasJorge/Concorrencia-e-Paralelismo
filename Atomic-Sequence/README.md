# Atomic-Sequence

Este projeto demonstra o uso de `std::atomic<int>` para gerar IDs sequenciais seguros entre múltiplas threads.

Arquivo principal:
- `main.cpp` — contém a classe `SequenceGenerator` que usa `std::atomic<int>` e cria 100 threads que pedem um ID e imprimem no console.

Como compilar e executar (Windows / PowerShell):

```powershell
# com MinGW / g++
g++ -std=c++11 -O2 -pthread main.cpp -o main.exe; .\main.exe

# com MSVC (Developer PowerShell)
cl /EHsc /std:c++17 main.cpp; .\main.exe
```

O que este exemplo mostra
- Ao invés de usar `mutex` para proteger um contador global, usamos `std::atomic<int>` para que a operação de incremento seja feita de forma atômica e sem data-races.

✅ O que significa “operação atômica”?

Uma operação atômica é aquela que é indivisível: ela não pode ser interrompida no meio por outra thread. Ou seja, se você tem `x++` e ele é atômico, nenhuma thread vê um estado intermediário (por exemplo, ler antes da soma).

No nível do hardware, isso é garantido por instruções especiais da CPU que fazem leitura, modificação e escrita como um único passo atômico.

🔍 Como isso funciona por baixo dos panos?

Vamos analisar por camadas:

1) Problema sem atomicidade

Imagine duas threads executando:

	number++; // número global

Essa operação não é simples — na verdade é 3 passos:

1. Ler valor de number da memória
2. Incrementar valor
3. Escrever valor de volta

Se duas threads fizerem isso ao mesmo tempo, pode ocorrer:

Thread A lê number = 10
Thread B lê number = 10
Thread A incrementa -> 11
Thread B incrementa -> 11 (oops, perdeu um incremento!)

Esse é o clássico data race.

2) Como a CPU resolve isso?

A CPU fornece instruções atômicas como:

- x86: `LOCK XADD`, `CMPXCHG`
- ARM: `LDXR` / `STXR` (load-exclusive / store-exclusive)
- RISC-V: `AMOSWAP`, etc.

Essas instruções fazem read-modify-write com exclusividade na cache line ou no barramento, garantindo que nenhuma outra CPU/Thread modifique aquele endereço enquanto a operação ocorre.

3) Exemplo em assembly (x86)

Implementações de alto nível (ex.: `Interlocked.Increment` no C# ou `std::atomic` no C++) frequentemente geram algo equivalente a:

	lock xadd [number], eax

Explicando:

- `lock` → prefixo que garante exclusividade no barramento/cache
- `xadd` → troca o valor e soma (faz o RMW — read-modify-write) em uma única instrução

Tudo isso é feito em uma única operação de CPU, então nenhuma outra thread verá um estado intermediário.

4) Compare-And-Swap (CAS)

Muitas implementações usam CAS (Compare-And-Swap):

	bool CAS(int* addr, int oldVal, int newVal) {
		if (*addr == oldVal) {
			*addr = newVal;
			return true;
		}
		return false;
	}

Em assembly x86 isso usa `lock cmpxchg [addr], ecx`.

CAS é a base para construir algoritmos lock-free (filas, pilhas, contadores combinados, etc.). Um padrão comum é: ler, calcular, tentar CAS; se falhar, repetir.

5) Nível de cache (protocolo MESI)

Quando múltiplas CPUs acessam a mesma variável, o protocolo de coerência de cache (ex.: MESI) e a própria instrução atômica garantem exclusividade sobre a cache line enquanto a operação acontece. Isso evita que outra CPU veja ou escreva um valor intermédio.

✅ Resumindo

- Atômico ≠ mágico: é implementado por instruções de hardware que realizam read-modify-write atomically.
- Atômicos garantem atomicidade, mas não necessariamente ordenação: por isso existem memory barriers e os diferentes memory orders em `std::atomic` (relaxed, acquire/release, seq_cst).
- Muito mais rápido que mutex para operações simples e de alta frequência porque evita custos de kernel e trocas de contexto; porém tem limitações quando você precisa compor múltiplas operações ou garantir ordens complexas.

Notas práticas sobre o exemplo

- No `main.cpp`, a chamada `++current` em um `std::atomic<int>` executa um incremento atômico (equivalente, em comportamento atômico, a `fetch_add(1)`).
- A escrita em `std::cout` não é atômica: saídas de múltiplas threads podem se entrelaçar. Proteja `cout` com `std::mutex` se precisar de linhas inteiras sem mistura.
- Cuidado com overflow (wrap-around) — use um tipo maior se for necessário.

Leitura recomendada
- cppreference (procure por `std::atomic` e memory orderings)
- Artigos sobre mecanismos lock-free e sobre protocolos de coerência de cache (MESI)
