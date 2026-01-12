using CopilotDemoApp.Server.Database;
using CopilotDemoApp.Server.Features.Product;
using CopilotDemoApp.Server.Shared;
using System;
using Xunit;

namespace CopilotDemoApp.Server.Tests.Features.Product;

public class ProductMapperTests
{
	[Fact]
	public void Maps_All_Fields_Correctly()
	{
		var entity = new Database.Product
		{
			Id = Guid.NewGuid(),
			Name = "Test Product",
			Description = "Test Description",
			Price = 9.99m,
			IsActive = true,
			CreatedDate = DateTime.UtcNow,
			UpdatedDate = DateTime.UtcNow.AddDays(1)
		};

		var domain = ProductMapper.MapEntityToDomain(entity);

		Assert.Equal(entity.Id, domain.Id);
		Assert.Equal(entity.Name is not null ? Option<string>.Some(entity.Name) : Option<string>.None(), domain.Name);
		Assert.Equal(entity.Description is not null ? Option<string>.Some(entity.Description) : Option<string>.None(), domain.Description);
		Assert.Equal(entity.Price.HasValue ? Option<decimal>.Some(entity.Price.Value) : Option<decimal>.None(), domain.Price);
		Assert.Equal(entity.IsActive, domain.IsActive);
		Assert.Equal(entity.CreatedDate, domain.CreatedDate);
		Assert.Equal(entity.UpdatedDate, domain.UpdatedDate);
	}

	[Fact]
	public void Maps_Nullable_Fields_To_None()
	{
		var entity = new Database.Product
		{
			Id = Guid.NewGuid(),
			Name = null,
			Description = null,
			Price = null,
			IsActive = false,
			CreatedDate = DateTime.UtcNow,
			UpdatedDate = DateTime.UtcNow.AddDays(1)
		};

		var domain = ProductMapper.MapEntityToDomain(entity);

		Assert.Equal(Option<string>.None(), domain.Name);
		Assert.Equal(Option<string>.None(), domain.Description);
		Assert.Equal(Option<decimal>.None(), domain.Price);
		Assert.Equal(entity.UpdatedDate, domain.UpdatedDate);
	}
}
