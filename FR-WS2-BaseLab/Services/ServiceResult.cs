public class ServiceResult
{
	public bool Succeeded { get; }
	public string? ErrorMessage { get; }

	protected ServiceResult(bool succeeded, string? errorMessage = null)
	{
		Succeeded = succeeded;
		ErrorMessage = errorMessage;
	}

	public static ServiceResult Success()
		=> new(true);

	public static ServiceResult Failure(string errorMessage)
		=> new(false, errorMessage);
}

public class ServiceResult<T> : ServiceResult
{
	public T? Value { get; }

	private ServiceResult(bool succeeded, T? value = default, string? errorMessage = null)
		: base(succeeded, errorMessage)
	{
		Value = value;
	}

	public static ServiceResult<T> Success(T value)
		=> new(true, value);

	// Cache la méthode parente pour retourner le bon type
	public new static ServiceResult<T> Failure(string errorMessage)
		=> new(false, default, errorMessage);
}