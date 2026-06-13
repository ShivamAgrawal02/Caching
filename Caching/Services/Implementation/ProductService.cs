using Caching.Cache;
using Caching.Model;
using Caching.Data;
using Caching.Services.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;
using Caching.Model.DTOs;

namespace Caching.Services.Implementation
{

    public class ProductService : IProductService
    {
        private readonly ICacheService _cache;
        private readonly CachingDBContext _context;
        private const string AllProductsCacheKey = "AllProduct";

        public ProductService(ICacheService cache, CachingDBContext context)
        {
            _cache = cache;
            _context = context;
        }


        public async Task CreateProductAsync(Product product)
        {
            await  _context.AddAsync(product);
            await _context.SaveChangesAsync();
            //Remove Old Invalid Data
            await _cache.RemoveAsync(AllProductsCacheKey);
            //Setting New Data in Cache
            await _cache.SetAsync($"Product_{product.Id}", product, TimeSpan.FromMinutes(4));

        }

        public async Task DeleteProductAsync(int id)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);

            if(product == null)
            {
                throw new KeyNotFoundException("Product not found");
            }
            //Remove From DB
            _context.Remove(product);
            await _context.SaveChangesAsync();
            //Remove From Cache
            await _cache.RemoveAsync(AllProductsCacheKey);
            await _cache.RemoveAsync($"Product_{product.Id}");
            
        }

        public async Task<Product> GetProductByIdAsync(int id)
        {
            string key = $"Product_{id}";
            var data= await _cache.GetAsync<Product>(key);
            if(data == null)
            {
               var product = await _context.Products.FirstOrDefaultAsync(x => x.Id == id);
                if (product==null)
                {
                    throw new KeyNotFoundException("Product not found");
                }
                
                await _cache.SetAsync(key, product, TimeSpan.FromMinutes(4));
                
                return product;
            }
            return data;
        }

        public async Task<List<Product>> GetProductsAsync()
        {
            
            var data = await _cache.GetAsync<List<Product>>(AllProductsCacheKey);
            if(data == null)
            {
                var products = await  _context.Products.ToListAsync();
                await _cache.SetAsync(AllProductsCacheKey, products, TimeSpan.FromMinutes(4));
                return products;

            }
            return data;
        }

        public async Task UpdateProductAsync(int id, ProductRequestDTO product)
        {
            var existing = await _context.Products.FindAsync(id);

            if(existing== null)
            {
                throw new KeyNotFoundException("No Product found with Id" + id);
            }
            
            existing.Stock = product.Stock;
            existing.Name = product.Name;
            existing.Amount = product.Amount;
            existing.Description = product.Description;
                
            await _context.SaveChangesAsync();
            
            await _cache.RemoveAsync($"Product_{id}");
            await _cache.RemoveAsync(AllProductsCacheKey);
            
        }
    }
}
