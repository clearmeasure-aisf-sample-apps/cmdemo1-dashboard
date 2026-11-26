using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="DashboardEvent" />, the one record every detector of
/// this page writes what it observed into - the moment, the kind of thing, how the page weighs it, the environment,
/// the node and the words - declared in <c>src/Dashboard/Health/DashboardEvents.cs</c>. The record holds its six
/// parts and nothing else: it counts nothing, decides nothing and says nothing of its own, since what each part is
/// handed is settled where the events are made.
/// <para>
/// What is already pinned elsewhere is not pinned again here. <see cref="EventKindEdgeCaseTests" /> and
/// <see cref="EventLevelEdgeCaseTests" /> pin the two vocabularies an event names its kind and its level with, and
/// each ends by holding an event to the kind and to the level it is given - a number outside its vocabulary included;
/// <see cref="EventLogEdgeCaseTests" /> pins that a log keeps the event it is handed word for word, all six parts,
/// and compares events by the moment they carry; and <see cref="EventDetectorTests" />,
/// <see cref="EventDetectorEdgeCaseTests" />, <see cref="ClusterEventDetectorTests" />,
/// <see cref="ClusterEventDetectorEdgeCaseTests" />, <see cref="RotationTests" /> and
/// <see cref="KustomizationPinMonitorTests" /> pin which moment, kind, level, environment, node and words each
/// detector puts in.
/// </para>
/// <para>
/// Left open by all of that, and pinned here: the constructor's six parts by the names of their own properties, each
/// held as the very instance it was given and none of them refused - the environment and the node of an event of the
/// page itself, the node of a traffic event and the words a caller hands over as nothing at all, the empty and the
/// padded and the ten-thousand-letter word kept as written; every kind and every level as the part it is on an event;
/// the moment at the ends of its number, to the tick, with the hours it was written in rather than the instant those
/// hours stand at; two events being one while their six parts are, and another in any one of them, the hash following
/// and the parts standing apart in a set; events that are not there; the copy a <c>with</c> makes and the one part it
/// turns; the six parts read out in the order they stand in; the line the record says about itself, part by part and
/// in the hours its reader speaks; and the record's whole public surface, so that the day a seventh part is added the
/// list below says so.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: an event holds six parts, calls nothing
/// at all, and every moment below is one the test brought.
/// </para>
/// </summary>
public class DashboardEventEdgeCaseTests
{
    /// <summary>The moment the page observed the thing, on the meridian: every event below stands on it unless it
    /// says another.</summary>
    private static readonly DateTimeOffset At = new(2026, 11, 21, 9, 0, 0, TimeSpan.Zero);

    /// <summary>The very instant of <see cref="At" />, five and a half hours east of the meridian.</summary>
    private static readonly DateTimeOffset AtInOtherHours = new(2026, 11, 21, 14, 30, 0, TimeSpan.FromMinutes(330));

    /// <summary>An event of the six parts it is told; the parts left out stand as they stand on any event of the
    /// page.</summary>
    private static DashboardEvent Event(
        DateTimeOffset? at = null,
        EventKind kind = EventKind.Health,
        EventLevel level = EventLevel.Info,
        string? environment = "prod",
        string? node = "westus3",
        string? text = "ui is healthy") =>
        new(at ?? At, kind, level, environment, node, text!);

    /// <summary>The event of nothing at all: the first moment there is, the first kind and the first level, no
    /// environment, no node and a word of no letters - every part said, none of them said of anything.</summary>
    private static DashboardEvent Nothing() => new(default, default, default, null, null, string.Empty);

    // ------------------------------------------- the six parts, by the names of their own properties

