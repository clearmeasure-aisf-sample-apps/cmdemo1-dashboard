using System.Globalization;
using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// <see cref="PersonSignOff" /> is the newest deployment of a deployable to an environment that a person signed off
/// (<c>lastPersonSignOff</c> of an entry in <c>delivery.json</c>): the version that deployment deployed, the person
/// by their user name in Octopus Deploy, when they signed it off, the note they gave with it, and the page of that
/// release. The record holds its five parts and nothing else - it counts nothing and draws no verdict, since the
/// words are the card's and are pinned where they are said.
/// <para>
/// Where a sign-off comes from and what the card says of it is already pinned where it belongs:
/// <see cref="DeliveryTests" /> pins the sign-off the file reads - every part of it optional, the person's name
/// trimmed of the whitespace the file pads it with, the version read without the build metadata after the
/// <c>+</c>, no version and no person no sign-off at all, and the sign-off of the fixture the whole class is read
/// from; <see cref="DeployableFactsTests" /> pins the words the delivery card says of it - the version, the
/// person, the age of the moment the sign-off stands on, and the reason that is the sign-off's and not the
/// entry's.
/// </para>
/// <para>
/// Left open by all of that, and pinned here: the record of the five parts itself, at the ends the tests of the
/// file leave open. First the five parts by the names of their own properties: the constructor handing each part
/// to the property of its name and holding it exactly - the very instances it was given, the fixed parts the file
/// gives, the parts left out, and the empty and the padded words the file gives kept as written. Then the moment
/// the sign-off stands on, held to the tick at the ends of its number, and the same instant spelled in other
/// hours. Then the sign-off that is its parts: two sign-offs one while their five parts are - the words as
/// written, the address as the address is - the hash that follows, the difference in any one of them, and the
/// sign-offs that are not there - while the members every assertion above leans on without ever saying a word
/// about them are pinned for themselves: the copy a <c>with</c> makes, the five parts read out in the order they
/// stand in, the line the record says about itself, and the record's whole public surface, so that the day a sixth
/// part is added the list below says so.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: a sign-off holds five parts, calls
/// nothing at all, and every part below is one the test brought.
/// </para>
/// </summary>
public class PersonSignOffEdgeCaseTests
{
    /// <summary>The moment the person signed off, on the meridian: the fixture's moment, and the sign-offs below stand on it.</summary>
    private static readonly DateTimeOffset SignedAt = new(2026, 10, 1, 15, 12, 41, TimeSpan.Zero);

    /// <summary>The very instant of <see cref="SignedAt" />, five and a half hours east of the meridian.</summary>
    private static readonly DateTimeOffset SignedAtInOtherHours = new(2026, 10, 1, 20, 42, 41, TimeSpan.FromMinutes(330));

    /// <summary>A sign-off of the parts it is told, the parts left out the way the file leaves them out.</summary>
    private static PersonSignOff SignOff(string? version = null, string? by = null, DateTimeOffset? at = null,
        string? reason = null, Uri? release = null) => new(version, by, at, reason, release);

    // ---------------------------------------------------------------- the five parts, by the names of their own properties

    /// <summary>
    /// The constructor's five parts, position by position, come back as the property of the same name: the version
    /// first, the person, the moment, the note and the page of the release last - the order the file names them
    /// in. The fixed parts the file gives stand as they are, and the words and the page the record is told it
    /// holds: the very instances, so a sign-off the page reads and a sign-off the test makes stand on the same
    /// parts and not on copies of them.
    /// </summary>
    [Fact]
    public void TheConstructorHandsEachPartToThePropertyOfItsName()
    {
        var by = new string("pat.morgan".ToCharArray());
        var release = new Uri("https://octopus.example.net/r/2.4.11");
        var signOff = new PersonSignOff("2.4.11", by, SignedAt, "Reviewed with the customer", release);

        Assert.Equal("2.4.11", signOff.Version);
        Assert.Equal("pat.morgan", signOff.By);
        Assert.Equal(SignedAt, signOff.At);
        Assert.Equal("Reviewed with the customer", signOff.Reason);
        Assert.Equal("https://octopus.example.net/r/2.4.11", signOff.ReleaseUrl?.AbsoluteUri);
        Assert.Same(by, signOff.By);
        Assert.Same(release, signOff.ReleaseUrl);

        // Each part named for itself lands in its own place and the rest stand where the file leaves them: out.
        Assert.Equal("a", SignOff(version: "a").Version);
        Assert.Equal("b", SignOff(by: "b").By);
        Assert.Equal(SignedAt, SignOff(at: SignedAt).At);
        Assert.Equal("c", SignOff(reason: "c").Reason);
        Assert.Equal(
            "https://octopus.example.net/r/7",
            SignOff(release: new Uri("https://octopus.example.net/r/7")).ReleaseUrl?.AbsoluteUri);
    }

