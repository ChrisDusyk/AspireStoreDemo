using CopilotDemoApp.Server.Database;
using CopilotDemoApp.Server.Shared;
using CopilotDemoApp.Shared.Messages;
using Microsoft.EntityFrameworkCore;

namespace CopilotDemoApp.Server.Features.Order;

public class CreateOrderCommandHandler(AppDbContext context, IOrderMessagePublisher messagePublisher) : ICommandHandler<CreateOrderCommand, Order>
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

		// Payment validation
		if (string.IsNullOrWhiteSpace(command.CardNumber))
			return Result<Order>.Failure(new Error(ErrorCodes.ValidationFailed, "CARD_NUMBER_REQUIRED: Card number is required"));

		if (string.IsNullOrWhiteSpace(command.CardholderName))
			return Result<Order>.Failure(new Error(ErrorCodes.ValidationFailed, "CARDHOLDER_NAME_REQUIRED: Cardholder name is required"));

		if (string.IsNullOrWhiteSpace(command.ExpiryDate))
			return Result<Order>.Failure(new Error(ErrorCodes.ValidationFailed, "EXPIRY_DATE_REQUIRED: Expiry date is required"));

		if (string.IsNullOrWhiteSpace(command.Cvv))
			return Result<Order>.Failure(new Error(ErrorCodes.ValidationFailed, "CVV_REQUIRED: CVV is required"));

		// Strip spaces from card number and validate
		var cardNumberDigits = command.CardNumber.Replace(" ", "");
		if (cardNumberDigits.Length != 16 || !cardNumberDigits.All(char.IsDigit))
			return Result<Order>.Failure(new Error(ErrorCodes.ValidationFailed, "INVALID_CARD_NUMBER: Card number must be exactly 16 digits"));

		// Validate expiry date format (MM/YY)
		var expiryRegex = new System.Text.RegularExpressions.Regex(@"^(0[1-9]|1[0-2])\/[0-9]{2}$");
		if (!expiryRegex.IsMatch(command.ExpiryDate))
			return Result<Order>.Failure(new Error(ErrorCodes.ValidationFailed, "INVALID_EXPIRY_FORMAT: Expiry date must be in MM/YY format"));

		// Validate expiry date is in the future
		var expiryParts = command.ExpiryDate.Split('/');
		var expiryMonth = int.Parse(expiryParts[0]);
		var expiryYear = 2000 + int.Parse(expiryParts[1]);
		var expiryDate = new DateTime(expiryYear, expiryMonth, 1).AddMonths(1).AddDays(-1); // Last day of expiry month
		if (expiryDate < DateTime.UtcNow.Date)
			return Result<Order>.Failure(new Error(ErrorCodes.ValidationFailed, "CARD_EXPIRED: Card expiry date must be in the future"));

		// Validate CVV
		if (command.Cvv.Length < 3 || command.Cvv.Length > 4 || !command.Cvv.All(char.IsDigit))
			return Result<Order>.Failure(new Error(ErrorCodes.ValidationFailed, "INVALID_CVV: CVV must be 3 or 4 digits"));

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

			// Publish OrderCreatedEvent to message queue
			var orderCreatedEvent = new OrderCreatedEvent(
				OrderId: orderEntity.Id,
				UserId: orderEntity.UserId,
				UserEmail: orderEntity.UserEmail,
				OrderDate: orderEntity.OrderDate,
				TotalAmount: orderEntity.TotalAmount,
				ShippingAddress: new ShippingAddressInfo(
					Address: orderEntity.ShippingAddress,
					City: orderEntity.ShippingCity,
					Province: orderEntity.ShippingProvince,
					PostalCode: orderEntity.ShippingPostalCode
				),
				LineItems: orderEntity.LineItems.Select(li => new OrderLineItemInfo(
					ProductId: li.ProductId,
					ProductName: li.ProductName,
					Quantity: li.Quantity,
					Price: li.ProductPrice
				)).ToList()
			);

			// Publish OrderCreatedEvent to message queue (best-effort, don't fail order creation)
			var publishResult = await messagePublisher.PublishOrderCreatedAsync(orderCreatedEvent, cancellationToken);
			if (!publishResult.IsSuccess)
			{
				// Message publishing failed, but order was successfully saved
				// Error is already logged by OrderMessagePublisher
				context.ChangeTracker.Clear(); // Prevent tracking issues
			}

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
			orderEntity.TrackingNumber,
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
