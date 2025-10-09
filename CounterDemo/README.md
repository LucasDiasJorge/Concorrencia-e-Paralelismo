# Demonstração de Incremento Atômico vs Não-Atômico

Este projeto demonstra a diferença entre operações atômicas e não-atômicas em um banco de dados MySQL, com foco em incrementos de contadores (como estoque de produtos) em ambientes concorrentes.

## ?? O Problema das Race Conditions

Quando múltiplas threads ou processos tentam incrementar um contador simultaneamente, podem ocorrer **race conditions** se a operação não for atômica. Isso resulta em:

- **Incrementos perdidos**: Várias operações leem o mesmo valor inicial antes de atualizá-lo
- **Inconsistências de dados**: O valor final é menor que o esperado
- **Bugs difíceis de reproduzir**: O problema só aparece em alta concorrência

## ?? Solução: Incremento Atômico

Comparamos duas abordagens:

1. **Atômica**: Incremento realizado diretamente no banco de dados com `UPDATE SET valor = valor + 1`
2. **Não-Atômica**: Operação de leitura-modificação-escrita em passos separados

## ?? Resultados Esperados

- **Método Atômico**: Todos os incrementos são contabilizados corretamente
- **Método Não-Atômico**: Vários incrementos são "perdidos" devido a race conditions

## ?? Scripts Principais

### Setup do Banco

```
CREATE DATABASE IF NOT EXISTS counter_demo;
USE counter_demo;

CREATE TABLE IF NOT EXISTS products (
    id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    stock_quantity INT NOT NULL DEFAULT 0
);

-- Insere ou atualiza o produto de demonstração
INSERT INTO products (id, name, stock_quantity) 
VALUES (1, 'Produto Teste', 0)
ON DUPLICATE KEY UPDATE name = 'Produto Teste';
```

### Implementação Atômica

```
public async Task<int> IncrementStockAsync(int productId, int amount)
{
    const string sql = @"
        UPDATE products 
        SET stock_quantity = stock_quantity + @amount 
        WHERE id = @productId;
        
        SELECT stock_quantity FROM products WHERE id = @productId;";

    using var connection = new MySqlConnection(_connectionString);
    await connection.OpenAsync();
    
    using var command = new MySqlCommand(sql, connection);
    command.Parameters.AddWithValue("@productId", productId);
    command.Parameters.AddWithValue("@amount", amount);
    
    var result = await command.ExecuteScalarAsync();
    return result != null ? Convert.ToInt32(result) : throw new InvalidOperationException("Produto não encontrado");
}
```

### Implementação Não-Atômica (Suscetível a Race Conditions)

```
public async Task<int> IncrementStockAsync(int productId, int amount)
{
    // 1. Lê o valor atual
    Product product = await GetProductAsync(productId);

    // 2. Incrementa em memória
    int newStock = product.StockQuantity + amount;

    // Simula um pequeno atraso para aumentar a chance de race condition
    await Task.Delay(10);

    // 3. Atualiza o banco com o novo valor
    const string updateSql = "UPDATE products SET stock_quantity = @newStock WHERE id = @productId";
    
    using var connection = new MySqlConnection(_connectionString);
    await connection.OpenAsync();
    
    using var command = new MySqlCommand(updateSql, connection);
    command.Parameters.AddWithValue("@productId", productId);
    command.Parameters.AddWithValue("@newStock", newStock);
    
    await command.ExecuteNonQueryAsync();
    
    return newStock;
}
```

### Demonstração

```
public async Task RunDemoAsync()
{
    // Testa implementação atômica
    await _atomicRepository.ResetDemoAsync(_productId, _initialStock);
    var atomicResult = await RunTestAsync(_atomicRepository);
    
    // Testa implementação não-atômica
    await _nonAtomicRepository.ResetDemoAsync(_productId, _initialStock);
    var nonAtomicResult = await RunTestAsync(_nonAtomicRepository);
    
    // Exibe resultados
    Console.WriteLine($"Incrementos esperados: {_threadCount * _incrementsPerThread}");
    Console.WriteLine($"Método ATÔMICO - Estoque final: {atomicResult.FinalStock}");
    Console.WriteLine($"Método NÃO-ATÔMICO - Estoque final: {nonAtomicResult.FinalStock}");
    
    var atomicDiff = _threadCount * _incrementsPerThread - atomicResult.FinalStock;
    var nonAtomicDiff = _threadCount * _incrementsPerThread - nonAtomicResult.FinalStock;
    
    Console.WriteLine($"Incrementos perdidos (ATÔMICO): {atomicDiff}");
    Console.WriteLine($"Incrementos perdidos (NÃO-ATÔMICO): {nonAtomicDiff}");
}
```

## ?? Instruções para Execução

1. Execute o script SQL para criar o banco e tabela
2. Ajuste a string de conexão no `Program.cs`
3. Execute o projeto
4. Observe os resultados: a versão não-atômica terá incrementos perdidos

## ?? Conclusões

Este demo prova que para contadores em ambientes concorrentes:
- **Operações atômicas no banco** são essenciais para manter a consistência
- Abordagens **não-atômicas** inevitavelmente perderão atualizações
- A diferença se torna mais evidente com **maior número de threads**

As implicações para sistemas reais são sérias: contadores de visualizações, estoques, métricas e outros valores incrementais precisam utilizar operações atômicas para garantir precisão.