    /// <summary>
    /// The part the file does not say is kept as not said: the sign-off told of nothing is a sign-off with no
    /// part in any of the five places - and a sign-off told of one part is told of that one: the version with no
    /// person and no moment, the person with no version, the moment with no words at all. That is the sign-off
    /// the file reads when it says one thing and leaves the rest out, which <see cref="DeliveryTests" /> pins
    /// from the file.
    /// </summary>
    [Fact]
    public void ThePartTheFileDoesNotSayIsKeptAsNotSaid()
    {
        var none = new PersonSignOff(null, null, null, null, null);

        Assert.Null(none.Version);
        Assert.Null(none.By);
        Assert.Null(none.At);
        Assert.Null(none.Reason);
        Assert.Null(none.ReleaseUrl);

        Assert.Equal(new PersonSignOff("1.0.30", null, null, null, null), SignOff(version: "1.0.30"));
        Assert.Equal(new PersonSignOff(null, "pat.morgan", null, null, null), SignOff(by: "pat.morgan"));
        Assert.Equal(new PersonSignOff(null, null, SignedAt, null, null), SignOff(at: SignedAt));
        Assert.Equal(new PersonSignOff(null, null, null, "Looks right", null), SignOff(reason: "Looks right"));
        Assert.Equal(
            new PersonSignOff(null, null, null, null, new Uri("https://octopus.example.net/r/1.0.30")),
            SignOff(release: new Uri("https://octopus.example.net/r/1.0.30")));
    }

    /// <summary>
    /// The words the record is told it holds, it holds as they were told: the empty word is no word but it is
    /// still the word the sign-off stands on, the word of one letter is one letter, and the whitespace the file
    /// may pad a name with and the build metadata after the <c>+</c> a version may carry are kept where they
    /// are - the trimming and the stripping of them is the file's question, pinned where the file is read, and
    /// the record takes a word the file could not give for the word it is, however long it is.
    /// </summary>
    [Fact]
    public void TheWordsTheSignOffHoldsAreKeptAsTheyWereWritten()
    {
        var padded = new PersonSignOff(" 2.4.11+abc ", " pat.morgan ", SignedAt, "  ", null);

        Assert.Equal(" 2.4.11+abc ", padded.Version);
        Assert.Equal(" pat.morgan ", padded.By);
        Assert.Equal("  ", padded.Reason);

        Assert.Equal(string.Empty, SignOff(version: string.Empty).Version);
        Assert.Equal(string.Empty, SignOff(by: string.Empty).By);
        Assert.Equal(string.Empty, SignOff(reason: string.Empty).Reason);
        Assert.Equal(" ", SignOff(by: " ").By);
        Assert.Equal("x", SignOff(version: "x").Version);
        Assert.Equal("+", SignOff(version: "+").Version);

        // The word without an end of length is the word it is: the record does not measure it.
        var longVersion = new string('9', 10_000);
        Assert.Same(longVersion, SignOff(version: longVersion).Version);
    }

    // ------------------------------------------------------------------ the moment, at the ends of its number

