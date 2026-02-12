namespace CopilotDemoApp.Server.Shared.Messages;

/// <summary>
/// Generic message envelope for wrapping domain events and messages with metadata.
/// Provides consistent message identification, timestamping, and correlation across all message types.
/// </summary>
/// <typeparam name="TPayload">The type of the message payload</typeparam>
public sealed record MessageEnvelope<TPayload>(
	Guid MessageId,
	DateTimeOffset Timestamp,
	string MessageType,
	TPayload Payload,
	Guid? CorrelationId = null
);
