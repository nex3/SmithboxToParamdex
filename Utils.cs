using System.Text.RegularExpressions;

class Utils
{
    private static Regex jpNameRegex = new(
        @"(?:(^.*) -- )?(.*[\u3000-\u30ff\uff00-\uffef\u4e00-\u9faf])"
    );

    public static (string?, string?) SplitJapaneseName(string name)
    {
        var match = jpNameRegex.Match(name);
        if (!match.Success)
        {
            return (name, null);
        }

        return (match.Groups[1].Value, match.Groups[2].Value);
    }
}
