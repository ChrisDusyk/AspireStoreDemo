
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

public enum OrderStatus
{
	Pending,
	Processing,
	Shipped,
	Delivered
}

public class Order
{
	public Guid Id { get; set; }
	public string UserId { get; set; } = string.Empty;
	public string UserEmail { get; set; } = string.Empty;
	public string ShippingAddress { get; set; } = string.Empty;
	public string ShippingCity { get; set; } = string.Empty;
	public string ShippingState { get; set; } = string.Empty;
	public string ShippingZip { get; set; } = string.Empty;
	public DateTime OrderDate { get; set; }
	public OrderStatus Status { get; set; }
	public decimal TotalAmount { get; set; }
	public List<OrderLineItem> LineItems { get; set; } = new();
}

public class OrderLineItem
{
	public Guid Id { get; set; }
	public Guid OrderId { get; set; }
	public Order Order { get; set; } = null!;
	public Guid ProductId { get; set; }
	public string ProductName { get; set; } = string.Empty;
	public decimal ProductPrice { get; set; }
	public int Quantity { get; set; }
}

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
	public DbSet<Product> Products { get; set; } = null!;
	public DbSet<Order> Orders { get; set; } = null!;
	public DbSet<OrderLineItem> OrderLineItems { get; set; } = null!;
}
