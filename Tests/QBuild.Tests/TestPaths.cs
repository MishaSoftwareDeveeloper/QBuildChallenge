namespace QBuild.Tests;

internal static class TestPaths
{
    public static string DataDirectory
    {
        get
        {
            var outputData = Path.Combine(AppContext.BaseDirectory, "Data");
            if (File.Exists(Path.Combine(outputData, "bom.csv")))
            {
                return outputData;
            }

            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory is not null)
            {
                var candidate = Path.Combine(directory.FullName, "Data");
                if (File.Exists(Path.Combine(candidate, "bom.csv")))
                {
                    return candidate;
                }

                directory = directory.Parent;
            }

            throw new DirectoryNotFoundException("Could not locate Data/bom.csv for tests.");
        }
    }
}
