using System.Text.Json;

namespace Dashboard.Tests;

/// <summary>
/// The ObjectMother of the runtime payload's one repeated contract: the subscription's portal link, the same
/// address and the same one sentence of title the app writes once in <see cref="RuntimeBoxLink" /> - and the
/// payload-JSON entry points the runtime payload tests share: the payload of just these names, the payload of one
/// edge mark, and the one entry of the payload's list of names as the script reads it. The mothers of the repeated
/// default: a test that wants a link that is <em>not</em> this one (a reference-identity pin, a task in Octopus, a
/// resource group) keeps its own value; this is where the copies stop being word for word.
/// </summary>
public static class RuntimePayloadFixture
{
    /// <summary>The subscription's portal page: the address every box of a subscription leads along.</summary>
    public const string SubscriptionHref = "https://portal.example.net/#subscriptions/00000000-0000-0000-0000-000000000000";

    /// <summary>The words over the address: the one sentence the app writes for a subscription's portal link.</summary>
    public const string SubscriptionTitle = "The Azure subscription in the Azure portal (opens in a new tab; the Azure portal asks you to sign in)";

    /// <summary>The subscription's portal link: the address and the words above, as one <see cref="RuntimeLink" />.</summary>
    public static readonly RuntimeLink Subscription = new(SubscriptionHref, SubscriptionTitle);

    /// <summary>The payload of these boxes that are nothing but a name, as the script is handed it.</summary>
    public static string JsonOfNames(params RuntimeName[] names) => new RuntimePayload([], [], [], names).ToJson();

    /// <summary>The payload of one mark of a relationship, as the script is handed it.</summary>
    public static string JsonOfEdge(RuntimeEdgeMark edge) => new RuntimePayload([], [], [edge]).ToJson();

    /// <summary>The one entry of the payload's list of names for this name - the drawn name the script reads.</summary>
    public static JsonElement Name(RuntimeName name)
    {
        using var json = JsonDocument.Parse(JsonOfNames(name));
        return json.RootElement.GetProperty("names")[0].Clone();
    }
}
