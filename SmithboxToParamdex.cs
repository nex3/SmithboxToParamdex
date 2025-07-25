using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Xml;

class SmithboxToParamdex
{
    private static void syncParamNames(Game game)
    {
        foreach (var param in game.Params())
        {
            Queue<ParamdexRow> paramdexParams = new(param.ParamdexRows);
            Queue<ParamdexRow> outputParams = [];
            Queue<ParamdexRow> jpOutputParams = [];

            foreach (var entry in param.SmithboxRows)
            {
                ParamdexRow? paramdexRow = paramdexParams.Count > 0 ? paramdexParams.Peek() : null;
                if (paramdexRow?.ID == entry.ID)
                {
                    paramdexParams.Dequeue();
                }

                if (entry.Name != "")
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
                else if (paramdexRow is ParamdexRow existingRow && existingRow.ID == entry.ID)
                {
                    outputParams.Enqueue(existingRow);
                }
            }

            param.ParamdexRows = new(outputParams);
            param.ParamdexJapaneseRows = new(jpOutputParams);
            param.WriteParamdex();
        }
    }

    private static void changeNodeName(XmlNode node, string newName)
    {
        var newNode = ((XmlDocument)node.OwnerDocument!).CreateNode(node.NodeType, newName, null);
        newNode.InnerXml = node.InnerXml;
        node.ParentNode!.ReplaceChild(newNode, node);
    }

    public static void Run(Data data)
    {
        foreach (var game in data.Games())
        {
            syncParamNames(game);

            var defsDir = Path.Join(game.SmithboxPath, "Defs");
            var tdfsDir = Path.Join(game.SmithboxPath, "Tdfs");
            foreach (
                var file in (
                    Directory.Exists(defsDir) ? Directory.GetFiles(defsDir, "*.xml") : []
                ).Concat((Directory.Exists(tdfsDir) ? Directory.GetFiles(tdfsDir, "*.tdf") : []))
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
                    game.ParamdexPath,
                    game.Name,
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

            Console.WriteLine($"Converted {game.Name}");
        }
    }
}
