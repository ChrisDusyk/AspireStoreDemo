using CopilotDemoApp.Server.Database;
using CopilotDemoApp.Server.Shared;
using System;

namespace CopilotDemoApp.Server.Features.Product;

public static class ProductMapper
{
	public static Product MapEntityToDomain(Database.Product entity) =>
		new(
			entity.Id,
			Option<string>.From(entity.Name),
			Option<string>.From(entity.Description),
			entity.Price.HasValue ? Option<decimal>.Some(entity.Price.Value) : Option<decimal>.None(),
			entity.IsActive,
			entity.CreatedDate,
			entity.UpdatedDate
		);
}
