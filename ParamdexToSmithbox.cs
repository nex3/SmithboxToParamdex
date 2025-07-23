using System.Text.Json;

class ParamdexToSmithbox
{
    private static JsonSerializerOptions jsonOptions = new()
    {
        NewLine = "\n",
        WriteIndented = true,
        IndentSize = 2,
    };

    public static void Run(string paramdexPath, string smithboxPath)
    {
        foreach (var dir in Directory.GetDirectories(paramdexPath))
        {
            var namesDir = Path.Join(dir, "Names");
            if (!Directory.Exists(namesDir))
            {
                continue;
            }

            var gameCode = Path.GetFileName(dir);
            var smithboxFile = Path.Join(
                smithboxPath,
                "src/Smithbox.Data/Assets/PARAM",
                gameCode,
                "Community Row Names.json"
            );
            if (!File.Exists(smithboxFile))
            {
                Console.Error.WriteLine($"Smithbox doesn't track {gameCode} param names");
                continue;
            }

            var smithboxStore = JsonSerializer.Deserialize<RowNameStore>(
                File.ReadAllText(smithboxFile)
            )!;
            Dictionary<string, List<RowNameEntry>> smithboxByName =
                smithboxStore.Params.ToDictionary(param => param.Name, param => param.Entries);

            foreach (var paramdexFile in Directory.GetFiles(namesDir, "*.txt"))
            {
                var paramName = Path.GetFileName(paramdexFile)[0..^4];
                var paramdexParams = Paramdex.readFile(paramdexFile).ToList();
                if (paramdexParams.Count == 0)
                {
                    continue;
                }

                if (smithboxByName.TryGetValue(paramName, out var smithboxRows))
                {
                    var paramdexParamsById = paramdexParams.ToLookup((pair) => pair.Item1);
                    List<RowNameEntry> newSmithboxRows = [];
                    for (var i = 0; i < smithboxRows.Count; i++)
                    {
                        var id = smithboxRows[i].ID;

                        foreach (var (paramdexID, originalParamdexName) in paramdexParamsById[id])
                        {
                            var (paramdexEnName, paramdexJpName) = Utils.SplitJapaneseName(
                                originalParamdexName
                            );
                            var paramdexName = paramdexEnName ?? originalParamdexName;

                            var row = i < smithboxRows.Count ? smithboxRows[i] : null;
                            var nextIndex = Math.Max(
                                newSmithboxRows.Count == 0
                                    ? 0
                                    : newSmithboxRows[newSmithboxRows.Count - 1].Index + 1,
                                row?.Index ?? 0
                            );
                            if (row?.ID == id)
                            {
                                if (row.Name == "")
                                {
                                    row.Name = paramdexName;
                                }
                                row.Index = nextIndex;
                                newSmithboxRows.Add(row);
                                i++;
                            }
                            else
                            {
                                newSmithboxRows.Add(
                                    new()
                                    {
                                        Index = nextIndex,
                                        ID = id,
                                        Name = paramdexName,
                                    }
                                );
                            }
                        }

                        while (i < smithboxRows.Count && smithboxRows[i].ID == id)
                        {
                            var row = smithboxRows[i];
                            row.Index = Math.Max(
                                newSmithboxRows.Count == 0
                                    ? 0
                                    : newSmithboxRows[newSmithboxRows.Count - 1].Index + 1,
                                row.Index
                            );
                            newSmithboxRows.Add(row);
                            i++;
                        }
                    }
                }
                else
                {
                    smithboxStore.Params.Add(
                        new()
                        {
                            Name = paramName,
                            Entries = paramdexParams
                                .Select(
                                    (pair, index) =>
                                        new RowNameEntry()
                                        {
                                            Index = index,
                                            ID = pair.Item1,
                                            Name = pair.Item2,
                                        }
                                )
                                .ToList(),
                        }
                    );
                }
            }

            File.WriteAllText(smithboxFile, JsonSerializer.Serialize(smithboxStore, jsonOptions));
        }
    }
}
