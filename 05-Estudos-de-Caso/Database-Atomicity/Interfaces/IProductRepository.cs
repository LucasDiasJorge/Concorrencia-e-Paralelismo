using CounterDemo.Models;

namespace CounterDemo.Interfaces;

public interface IProductRepository
{
    Task<Product> GetProductAsync(int productId);
    Task<int> IncrementStockAsync(int productId, int amount);
    Task ResetDemoAsync(int productId, int initialStock);
}
