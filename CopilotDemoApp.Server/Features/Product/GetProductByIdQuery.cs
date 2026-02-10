using CopilotDemoApp.Server.Shared;
using CopilotDemoApp.Server.Database;
using System.Threading;
using System.Threading.Tasks;

namespace CopilotDemoApp.Server.Features.Product;

public sealed record GetProductByIdQuery(Guid Id) : IQuery<Option<ProductResponse>>;

public sealed class GetProductByIdQueryHandler(AppDbContext db) : IQueryHandler<GetProductByIdQuery, Option<ProductResponse>>
{
	public async Task<Result<Option<ProductResponse>>> HandleAsync(GetProductByIdQuery query, CancellationToken ct)
	{
		try
		{

			var entity = await db.Products.FindAsync(new object[] { query.Id }, ct);
			if (entity is null)
				return Result<Option<ProductResponse>>.Success(Option<ProductResponse>.None());
			// Map EF entity to domain Product, then to ProductResponse
			var domain = new Product(
				entity.Id,
				Option<string>.From(entity.Name),
				Option<string>.From(entity.Description),
				entity.Price.HasValue ? Option<decimal>.Some(entity.Price.Value) : Option<decimal>.None(),
				entity.IsActive,
				entity.CreatedDate,
				entity.UpdatedDate,
				Option<string>.From(entity.ImageUrl)
			);
			var response = ProductResponseMapper.MapDomainToResponse(domain);
			return Result<Option<ProductResponse>>.Success(Option<ProductResponse>.Some(response));
		}
		catch (Exception ex)
		{
			return Result<Option<ProductResponse>>.Failure(
				new Error(ErrorCodes.DatabaseError, "A database error occurred while retrieving the product by ID.", ex)
			);
		}
	}
}