    /// <summary>
    /// The moment the person signed off is held to the tick, at the ends of its number: the first and the last
    /// moment there are, and a tick away from the moment the file writes either side of it - the record compares
    /// the tick, so a sign-off of the moment one tick later is another, though it is the same second the moment
    /// is said in.
    /// </summary>
    [Fact]
    public void TheMomentTheSignOffStoodOnIsHeldToTheTick()
    {
        Assert.Equal(DateTimeOffset.MinValue, SignOff(at: DateTimeOffset.MinValue).At);
        Assert.Equal(DateTimeOffset.MaxValue, SignOff(at: DateTimeOffset.MaxValue).At);

        var tickBefore = SignedAt.AddTicks(-1);
        var tickAfter = SignedAt.AddTicks(1);

        Assert.Equal(tickBefore, SignOff(at: tickBefore).At);
        Assert.Equal(tickAfter, SignOff(at: tickAfter).At);
        Assert.NotEqual(SignOff(at: SignedAt), SignOff(at: tickAfter));
        Assert.NotEqual(SignOff(at: SignedAt), SignOff(at: tickBefore));
        Assert.NotEqual(SignOff(at: tickAfter), SignOff(at: tickBefore));
    }

    /// <summary>
    /// The same instant, whichever hours write it, is the same sign-off: the moment the file writes on the
    /// meridian and the very moment five and a half hours east of it are one sign-off - the equality of the
    /// operator, of <c>Equals</c> boxed, and the hash that goes with it - while the instant one second later in
    /// the other hours is another.
    /// </summary>
    [Fact]
    public void TheSameInstantInOtherHoursIsTheSameSignOff()
    {
        var onTheMeridian = SignOff("2.4.11", "pat.morgan", SignedAt, null, null);
        var inOtherHours = SignOff("2.4.11", "pat.morgan", SignedAtInOtherHours, null, null);

        Assert.True(onTheMeridian == inOtherHours);
        Assert.True(onTheMeridian.Equals((object)inOtherHours));
        Assert.Equal(onTheMeridian, inOtherHours);
        Assert.Equal(onTheMeridian.GetHashCode(), inOtherHours.GetHashCode());
        Assert.NotEqual(onTheMeridian, SignOff("2.4.11", "pat.morgan", SignedAtInOtherHours.AddSeconds(1), null, null));
    }

    // ------------------------------------------------------------------------ the sign-off itself

    /// <summary>
    /// Two sign-offs are one while their five parts are - each a part's own equality, the moment as the instant
    /// it is and the page as the address it is - the equality of the operator, of <c>Equals</c> boxed and
    /// unboxed, and the hash that follows them and stands from call to call, so a sign-off is usable as the key
    /// of a row. A part of another kind is no sign-off's equal.
    /// </summary>
    [Fact]
    public void TwoSignOffsAreOneWhileTheirFivePartsAre()
    {
        var one = SignOff("2.4.11", "pat.morgan", SignedAt, "Reviewed with the customer", new Uri("https://octopus.example.net/r/2.4.11"));
        var two = SignOff("2.4.11", "pat.morgan", SignedAt, "Reviewed with the customer", new Uri("https://octopus.example.net/r/2.4.11"));

        Assert.Equal(one, two);
        Assert.True(one == two);
        Assert.True(two == one);
        Assert.False(one != two);
        Assert.True(one.Equals(two));
        Assert.True(two.Equals(one));
        Assert.True(one.Equals((object)two));
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.Equal(one.GetHashCode(), one.GetHashCode());
        Assert.Single(new HashSet<PersonSignOff> { one, two });

        Assert.False(one.Equals(null));
        Assert.False(one.Equals("2.4.11"));
        Assert.False(one.Equals("pat.morgan"));
        Assert.False(one.Equals(SignedAt));
        Assert.False(one.Equals("Reviewed with the customer"));
        Assert.False(one.Equals(new Uri("https://octopus.example.net/r/2.4.11")));
        Assert.False(one.Equals(new HealthWindow(24, 23)));
    }

    /// <summary>
    /// The words are compared as they were written: the user name in another case is another person to the
    /// record - the lookup of an entry folds the case the file writes it in, which is the file's question and is
    /// pinned where the file is read - and so is the word with the whitespace the file pads it with, the version
    /// with its build metadata, and the note told without the words it was told with.
    /// </summary>
    [Fact]
    public void TheWordsAreComparedAsTheyWereWritten()
    {
        var signOff = SignOff("2.4.11", "pat.morgan", SignedAt, "Reviewed with the customer", null);

        Assert.NotEqual(signOff, SignOff("2.4.11", "PAT.MORGAN", SignedAt, "Reviewed with the customer", null));
        Assert.NotEqual(signOff, SignOff("2.4.11", " pat.morgan ", SignedAt, "Reviewed with the customer", null));
        Assert.NotEqual(signOff, SignOff("2.4.11+abc", "pat.morgan", SignedAt, "Reviewed with the customer", null));
        Assert.NotEqual(signOff, SignOff("2.4.11", "pat.morgan", SignedAt, "Reviewed with the customer ", null));
        Assert.NotEqual(signOff, SignOff("2.4.11", "pat.morgan", SignedAt, "reviewed with the customer", null));
    }

