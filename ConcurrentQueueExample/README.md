# ConcurrentQueueExample

Este projeto demonstra o uso de `ConcurrentQueue<T>` com múltiplos produtores e consumidores, além de uma alternativa com `BlockingCollection<T>` encapsulando uma `ConcurrentQueue<T>`.

Arquivos:
- `ConcurrentQueueExample.csproj` - projeto .NET 8
- `Program.cs` - contém a demo para `ConcurrentQueue` e versão `blocking` com `BlockingCollection`.

Como executar (PowerShell):

1. Abrir PowerShell e navegar para a pasta do exemplo:

```powershell
cd "c:\Users\Lucas Jorge\Documents\Default Projects\Others\Concorrencia-e-Paralelismo\RaceCondition-CSharp\Examples\ConcurrentQueueExample"
```

2. Rodar com dotnet run (ConcurrentQueue):

```powershell
dotnet run --project .
```

3. Rodar a versão com `BlockingCollection` (modo blocking):

```powershell
dotnet run --project . -- blocking
```

O exemplo demonstra:
- `Enqueue` por múltiplos produtores
- `TryDequeue` por múltiplos consumidores
- `TryPeek` e `ToArray()` para snapshot da fila
- Uso de `Interlocked.Increment` e `Volatile.Read` para contagem segura
- Alternativa com `BlockingCollection<T>` para bloqueio e capacidade limitada

Notas:
- `Count` em `ConcurrentQueue<T>` existe, mas não é estável em cenários altamente concorrentes; prefira `ToArray()` para snapshot ou lógica sincronizada.
- `BlockingCollection<T>` simplifica padrão produtor/consumidor quando desejar bloqueio em vez de polling.
