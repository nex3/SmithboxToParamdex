class Data
{
    /// The root directory of the Smithbox repository.
    public readonly string SmithboxPath;

    /// The root directory of the Paramdex repository.
    public readonly string ParamdexPath;

    /// The names of games to convert, or null if all games should be converted.
    public readonly HashSet<string>? gameNames;

    public Data(string smithboxPath, string paramdexPath, IEnumerable<string>? games = null)
    {
        SmithboxPath = smithboxPath;
        ParamdexPath = paramdexPath;
        gameNames = games == null ? null : new(games);
    }

    public IEnumerable<Game> Games()
    {
        HashSet<string> gameNames;
        if (this.gameNames is null)
        {
            gameNames = Directory
                .GetDirectories(ParamdexPath)
                .Select(dir => Path.GetFileName(dir)!)
                .Where(name => !name.StartsWith("."))
                .ToHashSet();
            gameNames.UnionWith(
                Directory
                    .GetDirectories(Path.Join(SmithboxPath, "src/Smithbox.Data/Assets/PARAM"))
                    .Select(dir => Path.GetFileName(dir)!)
                    .Where(name => !name.StartsWith("."))
            );
        }
        else
        {
            gameNames = this.gameNames;
        }

        return gameNames.Select(gameName => new Game(
            gameName,
            Path.Join(SmithboxPath, "src/Smithbox.Data/Assets/PARAM", gameName),
            Path.Join(ParamdexPath, gameName)
        ));
    }
}
