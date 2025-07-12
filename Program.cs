using System.Collections.Generic;
using System.IO;
using System.Text.Json;

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
        }
    }
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
        var file in (Directory.Exists(defsDir) ? Directory.GetFiles(defsDir) : []).Concat(
            (Directory.Exists(tdfsDir) ? Directory.GetFiles(tdfsDir) : [])
        )
    )
    {
        var paramdexFile = Path.Join(
            args[2],
            Path.GetFileName(dir),
            Path.GetFileName(Path.GetDirectoryName(file)),
            Path.GetFileName(file)
        );
        if (!File.Exists(paramdexFile))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(paramdexFile)!);
            File.Copy(file, paramdexFile, overwrite: true);
        }
    }

    Console.WriteLine(dir);
}
