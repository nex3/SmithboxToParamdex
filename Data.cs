internal sealed class Data(
    string smithboxPath,
    string paramdexPath,
    IEnumerable<string>? games = null
)
{
    /// The root directory of the Smithbox repository.
    public readonly string SmithboxPath = smithboxPath;

    /// The root directory of the Paramdex repository.
    public readonly string ParamdexPath = paramdexPath;

    /// The names of games to convert, or null if all games should be converted.
    public readonly HashSet<string> gameNames = games == null ? [] : new(games);

    public IEnumerable<Game> Games()
    {
        HashSet<string> gameNames;
        if (this.gameNames.Count == 0)
        {
            gameNames =
            [
                .. Directory
                    .GetDirectories(ParamdexPath)
                    .Select(dir => Path.GetFileName(dir)!)
                    .Where(name => !name.StartsWith('.')),
            ];
            gameNames.UnionWith(
                Directory
                    .GetDirectories(Path.Join(SmithboxPath, "src/Smithbox.Data/Assets/PARAM"))
                    .Select(dir => Path.GetFileName(dir)!)
                    .Where(name => !name.StartsWith('.'))
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
