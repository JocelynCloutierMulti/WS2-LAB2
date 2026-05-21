namespace FR_WS2_BaseLab.Services;

public class ServiceResult<T>
{
    public bool Succeeded { get; private set; }
    public T? Value { get; private set; }
    public string? Error { get; private set; }

    public static ServiceResult<T> Ok(T value) =>
        new() { Succeeded = true, Value = value };
 
    public static ServiceResult<T> Fail(string error) =>
        new() { Succeeded = false, Error = error };
}