    /// <summary>
    /// The page of the release is compared as the address it is: the page read again from its words is the same
    /// sign-off, whatever instance it stands in - the record asks the address of the page and not the reference
    /// of it - while the page of another release is another sign-off, and the page the sign-off does not hold is
    /// yet another.
    /// </summary>
    [Fact]
    public void ThePageOfTheReleaseIsComparedAsTheAddressItIs()
    {
        var signOff = SignOff("1.0.30", "pat.morgan", SignedAt, null, new Uri("https://octopus.example.net/r/1.0.30"));
        var readAgain = signOff with { ReleaseUrl = new Uri("https://octopus.example.net/r/1.0.30") };

        Assert.Equal(signOff, readAgain);
        Assert.True(signOff == readAgain);
        Assert.Equal(signOff.GetHashCode(), readAgain.GetHashCode());
        Assert.NotEqual(signOff, signOff with { ReleaseUrl = new Uri("https://octopus.example.net/r/1.0.31") });
        Assert.NotEqual(signOff, signOff with { ReleaseUrl = null });
    }

    /// <summary>
    /// A sign-off differs in any one of its parts: the version to another or to no version, the person to
    /// another or to no person, the moment to another tick or to no moment, the note to another or to no note,
    /// the page to another or to no page - and the difference stands whichever way the question is asked, the
    /// five of them standing apart in a set.
    /// </summary>
    [Fact]
    public void ADifferenceInAnyPartIsAnotherSignOff()
    {
        var signOff = SignOff("2.4.11", "pat.morgan", SignedAt, "Reviewed with the customer", new Uri("https://octopus.example.net/r/2.4.11"));

        Assert.NotEqual(signOff, signOff with { Version = "2.4.12" });
        Assert.NotEqual(signOff, signOff with { Version = null });
        Assert.NotEqual(signOff, signOff with { By = "cm-ai-ops" });
        Assert.NotEqual(signOff, signOff with { By = null });
        Assert.NotEqual(signOff, signOff with { At = SignedAt.AddSeconds(1) });
        Assert.NotEqual(signOff, signOff with { At = null });
        Assert.NotEqual(signOff, signOff with { Reason = "Looks right" });
        Assert.NotEqual(signOff, signOff with { Reason = null });
        Assert.NotEqual(signOff, signOff with { ReleaseUrl = new Uri("https://octopus.example.net/r/2.4.10") });
        Assert.NotEqual(signOff, signOff with { ReleaseUrl = null });
        Assert.True(signOff != signOff with { At = null });
        Assert.False((signOff with { At = null }).Equals(signOff));

        Assert.Equal(
            5,
            new HashSet<PersonSignOff>
            {
                signOff with { Version = null },
                signOff with { By = null },
                signOff with { At = null },
                signOff with { Reason = null },
                signOff with { ReleaseUrl = null },
            }.Count);
    }

    /// <summary>
    /// Sign-offs that are not there are no sign-offs: the record's default is no sign-off, and no sign-off is
    /// never equal to a sign-off however the question is asked - the operators and <c>Equals</c> asked about the
    /// sign-off that is - while the sign-off that is, with every part left out, is a sign-off.
    /// </summary>
    [Fact]
    public void SignOffsThatAreNotThereAreNoSignOffs()
    {
        Assert.Null(default(PersonSignOff));

        PersonSignOff? none = null;
        PersonSignOff? alsoNone = null;
        var there = SignOff();

        Assert.True(none == alsoNone);
        Assert.False(none != alsoNone);
        Assert.False(none == there);
        Assert.True(none != there);
        Assert.False(there.Equals(none));
        Assert.False(there == none);
        Assert.True(there != none);
        Assert.True(there is not null);
    }

