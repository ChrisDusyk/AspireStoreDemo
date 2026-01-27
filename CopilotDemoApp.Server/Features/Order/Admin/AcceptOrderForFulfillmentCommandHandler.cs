using CopilotDemoApp.Server.Database;
using CopilotDemoApp.Server.Shared;
using Microsoft.EntityFrameworkCore;

namespace CopilotDemoApp.Server.Features.Order.Admin;

public class AcceptOrderForFulfillmentCommandHandler(AppDbContext context) : ICommandHandler<AcceptOrderForFulfillmentCommand, Unit>
{
	public async Task<Result<Unit>> HandleAsync(AcceptOrderForFulfillmentCommand command, CancellationToken cancellationToken = default)
	{
		try
		{
			var orderEntity = await context.Orders
				.FirstOrDefaultAsync(o => o.Id == command.OrderId, cancellationToken);

			if (orderEntity == null)
			{
				return Result<Unit>.Failure(new Error(ErrorCodes.NotFound, $"Order with ID {command.OrderId} not found"));
			}

			if (orderEntity.Status != OrderStatus.Pending)
			{
				return Result<Unit>.Failure(new Error(ErrorCodes.ValidationFailed, $"Order cannot be accepted for fulfillment. Current status is {orderEntity.Status}, expected Pending"));
			}

			orderEntity.Status = OrderStatus.Processing;
			await context.SaveChangesAsync(cancellationToken);

			return Result<Unit>.Success(Unit.Value);
		}
		catch (Exception ex)
		{
			return Result<Unit>.Failure(new Error(ErrorCodes.DatabaseError, "An error occurred while accepting the order for fulfillment", ex));
		}
	}
}
