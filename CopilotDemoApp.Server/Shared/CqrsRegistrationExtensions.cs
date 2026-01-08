using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace CopilotDemoApp.Server.Shared;

/// <summary>
/// Extension methods for registering CQRS handlers in DI.
/// </summary>
public static class CqrsRegistrationExtensions
{
	public static IServiceCollection AddCqrsHandlers(this IServiceCollection services, Assembly? assembly = null)
	{
		assembly ??= Assembly.GetExecutingAssembly();
		var handlerTypes = assembly.GetTypes()
			.Where(t => !t.IsAbstract && !t.IsInterface)
			.SelectMany(t => t.GetInterfaces(), (t, i) => new { Type = t, Interface = i })
			.Where(x =>
				x.Interface.IsGenericType &&
				((x.Interface.GetGenericTypeDefinition() == typeof(IQueryHandler<,>)) ||
				 (x.Interface.GetGenericTypeDefinition() == typeof(ICommandHandler<,>))))
			.ToList();

		foreach (var handler in handlerTypes)
		{
			services.AddScoped(handler.Interface, handler.Type);
		}
		return services;
	}
}
