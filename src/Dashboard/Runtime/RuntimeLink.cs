namespace Dashboard.Runtime;

/// <summary>A link the script draws as a real <c>a</c> element: it opens a new tab.</summary>
/// <param name="Href">The address.</param>
/// <param name="Title">Where it goes, and that the destination asks for a sign-in.</param>
public sealed record RuntimeLink(string Href, string Title)
{
    /// <summary>
    /// The link that leads to <paramref name="address"/> and carries <paramref name="title"/>; no address at all is no
    /// link, whichever words it was given.
    /// <para>
    /// An address that is relative to a page is refused by the address itself, naming no argument - not <c>href</c>,
    /// not <paramref name="address"/> - and that is the rule for every place a <em>link</em> is built (#1556):
    /// <see cref="Uri.AbsoluteUri"/> answers with its own <see cref="InvalidOperationException"/>, <c>This operation
    /// is not supported for a relative URI.</c> A link is the address the deployment or the manifest wrote and nothing
    /// else - this record holds no <see cref="UriKind"/> of its own and no page of its own to resolve against, so it
    /// has no better word for the address than the address has, and inventing a host nobody named is what a link on
    /// this page must never do. The other half of the decision is a <em>probe</em>: there the caller owns the base it
    /// requests at, which is why #1544 would have <see cref="Health.ProbeUrl.Combine"/> name <c>baseAddress</c>.
    /// <see cref="Health.LinkText.Release"/> says this very sentence for the release page, deliberately the same one.
    /// </para>
    /// </summary>
    /// <param name="address">Where the link leads, as the manifest or the topology wrote it; null when there is
    /// none, and a <em>relative</em> one is refused by <see cref="Uri"/> itself rather than by a guess here.</param>
    /// <param name="title">The words that say where it goes; taken as they come, and no words at all is a title the
    /// record holds.</param>
    public static RuntimeLink? To(Uri? address, string title) => address is null ? null : new RuntimeLink(address.AbsoluteUri, title);
}
