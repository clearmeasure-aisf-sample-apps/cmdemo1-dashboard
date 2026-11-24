namespace Dashboard.Tests;

/// <summary>
/// The fixed words a record says about itself when every part of it is the nothing it was given: the
/// constants every edge-case file pins through interpolation instead of repeating. One change to a
/// record's print shape changes this file once, and the pins that name the constant see it everywhere.
/// </summary>
internal static class RecordWords
{
    /// <summary>The words of a <see cref="ResourceUse" /> whose four parts are each nothing: the cell's dash,
    /// the sum's absent field and the row's no measurement all stand on this string. The bytes are the pinned
    /// bytes that four edge-case files repeat; the constant here is the one they all name.</summary>
    public const string EmptyResourceUse = "ResourceUse { Usage = , Requests = , Limits = , Allocatable =  }";
}
