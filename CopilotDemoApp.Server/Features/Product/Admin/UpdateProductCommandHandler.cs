using CopilotDemoApp.Server.Database;
using CopilotDemoApp.Server.Shared;
using Microsoft.EntityFrameworkCore;

namespace CopilotDemoApp.Server.Features.Product.Admin;

public class UpdateProductCommandHandler(AppDbContext db) : ICommandHandler<UpdateProductCommand, ProductResponse>
{
	public async Task<Result<ProductResponse>> HandleAsync(UpdateProductCommand command, CancellationToken cancellationToken = default)
	{
		try
		{
			// Validate input
			if (string.IsNullOrWhiteSpace(command.Name))
			{
				return Result<ProductResponse>.Failure(
					new Error(ErrorCodes.ValidationFailed, "Product name is required.")
				);
			}

			if (command.Price <= 0)
			{
				return Result<ProductResponse>.Failure(
					new Error(ErrorCodes.ValidationFailed, "Product price must be greater than 0.")
				);
			}

			// Fetch product by ID
			var entity = await db.Products.FindAsync([command.Id], cancellationToken);
			if (entity is null)
			{
				return Result<ProductResponse>.Failure(
					new Error(ErrorCodes.NotFound, $"Product with ID {command.Id} not found.")
				);
			}

			// Update entity fields
			entity.Name = command.Name;
			entity.Description = command.Description;
			entity.Price = command.Price;
			entity.IsActive = command.IsActive;
			entity.UpdatedDate = DateTime.UtcNow;

			await db.SaveChangesAsync(cancellationToken);

			// Map entity to domain Product, then to ProductResponse
			var domain = ProductMapper.MapEntityToDomain(entity);
			var response = ProductResponseMapper.MapDomainToResponse(domain);

			return Result<ProductResponse>.Success(response);
		}
		catch (Exception ex)
		{
			return Result<ProductResponse>.Failure(
				new Error(ErrorCodes.DatabaseError, "A database error occurred while updating the product.", ex)
			);
		}
	}
}
