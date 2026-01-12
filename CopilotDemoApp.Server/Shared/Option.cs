namespace CopilotDemoApp.Server.Shared;

/// <summary>
/// Represents an optional value, replacing nulls in domain code.
/// </summary>
public readonly struct Option<T>
{
	private readonly T? _value;
	public bool HasValue { get; }
	public T Value => HasValue ? _value! : throw new InvalidOperationException("No value present.");

	private Option(T value)
	{
		_value = value;
		HasValue = true;
	}

	public static Option<T> Some(T value) => new(value);
	public static Option<T> None() => new();

	public Option<TResult> Map<TResult>(Func<T, TResult> mapper) =>
		HasValue ? Option<TResult>.Some(mapper(_value!)) : Option<TResult>.None();

	public Option<TResult> Bind<TResult>(Func<T, Option<TResult>> binder) =>
		HasValue ? binder(_value!) : Option<TResult>.None();

	public T GetValueOrDefault(T defaultValue) => HasValue ? _value! : defaultValue;
	public T? GetValueOrNull() => HasValue ? _value : default;

	public override string ToString() => HasValue ? $"Some({_value})" : "None";

	/// <summary>
	/// Creates an Option from a possibly-null value.
	/// </summary>
	public static Option<T> From(T? value) => value is not null ? Some(value) : None();

	/// <summary>
	/// Pattern matching for Option value.
	/// </summary>
	public TResult Match<TResult>(Func<T, TResult> onSome, Func<TResult> onNone)
	{
		return HasValue ? onSome(_value!) : onNone();
	}
}
