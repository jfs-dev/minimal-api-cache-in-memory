using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using minimal_api_cache_in_memory.Data;
using minimal_api_cache_in_memory.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("minimal-api-cache-in-memory"));

builder.Services.AddMemoryCache();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    dbContext.Customers.AddRange(
        new Customer { Name = "Peter Parker", Email = "peter.parker@marvel.com" },
        new Customer { Name = "Mary Jane", Email = "mary.jane@marvel.com" },
        new Customer { Name = "Ben Parker", Email = "ben.parker@marvel.com" }
    );

    dbContext.SaveChanges();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/Customers", async (IMemoryCache cache, AppDbContext dbContext) =>
{
    var cacheKey = "customersCache";
    if (!cache.TryGetValue(cacheKey, out List<Customer>? customers))
    {
        customers = await dbContext.Customers.AsNoTracking().ToListAsync();
        
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromSeconds(30));
        
        cache.Set(cacheKey, customers, cacheEntryOptions);
    }

    return Results.Ok(customers);
});

app.MapPost("customers/", async (Customer model, AppDbContext dbContext) =>
{
    await dbContext.Customers.AddAsync(model);
    await dbContext.SaveChangesAsync();

    return Results.Created($"/customers/{model.Id}", model);    
});

app.Run();
