# Caching Tutorial Web API

A .NET 8.0 Web API illustrating caching concepts, database integration, and proper REST API design. This application showcases the **Cache-Aside Pattern** using EF Core, MySQL, and ASP.NET Core In-Memory Caching (`IMemoryCache`).

---

## 📖 Introduction to Caching (System Design Concepts)

### What is Caching?
Caching is a technique used to **store frequently accessed data in a fast-access location** (like computer memory/RAM) so it can be retrieved much faster the next time it is requested, instead of executing a slow database query or network request.

### Caching in .NET
.NET provides several ways to cache data, but the two most common are:
1. **In-Memory Caching**: Data is saved directly in the web server's RAM. It is extremely fast but gets wiped when the application restarts, and it cannot be shared across multiple server instances. *(This project uses In-Memory Caching)*.
2. **Distributed Caching**: Data is stored in an external shared cache service (like Redis or Memcached). Multiple application servers can access it, and data survives application restarts.

---

## 🚀 Caching Strategies

Here are the four primary strategies used to read and write data from a cache:

### 1. Cache-Aside (Lazy Loading) - *Used in this project*
* **How it works**:
  1. The application checks the cache first.
  2. If the data is found (**Cache Hit**), it is returned.
  3. If not found (**Cache Miss**), the application fetches it from the database, writes it to the cache, and then returns it.
* **Writes**: Updates go directly to the database first, and then the application invalidates (removes) the cache entry.
* **Pros**: Simple; only stores requested data; resilient (app still runs even if the cache crashes).
* **Cons**: The first read is slow (miss); requires manual cache clearing code on writes.

### 2. Read-Through
* **How it works**: The application only talks to the cache. If a cache miss occurs, the cache itself automatically queries the database, saves the result, and returns it to the application.
* **Pros**: Simplifies the application code.
* **Cons**: Requires custom provider code in the cache system.

### 3. Write-Through
* **How it works**: When updating data, the application writes to the cache first, and the cache immediately writes it to the database synchronously.
* **Pros**: Cache is never stale; reads are always fast.
* **Cons**: Write speed is slower because every write must complete in both places.

### 4. Write-Behind (Write-Back)
* **How it works**: The application writes to the cache, which returns success immediately. The cache then writes the updates to the database in batches in the background asynchronously.
* **Pros**: Extremely fast write operations; handles traffic spikes well.
* **Cons**: Risk of data loss if the cache server crashes before the background database write finishes.

---

## ⚠️ Classic Caching Problems & Solutions

When designing caches, you must protect your system against three common scenarios:

### Problem 1: Cache Penetration
* **The Scenario**: Someone requests data that **does not exist** (e.g., product ID `-999`). The request misses the cache, hits the database, and returns nothing. If an attacker spams these requests, they will crash your database.
* **The Solution**: 
  1. **Cache Nulls**: Cache the empty response as a `null` value with a short expiration (e.g., 30 seconds).
  2. **Bloom Filter**: Use a fast, memory-efficient filter to check if a key exists before hitting the cache/database.

### Problem 2: Cache Breakdown (Thundering Herd / Stampede)
* **The Scenario**: A very popular "hot" item's cache expires. Since millions of users are asking for it at that exact second, they all experience a cache miss and query the database at the same time, overloading it.
* **The Solution**: 
  * **Mutex Locking (Single-Flight)**: Only allow the first request to query the database and update the cache. All other concurrent requests wait and read the value once it is cached.

### Problem 3: Cache Avalanche
* **The Scenario**: Many cached items expire at the **exact same time** (e.g., if you preload 10,000 products with a fixed 4-hour TTL). When they expire, the database gets slammed with queries all at once.
* **The Solution**:
  * **Random Jitter**: Add a small, random deviation to your expiration times (e.g., `4 hours + random seconds between 1 and 60`) to spread out the expiration times.

---

## Getting Started

Follow these steps to configure and run the project locally.

### Prerequisites
* **.NET 8.0 SDK** (or later)
* **MySQL Server** (running on port 3306)
* **EF Core CLI Tools** (Install using `dotnet tool install --global dotnet-ef` if not already installed)

### Setup & Configuration

1. **Clone the Repository**
   ```bash
   git clone <your-repository-url>
   cd Caching
   ```

2. **Configure Database Connection**
   Open the `appsettings.json` file inside the `Caching` project directory and adjust your MySQL connection credentials:
   ```json
   "ConnectionStrings": {
     "CachingConnectionString": "server=localhost;port=3306;database=CachingDB;user=root;password=root;"
   }
   ```

3. **Restore Dependencies**
   Run the following command at the solution directory level:
   ```bash
   dotnet restore
   ```

4. **Update the Database Schema**
   Apply EF Core migrations to build the tables in your MySQL instance:
   ```bash
   dotnet ef database update --project Caching
   ```

5. **Start the Web API**
   Run the project:
   ```bash
   dotnet run --project Caching
   ```

6. **Access Swagger Documentation**
   Open your browser to test and explore the endpoints via Swagger UI:
   * **URL**: `https://localhost:<port>/swagger` (See terminal output for port details)

---

## API Endpoints

All endpoints are mapped under `api/Products`.

| Method | Endpoint | Description | Cache Behavior |
| :--- | :--- | :--- | :--- |
| **GET** | `/api/products` | Get list of all products. | Checks `"AllProduct"` cache key. Cache miss queries DB and caches for 4 mins. |
| **GET** | `/api/products/{id}` | Get product by ID. | Checks `"Product_{id}"` cache key. Cache miss queries DB and caches for 4 mins. |
| **POST** | `/api/products` | Create a new product. | Inserts into DB, invalidates `"AllProduct"`, and caches single product. |
| **PUT** | `/api/products/{id}` | Update an existing product. | Updates DB, invalidates `"Product_{id}"` and `"AllProduct"` caches. |
| **DELETE** | `/api/products/{id}` | Delete a product. | Deletes from DB, invalidates `"Product_{id}"` and `"AllProduct"` caches. |

