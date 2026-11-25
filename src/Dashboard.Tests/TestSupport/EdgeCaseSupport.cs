namespace Dashboard.Tests;

/// <summary>
/// The helpers the edge-case test files of the records carried privately beside the shared surface walk of
/// <see cref="RecordSurface" /> and the shared culture scope of <see cref="CultureScope" />, gathered in one file
/// so a file keeps only the list and the words it pins and not its own copy of the machinery beside them.
/// <para>
/// The copies the files still carry are held to the answers gathered here - see
/// <see cref="EdgeCaseSupportTests" />, which walks the assembly's private <c>Own</c> and culture-harness copies and
/// holds each to the shared answer - so a copy may be retired file by file without changing a test's expectation.
/// </para>
/// </summary>
internal static class EdgeCaseSupport
{
    /// <summary>The letters of <paramref name="word" /> gathered into a string of their own, which no other word in
    /// the test shares a reference with: what a record does with a word it is given - keeping the one and not
    /// another that spells the same - is seen only when the two are apart. The empty word comes back as the empty
    /// string whatever this does, so a caller that pins the reference as well as the letters says the length too.
    /// A word that is not there at all is refused where it stands rather than answered with a throw of the
    /// reference.</summary>
    public static string Own(string word)
    {
        ArgumentNullException.ThrowIfNull(word);

        return new(word.ToCharArray());
    }
}
