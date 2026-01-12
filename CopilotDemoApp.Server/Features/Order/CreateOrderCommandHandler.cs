using CopilotDemoApp.Server.Database;
using CopilotDemoApp.Server.Shared;
using Microsoft.EntityFrameworkCore;

namespace CopilotDemoApp.Server.Features.Order;

public class CreateOrderCommandHandler(AppDbContext context) : ICommandHandler<CreateOrderCommand, Order>
{
	public async Task<Result<Order>> HandleAsync(CreateOrderCommand command, CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(command.ShippingAddress))
			return Result<Order>.Failure(new Error("ShippingAddress", "Shipping address is required"));

		if (string.IsNullOrWhiteSpace(command.ShippingCity))
			return Result<Order>.Failure(new Error("ShippingCity", "Shipping city is required"));

		if (string.IsNullOrWhiteSpace(command.ShippingState))
			return Result<Order>.Failure(new Error("ShippingState", "Shipping state is required"));

		if (string.IsNullOrWhiteSpace(command.ShippingPostalCode))
			return Result<Order>.Failure(new Error("ShippingPostalCode", "Shipping postal code is required"));

		if (command.LineItems == null || !command.LineItems.Any())
			return Result<Order>.Failure(new Error("LineItems", "Order must contain at least one item"));

		var totalAmount = command.LineItems.Sum(item => item.ProductPrice * item.Quantity);

		var orderEntity = new Database.Order
		{
			Id = Guid.NewGuid(),
			UserId = command.UserId,
			UserEmail = command.UserEmail,
			ShippingAddress = command.ShippingAddress,
			ShippingCity = command.ShippingCity,
			ShippingState = command.ShippingState,
			ShippingPostalCode = command.ShippingPostalCode,
			OrderDate = DateTime.UtcNow,
			Status = OrderStatus.Pending,
			TotalAmount = totalAmount,
			LineItems = command.LineItems.Select(item => new Database.OrderLineItem
			{
				Id = Guid.NewGuid(),
				ProductId = item.ProductId,
				ProductName = item.ProductName,
				ProductPrice = item.ProductPrice,
				Quantity = item.Quantity
			}).ToList()
		};

		context.Orders.Add(orderEntity);
		await context.SaveChangesAsync(cancellationToken);

		var domainOrder = new Order(
			orderEntity.Id,
			orderEntity.UserId,
			orderEntity.UserEmail,
			orderEntity.ShippingAddress,
			orderEntity.ShippingCity,
			orderEntity.ShippingState,
			orderEntity.ShippingPostalCode,
			orderEntity.OrderDate,
			orderEntity.Status,
			orderEntity.TotalAmount,
			orderEntity.LineItems.Select(li => new OrderLineItem(
				li.Id,
				li.OrderId,
				li.ProductId,
				li.ProductName,
				li.ProductPrice,
				li.Quantity
			)).ToList()
		);

		return Result<Order>.Success(domainOrder);
	}
}
