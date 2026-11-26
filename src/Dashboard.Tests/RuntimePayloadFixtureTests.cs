using System.Text.Json;

namespace Dashboard.Tests;

/// <summary>
/// The <see cref="RuntimePayloadFixture" /> object mother, pinned where it lives (#1093): the subscription's portal
/// link - the address and the one sentence of title the app writes once in <see cref="RuntimeBoxLink" /> - and the
/// payload-JSON entry points the runtime payload tests share: the payload of just these names, the payload of one
/// edge mark, and the one entry of the payload's list of names as the script reads it. The words of the link are
/// pinned cut for cut here, and the copies the test files keep of their own (a reference-identity pin, a task in
/// Octopus, a resource group) stay where they are: this is the repeated default, not every value.
/// </summary>
public class RuntimePayloadFixtureTests
{
    /// <summary>The one sentence the app writes for a subscription's portal link, and the address it stands over:
    /// both are the fixture's, word for word, and the link the fixture hands out is the two of them as one
    /// <see cref="RuntimeLink" />.</summary>
    [Fact]
    public void TheSubscriptionLinkIsTheAddressAndTheSentenceTheAppWritesOnce()
    {
        Assert.Equal("https://portal.example.net/#subscriptions/00000000-0000-0000-0000-000000000000", RuntimePayloadFixture.SubscriptionHref);
        Assert.Equal("The Azure subscription in the Azure portal (opens in a new tab; the Azure portal asks you to sign in)", RuntimePayloadFixture.SubscriptionTitle);

        Assert.Equal(RuntimePayloadFixture.SubscriptionHref, RuntimePayloadFixture.Subscription.Href);
        Assert.Equal(RuntimePayloadFixture.SubscriptionTitle, RuntimePayloadFixture.Subscription.Title);
        Assert.Equal(new RuntimeLink(RuntimePayloadFixture.SubscriptionHref, RuntimePayloadFixture.SubscriptionTitle), RuntimePayloadFixture.Subscription);
    }

    /// <summary>The payload of just these names is a payload with nothing but its names: the four lists stand in
    /// their order, the other three of them empty, and the names are the ones the caller handed, each written with
    /// its alias and the link of its name.</summary>
    [Fact]
    public void ThePayloadOfJustTheseNamesIsNothingButItsNames()
    {
        var name = new RuntimeName("sub", RuntimePayloadFixture.Subscription);

        using var json = JsonDocument.Parse(RuntimePayloadFixture.JsonOfNames(name));
        var drawn = json.RootElement;

        Assert.Equal(["nodes", "regions", "edges", "names"], drawn.EnumerateObject().Select(property => property.Name));
        Assert.Equal("[]", drawn.GetProperty("nodes").GetRawText());
        Assert.Equal("[]", drawn.GetProperty("regions").GetRawText());
        Assert.Equal("[]", drawn.GetProperty("edges").GetRawText());
        Assert.Equal(1, drawn.GetProperty("names").GetArrayLength());
        Assert.Equal("sub", drawn.GetProperty("names")[0].GetProperty("alias").GetString());
        Assert.Equal(RuntimePayloadFixture.SubscriptionHref, drawn.GetProperty("names")[0].GetProperty("nameLink").GetProperty("href").GetString());
        Assert.Equal(RuntimePayloadFixture.SubscriptionTitle, drawn.GetProperty("names")[0].GetProperty("nameLink").GetProperty("title").GetString());
    }

    /// <summary>The payload of no names is still the four lists, and the names of it are the empty list - the same
    /// payload the script is handed when the system has no box that is nothing but a name.</summary>
    [Fact]
    public void ThePayloadOfNoNamesIsTheFourListsWithNoneOfThem()
    {
        using var json = JsonDocument.Parse(RuntimePayloadFixture.JsonOfNames());

        Assert.Equal("""{"nodes":[],"regions":[],"edges":[],"names":[]}""", json.RootElement.GetRawText());
    }

