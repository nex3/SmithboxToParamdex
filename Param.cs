using System.Text.Encodings.Web;
using System.Text.Json;

internal sealed class Param
{
    private static readonly JsonSerializerOptions jsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        NewLine = "\n",
        WriteIndented = true,
        IndentSize = 2,
    };

    /// The parameter name.
    public readonly string Name;

    /// The Smithbox parameter object.
    private readonly string smithboxGamePath;

    /// The path to the Paramdex file with row names.
    private string SmithboxNamesPath =>
        Path.Join(smithboxGamePath, "Community Row Names", Name + ".json");

    /// The Smithbox parameter data.
    public List<RowNameEntry> SmithboxRows;

    /// The path to the game directory containing data about this parameter in
    /// Paramdex.
    private readonly string paramdexGamePath;

    /// The path to the Paramdex file with English or mixed-language row names.
    private string ParamdexNamesPath => Path.Join(paramdexGamePath, "Names", Name + ".txt");

    /// The path to the Paramdex file with Japanese row names.
    private string ParamdexJapaneseNamesPath =>
        Path.Join(paramdexGamePath, "Names", "jp", Name + ".txt");

    /// The Paramdex parameter data.
    public List<ParamdexRow> ParamdexRows;

    /// A lookup table from paramdex row IDs to all the rows with that ID in order.
    public ILookup<int, ParamdexRow> ParamdexRowsByID => paramdexRowsByID.Value;

    private readonly Lazy<ILookup<int, ParamdexRow>> paramdexRowsByID;

    /// The Paramdex Japanese parameter data.
    public List<ParamdexRow> ParamdexJapaneseRows;

    public Param(string name, string smithboxGamePath, string paramdexGamePath)
    {
        Name = name;
        this.smithboxGamePath = smithboxGamePath;
        SmithboxRows = File.Exists(SmithboxNamesPath)
            ? JsonSerializer.Deserialize<RowNameParam>(File.ReadAllText(SmithboxNamesPath))!.Entries
            : [];

        this.paramdexGamePath = paramdexGamePath;
        ParamdexRows = File.Exists(ParamdexNamesPath)
            ? [.. ReadParamdexRows(ParamdexNamesPath)]
            : [];
        ParamdexJapaneseRows = File.Exists(ParamdexJapaneseNamesPath)
            ? [.. ReadParamdexRows(ParamdexJapaneseNamesPath)]
            : [];

        paramdexRowsByID = new(() => ParamdexRows.ToLookup(row => row.ID));
    }

    /// Reads a file in paramdex format, accounting for the fact that names may
    /// spill across multiple lines.
    private static IEnumerable<ParamdexRow> ReadParamdexRows(string path)
    {
        using StreamReader reader = new(path);
        int? id = null;
        string? name = null;
        while (true)
        {
            string? line = reader.ReadLine();
            if (line == null)
            {
                break;
            }

            int spaceIndex = line.IndexOf(' ');
            if (spaceIndex != -1)
            {
                if (int.TryParse(line[0..spaceIndex], out int parsedId))
                {
                    if (id is not null && name is not null)
                    {
                        yield return new((int)id, name);
                    }
                    id = parsedId;
                    name = line[(spaceIndex + 1)..];
                    continue;
                }
            }

            if (id == null)
            {
                Console.Error.WriteLine($"Invalid line in {path}: {line}");
                Environment.Exit(1);
            }

            name += "\n" + line;
        }

        if (id is not null && name is not null)
        {
            yield return new((int)id, name);
        }
    }

    /// Writes Paramdex's paramter name data for this parameter.
    public void WriteParamdex()
    {
        if (ParamdexRows.Count > 0)
        {
            _ = Directory.CreateDirectory(Path.GetDirectoryName(ParamdexNamesPath)!);
            File.WriteAllText(ParamdexNamesPath, string.Join("", ParamdexRows));

            if (ParamdexJapaneseRows.Count > 0)
            {
                _ = Directory.CreateDirectory(Path.GetDirectoryName(ParamdexJapaneseNamesPath)!);
                File.WriteAllText(ParamdexJapaneseNamesPath, string.Join("", ParamdexJapaneseRows));
            }
        }
    }

    /// Writes Smithbox's parameter name data for this game.
    public void WriteSmithbox()
    {
        _ = Directory.CreateDirectory(Path.GetDirectoryName(SmithboxNamesPath)!);
        File.WriteAllText(
            SmithboxNamesPath,
            JsonSerializer.Serialize(
                new RowNameParam() { Name = Name, Entries = SmithboxRows },
                jsonOptions
            )
        );
    }
}
