using CopilotDemoApp.Server.Shared;
using Xunit;

namespace CopilotDemoApp.Server.Tests.Shared;

public class UnitTests
{
	[Fact]
	public void Unit_Value_IsSingleton()
	{
		var unit1 = Unit.Value;
		var unit2 = Unit.Value;
		Assert.Equal(unit1, unit2);
	}
}
