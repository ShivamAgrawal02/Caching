using Caching.Cache;
using Caching.Model;
using Caching.Data;
using Caching.Services.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Caching.Services.Implementation
{

    public class ProductService : IProductService
    {
        private readonly ICacheService _cache;
        private readonly CachingDBContext _context;
        public ProductService(ICacheService cache, CachingDBContext context)
        {
            _cache = cache;
            _context = context;
        }
        public async Task CreateProductAsync(Product product)
        {
            await  _context.AddAsync(product);
            await _context.SaveChangesAsync();
            await _cache.SetAsync($"product_{product.Id}", product, TimeSpan.FromMinutes(4));

        }

        public async Task DeleteProductAsync(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if(product == null)
            {
                throw new Exception("Product not found");
            }
             _context.Remove(product);
            await _cache.RemoveAsync("Product_" + product.Id);
        }

        public async Task<Product> GetProductByIdAsync(int id)
        {
            string key = "Product_" + id;
            var data= await _cache.GetAsync<Product>(key);
            if(data == null)
            {
               var product = await _context.Products.FirstOrDefaultAsync(x => x.Id == id);
                if (product==null)
                {
                    throw new Exception("Product not found");
                }
                else
                {
                    await _cache.SetAsync($"Product_{product.Id}", product, TimeSpan.FromMinutes(4));
                }
                return product;
            }
            return data;
        }

        public async Task<List<Product>> GetProductsAsync()
        {
            string key = "AllProduct";
            var data = await _cache.GetAsync<List<Product>>(key);
            if(data == null)
            {
                var products = await  _context.Products.ToListAsync();
                if (products == null)
                {
                    throw new Exception("No Products Found");
                }
                await _cache.SetAsync(key, products, TimeSpan.FromMinutes(4));
                return products;

            }
            return data;
        }

        public async Task UpdateProductAsync(int id, Product product)
        {
            var data = await _cache.GetAsync<Product>($"Product_{id}");
            if(data!=null)
            {
                await _cache.RemoveAsync($"Product_{id}");
            }
             _context.Update(product);
            await _context.SaveChangesAsync();
            await _cache.SetAsync($"Product_{id}", product, TimeSpan.FromMinutes(4));
        }
    }
}
