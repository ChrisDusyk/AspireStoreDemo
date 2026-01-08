using CopilotDemoApp.Server.Database;
using CopilotDemoApp.Server.Shared;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace CopilotDemoApp.Server.Features.Product;

public class GetProductsQueryHandler(AppDbContext db) : IQueryHandler<GetProductsQuery, PagedProductResponse>
{
	public async Task<Result<PagedProductResponse>> HandleAsync(GetProductsQuery query, CancellationToken cancellationToken = default)
	{
		try
		{
			var filter = query.Filter;
			var productsQuery = db.Products.AsQueryable();

			// Filter by name (contains, case-insensitive)
			if (!string.IsNullOrWhiteSpace(filter.Name))
			{
				productsQuery = productsQuery.Where(p => p.Name != null && p.Name.ToLower().Contains(filter.Name.ToLower()));
			}

			// Filter by IsActive (default: true)
			if (filter.IsActive.HasValue)
			{
				productsQuery = productsQuery.Where(p => p.IsActive == filter.IsActive.Value);
			}
			else
			{
				productsQuery = productsQuery.Where(p => p.IsActive);
			}

			var totalCount = await productsQuery.CountAsync(cancellationToken);
			var page = filter.Page > 0 ? filter.Page : 1;
			var pageSize = filter.PageSize > 0 ? filter.PageSize : 25;
			var skip = (page - 1) * pageSize;

			var products = await productsQuery
				.OrderByDescending(p => p.CreatedDate)
				.Skip(skip)
				.Take(pageSize)
				.ToListAsync(cancellationToken);

			var domainProducts = products.Select(ProductMapper.MapEntityToDomain).ToList();
			var pagedResponse = ProductResponseMapper.MapToPagedResponse(domainProducts, totalCount, page, pageSize);
			return Result<PagedProductResponse>.Success(pagedResponse);
		}
		catch (Exception ex)
		{
			return Result<PagedProductResponse>.Failure(
				new Error(ErrorCodes.DatabaseError, "A database error occurred while retrieving products.", ex)
			);
		}
	}
}
