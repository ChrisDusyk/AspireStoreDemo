using CopilotDemoApp.Server.Database;
using CopilotDemoApp.Server.Shared;

namespace CopilotDemoApp.Server.Features.Product.Admin;

public class CreateProductCommandHandler(AppDbContext db) : ICommandHandler<CreateProductCommand, Guid>
{
	public async Task<Result<Guid>> HandleAsync(CreateProductCommand command, CancellationToken cancellationToken = default)
	{
		try
		{
			// Validate input
			if (string.IsNullOrWhiteSpace(command.Name))
			{
				return Result<Guid>.Failure(
					new Error(ErrorCodes.ValidationFailed, "Product name is required.")
				);
			}

			if (command.Price <= 0)
			{
				return Result<Guid>.Failure(
					new Error(ErrorCodes.ValidationFailed, "Product price must be greater than 0.")
				);
			}

			var entity = new Database.Product
			{
				Id = Guid.NewGuid(),
				Name = command.Name,
				Description = command.Description,
				Price = command.Price,
				IsActive = command.IsActive,
				CreatedDate = DateTime.UtcNow,
				UpdatedDate = DateTime.UtcNow
			};

			db.Products.Add(entity);
			await db.SaveChangesAsync(cancellationToken);

			return Result<Guid>.Success(entity.Id);
		}
		catch (Exception ex)
		{
			return Result<Guid>.Failure(
				new Error(ErrorCodes.DatabaseError, "A database error occurred while creating the product.", ex)
			);
		}
	}
}
