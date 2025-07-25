class Param
{
    /// The parameter name.
    public readonly string Name;

    /// The Smithbox parameter object.
    private readonly RowNameParam smithboxParam;

    /// The Smithbox parameter data.
    public List<RowNameEntry> SmithboxRows
    {
        get => smithboxParam.Entries;
        set { smithboxParam.Entries = value; }
    }

    /// The path to the game directory containing data about this parameter in
    /// Paramdex.
    private readonly string paramdexGamePath;

    /// The path to the Paramdex file with English or mixed-language row names.
    private string paramdexNamesPath => Path.Join(paramdexGamePath, "Names", Name + ".txt");

    /// The path to the Paramdex file with Japanese row names.
    private string paramdexJapaneseNamesPath =>
        Path.Join(paramdexGamePath, "Names", "jp", Name + ".txt");

    /// The Paramdex parameter data.
    public List<ParamdexRow> ParamdexRows;

    /// A lookup table from paramdex row IDs to all the rows with that ID in order.
    public ILookup<int, ParamdexRow> ParamdexRowsByID => paramdexRowsByID.Value;
    readonly Lazy<ILookup<int, ParamdexRow>> paramdexRowsByID;

    /// The Paramdex Japanese parameter data.
    public List<ParamdexRow> ParamdexJapaneseRows;

    public Param(string name, RowNameParam smithboxParam, string paramdexGamePath)
    {
        Name = name;
        this.smithboxParam = smithboxParam;
        this.paramdexGamePath = paramdexGamePath;
        ParamdexRows = File.Exists(paramdexNamesPath)
            ? readParamdexRows(paramdexNamesPath).ToList()
            : [];
        ParamdexJapaneseRows = File.Exists(paramdexJapaneseNamesPath)
            ? readParamdexRows(paramdexJapaneseNamesPath).ToList()
            : [];

        paramdexRowsByID = new(() => ParamdexRows.ToLookup(row => row.ID));
    }

    /// Reads a file in paramdex format, accounting for the fact that names may
    /// spill across multiple lines.
    public static IEnumerable<ParamdexRow> readParamdexRows(string path) =>
        readParamdexRowsWithoutPost(path)
            .Select(row => new ParamdexRow()
            {
                ID = row.ID,
                Name = row.Name == "UNKNOWN" ? "" : row.Name,
            });

    public static IEnumerable<ParamdexRow> readParamdexRowsWithoutPost(string path)
    {
        using var reader = new StreamReader(path);
        int? id = null;
        string? name = null;
        while (true)
        {
            var line = reader.ReadLine();
            if (line == null)
            {
                break;
            }

            var spaceIndex = line.IndexOf(' ');
            if (spaceIndex != -1)
            {
                if (Int32.TryParse(line[0..spaceIndex], out var parsedId))
                {
                    if (id is not null && name is not null)
                    {
                        yield return new() { ID = (int)id, Name = name };
                    }
                    id = parsedId;
                    name = line[(spaceIndex + 1)..];
                    continue;
                }
            }

            if (id == null)
            {
                Console.Error.WriteLine($"Invalid line in {path}: {line}");
                System.Environment.Exit(1);
            }

            name += "\n" + line;
        }

        if (id is not null && name is not null)
        {
            yield return new() { ID = (int)id, Name = name };
        }
    }

    /// Writes Paramdex's paramter name data for this parameter.
    public void WriteParamdex()
    {
        if (ParamdexRows.Count > 0)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(paramdexNamesPath)!);
            File.WriteAllText(paramdexNamesPath, String.Join("", ParamdexRows));

            if (ParamdexJapaneseRows.Count > 0)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(paramdexJapaneseNamesPath)!);
                File.WriteAllText(paramdexJapaneseNamesPath, String.Join("", ParamdexJapaneseRows));
            }
        }
    }
}
