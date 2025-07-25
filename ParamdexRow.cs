using System.Text.RegularExpressions;

/// A single row from a Paramdex parameter list.
struct ParamdexRow
{
    private static Regex jpNameRegex = new(
        @"(?:(^.*) -- )?(.*[\u3000-\u30ff\uff00-\uffef\u4e00-\u9faf])"
    );

    public int ID;
    public string Name;

    public (string?, string?) SplitJapaneseName()
    {
        var match = jpNameRegex.Match(Name);
        if (!match.Success)
        {
            return (Name, null);
        }

        return (match.Groups[1].Value, match.Groups[2].Value);
    }

    public override string ToString() => $"{ID} {Name}\n";
}
