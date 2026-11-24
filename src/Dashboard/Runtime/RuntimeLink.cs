namespace Dashboard.Runtime;

/// <summary>A link the script draws as a real <c>a</c> element: it opens a new tab.</summary>
/// <param name="Href">The address.</param>
/// <param name="Title">Where it goes, and that the destination asks for a sign-in.</param>
public sealed record RuntimeLink(string Href, string Title)
{
    public static RuntimeLink? To(Uri? address, string title) => address is null ? null : new RuntimeLink(address.AbsoluteUri, title);
}
