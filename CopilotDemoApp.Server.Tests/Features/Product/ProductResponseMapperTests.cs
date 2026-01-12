using CopilotDemoApp.Server.Features.Product;
using CopilotDemoApp.Server.Shared;
using System;
using System.Collections.Generic;
using Xunit;

namespace CopilotDemoApp.Server.Tests.Features.Product;

public class ProductResponseMapperTests
{
	[Fact]
	public void Maps_Domain_To_Response_Correctly()
	{
		var domain = new CopilotDemoApp.Server.Features.Product.Product(
			Guid.NewGuid(),
			Option<string>.Some("Name"),
			Option<string>.Some("Desc"),
			Option<decimal>.Some(10.5m),
			true,
			DateTime.UtcNow,
			DateTime.UtcNow.AddDays(1)
		);

		var response = ProductResponseMapper.MapDomainToResponse(domain);

		Assert.Equal(domain.Id, response.Id);
		Assert.Equal("Name", response.Name);
		Assert.Equal("Desc", response.Description);
		Assert.Equal(10.5m, response.Price);
		Assert.True(response.IsActive);
		Assert.Equal(domain.CreatedDate, response.CreatedDate);
		Assert.Equal(domain.UpdatedDate, response.UpdatedDate);
	}

	[Fact]
	public void Maps_None_Options_To_Null()
	{
		var domain = new CopilotDemoApp.Server.Features.Product.Product(
			Guid.NewGuid(),
			Option<string>.None(),
			Option<string>.None(),
			Option<decimal>.None(),
			false,
			DateTime.UtcNow,
			DateTime.UtcNow.AddDays(1)
		);

		var response = ProductResponseMapper.MapDomainToResponse(domain);

		Assert.Null(response.Name);
		Assert.Null(response.Description);
		Assert.Null(response.Price);
		Assert.Equal(domain.UpdatedDate, response.UpdatedDate);
	}

	[Fact]
	public void Maps_Paged_Response_Correctly()
	{
		var products = new List<CopilotDemoApp.Server.Features.Product.Product>
		{
			new(Guid.NewGuid(), Option<string>.Some("A"), Option<string>.None(), Option<decimal>.Some(1), true, DateTime.UtcNow, DateTime.UtcNow.AddDays(1)),
			new(Guid.NewGuid(), Option<string>.Some("B"), Option<string>.None(), Option<decimal>.Some(2), true, DateTime.UtcNow, DateTime.UtcNow.AddDays(2))
		};
		var totalCount = 27;
		var page = 2;
		var pageSize = 25;

		var paged = ProductResponseMapper.MapToPagedResponse(products, totalCount, page, pageSize);

		Assert.Equal(totalCount, paged.TotalCount);
		Assert.Equal(page, paged.Page);
		Assert.Equal(pageSize, paged.PageSize);
		Assert.Equal(2, paged.TotalPages); // 27/25 = 1.08, round up to 2
		Assert.Equal(2, paged.Products.Count);
	}
}