    /// <summary>
    /// The constructor's six parts, position by position, come back as the property of the same name: the moment
    /// first, then the kind, the level, the environment, the node and the words last - the order the record declares
    /// them in. Each part named on its own lands in its own place and leaves the other five where they were put, and
    /// every one of them is held and not copied: the very string instance the page wrote the environment, the node
    /// and the words in is the instance the event carries.
    /// </summary>
    [Fact]
    public void TheConstructorHandsEachPartToThePropertyOfItsName()
    {
        var environment = new string("prod".ToCharArray());
        var node = new string("Front Door".ToCharArray());
        var text = new string("ui: 2.4.14 → 2.4.15, deployed".ToCharArray());
        var entry = new DashboardEvent(At, EventKind.Version, EventLevel.Good, environment, node, text);

        Assert.Equal(At, entry.At);
        Assert.Equal(EventKind.Version, entry.Kind);
        Assert.Equal(EventLevel.Good, entry.Level);
        Assert.Equal("prod", entry.Environment);
        Assert.Equal("Front Door", entry.Node);
        Assert.Equal("ui: 2.4.14 → 2.4.15, deployed", entry.Text);
        Assert.Same(environment, entry.Environment);
        Assert.Same(node, entry.Node);
        Assert.Same(text, entry.Text);

        // Each part named for itself, and the rest standing as the event was written without it.
        Assert.Equal(AtInOtherHours, Event(at: AtInOtherHours).At);
        Assert.Equal(EventKind.Cluster, Event(kind: EventKind.Cluster).Kind);
        Assert.Equal(EventLevel.Problem, Event(level: EventLevel.Problem).Level);
        Assert.Equal("uat", Event(environment: "uat").Environment);
        Assert.Equal("cluster", Event(node: "cluster").Node);
        Assert.Equal("Traffic started", Event(text: "Traffic started").Text);
        Assert.Equal(EventKind.Restart, Event(kind: EventKind.Restart, text: "ui restarted, up 3 s").Kind);
        Assert.Equal("ui restarted, up 3 s", Event(kind: EventKind.Restart, text: "ui restarted, up 3 s").Text);
    }

    /// <summary>
    /// The parts an event has none of are kept as none rather than refused: no environment, which is an event of the
    /// page itself, no node, which is how <c>EventDetector.Traffic</c> writes the traffic button, and no words at all
    /// - the record checks none of its six parts, so even the words, which the record promises are there, come back as
    /// the nothing a caller handed over. The event of nothing at all is still an event, and another one from a full
    /// event, and no part of it is missing.
    /// </summary>
    [Fact]
    public void ThePartsAnEventHasNoneOfAreKeptAsNoneAndNothingIsRefused()
    {
        var ofThePage = new DashboardEvent(At, EventKind.Traffic, EventLevel.Info, null, null, "Traffic started");

        Assert.Null(ofThePage.Environment);
        Assert.Null(ofThePage.Node);
        Assert.Equal("Traffic started", ofThePage.Text);

        var bare = Nothing();

        Assert.Equal(DateTimeOffset.MinValue, bare.At);
        Assert.Equal(EventKind.Health, bare.Kind);
        Assert.Equal(EventLevel.Info, bare.Level);
        Assert.Null(bare.Environment);
        Assert.Null(bare.Node);
        Assert.Equal(string.Empty, bare.Text);
        Assert.Equal(Nothing(), bare);
        Assert.NotEqual(Event(), ofThePage);
        Assert.NotEqual(Event(), bare);

        // The record is no gatekeeper: nothing it is told to hold is turned away, by name or otherwise.
        Assert.Null(Event(text: null!).Text);
        Assert.Null(Event(environment: null, node: null, text: null!).Node);
        Assert.Equal(Event(), Event(environment: "prod", node: "westus3", text: "ui is healthy"));
    }

    /// <summary>
    /// The words an event carries are kept as they were written: the empty word is no word but is still the word the
    /// event stands on, a space is a space, the whitespace around a name is left where it stands, the arrow a
    /// detector writes and the letters with accents on them are what they are whatever culture reads the page, and a
    /// word of ten thousand letters is the same instance it came in as - the record measures nothing, trims nothing
    /// and shortens nothing.
    /// </summary>
    [Fact]
    public void TheWordsAnEventCarriesAreKeptAsTheyWereWritten()
    {
        Assert.Equal(string.Empty, Event(text: string.Empty).Text);
        Assert.Equal(" ", Event(text: " ").Text);
        Assert.Equal("  ", Event(environment: "  ", node: " ", text: "  ").Environment);
        Assert.Equal("→", Event(text: "→").Text);
        Assert.Equal("ü.é", Event(text: "ü.é").Text);
        Assert.Equal(" westus3 ", Event(node: " westus3 ").Node);
        Assert.Equal(
            "ui: LlmGateway Healthy → Degraded. Chat client answered slowly",
            Event(text: "ui: LlmGateway Healthy → Degraded. Chat client answered slowly").Text);

        var longText = new string('x', 10_000);

        Assert.Same(longText, Event(text: longText).Text);
        Assert.Equal(10_000, Event(text: longText).Text.Length);
    }

