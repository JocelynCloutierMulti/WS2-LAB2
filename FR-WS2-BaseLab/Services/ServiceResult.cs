namespace FR_WS2_BaseLab.Services;

public class ServiceResult<T>
{
    public bool Succeeded { get; }
    public T? Value { get; }
    public string? ErrorMessage { get; }

    private ServiceResult(bool succeeded, T? value = default,
        string? errorMessage = null)
    {
        Succeeded = succeeded;
        Value = value;
        ErrorMessage = errorMessage;
    }

    public static ServiceResult<T> Success(T value)
    {
        return new ServiceResult<T>(true, value);
    }

    public static ServiceResult<T> Failure(string errorMessage)
    {
        return new ServiceResult<T>(false, default, errorMessage);
    }
}