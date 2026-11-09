namespace Dashboard.Work;

/// <summary>
/// The board the work items move across, in the order a reader expects. A column the file names differently is still
/// shown: it is not dropped, and it is not treated as done.
/// </summary>
public static class WorkColumns
{
    public const string Todo = "Todo";
    public const string InProgress = "In Progress";
    public const string TechnicalDesign = "Technical Design";
    public const string Development = "Development";
    public const string FunctionalTesting = "Functional Testing";
    public const string ReleaseQueue = "Release Queue";
    public const string Done = "Done";

    /// <summary>A column the file left blank: the item is shown, under this name, with the active ones.</summary>
    public const string Unplaced = "Unplaced";

    /// <summary>Every column, left to right, Done last.</summary>
    public static readonly IReadOnlyList<string> Order =
    [
        Todo,
        InProgress,
        TechnicalDesign,
        Development,
        FunctionalTesting,
        ReleaseQueue,
        Done,
    ];

    /// <summary>The columns of work that is not done, in board order.</summary>
    public static readonly IReadOnlyList<string> Active = [.. Order.Where(column => !IsDone(column))];

    public static bool Same(string? left, string right) =>
        string.Equals(left, right, StringComparison.OrdinalIgnoreCase);

    public static bool IsDone(string? column) => Same(column, Done);

    public static bool IsKnown(string? column) => column is not null && Order.Any(name => Same(column, name));

    /// <summary>The column's own spelling when it is one of <see cref="Order"/>; the file's spelling otherwise.</summary>
    public static string Canonical(string? column) =>
        Order.FirstOrDefault(name => Same(column, name)) ?? (string.IsNullOrWhiteSpace(column) ? Unplaced : column.Trim());
}
