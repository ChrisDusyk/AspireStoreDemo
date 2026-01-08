namespace CopilotDemoApp.Server.Shared;

/// <summary>
/// Represents a void result for functional patterns.
/// </summary>
public readonly struct Unit
{
	public static readonly Unit Value = new();
}