    /// <summary>
    /// Every kind of the eight and every level of the four is a place an event can stand: the first of the vocabulary,
    /// the last and each one between them, each event holding its own kind and its own level and standing apart from
    /// the rest of them - in a set, in <c>Distinct</c> and in the pair of the two vocabularies, which is thirty-two
    /// events and not eight. A kind or a level an event's equality walked past would be seen here.
    /// </summary>
    [Fact]
    public void EveryKindAndEveryLevelIsAPlaceAnEventStandsOnApart()
    {
        var kinds = Enum.GetValues<EventKind>();
        var levels = Enum.GetValues<EventLevel>();

        var eventsOfTheKinds = kinds.Select(kind => Event(kind: kind)).ToList();

        Assert.Equal(8, kinds.Length);
        Assert.Equal(kinds, eventsOfTheKinds.Select(entry => entry.Kind));
        Assert.Equal(kinds.Length, eventsOfTheKinds.Distinct().Count());
        Assert.Equal(kinds.Length, new HashSet<DashboardEvent>(eventsOfTheKinds).Count);
        Assert.All(kinds, kind => Assert.Equal(kind, Event(kind: kind).Kind));

        var eventsOfTheLevels = levels.Select(level => Event(level: level)).ToList();

        Assert.Equal(4, levels.Length);
        Assert.Equal(levels, eventsOfTheLevels.Select(entry => entry.Level));
        Assert.Equal(levels.Length, eventsOfTheLevels.Distinct().Count());
        Assert.Equal(levels.Length, new HashSet<DashboardEvent>(eventsOfTheLevels).Count);
        Assert.All(levels, level => Assert.Equal(level, Event(level: level).Level));

        // Kind and level are two places and not one: the same kind on another level is another event.
        Assert.Equal(
            32,
            kinds.SelectMany(kind => levels.Select(level => Event(kind: kind, level: level))).Distinct().Count());
    }

    // ------------------------------------------ the moment, at the ends of its number

    /// <summary>
    /// The moment an event stands on is held to the tick, at the ends of its number: the first moment there is, the
    /// last, and a tick either side of the moment the page observed - each of them another event from the moment
    /// itself and from one another, since a log of two events of one tick is two events. A second, a minute and a day
    /// on are each their own moment and each their own event; the record keeps the moment it is given and moves
    /// nothing.
    /// </summary>
    [Fact]
    public void TheMomentAnEventStandsOnIsHeldToTheTickAtTheEndsOfItsNumber()
    {
        Assert.Equal(DateTimeOffset.MinValue, Event(at: DateTimeOffset.MinValue).At);
        Assert.Equal(DateTimeOffset.MinValue, Nothing().At);
        Assert.Equal(DateTimeOffset.MaxValue, Event(at: DateTimeOffset.MaxValue).At);

        var tickBefore = At.AddTicks(-1);
        var tickAfter = At.AddTicks(1);

        Assert.Equal(tickBefore, Event(at: tickBefore).At);
        Assert.Equal(tickAfter, Event(at: tickAfter).At);
        Assert.NotEqual(Event(), Event(at: tickAfter));
        Assert.NotEqual(Event(), Event(at: tickBefore));
        Assert.NotEqual(Event(at: tickAfter), Event(at: tickBefore));
        Assert.Equal(3, new[] { tickBefore, At, tickAfter }.Select(at => Event(at: at)).Distinct().Count());

        // A second, a minute and a day on are each another event, and the moment keeps its own distance from them.
        Assert.Equal(
            4,
            new[] { At, At.AddSeconds(1), At.AddMinutes(1), At.AddDays(1) }.Select(at => Event(at: at)).Distinct().Count());
        Assert.Equal(At.AddYears(1), Event(at: At.AddYears(1)).At);
        Assert.NotEqual(Event(at: At.AddYears(1)), Event());
    }

