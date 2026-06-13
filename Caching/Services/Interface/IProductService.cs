using Caching.Model;
using Caching.Model.DTOs;

namespace Caching.Services.Interface
{
    public interface IProductService
    {
        Task<List<Product>> GetProductsAsync();
        Task<Product> GetProductByIdAsync(int id);
        Task CreateProductAsync(Product product);
        Task UpdateProductAsync(int id, ProductRequestDTO product);
        Task DeleteProductAsync(int id);
    }
}
