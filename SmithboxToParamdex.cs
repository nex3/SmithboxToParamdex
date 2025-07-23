using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Xml;

class SmithboxToParamdex
{
    private static void syncParamNames(string paramdexPath, string smithboxFile)
    {
        var gameCode = Path.GetFileName(Path.GetDirectoryName(smithboxFile));
        var smithboxStore = JsonSerializer.Deserialize<RowNameStore>(
            File.ReadAllText(smithboxFile)
        )!;

        foreach (var smithboxParams in smithboxStore.Params)
        {
            var paramdexFile = Path.Join(
                paramdexPath,
                gameCode,
                "Names",
                smithboxParams.Name + ".txt"
            );
            Queue<(int, string)> paramdexParams = File.Exists(paramdexFile)
                ? new(Paramdex.readFile(paramdexFile))
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
                        if (
                            Utils.SplitJapaneseName(paramdexName) is (_, string jpParamdexName)
                            && !entry.Name.Contains(jpParamdexName)
                        )
                        {
                            jpOutputParams.Enqueue(((int)paramdexID!, jpParamdexName));
                        }
                    }
                }
                else if (
                    paramdexID is not null
                    && paramdexName is not null
                    && paramdexID == entry.ID
                )
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

    private static void changeNodeName(XmlNode node, string newName)
    {
        var newNode = ((XmlDocument)node.OwnerDocument!).CreateNode(node.NodeType, newName, null);
        newNode.InnerXml = node.InnerXml;
        node.ParentNode!.ReplaceChild(newNode, node);
    }

    public static void Run(string smithboxPath, string paramdexPath)
    {
        foreach (
            var dir in Directory.GetDirectories(
                Path.Join(smithboxPath, "src/Smithbox.Data/Assets/PARAM")
            )
        )
        {
            var smithboxFile = Path.Join(dir, "Community Row Names.json");
            if (!File.Exists(smithboxFile))
            {
                Console.Error.WriteLine($"{smithboxFile} does not exist, not copying.");
            }
            else
            {
                syncParamNames(paramdexPath, smithboxFile);
            }

            var defsDir = Path.Join(dir, "Defs");
            var tdfsDir = Path.Join(dir, "Tdfs");
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
                    paramdexPath,
                    Path.GetFileName(dir),
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

            Console.WriteLine(dir);
        }
    }
}
