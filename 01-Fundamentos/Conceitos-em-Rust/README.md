# Paralelismo vs Concorrência em Rust

Esta pasta contém exemplos básicos em Rust que ilustram diferenças entre concorrência e paralelismo,
equivalentes aos exemplos em C da pasta 101.

## Conteúdo
- `concurrent.rs` — exemplo simples de concorrência/threads
- `parallel.rs` — exemplo orientado a paralelismo (divisão de trabalho)
- `Cargo.toml` — arquivo de configuração do projeto Rust

## Como compilar e executar

### Usando Cargo (recomendado):

```bash
# Compilar todos os binários
cargo build --release

# Executar o exemplo de concorrência
cargo run --release --bin concurrent

# Executar o exemplo de paralelismo 
cargo run --release --bin parallel

# Opcionalmente, especificar o número de threads
cargo run --release --bin concurrent -- 4
cargo run --release --bin parallel -- 8
```

### Compilando diretamente com rustc:

```bash
# Compilar individualmente
rustc -O concurrent.rs -o concurrent
rustc -O parallel.rs -o parallel

# Executar
./concurrent
./parallel

# Opcionalmente, especificar o número de threads
./concurrent 4
./parallel 8
```

## Diferenças entre os exemplos

- `concurrent.rs`: Demonstra concorrência com múltiplas threads executando a mesma tarefa de forma concorrente.
- `parallel.rs`: Ilustra paralelismo, onde múltiplas threads trabalham em conjunto para realizar uma tarefa.

## Conceitos de Rust utilizados

- `thread::spawn`: Cria novas threads.
- Closures: Utilizadas para capturar variáveis para uso dentro das threads.
- Join handles: Permitem aguardar a conclusão das threads.
- Sistema de propriedade (ownership): Garante segurança de memória sem necessidade de um coletor de lixo.

Rust oferece garantias de segurança de memória em tempo de compilação, eliminando problemas como 
condições de corrida (race conditions) em certos cenários através de seu sistema de tipos.