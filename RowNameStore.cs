/// <summary>
/// Full information for row name stripping
/// </summary>
internal sealed class RowNameStore
{
    /// <summary>
    /// The list of params
    /// </summary>
    public required List<RowNameParam> Params { get; set; }
}

/// <summary>
/// Full information for row name stripping
/// </summary>
internal sealed class RowNameParam
{
    /// <summary>
    /// The name of the param
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The row name entries for this param
    /// </summary>
    public required List<RowNameEntry> Entries { get; set; }
}

/// <summary>
/// Full information for row name stripping
/// </summary>
internal sealed class RowNameEntry
{
    /// <summary>
    /// The index of the row
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// The row ID
    /// </summary>
    public int ID { get; set; }

    /// <summary>
    /// The row name
    /// </summary>
    public required string Name { get; set; }
}
