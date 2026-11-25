namespace QBuild.DataAccess.Configuration;

public sealed class CsvSettings
{
    public const string BomFileName = "bom.csv";
    public const string PartFileName = "part.csv";

    public required string DataDirectory { get; init; }

    public string BomFilePath => Path.Combine(DataDirectory, BomFileName);

    public string PartFilePath => Path.Combine(DataDirectory, PartFileName);

    public static CsvSettings FromDirectory(string dataDirectory)
    {
        if (string.IsNullOrWhiteSpace(dataDirectory))
        {
            throw new ArgumentException("Data directory is required.", nameof(dataDirectory));
        }

        return new CsvSettings
        {
            DataDirectory = Path.GetFullPath(dataDirectory)
        };
    }
}
