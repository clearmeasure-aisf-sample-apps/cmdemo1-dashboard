namespace Dashboard.Runtime;

/// <summary>One entry of a detailed health check as a small mark: its shape says the state, its title the rest.</summary>
/// <param name="State"><c>healthy</c> (a check), <c>degraded</c> (a warning triangle), <c>failed</c> (a cross) or <c>unknown</c> (dots).</param>
/// <param name="Title">The entry's name, its state in words, its description and how long it took.</param>
public sealed record RuntimeCheckMark(string State, string Title);
