namespace AssetManagement.Utility;

public sealed class Envelope : Envelope<object>
{
    private Envelope(object? data, string message, int statusCode)
        : base(data, message, statusCode) { }

    public static new Envelope Ok(object? data, string message, int statusCode)
    {
        return new Envelope(data, message, statusCode);
    }

    public static new Envelope Error(string message, int statusCode)
    {
        return new Envelope(null, message, statusCode);
    }
}