namespace Dashboard.Runtime;

/// <summary>One piece of a line: words, and where they lead when they are a link.</summary>
public sealed record RuntimeTextPart(string Text, RuntimeLink? Link = null);
