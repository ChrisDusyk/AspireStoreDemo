using CopilotDemoApp.Server.Database;
using CopilotDemoApp.Server.Shared;
using Microsoft.EntityFrameworkCore;

namespace CopilotDemoApp.Server.Features.Order.Admin;

public class UpdateOrderToShippedCommandHandler(AppDbContext context) : ICommandHandler<UpdateOrderToShippedCommand, Unit>
{
	public async Task<Result<Unit>> HandleAsync(UpdateOrderToShippedCommand command, CancellationToken ct = default)
	{
		try
		{
			var order = await context.Orders
				.FirstOrDefaultAsync(o => o.Id == command.OrderId, ct);

			if (order is null)
			{
				return Result<Unit>.Failure(new Error(ErrorCodes.NotFound, $"Order with ID {command.OrderId} not found."));
			}

			if (order.Status != OrderStatus.Processing)
			{
				return Result<Unit>.Failure(new Error(ErrorCodes.ValidationFailed, $"Order must be in Processing status to mark as shipped. Current status: {order.Status}"));
			}

			if (string.IsNullOrWhiteSpace(command.TrackingNumber))
			{
				return Result<Unit>.Failure(new Error(ErrorCodes.ValidationFailed, "Tracking number is required."));
			}

			order.Status = OrderStatus.Shipped;
			order.TrackingNumber = command.TrackingNumber;
			await context.SaveChangesAsync(ct);

			return Result<Unit>.Success(Unit.Value);
		}
		catch (Exception ex)
		{
			return Result<Unit>.Failure(new Error(ErrorCodes.DatabaseError, "Failed to update order to shipped status.", ex));
		}
	}
}
