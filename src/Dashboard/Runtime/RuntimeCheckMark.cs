namespace Dashboard.Runtime;

/// <summary>One entry of a detailed health check as a small mark: its shape says the state, its title the rest.</summary>
/// <param name="State"><see cref="RuntimePayloadBuilder.Healthy"/> (a check), <see cref="RuntimeStateWords.Degraded"/> (a warning triangle), <see cref="RuntimeStateWords.Failed"/> (a cross) or <see cref="RuntimeStateWords.Unknown"/> (dots).</param>
/// <param name="Title">The entry's name, its state in words, its description and how long it took.</param>
public sealed record RuntimeCheckMark(string State, string Title);
