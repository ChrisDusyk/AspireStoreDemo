using System;
using System.Collections.Generic;
using System.Linq;

namespace CopilotDemoApp.Server.Features.Product;

public static class ProductResponseMapper
{
	public static ProductResponse MapDomainToResponse(Product domain) =>
		new(
			domain.Id,
			domain.Name.Match<string?>(n => n, () => null!),
			domain.Description.Match<string?>(d => d, () => null!),
			domain.Price.Match<decimal?>(p => p, () => null),
			domain.IsActive,
			domain.CreatedDate,
			domain.UpdatedDate
		);

	public static PagedProductResponse MapToPagedResponse(
		IReadOnlyList<Product> products,
		int totalCount,
		int page,
		int pageSize)
	{
		var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
		var responses = products.Select(MapDomainToResponse).ToList();
		return new PagedProductResponse(responses, totalCount, page, pageSize, totalPages);
	}
}
