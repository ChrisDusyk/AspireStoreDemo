using CopilotDemoApp.Server.Shared;
using Xunit;

namespace CopilotDemoApp.Server.Tests.Shared;

public class OptionTests
{
	[Fact]
	public void Some_HasValue_AndReturnsValue()
	{
		var opt = Option<int>.Some(42);
		Assert.True(opt.HasValue);
		Assert.Equal(42, opt.Value);
	}

	[Fact]
	public void None_HasNoValue_ThrowsOnValue()
	{
		var opt = Option<int>.None();
		Assert.False(opt.HasValue);
		Assert.Throws<InvalidOperationException>(() => _ = opt.Value);
	}

	[Fact]
	public void Map_TransformsValue_WhenSome()
	{
		var opt = Option<int>.Some(5).Map(x => x * 2);
		Assert.True(opt.HasValue);
		Assert.Equal(10, opt.Value);
	}

	[Fact]
	public void Map_None_RemainsNone()
	{
		var opt = Option<int>.None().Map(x => x * 2);
		Assert.False(opt.HasValue);
	}

	[Fact]
	public void Bind_ChainsOptions_WhenSome()
	{
		var opt = Option<int>.Some(3).Bind(x => Option<string>.Some($"Num:{x}"));
		Assert.True(opt.HasValue);
		Assert.Equal("Num:3", opt.Value);
	}

	[Fact]
	public void Bind_None_RemainsNone()
	{
		var opt = Option<int>.None().Bind(x => Option<string>.Some($"Num:{x}"));
		Assert.False(opt.HasValue);
	}

	[Fact]
	public void GetValueOrDefault_ReturnsValueOrDefault()
	{
		var some = Option<string>.Some("abc");
		var none = Option<string>.None();
		Assert.Equal("abc", some.GetValueOrDefault("zzz"));
		Assert.Equal("zzz", none.GetValueOrDefault("zzz"));
	}

	[Fact]
	public void GetValueOrNull_ReturnsValueOrNull()
	{
		var some = Option<int?>.Some(7);
		var none = Option<int?>.None();
		Assert.Equal(7, some.GetValueOrNull());
		Assert.Null(none.GetValueOrNull());
	}
}
