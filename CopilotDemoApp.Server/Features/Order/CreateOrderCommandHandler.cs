using CopilotDemoApp.Server.Database;
using CopilotDemoApp.Server.Shared;
using Microsoft.EntityFrameworkCore;

namespace CopilotDemoApp.Server.Features.Order;

public class CreateOrderCommandHandler(AppDbContext context) : ICommandHandler<CreateOrderCommand, Order>
{
	public async Task<Result<Order>> HandleAsync(CreateOrderCommand command, CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(command.ShippingAddress))
			return Result<Order>.Failure(new Error(ErrorCodes.ValidationFailed, "Shipping address is required"));

		if (string.IsNullOrWhiteSpace(command.ShippingCity))
			return Result<Order>.Failure(new Error(ErrorCodes.ValidationFailed, "Shipping city is required"));

		if (string.IsNullOrWhiteSpace(command.ShippingProvince))
			return Result<Order>.Failure(new Error(ErrorCodes.ValidationFailed, "Shipping province is required"));

		if (string.IsNullOrWhiteSpace(command.ShippingPostalCode))
			return Result<Order>.Failure(new Error(ErrorCodes.ValidationFailed, "Shipping postal code is required"));

		if (command.LineItems == null || !command.LineItems.Any())
			return Result<Order>.Failure(new Error(ErrorCodes.ValidationFailed, "Order must contain at least one item"));

		if (command.LineItems.Any(item => item.Quantity <= 0))
			return Result<Order>.Failure(new Error(ErrorCodes.ValidationFailed, "All line items must have a quantity greater than zero"));

		var totalAmount = command.LineItems.Sum(item => item.ProductPrice * item.Quantity);

		try
		{

			var orderEntity = new Database.Order
			{
				Id = Guid.NewGuid(),
				UserId = command.UserId,
				UserEmail = command.UserEmail,
				ShippingAddress = command.ShippingAddress,
				ShippingCity = command.ShippingCity,
				ShippingProvince = command.ShippingProvince,
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
				orderEntity.ShippingProvince,
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
		catch (Exception ex)
		{
			return Result<Order>.Failure(new Error(ErrorCodes.DatabaseError, "An error occurred while creating the order", ex));
		}
	}
}
