internal sealed class ParamdexToSmithbox
{
    public static void Run(Data data, bool overwriteNames = false)
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
                    Queue<string> smithboxEntries = new(smithboxRow.Entries);
                    smithboxRow.Entries = [];

                    // Paramdex generally only includes rows with names while Smithbox includes
                    // unnamed rows as well, so if the rows mismatch we assume it's because Paramdex
                    // is just missing this row. This does mean that we can't handle cases where
                    // Paramdex adds new rows in the middle, but that really calls for a new sync
                    // between Smithbox and the game data itself which is out-of-scope for this
                    // tool.
                    while (
                        paramdexRows.TryPeek(out ParamdexRow paramdexRow)
                        && paramdexRow.ID == smithboxRow.ID
                    )
                    {
                        (string? paramdexEnName, _) = paramdexRow.SplitJapaneseName();
                        string paramdexName = paramdexEnName ?? paramdexRow.Name;

                        if (
                            smithboxEntries.TryDequeue(out string? smithboxName)
                            && smithboxName != ""
                            && (!overwriteNames || paramdexName == "")
                        )
                        {
                            smithboxRow.Entries.Add(smithboxName);
                        }
                        else
                        {
                            smithboxRow.Entries.Add(paramdexName);
                        }
                        _ = paramdexRows.Dequeue();
                    }

                    while (smithboxEntries.TryDequeue(out string? smithboxName))
                    {
                        smithboxRow.Entries.Add(smithboxName);
                    }
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
                        int id = paramdexRows.Peek().ID;
                        RowNameEntry smithboxRow = new() { ID = id, Entries = [] };
                        param.SmithboxRows.Add(smithboxRow);

                        while (
                            paramdexRows.TryPeek(out ParamdexRow paramdexRow)
                            && paramdexRow.ID == id
                        )
                        {
                            (string? paramdexEnName, _) = paramdexRow.SplitJapaneseName();
                            string paramdexName = paramdexEnName ?? paramdexRow.Name;
                            smithboxRow.Entries.Add(paramdexName);
                            _ = paramdexRows.Dequeue();
                        }
                    }
                }
            }

            game.WriteSmithbox();
            Console.WriteLine($"Converted {game.Name}");
        }
    }
}
