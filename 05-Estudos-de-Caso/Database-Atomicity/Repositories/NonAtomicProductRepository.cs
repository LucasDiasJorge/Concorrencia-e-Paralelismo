using CounterDemo.Interfaces;
using CounterDemo.Models;
using MySqlConnector;

namespace CounterDemo.Repositories;

public class NonAtomicProductRepository : IProductRepository
{
    private readonly string _connectionString;

    public NonAtomicProductRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<Product> GetProductAsync(int productId)
    {
        const string sql = "SELECT id, name, stock_quantity FROM products WHERE id = @productId";

        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@productId", productId);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Product
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                StockQuantity = reader.GetInt32(2)
            };
        }

        throw new InvalidOperationException($"Produto com ID {productId} não encontrado");
    }

    public async Task<int> IncrementStockAsync(int productId, int amount)
    {
        // Operação NÃO-ATÔMICA: lê, modifica e escreve em passos separados
        // Isso cria uma race condition!

        // 1. Lê o valor atual
        Product product = await GetProductAsync(productId);

        // 2. Incrementa em memória
        int newStock = product.StockQuantity + amount;

        // Simula um pequeno atraso 
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

    public async Task ResetDemoAsync(int productId, int initialStock)
    {
        const string sql = "UPDATE products SET stock_quantity = @initialStock WHERE id = @productId";

        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@productId", productId);
        command.Parameters.AddWithValue("@initialStock", initialStock);

        await command.ExecuteNonQueryAsync();
    }
}
