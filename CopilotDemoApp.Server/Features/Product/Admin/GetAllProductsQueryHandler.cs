using CopilotDemoApp.Server.Database;
using CopilotDemoApp.Server.Shared;
using Microsoft.EntityFrameworkCore;

namespace CopilotDemoApp.Server.Features.Product.Admin;

public class GetAllProductsQueryHandler(AppDbContext db) : IQueryHandler<GetAllProductsQuery, PagedProductResponse>
{
	public async Task<Result<PagedProductResponse>> HandleAsync(GetAllProductsQuery query, CancellationToken cancellationToken = default)
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

			// Filter by IsActive (null = all products, true = active only, false = inactive only)
			if (filter.IsActive.HasValue)
			{
				productsQuery = productsQuery.Where(p => p.IsActive == filter.IsActive.Value);
			}

			// Apply sorting
			var sortBy = filter.SortBy?.ToLower() ?? "name";
			var sortDirection = filter.SortDirection?.ToLower() ?? "asc";

			productsQuery = sortBy switch
			{
				"name" => sortDirection == "desc"
					? productsQuery.OrderByDescending(p => p.Name)
					: productsQuery.OrderBy(p => p.Name),
				"createddate" => sortDirection == "asc"
					? productsQuery.OrderBy(p => p.CreatedDate)
					: productsQuery.OrderByDescending(p => p.CreatedDate),
				"updateddate" => sortDirection == "asc"
					? productsQuery.OrderBy(p => p.UpdatedDate)
					: productsQuery.OrderByDescending(p => p.UpdatedDate),
				_ => productsQuery.OrderBy(p => p.Name) // Default to name ascending
			};

			var totalCount = await productsQuery.CountAsync(cancellationToken);
			var page = filter.Page > 0 ? filter.Page : 1;
			var pageSize = filter.PageSize > 0 ? filter.PageSize : 25;
			var skip = (page - 1) * pageSize;

			var products = await productsQuery
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