    // ------------------------------------------------------------------ the copy, and the part it turns

    /// <summary>
    /// A copy of a sign-off is the sign-off again: the <c>with</c> that names no part copies it whole - the very
    /// instances the record was given - and the <c>with</c> that names one part keeps the other four exactly as
    /// they stood, the original left where it stood. A part turned to no part keeps the rest and is another
    /// sign-off.
    /// </summary>
    [Fact]
    public void ACopyTurnsThePartItNamesAndKeepsTheOthers()
    {
        var release = new Uri("https://octopus.example.net/r/2.4.11");
        var signOff = SignOff("2.4.11", "pat.morgan", SignedAt, "Reviewed with the customer", release);

        var copy = signOff with { };

        Assert.Equal(signOff, copy);
        Assert.NotSame(signOff, copy);
        Assert.Same(release, copy.ReleaseUrl);
        Assert.Equal(signOff.GetHashCode(), copy.GetHashCode());

        var anotherVersion = signOff with { Version = "2.4.12" };

        Assert.Equal("2.4.12", anotherVersion.Version);
        Assert.Equal("pat.morgan", anotherVersion.By);
        Assert.Equal(SignedAt, anotherVersion.At);
        Assert.Equal("Reviewed with the customer", anotherVersion.Reason);
        Assert.Same(release, anotherVersion.ReleaseUrl);
        Assert.NotEqual(signOff, anotherVersion);
        Assert.NotSame(signOff, anotherVersion);

        var anotherPerson = signOff with { By = "cm-ai-ops" };

        Assert.Equal("cm-ai-ops", anotherPerson.By);
        Assert.Equal("2.4.11", anotherPerson.Version);
        Assert.Equal(SignedAt, anotherPerson.At);

        var later = signOff with { At = SignedAt.AddHours(1) };

        Assert.Equal(SignedAt.AddHours(1), later.At);
        Assert.Equal("pat.morgan", later.By);
        Assert.Same(release, later.ReleaseUrl);

        var anotherNote = signOff with { Reason = "Looks right" };

        Assert.Equal("Looks right", anotherNote.Reason);
        Assert.Equal("2.4.11", anotherNote.Version);
        Assert.Same(release, anotherNote.ReleaseUrl);

        var anotherPage = signOff with { ReleaseUrl = new Uri("https://octopus.example.net/r/2.4.12") };

        Assert.Equal("https://octopus.example.net/r/2.4.12", anotherPage.ReleaseUrl!.AbsoluteUri);
        Assert.Equal("2.4.11", anotherPage.Version);
        Assert.Equal("pat.morgan", anotherPage.By);

        // The original stands where it stood, part for part.
        Assert.Equal("2.4.11", signOff.Version);
        Assert.Equal("pat.morgan", signOff.By);
        Assert.Equal(SignedAt, signOff.At);
        Assert.Equal("Reviewed with the customer", signOff.Reason);
        Assert.Same(release, signOff.ReleaseUrl);

        var repack = signOff with
        {
            Version = signOff.Version,
            By = signOff.By,
            At = signOff.At,
            Reason = signOff.Reason,
            ReleaseUrl = signOff.ReleaseUrl,
        };

        Assert.Equal(signOff, repack);
        Assert.NotSame(signOff, repack);

        // A part turned to no part keeps the other four.
        var noMoment = signOff with { At = null };

        Assert.Null(noMoment.At);
        Assert.Equal("2.4.11", noMoment.Version);
        Assert.Equal("pat.morgan", noMoment.By);
        Assert.Equal("Reviewed with the customer", noMoment.Reason);
        Assert.Same(release, noMoment.ReleaseUrl);
        Assert.NotEqual(signOff, noMoment);
    }

    // ------------------------------------------------------------ the five parts read out