    /// <summary>
    /// The moment is kept with the hours it was written in and not read back as the instant those hours stand at: the
    /// one event written on the meridian and five and a half hours east of it is one event - the operator,
    /// <c>Equals</c> boxed, and the hash that goes with it - while the event's own moment still says <c>+05:30</c> and
    /// the other still says <c>+00:00</c>, and while the instant one second later in those other hours is another
    /// event.
    /// </summary>
    [Fact]
    public void TheSameInstantInOtherHoursIsTheSameEventWrittenInOtherHours()
    {
        var onTheMeridian = Event();
        var inOtherHours = Event(at: AtInOtherHours);

        Assert.True(onTheMeridian == inOtherHours);
        Assert.False(onTheMeridian != inOtherHours);
        Assert.Equal(onTheMeridian, inOtherHours);
        Assert.True(onTheMeridian.Equals((object)inOtherHours));
        Assert.Equal(onTheMeridian.GetHashCode(), inOtherHours.GetHashCode());
        Assert.Single(new HashSet<DashboardEvent> { onTheMeridian, inOtherHours });

        // The hours the moment was written in are kept, even though the instant decides.
        Assert.Equal(TimeSpan.Zero, onTheMeridian.At.Offset);
        Assert.Equal(TimeSpan.FromMinutes(330), inOtherHours.At.Offset);
        Assert.Equal(onTheMeridian.At.UtcDateTime, inOtherHours.At.UtcDateTime);

        // The offset is no part of its own: the same instant in yet other hours is the same event again.
        Assert.Equal(onTheMeridian, Event(at: At.ToOffset(TimeSpan.FromHours(-12))));
        Assert.NotEqual(onTheMeridian, Event(at: AtInOtherHours.AddSeconds(1)));
    }

    // ------------------------------------------------------------------------- the event itself

    /// <summary>
    /// Two events are one while their six parts are - the moment as the instant it is, the kind and the level as the
    /// choices they are, the three words as the words they were written in - the equality of the operator, of
    /// <c>Equals</c> unboxed and boxed, and the hash that follows them and stands from call to call, so an event is
    /// usable as the key of a row. Nothing that is no event is an event's equal.
    /// </summary>
    [Fact]
    public void TwoEventsAreOneWhileTheirSixPartsAre()
    {
        var one = Event();
        var two = new DashboardEvent(At, EventKind.Health, EventLevel.Info, "prod", "westus3", "ui is healthy");

        Assert.Equal(one, two);
        Assert.True(one == two);
        Assert.True(two == one);
        Assert.False(one != two);
        Assert.True(one.Equals(two));
        Assert.True(two.Equals(one));
        Assert.True(one.Equals((object)two));
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.Equal(one.GetHashCode(), one.GetHashCode());
        Assert.Single(new HashSet<DashboardEvent> { one, two });

        Assert.False(one.Equals(null));
        Assert.False(one.Equals("ui is healthy"));
        Assert.False(one.Equals(At));
        Assert.False(one.Equals(EventKind.Health));
        Assert.False(one.Equals(EventLevel.Info));
        Assert.False(one.Equals(new HealthWindow(24, 23)));
        Assert.False(one.Equals(Nothing()));
    }

    /// <summary>
    /// An event is another in any one of its six parts: another moment by a tick, another kind, another level,
    /// another environment, another node, another word - and as much another when the part is turned to none as when
    /// it is turned to another. The difference stands whichever way the question is asked, and the six variants stand
    /// apart in a set, so no part of an event is compared by less than itself.
    /// </summary>
    [Fact]
    public void ADifferenceInAnyPartIsAnotherEvent()
    {
        var entry = Event();

        Assert.NotEqual(entry, entry with { At = At.AddTicks(1) });
        Assert.NotEqual(entry, entry with { Kind = EventKind.Cluster });
        Assert.NotEqual(entry, entry with { Level = EventLevel.Problem });
        Assert.NotEqual(entry, entry with { Environment = "uat" });
        Assert.NotEqual(entry, entry with { Node = "Front Door" });
        Assert.NotEqual(entry, entry with { Text = "ui is degraded" });

        Assert.NotEqual(entry, entry with { Environment = null });
        Assert.NotEqual(entry, entry with { Node = null });
        Assert.NotEqual(entry, entry with { Text = string.Empty });
        Assert.True(entry != entry with { Kind = EventKind.Cluster });
        Assert.False((entry with { Level = EventLevel.Problem }).Equals(entry));

        DashboardEvent[] apart = [
            entry with { At = At.AddTicks(1) },
            entry with { Kind = EventKind.Cluster },
            entry with { Level = EventLevel.Problem },
            entry with { Environment = null },
            entry with { Node = null },
            entry with { Text = string.Empty },
        ];

        Assert.Equal(6, apart.Distinct().Count());
        Assert.Equal(6, new HashSet<DashboardEvent>(apart).Count);
    }

