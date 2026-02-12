using System;

namespace CopilotDemoApp.Server.Shared;

/// <summary>
/// Represents an error, which can be an exception or a domain failure.
/// </summary>
public class Error(string code, string message, Exception? exception = null)
{
	public string Code { get; } = code;
	public string Message { get; } = message;
	public Exception? Exception { get; } = exception;

	public override string ToString() => $"Error[{Code}]: {Message}" + (Exception is not null ? $" Exception: {Exception}" : "");
}

public static class ErrorCodes
{
	public const string NotFound = "NotFound";
	public const string ValidationFailed = "ValidationFailed";
	public const string Unauthorized = "Unauthorized";
	public const string DatabaseError = "DatabaseError";
	public const string InvalidFileType = "InvalidFileType";
	public const string FileTooLarge = "FileTooLarge";
	public const string MessagePublishingFailed = "MessagePublishingFailed";
}
