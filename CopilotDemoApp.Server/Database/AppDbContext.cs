
using Microsoft.EntityFrameworkCore;
using System;

namespace CopilotDemoApp.Server.Database;

public class Product
{
	public Guid Id { get; set; }
	public string? Name { get; set; }
	public string? Description { get; set; }
	public decimal? Price { get; set; }
	public bool IsActive { get; set; }
	public DateTime CreatedDate { get; set; }
	public DateTime UpdatedDate { get; set; }
}

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
	public DbSet<Product> Products { get; set; } = null!;
}
