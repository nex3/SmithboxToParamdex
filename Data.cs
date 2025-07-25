class Data
{
    /// The root directory of the Smithbox repository.
    public readonly string SmithboxPath;

    /// The root directory of the Paramdex repository.
    public readonly string ParamdexPath;

    public Data(string smithboxPath, string paramdexPath)
    {
        SmithboxPath = smithboxPath;
        ParamdexPath = paramdexPath;
    }

    public IEnumerable<Game> Games()
    {
        var gameNames = Directory
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

        return gameNames.Select(gameName => new Game(
            gameName,
            Path.Join(SmithboxPath, "src/Smithbox.Data/Assets/PARAM", gameName),
            Path.Join(ParamdexPath, gameName)
        ));
    }
}
