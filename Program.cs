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

var smithboxPath = new Argument<string>("path/to/Smithbox");
rootCommand.Add(smithboxPath);

var paramdexPath = new Argument<string>("path/to/Paramdex");
rootCommand.Add(paramdexPath);

rootCommand.SetAction(parsedArgs =>
{
    if (parsedArgs.GetValue(paramdexToSmithboxOption)!)
    {
        ParamdexToSmithbox.Run(
            parsedArgs.GetValue(paramdexPath)!,
            parsedArgs.GetValue(smithboxPath)!
        );
    }
    else
    {
        SmithboxToParamdex.Run(
            parsedArgs.GetValue(smithboxPath)!,
            parsedArgs.GetValue(paramdexPath)!
        );
    }
});

rootCommand.Parse(args[1..]).Invoke();
