namespace QBuild.Models;

public sealed class DataInitializationException : Exception
{
    public DataInitializationException(string message)
        : base(message)
    {
    }

    public DataInitializationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
