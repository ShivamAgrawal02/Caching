using Caching.Model;
using Microsoft.EntityFrameworkCore;
namespace Caching.Data
{
    public class CachingDBContext:DbContext
    {
        public CachingDBContext(DbContextOptions<CachingDBContext>options):base(options)
        {
            
        }
        DbSet<Product> Products { get; set; }
        }
}
