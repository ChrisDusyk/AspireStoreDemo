using CopilotDemoApp.Server.Database;
using CopilotDemoApp.Server.Shared;
using Microsoft.EntityFrameworkCore;

namespace CopilotDemoApp.Server.Features.Order.Admin;

public class UpdateOrderToDeliveredCommandHandler(AppDbContext context) : ICommandHandler<UpdateOrderToDeliveredCommand, Unit>
{
	public async Task<Result<Unit>> HandleAsync(UpdateOrderToDeliveredCommand command, CancellationToken ct = default)
	{
		try
		{
			var order = await context.Orders
				.FirstOrDefaultAsync(o => o.Id == command.OrderId, ct);

			if (order is null)
			{
				return Result<Unit>.Failure(new Error(ErrorCodes.NotFound, $"Order with ID {command.OrderId} not found."));
			}

			if (order.Status != OrderStatus.Shipped)
			{
				return Result<Unit>.Failure(new Error(ErrorCodes.ValidationFailed, $"Order must be in Shipped status to mark as delivered. Current status: {order.Status}"));
			}

			order.Status = OrderStatus.Delivered;
			await context.SaveChangesAsync(ct);

			return Result<Unit>.Success(Unit.Value);
		}
		catch (Exception ex)
		{
			return Result<Unit>.Failure(new Error(ErrorCodes.DatabaseError, "Failed to update order to delivered status.", ex));
		}
	}
}