    /// <summary>
    /// The words of an event are compared as they were written: a name in other letters is another event, since no
    /// detector folds the case of a node or trims what a health check answered - the lookup that does either is the
    /// file's question and is pinned where the file is read - and the event with no words and the event with a word of
    /// no letters are two events, with two hashes, that say one and the same line about themselves.
    /// </summary>
    [Fact]
    public void TheWordsOfAnEventAreComparedAsTheyWereWritten()
    {
        var entry = Event(environment: "prod", node: "westus3", text: "ui is healthy");

        Assert.NotEqual(entry, Event(environment: "PROD", node: "westus3", text: "ui is healthy"));
        Assert.NotEqual(entry, Event(environment: "prod", node: "westus", text: "ui is healthy"));
        Assert.NotEqual(entry, Event(environment: "prod", node: "westus3", text: "ui is healthy "));
        Assert.NotEqual(entry, Event(environment: "prod", node: "westus3", text: "Ui is healthy"));
        Assert.NotEqual(entry, Event(environment: " prod", node: "westus3", text: "ui is healthy"));

        var withoutWords = Event(text: null!);
        var withEmptyWords = Event(text: string.Empty);

        Assert.NotEqual(withoutWords, withEmptyWords);
        Assert.NotEqual(withoutWords.GetHashCode(), withEmptyWords.GetHashCode());
        Assert.Equal(withoutWords.ToString(), withEmptyWords.ToString());
    }

    /// <summary>
    /// Events that are not there are no events: the record's default is no event, and no event is never equal to an
    /// event however the question is asked - the operators and <c>Equals</c> alike - while two of them that are not
    /// there are one another, and the event of nothing at all is an event that a caller can hold.
    /// </summary>
    [Fact]
    public void EventsThatAreNotThereAreNoEvents()
    {
        Assert.Null(default(DashboardEvent));

        DashboardEvent? none = null;
        DashboardEvent? alsoNone = null;
        var there = Event();

        Assert.True(none == alsoNone);
        Assert.False(none != alsoNone);
        Assert.False(none == there);
        Assert.True(none != there);
        Assert.False(there.Equals(none));
        Assert.False(there == none);
        Assert.True(there is not null);
        Assert.True(Event(text: null!) is not null);

        // The event of nothing at all is one event, held in a set beside the event of a moment and three words.
        Assert.Equal(2, new HashSet<DashboardEvent> { there, Nothing() }.Count);
    }

    // ------------------------------------------------ the copy a with makes, and the part it turns

    /// <summary>
    /// A copy of an event is the event again: the <c>with</c> that names no part copies it whole - the very string
    /// instances it was given, in another object - and the <c>with</c> that names one part keeps the other five
    /// exactly as they stood, the original left where it stood. A part turned to no part keeps the rest and is another
    /// event, and a copy that names all six parts where they already stand is the event it started from.
    /// </summary>
    [Fact]
    public void ACopyTurnsThePartItNamesAndKeepsTheOthers()
    {
        var text = new string("ui is healthy".ToCharArray());
        var entry = Event(text: text);

        var copy = entry with { };

        Assert.Equal(entry, copy);
        Assert.NotSame(entry, copy);
        Assert.Same(text, copy.Text);
        Assert.Equal(entry.GetHashCode(), copy.GetHashCode());

        var later = entry with { At = At.AddSeconds(1) };

        Assert.Equal(At.AddSeconds(1), later.At);
        Assert.Equal(EventKind.Health, later.Kind);
        Assert.Equal(EventLevel.Info, later.Level);
        Assert.Equal("prod", later.Environment);
        Assert.Equal("westus3", later.Node);
        Assert.Same(text, later.Text);
        Assert.NotEqual(entry, later);

        var degraded = entry with { Level = EventLevel.Warning, Text = "ui is degraded" };

        Assert.Equal(EventLevel.Warning, degraded.Level);
        Assert.Equal("ui is degraded", degraded.Text);
        Assert.Equal(EventKind.Health, degraded.Kind);
        Assert.Equal("prod", degraded.Environment);
        Assert.Equal(At, degraded.At);

        // The original stands where it stood, part for part.
        Assert.Equal(At, entry.At);
        Assert.Equal(EventKind.Health, entry.Kind);
        Assert.Equal(EventLevel.Info, entry.Level);
        Assert.Equal("prod", entry.Environment);
        Assert.Equal("westus3", entry.Node);
        Assert.Same(text, entry.Text);

        var repack = entry with
        {
            At = entry.At,
            Kind = entry.Kind,
            Level = entry.Level,
            Environment = entry.Environment,
            Node = entry.Node,
            Text = entry.Text,
        };

        Assert.Equal(entry, repack);
        Assert.NotSame(entry, repack);

        // A part turned to no part keeps the other five, and is another event.
        var noEnvironment = entry with { Environment = null };

        Assert.Null(noEnvironment.Environment);
        Assert.Equal(At, noEnvironment.At);
        Assert.Equal(EventKind.Health, noEnvironment.Kind);
        Assert.Equal(EventLevel.Info, noEnvironment.Level);
        Assert.Equal("westus3", noEnvironment.Node);
        Assert.Same(text, noEnvironment.Text);
        Assert.NotEqual(entry, noEnvironment);
    }

