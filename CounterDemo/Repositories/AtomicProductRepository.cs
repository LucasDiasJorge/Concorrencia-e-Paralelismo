using CounterDemo.Interfaces;
using CounterDemo.Models;
using MySqlConnector;

namespace CounterDemo.Repositories;

public class AtomicProductRepository : IProductRepository
{
    private readonly string _connectionString;

    public AtomicProductRepository(string connectionString)
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
        // Operação ATÔMICA: incrementa diretamente no banco de dados
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

        // Simula um pequeno atraso antes de executar o comando (assim a espera é observável)
        await Task.Delay(10000);

        var result = await command.ExecuteScalarAsync();

        return result != null ? Convert.ToInt32(result) : throw new InvalidOperationException("Produto não encontrado");
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