### Payload Examples

#### Create Product (`POST /api/products`)
```json
{
  "name": "Mechanical Keyboard",
  "description": "RGB backlight brown switches",
  "amount": 89.99,
  "stock": 50
}
```

#### Update Product (`PUT /api/products/{id}`)
```json
{
  "name": "Mechanical Keyboard v2",
  "description": "RGB backlight red switches",
  "amount": 99.99,
  "stock": 45
}
```

---

## Step-by-Step Development Guide (Build from Scratch)

If you want to recreate or build this project from scratch, follow these instructions:

### 1. Create the Web API Project
Initialize a new Web API template and install the Entity Framework Core packages for MySQL:
```bash
# Create the Web API project
dotnet new webapi -n Caching
cd Caching

# Install EF Core packages for MySQL
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Pomelo.EntityFrameworkCore.MySql
dotnet add package Microsoft.EntityFrameworkCore.Design
```

### 2. Create the Model and DTOs
Create the database models and request DTOs inside the project.

* **Product Model (`Model/Product.cs`)**:
  ```csharp
  namespace Caching.Model
  {
      public class Product
      {
          public int Id { get; set; }
          public required string Name { get; set; }
          public required string Description { get; set; }
          public double Amount { get; set; }
          public int Stock { get; set; }
      }
  }
  ```

* **Product Request DTO (`Model/DTOs/ProductRequestDTO.cs`)**:
  ```csharp
  namespace Caching.Model.DTOs
  {
      public class ProductRequestDTO
      {
          public required string Name { get; set; }
          public required string Description { get; set; }
          public double Amount { get; set; }
          public int Stock { get; set; }
      }
  }
  ```

* **Database Context (`Data/CachingDBContext.cs`)**:
  ```csharp
  using Caching.Model;
  using Microsoft.EntityFrameworkCore;

  namespace Caching.Data
  {
      public class CachingDBContext : DbContext
      {
          public CachingDBContext(DbContextOptions<CachingDBContext> options) : base(options) { }
          
          public DbSet<Product> Products { get; set; }
      }
  }
  ```

### 3. Create the Database and Schema (Migrations)
Add a connection string in `appsettings.json` under your server details, then execute migration commands:
```bash
# Generate the migration script
dotnet ef migrations add InitialCreate --project Caching

# Create database and apply schema to MySQL
dotnet ef database update --project Caching
```

### 4. Create the Generic & Asynchronous Cache Service
Implement a generic and asynchronous cache layer using `IMemoryCache`.

* **Interface (`Cache/ICacheService.cs`)**:
  ```csharp
  namespace Caching.Cache
  {
      public interface ICacheService
      {
          Task<T?> GetAsync<T>(string key);
          Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
          Task RemoveAsync(string key);
      }
  }
  ```

* **Implementation (`Cache/CacheService.cs`)**:
  ```csharp
  using Microsoft.Extensions.Caching.Memory;

  namespace Caching.Cache
  {
      public class CacheService : ICacheService
      {
          private readonly IMemoryCache _memoryCache;
          public CacheService(IMemoryCache memoryCache)
          {
             _memoryCache = memoryCache;
          }

          public Task<T?> GetAsync<T>(string key)
          {
              _memoryCache.TryGetValue(key, out T? value);
              return Task.FromResult(value);
          }

          public Task RemoveAsync(string key)
          {
              _memoryCache.Remove(key);
              return Task.CompletedTask;
          }

          public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
          {
              var options = new MemoryCacheEntryOptions();
              if (expiration.HasValue)
              {
                  options.AbsoluteExpirationRelativeToNow = expiration.Value;
                  options.SlidingExpiration = expiration.Value / 2;
              }
              _memoryCache.Set(key, value, options);
              return Task.CompletedTask;
          }
      }
  }
  ```

### 5. Register Services in `Program.cs`
Open `Program.cs` and add the caching service and DB context:
```csharp
builder.Services.AddMemoryCache();

// Register the CacheService as a Singleton
builder.Services.AddSingleton<ICacheService, CacheService>();

// Register DB Context and ProductService
builder.Services.AddDbContext<CachingDBContext>(options => ...);
builder.Services.AddScoped<IProductService, ProductService>();
```

> [!NOTE]
> **Why register `ICacheService` as `AddSingleton`?**
> `CacheService` depends exclusively on `IMemoryCache` (which is already registered as a Framework Singleton) and keeps no request-specific state or scoped dependencies. Registering it as a **Singleton** prevents allocating new wrapper instances for every HTTP request, saving garbage collection and memory overhead.

### 6. Create the Service and Controller
Implement your business logic inside a product service and consume it via a controller.

* **Product Service Interface (`Services/Interface/IProductService.cs`)**:
  Defines contracts for CRUD actions.
* **Product Service Implementation (`Services/Implementation/ProductService.cs`)**:
  Coordinates the cache invalidations on database mutations (POST/PUT/DELETE) and cache retrieval on GET queries.
* **Products Controller (`Controllers/ProductsController.cs`)**:
  Injects `IProductService`, maps endpoints to action methods returning proper status codes (`CreatedAtAction`, `NoContent`, `Ok`), and handles `KeyNotFoundException` to return `404 NotFound`.