    // -------------------------------------------------------------- the six parts read out

    /// <summary>
    /// The six parts read out of an event in the order they stand in it - the moment, the kind, the level, the
    /// environment, the node and the words - the very instances the record was given, and the event of nothing at all
    /// falling apart as it was told: the first moment there is, the first kind, the first level, no environment, no
    /// node and a word of no letters. What came out goes back in the same way, and asked part by part the same six
    /// parts come out in the same order.
    /// </summary>
    [Fact]
    public void TheSixPartsReadOutOfTheEvent()
    {
        var environment = new string("prod".ToCharArray());
        var node = new string("westus3".ToCharArray());
        var text = new string("ui is healthy".ToCharArray());

        var (at, kind, level, read, where, words) = new DashboardEvent(At, EventKind.Health, EventLevel.Info, environment, node, text);

        Assert.Equal(At, at);
        Assert.Equal(EventKind.Health, kind);
        Assert.Equal(EventLevel.Info, level);
        Assert.Same(environment, read);
        Assert.Same(node, where);
        Assert.Same(text, words);
        Assert.Equal(new DashboardEvent(At, EventKind.Health, EventLevel.Info, environment, node, text), new DashboardEvent(at, kind, level, read, where, words));

        var (noAt, noKind, noLevel, noEnvironment, noNode, noText) = Nothing();

        Assert.Equal(DateTimeOffset.MinValue, noAt);
        Assert.Equal(EventKind.Health, noKind);
        Assert.Equal(EventLevel.Info, noLevel);
        Assert.Null(noEnvironment);
        Assert.Null(noNode);
        Assert.Equal(string.Empty, noText);

        var entry = Event(at: AtInOtherHours, kind: EventKind.Serving, level: EventLevel.Good, environment: null, node: "ui", text: "westus3 serves traffic again.");
        entry.Deconstruct(out var askedAt, out var askedKind, out var askedLevel, out var askedEnvironment, out var askedNode, out var askedText);

        Assert.Equal(AtInOtherHours, askedAt);
        Assert.Equal(EventKind.Serving, askedKind);
        Assert.Equal(EventLevel.Good, askedLevel);
        Assert.Null(askedEnvironment);
        Assert.Equal("ui", askedNode);
        Assert.Equal("westus3 serves traffic again.", askedText);
        Assert.Equal(entry, new DashboardEvent(askedAt, askedKind, askedLevel, askedEnvironment, askedNode, askedText));
    }

    // ------------------------------------------------------ the line the event says about itself

