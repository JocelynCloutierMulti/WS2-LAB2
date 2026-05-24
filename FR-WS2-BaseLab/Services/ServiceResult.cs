public class ServiceResult<T>
{
	public bool Success { get; set; }
	public string? ErrorMessage { get; set; }
	public T? Value { get; set; }

	public static ServiceResult<T> Ok(T value) => new()
	{
		Success = true,
		Value = value
	};

	public static ServiceResult<T> Fail(string error) => new()
	{
		Success = false,
		ErrorMessage = error
	};
}
