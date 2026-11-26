namespace Dashboard.Health;

/// <summary>
/// One of the probes the page's Probe select offers: the <see cref="ProbeKind"/> the option names and the words the
/// viewer reads for it, declared together. The page's choices are the list of these (#1815), the way its interval
/// choices are a named list (#1796): an option's value and its label are one value, and the select's markup walks
/// the list instead of spelling the options by hand.
/// </summary>
/// <param name="Value">The probe the option selects.</param>
/// <param name="Label">The words the viewer reads for it.</param>
public sealed record ProbeChoice(ProbeKind Value, string Label);
