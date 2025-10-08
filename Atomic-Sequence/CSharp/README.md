# AtomicSequenceSafe (C#)

Este projeto em C# demonstra como incrementar uma variável compartilhada de forma segura entre várias threads usando `Interlocked.Increment`, evitando race conditions.

## Como funciona

- O programa cria várias threads (por padrão, uma por núcleo lógico disponível).
- Cada thread realiza um número configurável de incrementos na mesma variável inteira.
- O método `Interlocked.Increment(ref counter)` garante atomicidade, eliminando race conditions.
- Ao final, o valor do contador é comparado com o total esperado para assegurar que não houve perda de incrementos.

## Requisitos

- .NET SDK 6.0 ou superior (`dotnet --version` deve retornar >= 6.0).

## Executando

No diretório `Atomic-Sequence/CSharp` execute:

```powershell
# Windows PowerShell
 dotnet run --project AtomicSequenceSafe.csproj
```

```bash
# Linux / macOS / WSL
 dotnet run --project AtomicSequenceSafe.csproj
```

### Ajustando parâmetros

Você pode alterar o número de threads e de incrementos por thread passando dois argumentos extras:

```bash
dotnet run --project AtomicSequenceSafe.csproj -- 12 200000
```

No exemplo acima:
- `12` = número de threads
- `200000` = incrementos por thread

O total esperado será `12 * 200000 = 2.400.000`. A saída do programa mostra o valor final alcançado e o tempo gasto.

## Experimento sugerido

Troque `Interlocked.Increment` por `counter++` e rode novamente. Você deverá observar que o valor final deixa de bater com o esperado, evidenciando a race condition quando a operação não é atômica.
