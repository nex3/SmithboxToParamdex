using System.Text.Json;

internal sealed class Game
{
    private static readonly JsonSerializerOptions jsonOptions = new()
    {
        NewLine = "\n",
        WriteIndented = true,
        IndentSize = 2,
    };

    /// The game's name (actually its abbreviation).
    public readonly string Name;

    /// The directory of this game's PARAMS data in Smithbox.
    public readonly string SmithboxPath;

    /// The directory of this game's data in Paramdex.
    public readonly string ParamdexPath;

    /// The parsed <c>Community Param Names.json</c> file from Smithbox.
    private readonly RowNameStore smithboxStore;

    /// A map from parameter names to Smithbox parameters.
    private readonly Dictionary<string, RowNameParam> smithboxByName;

    /// All the parameters that exist in Paramdex but not Smithbox.
    ///
    /// These are reflected in <see>smithboxByName</see>, but not in
    /// <see>smithboxStore</see> until <see>WriteSmithbox</see> is called.
    private readonly HashSet<string> newSmithboxParams = [];

    public Game(string name, string smithboxPath, string paramdexPath)
    {
        Name = name;
        SmithboxPath = smithboxPath;
        ParamdexPath = paramdexPath;

        string smithboxFile = Path.Join(SmithboxPath, "Community Row Names.json");
        smithboxStore = File.Exists(smithboxFile)
            ? JsonSerializer.Deserialize<RowNameStore>(File.ReadAllText(smithboxFile))!
            : new() { Params = [] };
        smithboxByName = smithboxStore.Params.ToDictionary(param => param.Name, param => param);

        if (!File.Exists(smithboxFile) && !Directory.Exists(paramdexPath))
        {
            throw new InvalidOperationException($"Neither source has a game named {name}");
        }
    }

    public IEnumerable<Param> Params()
    {
        string paramdexNamesDir = Path.Join(ParamdexPath, "Names");
        HashSet<string> paramNames = Directory.Exists(paramdexNamesDir)
            ?
            [
                .. Directory
                    .GetFiles(paramdexNamesDir, "*.txt")
                    .Select(path => Path.GetFileName(path)[0..^4]),
            ]
            : [];
        paramNames.UnionWith(smithboxByName.Keys);

        return paramNames.Select(name =>
        {
            if (smithboxByName.TryGetValue(name, out RowNameParam? smithboxParam))
            {
                return new Param(name, smithboxParam, ParamdexPath);
            }
            else
            {
                smithboxParam = new RowNameParam() { Name = name, Entries = [] };
                smithboxByName[name] = smithboxParam;
                _ = newSmithboxParams.Add(name);
                return new Param(name, smithboxParam, ParamdexPath);
            }
        });
    }

    /// Writes Smithbox's <c>Community Param Names.json</c> for this game.
    public void WriteSmithbox()
    {
        foreach (string name in newSmithboxParams)
        {
            RowNameParam param = smithboxByName[name];
            if (param.Entries.Count > 0)
            {
                smithboxStore.Params.Add(param);
            }
        }

        if (smithboxStore.Params.Count > 0)
        {
            _ = Directory.CreateDirectory(SmithboxPath);
            File.WriteAllText(
                Path.Join(SmithboxPath, "Community Row Names.json"),
                JsonSerializer.Serialize(smithboxStore, jsonOptions)
            );
        }
    }
}
