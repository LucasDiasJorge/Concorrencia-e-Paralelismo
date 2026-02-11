# Async/Await Internals - Exemplos Práticos

Projeto C# demonstrando os conceitos internos de async/await no .NET.

## Execução

```bash
dotnet run
```

Menu interativo com 7 demos práticas.

## Exemplos Disponíveis

### 1. State Machine Demo
Demonstra como o compilador transforma async/await em máquina de estados (`IAsyncStateMachine`).

### 2. Sync vs Async
Compara código bloqueante com não-bloqueante, mostrando uso de threads e tempo de execução.

### 3. Deadlock Example
Simula cenários de deadlock (comuns em WinForms/WPF) e suas soluções.

### 4. Scalability Demo
Demonstra impacto na escalabilidade: abordagem bloqueante vs assíncrona sob carga.

### 5. Continuation Demo
Mostra como continuações são registradas e disparadas quando tasks completam.

### 6. await vs .Result vs .Wait()
Compara as diferenças técnicas entre as formas de obter resultados de tasks.

### 7. Sync vs Async Completion
Demonstra a diferença entre fast path (task completada) e slow path (task pendente).

## Conceitos Demonstrados

- Transformação do compilador em máquina de estados
- Preservação de estado entre suspensões
- Registro e disparo de continuações
- ThreadPool e reutilização de threads
- Impacto em performance e escalabilidade
- Risco de deadlock e como evitar
- Fast path vs slow path de await

## Documentação Completa

Veja `ASYNC-AWAIT-INTERNALS.md` para artigo técnico aprofundado.

## Requisitos

- .NET 8.0 SDK
