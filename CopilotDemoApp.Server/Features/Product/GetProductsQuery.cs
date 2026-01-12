using CopilotDemoApp.Server.Shared;
using System.Collections.Generic;

namespace CopilotDemoApp.Server.Features.Product;

public sealed record GetProductsQuery(ProductFilterRequest Filter) : IQuery<PagedProductResponse>;