    /// <summary>
    /// The event says its six parts in their order, the name of each with the value it stands on: the moment in the
    /// hours it was written in, the kind and the level by their declared words, and the three parts of text as they
    /// were written. The parts an event has none of say their name and nothing more - the empty word its emptiness,
    /// the whitespace its space, and no word at all the same line as a word of no letters - and the first and the last
    /// moment there are say themselves. (The words of a moment are the reader's, so they are checked in the invariant
    /// culture every reader shares.)
    /// </summary>
    [Fact]
    public void TheEventSaysItsSixPartsInTheirOrder()
    {
        CultureScope.Runs(CultureInfo.InvariantCulture, () =>
        {
            Assert.Equal(
                "DashboardEvent { At = 11/21/2026 09:00:00 +00:00, Kind = Health, Level = Info, Environment = prod, "
                + "Node = westus3, Text = ui is healthy }",
                Event().ToString());

            Assert.Equal(
                "DashboardEvent { At = 11/21/2026 09:00:00 +00:00, Kind = Traffic, Level = Info, Environment = , "
                + "Node = , Text = Traffic started }",
                Event(kind: EventKind.Traffic, environment: null, node: null, text: "Traffic started").ToString());

            // The part that is not there says its name and nothing after it; the empty word, no word at all and a
            // word of nothing but spaces each say a line of its own length.
            Assert.Equal(
                "DashboardEvent { At = 11/21/2026 09:00:00 +00:00, Kind = Health, Level = Info, Environment = , "
                + "Node = , Text =  }",
                Event(environment: null, node: null, text: string.Empty).ToString());
            Assert.Equal(
                "DashboardEvent { At = 11/21/2026 09:00:00 +00:00, Kind = Health, Level = Info, Environment = , "
                + "Node = , Text =  }",
                Event(environment: null, node: null, text: null!).ToString());
            Assert.Equal(
                "DashboardEvent { At = 11/21/2026 09:00:00 +00:00, Kind = Health, Level = Info, Environment = , "
                + "Node = , Text =    }",
                Event(environment: null, node: null, text: "  ").ToString());

            // The whitespace inside a word is kept where it stands.
            Assert.Equal(
                "DashboardEvent { At = 11/21/2026 09:00:00 +00:00, Kind = Health, Level = Info, Environment =  prod ,"
                + " Node =  westus3 , Text =  ui  is  healthy  }",
                Event(environment: " prod ", node: " westus3 ", text: " ui  is  healthy ").ToString());

            // The arrow a detector writes is one character whoever reads the line.
            Assert.Equal(
                "DashboardEvent { At = 11/21/2026 09:00:00 +00:00, Kind = Version, Level = Info, Environment = prod,"
                + " Node = westus3, Text = ui: 2.4.14 → 2.4.15, deployed }",
                Event(kind: EventKind.Version, text: "ui: 2.4.14 → 2.4.15, deployed").ToString());

            // The moment is said in the hours it was written in, at the ends of its number and in between.
            Assert.Equal(
                "DashboardEvent { At = 01/01/0001 00:00:00 +00:00, Kind = Health, Level = Info, Environment = prod, "
                + "Node = westus3, Text = ui is healthy }",
                Event(at: DateTimeOffset.MinValue).ToString());
            Assert.Equal(
                "DashboardEvent { At = 12/31/9999 23:59:59 +00:00, Kind = Health, Level = Info, Environment = prod, "
                + "Node = westus3, Text = ui is healthy }",
                Event(at: DateTimeOffset.MaxValue).ToString());
            Assert.Equal(
                "DashboardEvent { At = 11/21/2026 14:30:00 +05:30, Kind = Health, Level = Info, Environment = prod, "
                + "Node = westus3, Text = ui is healthy }",
                Event(at: AtInOtherHours).ToString());
        });
    }

    /// <summary>
    /// The moment the event names is said in the hours the reader speaks - <c>11/21/2026 09:00:00 +00:00</c> to a
    /// reader of the invariant culture and <c>21.11.2026 09:00:00 +00:00</c> to one of German - while the other five
    /// parts say themselves the same whichever the reader is, the kind and the level keeping their declared words
    /// because the page never shows them untranslated. That the line is the moment's own word and not the record's is
    /// pinned here so that a change of it is seen.
    /// </summary>
    [Fact]
    public void TheEventSaysItsMomentInTheHoursTheReaderSpeaks()
    {
        var entry = Event(kind: EventKind.Cluster, level: EventLevel.Warning, environment: "uat", node: "AKS", text: "The cluster status file changed.");

        CultureScope.Runs(CultureInfo.InvariantCulture, () =>
        {
            Assert.Equal(
                "DashboardEvent { At = 11/21/2026 09:00:00 +00:00, Kind = Cluster, Level = Warning, "
                + "Environment = uat, Node = AKS, Text = The cluster status file changed. }",
                entry.ToString());
        });

        ViewerCulture.German(() => Assert.Equal(
            "DashboardEvent { At = 21.11.2026 09:00:00 +00:00, Kind = Cluster, Level = Warning, "
            + "Environment = uat, Node = AKS, Text = The cluster status file changed. }",
            entry.ToString()));
    }

    // ------------------------------------------------------------------- the record's own shape

