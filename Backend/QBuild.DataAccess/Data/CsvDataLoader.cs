using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using QBuild.DataAccess.Configuration;
using QBuild.Models;

namespace QBuild.DataAccess.Data;

public sealed class CsvDataLoader
{
    private readonly CsvSettings _settings;
    private readonly object _gate = new();
    private bool _loaded;

    public CsvDataLoader(CsvSettings settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    public LoadedCsvData Load()
    {
        lock (_gate)
        {
            if (_loaded)
            {
                throw new DataInitializationException("CSV data has already been loaded. Duplicate initialization is not allowed.");
            }

            ValidateFiles();

            var bomRecords = ReadRecords(_settings.BomFilePath, ReadBomRecords);
            var parts = ReadRecords(_settings.PartFilePath, ReadPartRecords);

            _loaded = true;

            return new LoadedCsvData(bomRecords, parts);
        }
    }

    private void ValidateFiles()
    {
        if (!Directory.Exists(_settings.DataDirectory))
        {
            throw new DataInitializationException(
                $"Data directory was not found: '{_settings.DataDirectory}'.");
        }

        if (!File.Exists(_settings.BomFilePath))
        {
            throw new DataInitializationException(
                $"Required BOM file was not found: '{_settings.BomFilePath}'.");
        }

        if (!File.Exists(_settings.PartFilePath))
        {
            throw new DataInitializationException(
                $"Required PART file was not found: '{_settings.PartFilePath}'.");
        }
    }

    private static IReadOnlyList<T> ReadRecords<T>(string path, Func<CsvReader, IReadOnlyList<T>> read)
    {
        try
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            using var csv = new CsvReader(reader, CreateConfiguration());
            return read(csv);
        }
        catch (DataInitializationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new DataInitializationException($"Failed to parse CSV file '{path}'. {ex.Message}", ex);
        }
    }

    private static CsvConfiguration CreateConfiguration()
    {
        return new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            TrimOptions = TrimOptions.Trim,
            IgnoreBlankLines = true,
            MissingFieldFound = args =>
            {
                throw new DataInitializationException(
                    $"Missing CSV field at row {args.Context.Parser?.Row}.");
            },
            HeaderValidated = args =>
            {
                if (args.InvalidHeaders is { Length: > 0 })
                {
                    var names = string.Join(", ", args.InvalidHeaders.SelectMany(h => h.Names));
                    throw new DataInitializationException($"CSV header mapping failed for: {names}.");
                }
            },
            BadDataFound = args =>
            {
                throw new DataInitializationException(
                    $"Malformed CSV data at row {args.Context.Parser?.Row}: {args.RawRecord}");
            }
        };
    }

    private static IReadOnlyList<BomRecord> ReadBomRecords(CsvReader csv)
    {
        csv.Context.RegisterClassMap<BomRecordMap>();
        var records = new List<BomRecord>();

        foreach (var record in csv.GetRecords<BomRecord>())
        {
            if (string.IsNullOrWhiteSpace(record.ComponentName))
            {
                throw new DataInitializationException(
                    $"BOM COMPONENT_NAME is required at CSV row {csv.Context.Parser?.Row}.");
            }

            records.Add(record);
        }

        if (records.Count == 0)
        {
            throw new DataInitializationException("bom.csv does not contain any records.");
        }

        return records;
    }

    private static IReadOnlyList<Part> ReadPartRecords(CsvReader csv)
    {
        csv.Context.RegisterClassMap<PartMap>();
        var records = new List<Part>();
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var record in csv.GetRecords<Part>())
        {
            if (string.IsNullOrWhiteSpace(record.Name))
            {
                throw new DataInitializationException(
                    $"PART NAME is required at CSV row {csv.Context.Parser?.Row}.");
            }

            if (!names.Add(record.Name))
            {
                throw new DataInitializationException(
                    $"Duplicate PART.NAME '{record.Name}' at CSV row {csv.Context.Parser?.Row}.");
            }

            records.Add(record);
        }

        if (records.Count == 0)
        {
            throw new DataInitializationException("part.csv does not contain any records.");
        }

        return records;
    }

    private sealed class BomRecordMap : ClassMap<BomRecord>
    {
        public BomRecordMap()
        {
            Map(m => m.ParentName)
                .Name("PARENT_NAME")
                .Convert(args => (args.Row.GetField("PARENT_NAME") ?? string.Empty).Trim());
            Map(m => m.Quantity).Name("QUANTITY");
            Map(m => m.ComponentName)
                .Name("COMPONENT_NAME")
                .Convert(args => (args.Row.GetField("COMPONENT_NAME") ?? string.Empty).Trim());
        }
    }

    private sealed class PartMap : ClassMap<Part>
    {
        public PartMap()
        {
            Map(m => m.Name).Name("NAME").Convert(args => Normalize(args.Row.GetField("NAME")));
            Map(m => m.Type).Name("TYPE").Convert(args => EmptyToNull(args.Row.GetField("TYPE")));
            Map(m => m.Item).Name("ITEM").Convert(args => EmptyToNull(args.Row.GetField("ITEM")));
            Map(m => m.PartNumber).Name("PART_NUMBER").Convert(args => EmptyToNull(args.Row.GetField("PART_NUMBER")));
            Map(m => m.Title).Name("TITLE").Convert(args => EmptyToNull(args.Row.GetField("TITLE")));
            Map(m => m.Material).Name("MATERIAL").Convert(args => EmptyToNull(args.Row.GetField("MATERIAL")));
        }

        private static string Normalize(string? value) => (value ?? string.Empty).Trim();

        private static string? EmptyToNull(string? value)
        {
            var trimmed = (value ?? string.Empty).Trim();
            return trimmed.Length == 0 ? null : trimmed;
        }
    }
}

public sealed record LoadedCsvData(
    IReadOnlyList<BomRecord> BomRecords,
    IReadOnlyList<Part> Parts);
