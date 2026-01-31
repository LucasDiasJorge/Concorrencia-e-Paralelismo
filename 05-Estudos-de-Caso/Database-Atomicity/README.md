# Demonstração de Incremento Atômico vs Não-Atômico

Este projeto demonstra a diferença entre operações atômicas e não-atômicas aplicadas a um contador armazenado num banco de dados (MySQL). O foco é evidenciar como incrementos podem ser perdidos quando a atualização não é atômica.

## Problema

Quando múltiplas threads/processos tentam atualizar o mesmo valor ao mesmo tempo sem atomicidade, podem ocorrer:

- Incrementos perdidos
- Inconsistências de dados
- Bugs difíceis de reproduzir

## Abordagens

1. Atômica: executar um UPDATE que faça `stock_quantity = stock_quantity + 1` diretamente no banco.
2. Não-atômica: ler o valor, incrementar em memória e depois escrever o novo valor (read-modify-write em passos separados).

## Exemplo (SQL) — criação de tabela

```sql
CREATE DATABASE IF NOT EXISTS counter_demo;
USE counter_demo;

CREATE TABLE IF NOT EXISTS products (
    id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    stock_quantity INT NOT NULL DEFAULT 0
);

INSERT INTO products (id, name, stock_quantity) 
VALUES (1, 'Produto Teste', 0)
ON DUPLICATE KEY UPDATE name = 'Produto Teste';
```

## Trecho (pseudo-C#) — operação atômica no banco

```csharp
const string sql = @"
    UPDATE products
    SET stock_quantity = stock_quantity + @amount
    WHERE id = @productId;

    SELECT stock_quantity FROM products WHERE id = @productId;";

// Executa com parâmetros e retorna o novo valor
```

## Trecho (pseudo-C#) — operação não-atômica (suscetível a race condition)

```csharp
// 1) Ler
Product p = await GetProductAsync(productId);
// 2) Incrementar em memória
int newStock = p.StockQuantity + amount;
// 3) Atualizar
await UpdateProductStockAsync(productId, newStock);
```

## Instruções

1. Execute o script SQL para criar o banco e a tabela.
2. Ajuste a string de conexão no código C# (ou na aplicação que for usar).
3. Rode a demonstração (project/console) e compare a versão atômica com a não-atômica.

## Conclusão

Operações atômicas no banco de dados são essenciais para garantir consistência em contadores concorrentes. A versão não-atômica tende a perder atualizações quando há alta concorrência.
