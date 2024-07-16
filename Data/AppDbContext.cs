using Microsoft.EntityFrameworkCore;
using minimal_api_cache_in_memory.Models;

namespace minimal_api_cache_in_memory.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Customer> Customers { get; set; }
}