    /// <summary>The payload of one edge mark is a payload with nothing but its one edge: the three other lists are
    /// empty, and the edge is the mark the caller handed, written with its parts in the order the record declares
    /// them.</summary>
    [Fact]
    public void ThePayloadOfOneEdgeMarkIsNothingButTheMark()
    {
        var edge = new RuntimeEdgeMark("app_ui-to-sqldb", "active", "12", "sql/min", null, "The web app to the database.");

        using var json = JsonDocument.Parse(RuntimePayloadFixture.JsonOfEdge(edge));
        var drawn = json.RootElement;

        Assert.Equal(["nodes", "regions", "edges"], drawn.EnumerateObject().Select(property => property.Name));
        Assert.Equal("[]", drawn.GetProperty("nodes").GetRawText());
        Assert.Equal("[]", drawn.GetProperty("regions").GetRawText());
        Assert.Equal(1, drawn.GetProperty("edges").GetArrayLength());

        var mark = drawn.GetProperty("edges")[0];

        Assert.Equal(["id", "state", "number", "unit", "title"], mark.EnumerateObject().Select(property => property.Name));
        Assert.Equal("app_ui-to-sqldb", mark.GetProperty("id").GetString());
        Assert.Equal("active", mark.GetProperty("state").GetString());
        Assert.Equal("12", mark.GetProperty("number").GetString());
        Assert.Equal("sql/min", mark.GetProperty("unit").GetString());
        Assert.Equal("The web app to the database.", mark.GetProperty("title").GetString());
    }

    /// <summary>The drawn name is the one entry of the payload's list of names: the alias, then the link of the
    /// name with its two words - the same entry the script reads, and the words of the fixture's own link when the
    /// name stands on it.</summary>
    [Fact]
    public void TheDrawnNameIsTheOneEntryOfTheListTheScriptReads()
    {
        var name = new RuntimeName("sub", RuntimePayloadFixture.Subscription);
        var drawn = RuntimePayloadFixture.Name(name);

        Assert.Equal(["alias", "nameLink"], drawn.EnumerateObject().Select(property => property.Name));
        Assert.Equal(["href", "title"], drawn.GetProperty("nameLink").EnumerateObject().Select(property => property.Name));
        Assert.Equal("sub", drawn.GetProperty("alias").GetString());
        Assert.Equal(RuntimePayloadFixture.SubscriptionHref, drawn.GetProperty("nameLink").GetProperty("href").GetString());
        Assert.Equal(RuntimePayloadFixture.SubscriptionTitle, drawn.GetProperty("nameLink").GetProperty("title").GetString());

        // The drawn name is a copy the test may hold: the payload it was cut from goes away and the entry stands.
        var words = drawn.GetRawText();
        Assert.Equal(
            """{"alias":"sub","nameLink":{"href":"https://portal.example.net/#subscriptions/00000000-0000-0000-0000-000000000000","title":"The Azure subscription in the Azure portal (opens in a new tab; the Azure portal asks you to sign in)"}}""",
            words);
    }

    /// <summary>The drawn name keeps the words of the name it was drawn from, whatever they are - the fixture's own
    /// link is the default the test brings, and another name of another link is drawn with that link's words: the
    /// entry points write the name the caller handed and no name of their own.</summary>
    [Fact]
    public void TheDrawnNameKeepsTheWordsOfTheNameItWasDrawnFrom()
    {
        var other = new RuntimeName("browser", new RuntimeLink("https://demo.example.net", "Browser: demo.example.net, as a browser opens it (opens in a new tab)"));
        var drawn = RuntimePayloadFixture.Name(other);

        Assert.Equal("browser", drawn.GetProperty("alias").GetString());
        Assert.Equal("https://demo.example.net", drawn.GetProperty("nameLink").GetProperty("href").GetString());
        Assert.Equal("Browser: demo.example.net, as a browser opens it (opens in a new tab)", drawn.GetProperty("nameLink").GetProperty("title").GetString());
    }
}
