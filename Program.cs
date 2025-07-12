using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml;

if (args.Count() != 3)
{
    Console.Error.WriteLine("Usage: dotnet run Program.cs -- path/to/Smithbox path/to/Paramdex");
    System.Environment.Exit(1);
}

/// Reads a file in paramdex format, accounting for the fact that names may
/// spill across multiple lines.
IEnumerable<(int, string)> readParamdexFile(string path)
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
                    yield return ((int)id, name);
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
        yield return ((int)id, name);
    }
}

var jpNameRegex = new Regex(@"(?:^.* -- )?(.*[\u3000-\u30ff\uff00-\uffef\u4e00-\u9faf])");

void syncParamNames(string smithboxFile)
{
    var gameCode = Path.GetFileName(Path.GetDirectoryName(smithboxFile));
    var smithboxStore = JsonSerializer.Deserialize<RowNameStore>(File.ReadAllText(smithboxFile))!;

    foreach (var smithboxParams in smithboxStore.Params)
    {
        var paramdexFile = Path.Join(args[2], gameCode, "Names", smithboxParams.Name + ".txt");
        Queue<(int, string)> paramdexParams = File.Exists(paramdexFile)
            ? new(readParamdexFile(paramdexFile))
            : [];
        Queue<(int, string)> outputParams = [];
        Queue<(int, string)> jpOutputParams = [];

        foreach (var entry in smithboxParams.Entries)
        {
            int? paramdexID = null;
            string? paramdexName = null;
            if (paramdexParams.Count > 0)
            {
                (paramdexID, paramdexName) = paramdexParams.Peek();
            }
            if (paramdexID == entry.ID)
            {
                paramdexParams.Dequeue();
            }

            if (entry.Name != "")
            {
                outputParams.Enqueue((entry.ID, entry.Name));

                if (paramdexName is not null)
                {
                    var match = jpNameRegex.Match(paramdexName);
                    if (match.Success)
                    {
                        var jpParamdexName = match.Groups[1].Value;
                        if (!entry.Name.Contains(jpParamdexName))
                        {
                            jpOutputParams.Enqueue(((int)paramdexID!, jpParamdexName));
                        }
                    }
                }
            }
            else if (paramdexID is not null && paramdexName is not null && paramdexID == entry.ID)
            {
                outputParams.Enqueue(((int)paramdexID, paramdexName));
            }
        }

        if (outputParams.Count > 0)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(paramdexFile)!);
            File.WriteAllText(
                paramdexFile,
                String.Join("", outputParams.Select(tuple => $"{tuple.Item1} {tuple.Item2}\n"))
            );

            if (jpOutputParams.Count > 0)
            {
                var jpParamdexFile = Path.Join(
                    Path.GetDirectoryName(paramdexFile),
                    "jp",
                    Path.GetFileName(paramdexFile)
                );
                Directory.CreateDirectory(Path.GetDirectoryName(jpParamdexFile)!);
                File.WriteAllText(
                    jpParamdexFile,
                    String.Join(
                        "",
                        jpOutputParams.Select(tuple => $"{tuple.Item1} {tuple.Item2}\n")
                    )
                );
            }
        }
    }
}

void changeNodeName(XmlNode node, string newName)
{
    var newNode = ((XmlDocument)node.OwnerDocument!).CreateNode(node.NodeType, newName, null);
    newNode.InnerXml = node.InnerXml;
    node.ParentNode!.ReplaceChild(newNode, node);
}

foreach (var dir in Directory.GetDirectories(Path.Join(args[1], "src/Smithbox.Data/Assets/PARAM")))
{
    var smithboxFile = Path.Join(dir, "Community Row Names.json");
    if (!File.Exists(smithboxFile))
    {
        Console.Error.WriteLine($"{smithboxFile} does not exist, not copying.");
    }
    else
    {
        syncParamNames(smithboxFile);
    }

    var defsDir = Path.Join(dir, "Defs");
    var tdfsDir = Path.Join(dir, "Tdfs");
    foreach (
        var file in (Directory.Exists(defsDir) ? Directory.GetFiles(defsDir, "*.xml") : []).Concat(
            (Directory.Exists(tdfsDir) ? Directory.GetFiles(tdfsDir, "*.tdf") : [])
        )
    )
    {
        string? paramType = null;
        XmlDocument? doc = null;
        if (file.EndsWith(".xml"))
        {
            doc = new XmlDocument() { PreserveWhitespace = true };
            doc.Load(file);
            if (doc.SelectSingleNode("//ParamType") is { } paramTypeNode)
            {
                paramType = paramTypeNode.InnerText;
            }

            if (doc.SelectSingleNode("//Unk06") is { } dataVersionNode)
            {
                changeNodeName(dataVersionNode, "DataVersion");
            }

            if (doc.SelectSingleNode("//Version") is { } formatVersionNode)
            {
                changeNodeName(formatVersionNode, "FormatVersion");
            }
        }

        var paramdexFile = Path.Join(
            args[2],
            Path.GetFileName(dir),
            Path.GetFileName(Path.GetDirectoryName(file)),
            Path.GetFileName(file)
        );
        if (
            File.Exists(paramdexFile)
            || (
                paramType != null
                && File.Exists(Path.Join(Path.GetDirectoryName(paramdexFile), $"{paramType}.xml"))
            )
        )
        {
            continue;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(paramdexFile)!);
        if (doc is not null)
        {
            File.WriteAllText(paramdexFile, doc.OuterXml);
        }
        else
        {
            File.Copy(file, paramdexFile);
        }
    }

    Console.WriteLine(dir);
}