    /// <summary>
    /// The event's whole public surface, as the running assembly holds it: its six parts named in the order the
    /// constructor names them - the moment, the kind, the level, the environment, the node and the words - and the
    /// members every record is asked, falling apart, comparing, hashing and saying itself, with the <c>==</c> and the
    /// <c>!=</c> a caller writes, and nothing else to call. The parts are read and not written after the making: an
    /// event the page could turn the hour after it observed something would be an event no log can be trusted to hold.
    /// A seventh part added to the event has to be written here as well, which is the point: the event list cannot be
    /// shown a part nobody decided what it means.
    /// </summary>
    [Fact]
    public void TheEventHasItsSixPartsAndTheMembersEveryRecordIsAsked()
    {
        var type = typeof(DashboardEvent);

        Assert.True(type.IsSealed);
        Assert.False(type.IsValueType);
        Assert.Equal([typeof(IEquatable<DashboardEvent>)], type.GetInterfaces());
        Assert.Equal(typeof(object), type.BaseType);

        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        Assert.Equal(["At", "Kind", "Level", "Environment", "Node", "Text"], properties.Select(property => property.Name));
        Assert.Equal(
            [typeof(DateTimeOffset), typeof(EventKind), typeof(EventLevel), typeof(string), typeof(string), typeof(string)],
            properties.Select(property => property.PropertyType));
        Assert.All(properties, property =>
        {
            Assert.True(property.CanRead);
            Assert.True(property.CanWrite);
            Assert.NotNull(property.SetMethod);
            Assert.True(property.GetMethod!.IsPublic);

            // The parts are init-only: they are said once, at the making or at the copy, and never turned after.
            Assert.Contains(typeof(IsExternalInit), property.SetMethod!.ReturnParameter.GetRequiredCustomModifiers());
        });

        // The contract every record compares by is the compiler's private one: no public part of an event.
        Assert.Null(type.GetProperty("EqualityContract", BindingFlags.Public | BindingFlags.Instance));
        Assert.NotNull(type.GetProperty("EqualityContract", BindingFlags.NonPublic | BindingFlags.Instance));
        Assert.DoesNotContain("EqualityContract", PublicSurface(), StringComparer.Ordinal);

        Assert.Equal(
            ["At", "Deconstruct", "Environment", "Equals", "GetHashCode", "Kind", "Level", "Node", "Text", "ToString", "op_Equality", "op_Inequality"],
            PublicSurface());

        // Six parts, the one public constructor that takes them in that order, and none of them left to fall to
        // nothing on their own: an event cannot be made without a moment, a kind, a level, and words.
        var arguments = type.GetConstructors().Single().GetParameters();

        Assert.Single(type.GetConstructors(BindingFlags.Public | BindingFlags.Instance));
        Assert.Equal(6, arguments.Length);
        Assert.Equal(["At", "Kind", "Level", "Environment", "Node", "Text"], arguments.Select(argument => argument.Name));
        Assert.Equal(
            [typeof(DateTimeOffset), typeof(EventKind), typeof(EventLevel), typeof(string), typeof(string), typeof(string)],
            arguments.Select(argument => argument.ParameterType));
        Assert.All(arguments, argument => Assert.False(argument.HasDefaultValue));

        // The copy a with walks is the compiler's private one, and the only other way in.
        var copy = Assert.Single(type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance));
        Assert.Equal(type, copy.GetParameters().Single().ParameterType);

        // The two overloads of the equality every record is asked with, and the one Deconstruct of six parts.
        Assert.Equal(
            2,
            type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Count(method => method.Name == "Equals"));
        var parts = type.GetMethod("Deconstruct")!;

        Assert.Equal(typeof(void), parts.ReturnType);
        Assert.Equal(["At", "Kind", "Level", "Environment", "Node", "Text"], parts.GetParameters().Select(parameter => parameter.Name));
        Assert.All(parts.GetParameters(), parameter => Assert.True(parameter.IsOut));
    }

    /// <summary>The public members the event declares itself: its six properties and the methods that are not the get
    /// and the set of one - the two operators kept although the compiler names them specially, since they are the
    /// <c>==</c> and the <c>!=</c> a caller writes - with the record's compiler-named clone walked past. The
    /// constructor is no method, so it is named by the test above rather than by this list.</summary>
    private static List<string> PublicSurface() =>
        [.. typeof(DashboardEvent)
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is PropertyInfo
                             || member is MethodInfo method
                             && (!method.IsSpecialName || method.Name.StartsWith("op_", StringComparison.Ordinal)))
            .Where(member => !member.Name.Contains('<', StringComparison.Ordinal))
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];
}
