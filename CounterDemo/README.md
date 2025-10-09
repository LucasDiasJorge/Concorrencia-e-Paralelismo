# Demonstra��o de Incremento At�mico vs N�o-At�mico

Este projeto demonstra a diferen�a entre opera��es at�micas e n�o-at�micas em um banco de dados MySQL, com foco em incrementos de contadores (como estoque de produtos) em ambientes concorrentes.

## ?? O Problema das Race Conditions

Quando m�ltiplas threads ou processos tentam incrementar um contador simultaneamente, podem ocorrer **race conditions** se a opera��o n�o for at�mica. Isso resulta em:

- **Incrementos perdidos**: V�rias opera��es leem o mesmo valor inicial antes de atualiz�-lo
- **Inconsist�ncias de dados**: O valor final � menor que o esperado
- **Bugs dif�ceis de reproduzir**: O problema s� aparece em alta concorr�ncia

## ?? Solu��o: Incremento At�mico

Comparamos duas abordagens:

1. **At�mica**: Incremento realizado diretamente no banco de dados com `UPDATE SET valor = valor + 1`
2. **N�o-At�mica**: Opera��o de leitura-modifica��o-escrita em passos separados

## ?? Resultados Esperados

- **M�todo At�mico**: Todos os incrementos s�o contabilizados corretamente
- **M�todo N�o-At�mico**: V�rios incrementos s�o "perdidos" devido a race conditions

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

-- Insere ou atualiza o produto de demonstra��o
INSERT INTO products (id, name, stock_quantity) 
VALUES (1, 'Produto Teste', 0)
ON DUPLICATE KEY UPDATE name = 'Produto Teste';
```

### Implementa��o At�mica

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
    return result != null ? Convert.ToInt32(result) : throw new InvalidOperationException("Produto n�o encontrado");
}
```

### Implementa��o N�o-At�mica (Suscet�vel a Race Conditions)

```
public async Task<int> IncrementStockAsync(int productId, int amount)
{
    // 1. L� o valor atual
    Product product = await GetProductAsync(productId);

    // 2. Incrementa em mem�ria
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

### Demonstra��o

```
public async Task RunDemoAsync()
{
    // Testa implementa��o at�mica
    await _atomicRepository.ResetDemoAsync(_productId, _initialStock);
    var atomicResult = await RunTestAsync(_atomicRepository);
    
    // Testa implementa��o n�o-at�mica
    await _nonAtomicRepository.ResetDemoAsync(_productId, _initialStock);
    var nonAtomicResult = await RunTestAsync(_nonAtomicRepository);
    
    // Exibe resultados
    Console.WriteLine($"Incrementos esperados: {_threadCount * _incrementsPerThread}");
    Console.WriteLine($"M�todo AT�MICO - Estoque final: {atomicResult.FinalStock}");
    Console.WriteLine($"M�todo N�O-AT�MICO - Estoque final: {nonAtomicResult.FinalStock}");
    
    var atomicDiff = _threadCount * _incrementsPerThread - atomicResult.FinalStock;
    var nonAtomicDiff = _threadCount * _incrementsPerThread - nonAtomicResult.FinalStock;
    
    Console.WriteLine($"Incrementos perdidos (AT�MICO): {atomicDiff}");
    Console.WriteLine($"Incrementos perdidos (N�O-AT�MICO): {nonAtomicDiff}");
}
```

## ?? Instru��es para Execu��o

1. Execute o script SQL para criar o banco e tabela
2. Ajuste a string de conex�o no `Program.cs`
3. Execute o projeto
4. Observe os resultados: a vers�o n�o-at�mica ter� incrementos perdidos

## ?? Conclus�es

Este demo prova que para contadores em ambientes concorrentes:
- **Opera��es at�micas no banco** s�o essenciais para manter a consist�ncia
- Abordagens **n�o-at�micas** inevitavelmente perder�o atualiza��es
- A diferen�a se torna mais evidente com **maior n�mero de threads**

As implica��es para sistemas reais s�o s�rias: contadores de visualiza��es, estoques, m�tricas e outros valores incrementais precisam utilizar opera��es at�micas para garantir precis�o.
