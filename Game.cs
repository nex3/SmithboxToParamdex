internal sealed class Game
{
    /// The game's name (actually its abbreviation).
    public readonly string Name;

    /// The directory of this game's PARAMS data in Smithbox.
    public readonly string SmithboxPath;

    /// The directory of this game's data in Paramdex.
    public readonly string ParamdexPath;

    public Game(string name, string smithboxPath, string paramdexPath)
    {
        Name = name;
        SmithboxPath = smithboxPath;
        ParamdexPath = paramdexPath;

        if (!Directory.Exists(smithboxPath) && !Directory.Exists(paramdexPath))
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

        string smithboxNamesDir = Path.Join(SmithboxPath, "Community Row Names");
        paramNames.UnionWith(
            Directory.Exists(smithboxNamesDir)
                ?
                [
                    .. Directory
                        .GetFiles(smithboxNamesDir, "*.json")
                        .Select(path => Path.GetFileName(path)[0..^5]),
                ]
                : []
        );

        return paramNames.Select(name => new Param(name, SmithboxPath, ParamdexPath));
    }
}
