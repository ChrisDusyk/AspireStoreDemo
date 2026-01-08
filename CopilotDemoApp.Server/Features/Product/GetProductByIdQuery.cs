using CopilotDemoApp.Server.Shared;
using CopilotDemoApp.Server.Database;
using System.Threading;
using System.Threading.Tasks;

namespace CopilotDemoApp.Server.Features.Product;

public sealed record GetProductByIdQuery(Guid Id) : IQuery<Option<ProductResponse>>;

public sealed class GetProductByIdQueryHandler : IQueryHandler<GetProductByIdQuery, Option<ProductResponse>>
{
	private readonly AppDbContext _db;

	public GetProductByIdQueryHandler(AppDbContext db)
	{
		_db = db;
	}

	public async Task<Result<Option<ProductResponse>>> HandleAsync(GetProductByIdQuery query, CancellationToken ct)
	{
		var entity = await _db.Products.FindAsync(new object[] { query.Id }, ct);
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
			entity.UpdatedDate
		);
		var response = ProductResponseMapper.MapDomainToResponse(domain);
		return Result<Option<ProductResponse>>.Success(Option<ProductResponse>.Some(response));
	}

	// For backward compatibility if needed
	public Task<Result<Option<ProductResponse>>> Handle(GetProductByIdQuery query, CancellationToken ct)
		=> HandleAsync(query, ct);
}
