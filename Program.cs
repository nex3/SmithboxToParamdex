using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml;

var rootCommand = new RootCommand("Converts param names between Paramdex and Smithbox format.");

var paramdexToSmithboxOption = new Option<bool>("--paramdex-to-smithbox")
{
    Description =
        "Convert Paramdex names to Smithbox instead of the reverse.\n"
        + "Only supports row names, not param defs.",
};
rootCommand.Add(paramdexToSmithboxOption);

var gamesOption = new Option<List<string>>("--game")
{
    Description = "Only convert the given games.\n" + "May be passed multiple times.",
};
rootCommand.Add(gamesOption);

var smithboxPath = new Argument<string>("path/to/Smithbox");
rootCommand.Add(smithboxPath);

var paramdexPath = new Argument<string>("path/to/Paramdex");
rootCommand.Add(paramdexPath);

rootCommand.SetAction(parsedArgs =>
{
    var games = parsedArgs.GetValue(gamesOption);
    var data = new Data(
        parsedArgs.GetValue(smithboxPath)!,
        parsedArgs.GetValue(paramdexPath)!,
        games: games.Count > 0 ? games : null
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
