using QBuild.Models;

namespace QBuild.API;

public static class DataDirectoryResolver
{
    public static string Resolve(IConfiguration configuration, IHostEnvironment environment)
    {
        var configured = configuration["CsvData:Directory"];
        var candidates = new List<string>();

        void Add(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            candidates.Add(Path.GetFullPath(path));
        }

        if (!string.IsNullOrWhiteSpace(configured))
        {
            Add(Path.IsPathRooted(configured)
                ? configured
                : Path.Combine(environment.ContentRootPath, configured));
            Add(Path.Combine(AppContext.BaseDirectory, configured));
        }

        Add(Path.Combine(AppContext.BaseDirectory, "Data"));
        Add(Path.Combine(environment.ContentRootPath, "Data"));

        var directory = new DirectoryInfo(environment.ContentRootPath);
        while (directory is not null)
        {
            Add(Path.Combine(directory.FullName, "Data"));
            directory = directory.Parent;
        }

        foreach (var candidate in candidates.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var bom = Path.Combine(candidate, "bom.csv");
            var part = Path.Combine(candidate, "part.csv");
            if (File.Exists(bom) && File.Exists(part))
            {
                return candidate;
            }
        }

        throw new DataInitializationException(
            "CSV data files were not found. Expected bom.csv and part.csv in a Data directory. " +
            $"Searched: {string.Join("; ", candidates.Distinct(StringComparer.OrdinalIgnoreCase))}");
    }
}
