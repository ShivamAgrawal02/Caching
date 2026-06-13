using Caching.Model;

namespace Caching.Services.Interface
{
    public interface IProductService
    {
        Task<List<Product>> GetProductsAsync();
        Task<Product> GetProductByIdAsync(int id);
        Task CreateProductAsync(Product product);
        Task UpdateProductAsync(int id, Product product);
        Task DeleteProductAsync(int id);
    }
}
