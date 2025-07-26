using System.Xml;

internal sealed class SmithboxToParamdex
{
    private static void SyncParamNames(Game game, bool overwriteNames = false)
    {
        foreach (Param param in game.Params())
        {
            Queue<ParamdexRow> paramdexParams = new(param.ParamdexRows);
            Queue<ParamdexRow> outputParams = [];
            Queue<ParamdexRow> jpOutputParams = [];

            foreach (RowNameEntry entry in param.SmithboxRows)
            {
                ParamdexRow? paramdexRow =
                    paramdexParams.Count > 0 && paramdexParams.Peek().ID == entry.ID
                        ? paramdexParams.Dequeue()
                        : null;

                if (
                    (
                        paramdexRow?.ID != entry.ID
                        || overwriteNames
                        || paramdexRow is null
                        || paramdexRow?.Name == ""
                    )
                    && entry.Name != ""
                )
                {
                    outputParams.Enqueue(new() { ID = entry.ID, Name = entry.Name });

                    if (
                        paramdexRow?.SplitJapaneseName() is (_, string jpParamdexName)
                        && !entry.Name.Contains(jpParamdexName)
                    )
                    {
                        jpOutputParams.Enqueue(
                            new() { ID = (int)paramdexRow?.ID!, Name = jpParamdexName }
                        );
                    }
                }
                else if (paramdexRow is ParamdexRow existingRow)
                {
                    outputParams.Enqueue(existingRow);
                }
            }

            param.ParamdexRows = [.. outputParams, .. paramdexParams];
            param.ParamdexJapaneseRows = [.. jpOutputParams];
            param.WriteParamdex();
        }
    }

    private static void ChangeNodeName(XmlNode node, string newName)
    {
        XmlNode newNode = node.OwnerDocument!.CreateNode(node.NodeType, newName, null);
        newNode.InnerXml = node.InnerXml;
        _ = node.ParentNode!.ReplaceChild(newNode, node);
    }

    public static void Run(Data data, bool overwriteNames = false)
    {
        foreach (Game game in data.Games())
        {
            SyncParamNames(game, overwriteNames: overwriteNames);

            string defsDir = Path.Join(game.SmithboxPath, "Defs");
            string tdfsDir = Path.Join(game.SmithboxPath, "Tdfs");
            foreach (
                string? file in (
                    Directory.Exists(defsDir) ? Directory.GetFiles(defsDir, "*.xml") : []
                ).Concat(Directory.Exists(tdfsDir) ? Directory.GetFiles(tdfsDir, "*.tdf") : [])
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
                        ChangeNodeName(dataVersionNode, "DataVersion");
                    }

                    if (doc.SelectSingleNode("//Version") is { } formatVersionNode)
                    {
                        ChangeNodeName(formatVersionNode, "FormatVersion");
                    }
                }

                string paramdexFile = Path.Join(
                    game.ParamdexPath,
                    Path.GetFileName(Path.GetDirectoryName(file)),
                    Path.GetFileName(file)
                );
                if (
                    File.Exists(paramdexFile)
                    || (
                        paramType != null
                        && File.Exists(
                            Path.Join(Path.GetDirectoryName(paramdexFile), $"{paramType}.xml")
                        )
                    )
                )
                {
                    continue;
                }

                _ = Directory.CreateDirectory(Path.GetDirectoryName(paramdexFile)!);
                if (doc is not null)
                {
                    File.WriteAllText(paramdexFile, doc.OuterXml);
                }
                else
                {
                    File.Copy(file, paramdexFile);
                }
            }

            Console.WriteLine($"Converted {game.Name}");
        }
    }
}
