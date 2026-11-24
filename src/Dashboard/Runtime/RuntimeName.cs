namespace Dashboard.Runtime;

/// <summary>A box whose name is a link and that has nothing else to update: a frame, or the browser.</summary>
/// <param name="Alias">The frame's or the node's alias.</param>
/// <param name="NameLink">Where its name, and a click on the box, leads.</param>
public sealed record RuntimeName(string Alias, RuntimeLink NameLink);