    /// <summary>
    /// The five parts read out of the sign-off in the order they stand in it: the very instances the record was
    /// given - the words as written, the moment as the moment it is, the page as the instance it holds - and the
    /// sign-off told of nothing falling apart as not told, each of its parts coming out as
    /// <see langword="null" />.
    /// </summary>
    [Fact]
    public void TheFivePartsReadOutOfTheSignOff()
    {
        var release = new Uri("https://octopus.example.net/r/2.4.11");

        var (version, by, at, reason, page) = new PersonSignOff("2.4.11", "pat.morgan", SignedAt, "Reviewed with the customer", release);

        Assert.Equal("2.4.11", version);
        Assert.Equal("pat.morgan", by);
        Assert.Equal(SignedAt, at);
        Assert.Equal("Reviewed with the customer", reason);
        Assert.Same(release, page);

        // What came out goes back in the same way: the sign-off the parts fall apart into is the sign-off they
        // were.
        Assert.Equal(new PersonSignOff("2.4.11", "pat.morgan", SignedAt, "Reviewed with the customer", release), new PersonSignOff(version, by, at, reason, page));

        var (noVersion, noBy, noAt, noReason, noPage) = new PersonSignOff(null, null, null, null, null);

        Assert.Null(noVersion);
        Assert.Null(noBy);
        Assert.Null(noAt);
        Assert.Null(noReason);
        Assert.Null(noPage);

        // Asked part by part, the same five parts come out in the same order.
        var signOff = SignOff("1.0.30", "pat.morgan", SignedAt, null, release);
        signOff.Deconstruct(out var versionAsked, out var byAsked, out var atAsked, out var reasonAsked, out var pageAsked);

        Assert.Equal("1.0.30", versionAsked);
        Assert.Equal("pat.morgan", byAsked);
        Assert.Equal(SignedAt, atAsked);
        Assert.Null(reasonAsked);
        Assert.Same(release, pageAsked);
    }

    // ------------------------------------------------------------ the words the sign-off says itself

