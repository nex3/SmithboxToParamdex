using System.CommandLine;

#pragma warning disable IDE0028 // https://github.com/dotnet/roslyn/issues/79156
RootCommand rootCommand = new("Converts param names between Paramdex and Smithbox format.");
#pragma warning restore IDE0028

Option<bool> paramdexToSmithboxOption = new("--paramdex-to-smithbox")
{
    Description =
        "Convert Paramdex names to Smithbox instead of the reverse.\n"
        + "Only supports row names, not param defs.",
};
rootCommand.Add(paramdexToSmithboxOption);

Option<List<string>> gamesOption = new("--game")
{
    Description = "Only convert the given games.\n" + "May be passed multiple times.",
};
rootCommand.Add(gamesOption);

Argument<string> smithboxPath = new("path/to/Smithbox");
rootCommand.Add(smithboxPath);

Argument<string> paramdexPath = new("path/to/Paramdex");
rootCommand.Add(paramdexPath);

rootCommand.SetAction(parsedArgs =>
{
    Data data = new(
        parsedArgs.GetValue(smithboxPath)!,
        parsedArgs.GetValue(paramdexPath)!,
        games: parsedArgs.GetValue(gamesOption)
    );

    if (parsedArgs.GetValue(paramdexToSmithboxOption)!)
    {
        ParamdexToSmithbox.Run(data);
    }
    else
    {
        SmithboxToParamdex.Run(data);
    }
});

rootCommand.Parse(args[1..]).Invoke();
