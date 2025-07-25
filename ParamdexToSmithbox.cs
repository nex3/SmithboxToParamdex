internal sealed class ParamdexToSmithbox
{
    public static void Run(Data data)
    {
        foreach (Game game in data.Games())
        {
            foreach (Param param in game.Params())
            {
                if (param.ParamdexRows.Count == 0)
                {
                    continue;
                }

                Queue<RowNameEntry> smithboxRows = new(param.SmithboxRows);
                Queue<ParamdexRow> paramdexRows = new(param.ParamdexRows);
                while (smithboxRows.Count > 0 && paramdexRows.Count > 0)
                {
                    RowNameEntry smithboxRow = smithboxRows.Dequeue();
                    ParamdexRow paramdexRow = paramdexRows.Peek();

                    // Paramdex generally only includes rows with names while Smithbox includes
                    // unnamed rows as well, so if the rows mismatch we assume it's because Paramdex
                    // is just missing this row. This does mean that we can't handle cases where
                    // Paramdex adds new rows in the middle, but that really calls for a new sync
                    // between Smithbox and the game data itself which is out-of-scope for this
                    // tool.
                    if (paramdexRow.ID != smithboxRow.ID)
                    {
                        continue;
                    }

                    (string? paramdexEnName, string? _) = paramdexRow.SplitJapaneseName();
                    string paramdexName = paramdexEnName ?? paramdexRow.Name;

                    if (smithboxRow.Name == "")
                    {
                        smithboxRow.Name = paramdexName;
                    }
                    _ = paramdexRows.Dequeue();
                }

                if (
                    paramdexRows.Count > 0
                    && (
                        param.SmithboxRows.Count == 0
                        || paramdexRows.Peek().ID > param.SmithboxRows[^1].ID
                    )
                )
                {
                    while (paramdexRows.Count > 0)
                    {
                        ParamdexRow paramdexRow = paramdexRows.Dequeue();
                        param.SmithboxRows.Add(
                            new()
                            {
                                ID = paramdexRow.ID,
                                Name = paramdexRow.Name,
                                Index = param.SmithboxRows.Count,
                            }
                        );
                    }
                }
            }

            game.WriteSmithbox();
            Console.WriteLine($"Converted {game.Name}");
        }
    }
}
