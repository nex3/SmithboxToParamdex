class Paramdex
{
    /// Reads a file in paramdex format, accounting for the fact that names may
    /// spill across multiple lines.
    public static IEnumerable<(int, string)> readFile(string path)
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
}
