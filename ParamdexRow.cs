using System.Text.RegularExpressions;

/// A single row from a Paramdex parameter list.
internal partial struct ParamdexRow
{
    [GeneratedRegex(@"(?:(^.*) -- )?(.*[\u3000-\u30ff\uff00-\uffef\u4e00-\u9faf])")]
    private static partial Regex JapaneseNameRegex();

    public int ID;
    public string Name;

    public readonly (string?, string?) SplitJapaneseName()
    {
        Match match = JapaneseNameRegex().Match(Name);
        return !match.Success ? (Name, null) : (match.Groups[1].Value, match.Groups[2].Value);
    }

    public override readonly string ToString()
    {
        return $"{ID} {Name}\n";
    }
}