    /// <summary>
    /// The sign-off says its five parts in their order, the name of each with the value it stands on: the words
    /// as written, the moment in the hours it stands on, and the page as the address it is. The parts the
    /// sign-off does not have say their name and nothing more, the empty word its emptiness and the whitespace
    /// its space. (The words the moment says are the viewer's, so they are checked in the hours every viewer
    /// shares.)
    /// </summary>
    [Fact]
    public void TheSignOffSaysItsFivePartsInTheirOrder()
    {
        var culture = Thread.CurrentThread.CurrentCulture;
        try
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            Assert.Equal(
                "PersonSignOff { Version = 2.4.11, By = pat.morgan, At = 10/01/2026 15:12:41 +00:00, "
                + "Reason = Reviewed with the customer, ReleaseUrl = https://octopus.example.net/r/2.4.11 }",
                SignOff("2.4.11", "pat.morgan", SignedAt, "Reviewed with the customer", new Uri("https://octopus.example.net/r/2.4.11")).ToString());
            Assert.Equal(
                "PersonSignOff { Version = , By = , At = , Reason = , ReleaseUrl =  }",
                SignOff().ToString());
            Assert.Equal(
                "PersonSignOff { Version = 1.0.30, By = , At = , Reason = , ReleaseUrl =  }",
                SignOff(version: "1.0.30").ToString());
            Assert.Equal(
                "PersonSignOff { Version = , By =  , At = , Reason = , ReleaseUrl =  }",
                SignOff(by: " ").ToString());
            Assert.Equal(
                "PersonSignOff { Version = , By = , At = 01/01/0001 00:00:00 +00:00, Reason = , ReleaseUrl =  }",
                SignOff(at: DateTimeOffset.MinValue).ToString());
            Assert.Equal(
                "PersonSignOff { Version = , By = , At = 12/31/9999 23:59:59 +00:00, Reason = , ReleaseUrl =  }",
                SignOff(at: DateTimeOffset.MaxValue).ToString());
            Assert.Equal(
                "PersonSignOff { Version = , By = , At = 10/01/2026 20:42:41 +05:30, Reason = , ReleaseUrl =  }",
                SignOff(at: SignedAtInOtherHours).ToString());
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = culture;
        }
    }

    /// <summary>
    /// The moment the sign-off names is said in the hours the viewer speaks: the same sign-off is
    /// <c>10/01/2026 15:12:41 +00:00</c> to a reader of the invariant culture and
    /// <c>01.10.2026 15:12:41 +00:00</c> to one of German, the four other parts saying themselves the same
    /// whichever the reader is - which is the moment's own word and not the sign-off's, pinned here so that a
    /// change of it is seen.
    /// </summary>
    [Fact]
    public void TheSignOffSaysItsMomentInTheHoursTheViewerSpeaks()
    {
        var signOff = SignOff("2.4.11", "pat.morgan", SignedAt, "Reviewed with the customer", new Uri("https://octopus.example.net/r/2.4.11"));

        ViewerCulture.German(() => Assert.Equal(
            "PersonSignOff { Version = 2.4.11, By = pat.morgan, At = 01.10.2026 15:12:41 +00:00, "
            + "Reason = Reviewed with the customer, ReleaseUrl = https://octopus.example.net/r/2.4.11 }",
            signOff.ToString()));
    }

    // ------------------------------------------------------------------------ the record's own shape

    /// <summary>
    /// The sign-off's whole public surface, as it stands: the five parts named in the order the constructor names
    /// them - the version, the person, the moment, the note, the page - and the members every record is asked:
    /// falling apart, comparing, hashing and saying itself, with the <c>==</c> and the <c>!=</c> a caller writes.
    /// The parts are read and not written - a sign-off that could be told who signed it off would be a record the
    /// file did not make - while a <c>with</c> is the way the parts turn and the constructor the way they are
    /// named. A sixth part added to the record has to be written here as well, which is the point: the card cannot
    /// be shown a part nobody decided what it means.
    /// </summary>
    [Fact]
    public void TheSignOffHasItsFivePartsAndTheMembersEveryRecordIsAsked()
    {
        Assert.Equal(
            ["Version", "By", "At", "Reason", "ReleaseUrl"],
            [.. typeof(PersonSignOff)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(property => property.Name)]);

        Assert.Equal(
            ["At", "By", "Deconstruct", "Equals", "GetHashCode", "Reason", "ReleaseUrl", "ToString", "Version", "op_Equality", "op_Inequality"],
            PublicSurface());

        var properties = typeof(PersonSignOff)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .ToDictionary(property => property.Name, StringComparer.Ordinal);

        Assert.Equal(typeof(string), properties["Version"].PropertyType);
        Assert.Equal(typeof(string), properties["By"].PropertyType);
        Assert.Equal(typeof(DateTimeOffset?), properties["At"].PropertyType);
        Assert.Equal(typeof(string), properties["Reason"].PropertyType);
        Assert.Equal(typeof(Uri), properties["ReleaseUrl"].PropertyType);

        foreach (var property in properties.Values)
        {
            Assert.True(property.CanRead);
            Assert.True(property.CanWrite);
        }

        // Five parts, and the one constructor that takes them in that order.
        var constructors = typeof(PersonSignOff).GetConstructors();

        Assert.Single(constructors);
        var parameters = constructors[0].GetParameters();

        Assert.Equal(["Version", "By", "At", "Reason", "ReleaseUrl"], [.. parameters.Select(parameter => parameter.Name!)]);
        Assert.Equal(typeof(string), parameters[0].ParameterType);
        Assert.Equal(typeof(string), parameters[1].ParameterType);
        Assert.Equal(typeof(DateTimeOffset?), parameters[2].ParameterType);
        Assert.Equal(typeof(string), parameters[3].ParameterType);
        Assert.Equal(typeof(Uri), parameters[4].ParameterType);
    }

    // ---------------------------------------------------------------------------------- the machinery

    /// <summary>The public members the sign-off declares itself: its properties and the methods that are not the
    /// get and the set of one - the two operators kept although the compiler names them specially, since they
    /// are the <c>==</c> and the <c>!=</c> a caller writes - with the record's compiler-named clone walked past.
    /// The constructor is no method, so it is named by the test above rather than by this list.</summary>
    private static List<string> PublicSurface() =>
        [.. typeof(PersonSignOff)
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is PropertyInfo
                             || member is MethodInfo method
                             && (!method.IsSpecialName || method.Name.StartsWith("op_", StringComparison.Ordinal)))
            .Where(member => !member.Name.Contains('<', StringComparison.Ordinal))
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];
}
