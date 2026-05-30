namespace AssetManagement.Utility;

public class Envelope<T>
{
    public T? Data { get; set; }
    public string Message { get; set; }
    public int StatusCode { get; set; }

    protected internal Envelope(T? data, string message, int statusCode)
    {
        Data = data;
        Message = message;
        StatusCode = statusCode;
    }

    public static Envelope<T> Ok(T? data, string message, int statusCode)
    {
        return new Envelope<T>(data, message, statusCode);
    }

    public static Envelope<T> Error(string message, int statusCode)
    {
        return new Envelope<T>(default, message, statusCode);
    }
}