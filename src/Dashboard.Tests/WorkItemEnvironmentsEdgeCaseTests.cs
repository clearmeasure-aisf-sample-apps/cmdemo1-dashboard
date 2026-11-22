using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="WorkItemEnvironments" />, the record declared in
/// <c>src/Dashboard/Work/WorkItemsReport.cs</c> that says where one work item's release has got to: the three places a
/// board's item can stand - tdd, uat and prod, named in that order because that is the order the work runs through
/// them - each one a deployment or the nothing of an environment that has not got it.
/// <para>
/// What the other files already pin is left alone. The taking of the three places out of <c>workitems.json</c> is
/// <c>WorkItemEnvironments.Read</c>, which is internal to an app that declares no <c>InternalsVisibleTo</c> (as
/// <c>Dashboard.Tests.csproj</c> notes of <c>JsonRead</c>) and is therefore pinned through
/// <see cref="WorkItemsReport.Parse" />: <see cref="WorkItemsTests.EveryPartOfAnItemIsOptionalAndABadPartIsLeftOut" />
/// says that a place written in other letters is still that place and that a place written as no object at all is no
/// place; <see cref="WorkItemsTests.TheSampleFileIsTheBoardTheViewShows" /> and
/// <see cref="WorkItemsTests.TheFeedSampleUsesThePublishedNames" /> that the sample's and the feed's places arrive with
/// their version and their moment; and <see cref="WorkItemsTests.AMissingSliceIsCountedFromTheItems" /> that a file
/// naming prod alone names prod. What an item is worth to the page - the <c>tdd 2.4.86</c> of a label, the
/// <c>Deployed to prod, 3 h ago</c> of a title, the order the three are stood in - is
/// <see cref="WorkItemsText.Places" /> and <see cref="WorkItemsText.PlaceTitle" /> and is pinned in
/// <see cref="WorkItemsTextTests" />, where <c>EachPlaceIsTitledInItsOwnEnvironmentOrder</c> walks the three properties
/// into the page's own order. The version and the moment a place carries are <see cref="WorkDeployment" />'s own, and
/// the hours between a merge and a production deployment are <c>WorkBotText.MergeToProd</c>'s: both stand on a reading
/// or on a clock, and neither is a member of this record.
/// </para>
/// <para>
/// What is left, and is pinned here, is the record itself: the constructor handing each of its three deployments to
/// the property of its name and holding the very instance it was given, so that one deployment stood in all three
/// places is held thrice and a place written in the middle is not read at either end; the eight ways three places can
/// be filled being eight environments; a place with nothing said about its release being another environment than no
/// place at all; the shared <see cref="WorkItemEnvironments.None" />, one and the same value for every caller, and the
/// nothing a file that names no place lands on; the three places of a file coming into the three properties of their
/// own name whichever order the file writes them in; the copy that turns one place while it keeps the other two; the
/// three parts coming back out in the order they went in; the two environments being one while their three parts are -
/// by the parts' own equality, so that two deployments written apart are one deployment and the same instant written
/// in another offset is the same instant - with the hash, the set and the two operators following that, and the nothing
/// on either side of each; the words the environment says its three places in, which are the words of the places
/// inside them and follow whoever reads a date; and the public surface, so that the day a fourth environment is added
/// the list below says so.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: an environment holds three deployments
/// and calls nothing at all - no environment is asked whether it has the item, only told - so every value below is one
/// the test brought, and every call says the same thing twice.
/// </para>
/// </summary>
public class WorkItemEnvironmentsEdgeCaseTests
{
    /// <summary>The three moments of one item's road through the places, an hour and a quarter apart.</summary>
    private static readonly DateTimeOffset TddAt = new(2026, 11, 9, 16, 0, 0, TimeSpan.Zero);

    private static readonly DateTimeOffset UatAt = new(2026, 11, 9, 17, 15, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset ProdAt = new(2026, 11, 9, 18, 30, 0, TimeSpan.Zero);

    /// <summary>A release of the <c>2.4.&lt;build&gt;</c> shape, one per place, so that a part landing on another
    /// part's place is seen rather than quietly matching a third of the same record.</summary>
    private const string TddVersion = "2.4.80";

    private const string UatVersion = "2.4.81";
    private const string ProdVersion = "2.4.82";

    /// <summary>An environment with every place filled, each with its own release and its own moment.</summary>
    private static WorkItemEnvironments Full => Filled(true, true, true);

    /// <summary>An environment whose every place is said as a deployment, and every part of that deployment left
    /// unsaid: a place the item has got to, with nothing to tell about the release.</summary>
    private static WorkItemEnvironments UnsaidPlaces => new(new WorkDeployment(null, null), new WorkDeployment(null, null), new WorkDeployment(null, null));

    // --------------------------------- the three places, by the names of their own properties

    /// <summary>The constructor's three arguments, position by position, come back as the property of the same name:
    /// tdd first, uat beside it, prod last - the order the work runs in and the order the page says them. Each is held
    /// as the very deployment the environment was built of, never as a copy of it, and a new environment of the same
    /// three parts is another environment, not the one it was built of.</summary>
    [Fact]
    public void TheConstructorHandsEachDeploymentToThePropertyOfItsName()
    {
        var tdd = new WorkDeployment(TddVersion, TddAt);
        var uat = new WorkDeployment(UatVersion, UatAt);
        var prod = new WorkDeployment(ProdVersion, ProdAt);
        var environments = new WorkItemEnvironments(tdd, uat, prod);

        Assert.Same(tdd, environments.Tdd);
        Assert.Same(uat, environments.Uat);
        Assert.Same(prod, environments.Prod);

        var again = new WorkItemEnvironments(tdd, uat, prod);

        Assert.Equal(environments, again);
        Assert.NotSame(environments, again);
        Assert.Same(tdd, again.Tdd);

        // One deployment stood in all three places is held in all three, and is the one deployment it is.
        var once = new WorkDeployment("2.4.90", ProdAt);
        var thrice = new WorkItemEnvironments(once, once, once);

        Assert.Same(once, thrice.Tdd);
        Assert.Same(once, thrice.Uat);
        Assert.Same(once, thrice.Prod);
        Assert.Equal(new WorkItemEnvironments(once, new WorkDeployment("2.4.90", ProdAt), once), thrice);
        Assert.NotEqual(Full, thrice);
    }

    /// <summary>Each place stands on its own property and nowhere else: of the eight ways three places can be filled,
    /// every one says which of the three it filled, and a place left out is the nothing it was given and not the
    /// release of the place beside it.</summary>
    [Theory]
    [InlineData(false, false, false)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(false, false, true)]
    [InlineData(true, true, false)]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    [InlineData(true, true, true)]
    public void EachPlaceStandsOnItsOwnPropertyAndNowhereElse(bool tdd, bool uat, bool prod)
    {
        var environments = Filled(tdd, uat, prod);

        AssertPlace(environments.Tdd, tdd, TddVersion, TddAt);
        AssertPlace(environments.Uat, uat, UatVersion, UatAt);
        AssertPlace(environments.Prod, prod, ProdVersion, ProdAt);
    }

    /// <summary>The eight ways of filling three places are eight environments: no filling matches another, and no two
    /// of them are stood in one place of a set. The one of them that fills nothing is the shared
    /// <see cref="WorkItemEnvironments.None" /> in everything but its reference.</summary>
    [Fact]
    public void TheEightFillingsOfThreePlacesAreEightEnvironments()
    {
        var fillings = EveryFilling();

        Assert.Equal(8, fillings.Count);
        Assert.Equal(8, new HashSet<WorkItemEnvironments>(fillings).Count);
        Assert.Equal(8, fillings.Distinct().Count());
        Assert.Equal(
            ["none", "tdd", "uat", "tdd+uat", "prod", "tdd+prod", "uat+prod", "tdd+uat+prod"],
            [.. fillings.Select(PlacesOf)]);

        Assert.Equal(WorkItemEnvironments.None, fillings[0]);
        Assert.NotSame(WorkItemEnvironments.None, fillings[0]);
        Assert.Equal(WorkItemEnvironments.None, fillings[0] with { });
        Assert.NotSame(fillings[0], fillings[0] with { });
    }

    /// <summary>A place with nothing said about its release is still a place: a deployment of no version and no
    /// moment fills the property, and the environment that holds it is another environment than the one of no place at
    /// all - by its equality, its operators, its set and its words. Nothing here settles which of the two the page
    /// should show; <see cref="WorkItemsText.Places" /> counts a place that is there, and
    /// <see cref="WorkItemsText.PlaceTitle" /> says the one of no version without a version.</summary>
    [Fact]
    public void APlaceWithNothingSaidAboutItIsStillAPlace()
    {
        var empty = new WorkDeployment(null, null);
        var environments = new WorkItemEnvironments(empty, null, null);

        Assert.NotNull(environments.Tdd);
        Assert.Same(empty, environments.Tdd);
        Assert.Null(environments.Tdd.Version);
        Assert.Null(environments.Tdd.DeployedAt);
        Assert.Null(environments.Uat);
        Assert.Null(environments.Prod);

        Assert.NotEqual(WorkItemEnvironments.None, environments);
        Assert.True(environments != WorkItemEnvironments.None);
        Assert.False(environments == WorkItemEnvironments.None);
        Assert.Equal(2, new HashSet<WorkItemEnvironments> { WorkItemEnvironments.None, environments }.Count);
        Assert.NotEqual(Words(WorkItemEnvironments.None), Words(environments));

        // A place whose release is unsaid is not a place of an empty version either: the nothing of no place, the
        // nothing of a place with nothing to say and a place that says its release are three environments.
        Assert.NotEqual(environments, new WorkItemEnvironments(new WorkDeployment(string.Empty, null), null, null));
        Assert.Equal(3, new HashSet<WorkItemEnvironments> { WorkItemEnvironments.None, environments, Full }.Count);
        Assert.Equal(UnsaidPlaces, new WorkItemEnvironments(new WorkDeployment(null, null), new WorkDeployment(null, null), new WorkDeployment(null, null)));
        Assert.NotSame(UnsaidPlaces, new WorkItemEnvironments(new WorkDeployment(null, null), new WorkDeployment(null, null), new WorkDeployment(null, null)));
    }

    // ------------------------------------------------------------------ the one value of no place at all

    /// <summary><see cref="WorkItemEnvironments.None" /> is the one value every caller shares: asked twice, the same
    /// reference; in its own parts, the three nothings and no empty deployment among them; equal to - but never the
    /// same as - an environment built of three nothings. Turning one of its places turns it out of the singleton and
    /// leaves the singleton standing where it stood; turning every place of a full environment back off lands on it
    /// again.</summary>
    [Fact]
    public void NoneIsTheOneValueEveryCallerShares()
    {
        var none = WorkItemEnvironments.None;

        Assert.Same(none, WorkItemEnvironments.None);
        Assert.NotNull(none);
        Assert.Null(none.Tdd);
        Assert.Null(none.Uat);
        Assert.Null(none.Prod);

        Assert.Equal(none, new WorkItemEnvironments(null, null, null));
        Assert.True(none == new WorkItemEnvironments(null, null, null));
        Assert.False(none != new WorkItemEnvironments(null, null, null));
        Assert.NotSame(none, new WorkItemEnvironments(null, null, null));
        Assert.Equal(none.GetHashCode(), new WorkItemEnvironments(null, null, null).GetHashCode());
        Assert.Single(new HashSet<WorkItemEnvironments> { none, new(null, null, null) });

        var withProd = none with { Prod = new WorkDeployment(ProdVersion, ProdAt) };

        Assert.NotSame(none, withProd);
        Assert.NotEqual(none, withProd);
        Assert.Null(none.Prod);
        Assert.Equal(ProdVersion, withProd.Prod!.Version);
        Assert.Equal(none, withProd with { Prod = null });
        Assert.Equal(none, Full with { Tdd = null, Uat = null, Prod = null });
        Assert.Equal(none, UnsaidPlaces with { Tdd = null, Uat = null, Prod = null });
        Assert.NotEqual(none.GetHashCode(), withProd.GetHashCode());
    }

    /// <summary>A file that names no place lands on <see cref="WorkItemEnvironments.None" /> - no
    /// <c>environments</c> at all, one written as nothing, one written as an empty object, one written as an array, one
    /// written as a word, and one whose three names are all written as no object. Each is the nothing of no place
    /// rather than a place with nothing to say, and each is an environment of the reading's own making and not the
    /// shared value itself.</summary>
    [Theory]
    [InlineData("""{ "items": [ { "number": 1, "title": "No environments" } ] }""")]
    [InlineData("""{ "items": [ { "number": 1, "environments": null } ] }""")]
    [InlineData("""{ "items": [ { "number": 1, "environments": { } } ] }""")]
    [InlineData("""{ "items": [ { "number": 1, "environments": [] } ] }""")]
    [InlineData("""{ "items": [ { "number": 1, "environments": "tdd" } ] }""")]
    [InlineData("""{ "items": [ { "number": 1, "environments": { "tdd": null, "uat": null, "prod": null } } ] }""")]
    [InlineData("""{ "items": [ { "number": 1, "environments": { "tdd": 4, "uat": "2.4.80", "prod": true } } ] }""")]
    public void AFileThatNamesNoPlaceLandsOnNone(string body)
    {
        var item = Assert.Single(WorkItemsReport.Parse(body)!.Items);

        Assert.Equal(WorkItemEnvironments.None, item.Environments);
        Assert.NotSame(WorkItemEnvironments.None, item.Environments);
        Assert.Null(item.Environments.Tdd);
        Assert.Null(item.Environments.Uat);
        Assert.Null(item.Environments.Prod);
        Assert.Equal("WorkItemEnvironments { Tdd = , Uat = , Prod =  }", Words(item.Environments));
    }

    /// <summary>The three places of a file come into the three properties of their own name, whichever order the file
    /// writes them in: prod written before uat still stands on <see cref="WorkItemEnvironments.Prod" />, and the
    /// release and the moment each place carries are the ones that place was given. The letters a place is named in and
    /// the shape a place is written in are the reading's, pinned in <see cref="WorkItemsTests" />; what is pinned here
    /// is that the three properties are three and not one.</summary>
    [Fact]
    public void TheThreePlacesOfAFileComeIntoTheThreeProperties()
    {
        var report = WorkItemsReport.Parse("""
            { "items": [ { "number": 455, "title": "Everywhere", "environments": {
                "prod": { "version": "2.4.82", "deployedAt": "2026-11-09T18:30:00Z" },
                "tdd": { "version": "2.4.80", "deployedAt": "2026-11-09T16:00:00Z" },
                "uat": { "version": "2.4.81", "deployedAt": "2026-11-09T17:15:00Z" } } } ] }
            """)!;

        var environments = Assert.Single(report.Items).Environments;

        Assert.Equal(Full, environments);
        Assert.Equal(TddVersion, environments.Tdd!.Version);
        Assert.Equal(UatVersion, environments.Uat!.Version);
        Assert.Equal(ProdVersion, environments.Prod!.Version);
        Assert.Equal(TddAt, environments.Tdd.DeployedAt);
        Assert.Equal(UatAt, environments.Uat.DeployedAt);
        Assert.Equal(ProdAt, environments.Prod.DeployedAt);

        // One place filled and the other two left out of the file are one place filled and two nothings.
        var only = Assert.Single(WorkItemsReport.Parse("""
            { "items": [ { "number": 470, "environments": {
                "tdd": { "version": "2.4.80", "deployedAt": "2026-11-09T16:00:00Z" } } } ] }
            """)!.Items).Environments;

        Assert.Equal(new WorkItemEnvironments(new WorkDeployment(TddVersion, TddAt), null, null), only);
        Assert.Null(only.Uat);
        Assert.Null(only.Prod);
        Assert.NotEqual(Full, only);
        Assert.NotEqual(only, new WorkItemEnvironments(null, new WorkDeployment(TddVersion, TddAt), null));
    }

    // ------------------------------------------------------- a place turned, and the copy that turns it

    /// <summary>Each place may be turned as the environment is copied - filled where it was empty, emptied where it
    /// was filled, replaced with another release - and stands where it is turned to, while the places the turn does
    /// not name stay the very deployments the original was built of. A copy is another environment and never the same
    /// reference; a copy that names no place at all is the environment it was copied from, in its parts, in its hash
    /// and in its words; and the environment the copy was walked out of is untouched by it.</summary>
    [Fact]
    public void ACopyTurnsThePlaceItNamesAndKeepsTheOtherTwo()
    {
        var tdd = new WorkDeployment(TddVersion, TddAt);
        var uat = new WorkDeployment(UatVersion, UatAt);
        var environments = new WorkItemEnvironments(tdd, uat, null);

        var unchanged = environments with { };

        Assert.Equal(environments, unchanged);
        Assert.NotSame(environments, unchanged);
        Assert.Same(tdd, unchanged.Tdd);
        Assert.Equal(environments.GetHashCode(), unchanged.GetHashCode());
        Assert.Equal(Words(environments), Words(unchanged));

        var deployed = environments with { Prod = new WorkDeployment(ProdVersion, ProdAt) };

        Assert.Equal(Full, deployed);
        Assert.Same(tdd, deployed.Tdd);
        Assert.Same(uat, deployed.Uat);
        Assert.Null(environments.Prod);
        Assert.NotEqual(environments, deployed);

        var rolled = environments with { Tdd = new WorkDeployment("2.4.79", TddAt), Uat = null };

        Assert.Equal("2.4.79", rolled.Tdd!.Version);
        Assert.Equal(TddAt, rolled.Tdd.DeployedAt);
        Assert.Null(rolled.Uat);
        Assert.Null(rolled.Prod);
        Assert.Equal(uat, environments.Uat);
        Assert.Equal(new WorkItemEnvironments(new WorkDeployment("2.4.79", TddAt), null, null), rolled);

        var repacked = environments with { Tdd = environments.Tdd, Uat = environments.Uat, Prod = environments.Prod };

        Assert.Equal(environments, repacked);
        Assert.NotSame(environments, repacked);
        Assert.Same(tdd, repacked.Tdd);

        // The written-as-made form turns the same place the copy turns, and says it in another instance of it.
        var asMade = new WorkItemEnvironments(tdd, uat, null) { Prod = new WorkDeployment(ProdVersion, ProdAt) };

        Assert.Equal(deployed, asMade);
        Assert.NotSame(deployed, asMade);
        Assert.Equal(deployed.Prod, asMade.Prod);
        Assert.NotSame(deployed.Prod, asMade.Prod);
    }

    // --------------------------------------------- the environment falling apart, and its own declared shape

    /// <summary>The three parts come back out in the order they went in - tdd, uat, prod - by the positional
    /// <c>Deconstruct</c> every record is given, and the places nobody filled come out as the nothing they are and not
    /// as a deployment of no version. What came out goes back in the same way: the environment the three parts fall
    /// apart into is the environment they were, and no part is read again from somewhere else.</summary>
    [Fact]
    public void TheThreePartsComeOutAgainInTheOrderTheyWentIn()
    {
        var (tdd, uat, prod) = Full;

        Assert.Equal(TddVersion, tdd!.Version);
        Assert.Equal(UatVersion, uat!.Version);
        Assert.Equal(ProdVersion, prod!.Version);
        Assert.Equal(Full, new WorkItemEnvironments(tdd, uat, prod));
        Assert.Same(tdd, new WorkItemEnvironments(tdd, uat, prod).Tdd);
        Assert.NotSame(tdd, Full.Tdd);

        var (noTdd, noUat, noProd) = WorkItemEnvironments.None;

        Assert.Null(noTdd);
        Assert.Null(noUat);
        Assert.Null(noProd);
        Assert.Equal(WorkItemEnvironments.None, new WorkItemEnvironments(noTdd, noUat, noProd));

        // The place that stands alone stands in its own place: an environment of one place falls apart into that
        // place and two nothings, whichever of the three it is.
        var (only, middle, last) = new WorkItemEnvironments(null, null, new WorkDeployment(ProdVersion, ProdAt));

        Assert.Null(only);
        Assert.Null(middle);
        Assert.Equal(ProdVersion, last!.Version);
        Assert.Equal("prod", PlacesOf(new WorkItemEnvironments(only, middle, last)));
    }

    /// <summary>The environment's whole public surface, as the running assembly holds it: its three places named in
    /// the order the constructor names them, the one shared value of no place at all, and the members every record is
    /// asked - falling apart, comparing, hashing and saying itself - and nothing else to call. The way in is one: the
    /// three parts, none of them left out and none of them defaulted, for an environment of no place is had from
    /// <see cref="WorkItemEnvironments.None" /> or from three nothings and not from a shorter way in. A fourth place
    /// added to the record has to be written here as well, which is the point. And nothing on it is a moment, an
    /// address or a length of time the record would have to go and look up: three deployments, each of which only
    /// holds what the file said, and no clock among them - which is why no test in this file needs one.</summary>
    [Fact]
    public void TheRecordHasThreePartsAndTheMembersEveryRecordIsAsked()
    {
        var type = typeof(WorkItemEnvironments);

        Assert.True(type.IsSealed);
        Assert.False(type.IsValueType);
        Assert.Equal(typeof(object), type.BaseType);
        Assert.Equal([typeof(IEquatable<WorkItemEnvironments>)], type.GetInterfaces());

        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        Assert.Equal(["Tdd", "Uat", "Prod"], properties.Select(property => property.Name));
        Assert.All(properties, property =>
        {
            Assert.Equal(typeof(WorkDeployment), property.PropertyType);
            Assert.True(property.CanRead);
            Assert.True(property.CanWrite);
            Assert.NotNull(property.SetMethod);
            Assert.True(property.GetMethod!.IsPublic);

            // The parts are init-only: they are said once, at the making or at the copy, and never turned after.
            Assert.Contains(typeof(IsExternalInit), property.SetMethod!.ReturnParameter.GetRequiredCustomModifiers());

            // And each of the three is a place that may be empty, never a moment, an address or a word to follow.
            Assert.DoesNotContain(
                property.PropertyType,
                new[] { typeof(DateTimeOffset), typeof(DateTime), typeof(TimeSpan), typeof(Uri), typeof(string) });
        });

        // The shared nothing is the record's one static part: a property of its own type, with no way to turn it.
        var none = type.GetProperty("None", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)!;

        Assert.Equal(type, none.PropertyType);
        Assert.True(none.CanRead);
        Assert.Null(none.SetMethod);
        Assert.Null(type.GetProperty("None", BindingFlags.NonPublic | BindingFlags.Static));
        Assert.Same(WorkItemEnvironments.None, none.GetValue(null));
        Assert.Same(none.GetValue(null), none.GetValue(null));

        // The contract every record compares by is the compiler's private one: no public part of the environment.
        Assert.Null(type.GetProperty("EqualityContract", BindingFlags.Public | BindingFlags.Instance));
        Assert.NotNull(type.GetProperty("EqualityContract", BindingFlags.NonPublic | BindingFlags.Instance));
        Assert.DoesNotContain("EqualityContract", PublicSurface(), StringComparer.Ordinal);

        Assert.Equal(
            ["Deconstruct", "Equals", "GetHashCode", "None", "Prod", "Tdd", "ToString", "Uat", "op_Equality", "op_Inequality"],
            PublicSurface());

        var arguments = Assert.Single(type.GetConstructors(BindingFlags.Public | BindingFlags.Instance)).GetParameters();

        Assert.Equal(3, arguments.Length);
        Assert.Equal(["Tdd", "Uat", "Prod"], arguments.Select(argument => argument.Name));
        Assert.Equal([typeof(WorkDeployment), typeof(WorkDeployment), typeof(WorkDeployment)], arguments.Select(argument => argument.ParameterType));
        Assert.All(arguments, argument => Assert.False(argument.HasDefaultValue));

        // Every one of the three may be the nothing of an environment that has not got the item.
        var says = new NullabilityInfoContext();
        Assert.All(properties, property => Assert.Equal(NullabilityState.Nullable, says.Create(property).WriteState));

        // The copy the with walks is the compiler's private one, and the reading of the file is no public member.
        var copy = Assert.Single(type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance));
        Assert.Equal(type, copy.GetParameters().Single().ParameterType);
        Assert.Null(type.GetMethod("Read", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly));

        // The two overloads of the equality every record is asked with, and the one Deconstruct.
        Assert.Equal(
            2,
            type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Count(method => method.Name == "Equals"));
        var parts = type.GetMethod("Deconstruct")!;
        Assert.Equal(["Tdd", "Uat", "Prod"], parts.GetParameters().Select(parameter => parameter.Name));
        Assert.All(parts.GetParameters(), parameter =>
        {
            Assert.True(parameter.IsOut);
            Assert.True(parameter.ParameterType.IsByRef);
            Assert.Equal(typeof(WorkDeployment), parameter.ParameterType.GetElementType());
        });

        // And no member of it is given a moment, an address or a length of time to go and ask about.
        var other = type
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .SelectMany(method => method.GetParameters())
            .Select(parameter => parameter.ParameterType)
            .Where(parameter => parameter != type)
            .ToList();
        Assert.DoesNotContain(typeof(DateTimeOffset), other);
        Assert.DoesNotContain(typeof(DateTime), other);
        Assert.DoesNotContain(typeof(TimeSpan), other);
        Assert.DoesNotContain(typeof(Uri), other);
        Assert.DoesNotContain(typeof(string), other);
    }

    // -------------------------------------------------------------- the equality, the hash, and the two

    /// <summary>Two environments are one while their three places are - each place as the deployment it says, the
    /// nothing among them included - whichever of the ways of asking is used: the record's own <c>Equals</c>, the one
    /// of <see cref="object" />, <c>==</c> and <c>!=</c>, from either side. The parts are compared as themselves and
    /// not as references, so two deployments built apart and saying the same release in the same offset are one place;
    /// and a moment is compared as the instant it stands on, so the same arrival written in another offset is the same
    /// arrival, however the two are said.</summary>
    [Fact]
    public void TwoEnvironmentsAreOneWhileTheirThreePartsAre()
    {
        var one = Full;
        var two = Full;

        Assert.NotSame(one, two);
        Assert.NotSame(one.Tdd, two.Tdd);
        Assert.True(one.Equals(two));
        Assert.True(one.Equals(one));
        Assert.True(one.Equals((object)two));
        Assert.True(one == two);
        Assert.True(two == one);
        Assert.False(one != two);
        Assert.Equal(one, two);
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.Equal(one.GetHashCode(), ((object)one).GetHashCode());
        Assert.Equal(one.GetHashCode(), one.GetHashCode());
        Assert.Single(new HashSet<WorkItemEnvironments> { one, two });
        Assert.Contains(two, new HashSet<WorkItemEnvironments> { one });

        var byPlace = new Dictionary<WorkItemEnvironments, string> { [one] = "everywhere" };
        Assert.Equal("everywhere", byPlace[two]);

        Assert.Equal(WorkItemEnvironments.None, new WorkItemEnvironments(null, null, null));
        Assert.True(WorkItemEnvironments.None == new WorkItemEnvironments(null, null, null));
        Assert.Equal(WorkItemEnvironments.None, UnsaidPlaces with { Tdd = null, Uat = null, Prod = null });

        // The instant, not the way it is written: an arrival told in another offset is the same arrival, and one
        // environment, although the two records say it in different words.
        var written = new WorkItemEnvironments(new WorkDeployment(TddVersion, new DateTimeOffset(2026, 11, 9, 17, 0, 0, TimeSpan.FromHours(1))), null, null);
        var zulu = new WorkItemEnvironments(new WorkDeployment(TddVersion, TddAt), null, null);

        Assert.Equal(written, zulu);
        Assert.True(written == zulu);
        Assert.False(written != zulu);
        Assert.Equal(written.GetHashCode(), zulu.GetHashCode());
        Assert.Single(new HashSet<WorkItemEnvironments> { written, zulu });
        Assert.NotEqual(Words(written), Words(zulu));
    }

    /// <summary>A difference in one place is another environment: the item in tdd and not in prod, the same release
    /// arrived at an hour later, the version of one place turned, a place emptied of its release or of its moment, the
    /// three places stood in another order of the record's own making - and the environment of three empty places,
    /// which is not the environment of no places at all. A set of the environments that differ in one place each holds
    /// them all apart.</summary>
    [Fact]
    public void AnEnvironmentThatDiffersInOnePlaceIsAnotherEnvironment()
    {
        var one = Full;

        Assert.NotEqual(one, one with { Tdd = null });
        Assert.NotEqual(one, one with { Uat = null });
        Assert.NotEqual(one, one with { Prod = null });
        Assert.NotEqual(one, one with { Tdd = new WorkDeployment(null, TddAt) });
        Assert.NotEqual(one, one with { Tdd = new WorkDeployment(TddVersion, null) });
        Assert.NotEqual(one, one with { Tdd = new WorkDeployment(TddVersion, TddAt.AddHours(1)) });
        Assert.NotEqual(one, one with { Uat = new WorkDeployment("2.4.8", UatAt) });
        Assert.NotEqual(one, one with { Prod = new WorkDeployment(ProdVersion, ProdAt.AddSeconds(1)) });
        Assert.NotEqual(one, new WorkItemEnvironments(one.Uat, one.Prod, one.Tdd));
        Assert.NotEqual(WorkItemEnvironments.None, UnsaidPlaces);
        Assert.NotEqual(UnsaidPlaces, one);

        Assert.True(one != (one with { Uat = null }));
        Assert.False(one == (one with { Uat = null }));
        Assert.False(one.Equals((object)(one with { Prod = null })));
        Assert.False(one.Equals(one with { Tdd = null }));

        Assert.Equal(
            5,
            new HashSet<WorkItemEnvironments>
            {
                one,
                one with { Tdd = null },
                one with { Uat = null },
                one with { Prod = null },
                WorkItemEnvironments.None,
            }.Count);
    }

    /// <summary>The nothing on either side of the members every record is asked with: two environments that are no
    /// environment at all are one, an environment and no environment are two - the equality and the inequality each
    /// saying it, from either side - and the environment asked about itself says no to the nothing, to its own words as
    /// a name, to a bare number, to the anonymous thing that happens to carry its three parts, to the deployment that
    /// fills one of them and to the place the page makes of that deployment.</summary>
    [Fact]
    public void NoEnvironmentIsOneWithNoEnvironmentAndNotWithAnyOtherThing()
    {
        WorkItemEnvironments? nothing = null;
        WorkItemEnvironments? alsoNothing = null;
        var there = Full;

        Assert.Null(default(WorkItemEnvironments));
        Assert.True(nothing == alsoNothing);
        Assert.False(nothing != alsoNothing);
        Assert.False(nothing == there);
        Assert.True(nothing != there);
        Assert.False(there == (WorkItemEnvironments?)null);
        Assert.True(there != (WorkItemEnvironments?)null);
        Assert.True((WorkItemEnvironments?)null == nothing);
        Assert.False(there.Equals(nothing));
        Assert.False(there.Equals((object?)null));
        Assert.True(there.Equals(there));

        // An environment of no place at all is still an environment: what turns it into nothing to show is the page,
        // pinned in WorkItemsTextTests, and not the record.
        Assert.NotNull(WorkItemEnvironments.None);
        Assert.False(there.Equals(WorkItemEnvironments.None));
        Assert.False(there.Equals((WorkItemEnvironments?)null));

        Assert.False(there.Equals("WorkItemEnvironments { Tdd = , Uat = , Prod =  }"));
        Assert.False(there.Equals((object)455));
        Assert.False(there.Equals(new { Tdd = there.Tdd, Uat = there.Uat, Prod = there.Prod }));
        Assert.False(there.Equals(there.Tdd));
        Assert.False(there.Equals(new WorkPlace("tdd", TddVersion, TddAt)));
        Assert.False(there.Equals(UnsaidPlaces));
        Assert.Equal(2, new HashSet<WorkItemEnvironments?> { there, nothing, alsoNothing }.Count);
    }

    /// <summary>The hash says the three places and nothing else: the same parts hashed again are the same hash, a
    /// place turned is another hash, and the environments of nothing, of three empty places, of one place missing and
    /// of every place filled stand apart in a set of their own. One deployment stood in three places and three
    /// deployments that say the same thing hash alike, for the hash follows the parts and not the references.</summary>
    [Fact]
    public void TheHashFollowsThePartsAndStandsStillFromOneCallToTheNext()
    {
        var full = Full;
        var once = new WorkDeployment(TddVersion, TddAt);
        var thrice = new WorkItemEnvironments(once, once, once);

        Assert.Equal(full.GetHashCode(), Full.GetHashCode());
        Assert.Equal(full.GetHashCode(), ((object)full).GetHashCode());
        Assert.Equal(WorkItemEnvironments.None.GetHashCode(), new WorkItemEnvironments(null, null, null).GetHashCode());
        Assert.Equal(UnsaidPlaces.GetHashCode(), UnsaidPlaces.GetHashCode());
        Assert.Equal(
            thrice.GetHashCode(),
            new WorkItemEnvironments(
                new WorkDeployment(TddVersion, TddAt),
                new WorkDeployment(TddVersion, TddAt),
                new WorkDeployment(TddVersion, TddAt)).GetHashCode());
        Assert.NotEqual(full.GetHashCode(), WorkItemEnvironments.None.GetHashCode());
        Assert.NotEqual(WorkItemEnvironments.None.GetHashCode(), UnsaidPlaces.GetHashCode());
        Assert.NotEqual(full.GetHashCode(), (full with { Tdd = null }).GetHashCode());
        Assert.NotEqual(full.GetHashCode(), (full with { Uat = null }).GetHashCode());
        Assert.NotEqual(full.GetHashCode(), (full with { Prod = null }).GetHashCode());
        Assert.NotEqual(full.GetHashCode(), (full with { Prod = new WorkDeployment(ProdVersion, ProdAt.AddHours(1)) }).GetHashCode());
        Assert.Equal(4, new HashSet<WorkItemEnvironments> { full, UnsaidPlaces, full with { Prod = null }, WorkItemEnvironments.None }.Count);

        // The same moment told in another offset is the same hash: the words differ, the parts do not.
        Assert.Equal(
            new WorkItemEnvironments(new WorkDeployment(TddVersion, TddAt), null, null).GetHashCode(),
            new WorkItemEnvironments(new WorkDeployment(TddVersion, TddAt.ToOffset(TimeSpan.FromHours(2))), null, null).GetHashCode());
    }

    // ------------------------------------------------------------ the words the record says itself

    /// <summary>The words the environment says its three places in: its own name at the front, the places in the order
    /// the constructor names them, each empty place the empty space between the mark and the comma or the closing
    /// brace, and each filled place said in the words of the deployment inside it - for a nested record says itself,
    /// and this one has no words of its own to give it. These are the record's words and not the page's: no
    /// <c>Deployed to</c>, no <c>tdd 2.4.80</c>, no <c>·</c>, no <c> ago</c> and no dash - all of those are
    /// <see cref="WorkItemsText.Places" /> and <see cref="WorkItemsText.PlaceTitle" /> and are pinned there.</summary>
    [Fact]
    public void TheRecordSaysItsThreePlacesInTheOrderTheyAreNamed()
    {
        Assert.Equal("WorkItemEnvironments { Tdd = , Uat = , Prod =  }", Words(WorkItemEnvironments.None));
        Assert.Equal(
            "WorkItemEnvironments { Tdd = WorkDeployment { Version = , DeployedAt =  }, Uat = , Prod =  }",
            Words(new WorkItemEnvironments(new WorkDeployment(null, null), null, null)));
        Assert.Equal(
            "WorkItemEnvironments { Tdd = WorkDeployment { Version = 2.4.80, DeployedAt = 11/09/2026 16:00:00 +00:00 }, "
            + "Uat = WorkDeployment { Version = 2.4.81, DeployedAt = 11/09/2026 17:15:00 +00:00 }, "
            + "Prod = WorkDeployment { Version = 2.4.82, DeployedAt = 11/09/2026 18:30:00 +00:00 } }",
            Words(Full));
        Assert.Equal(
            "WorkItemEnvironments { Tdd = , Uat = , Prod = WorkDeployment { Version = 2.4.82, DeployedAt = 11/09/2026 18:30:00 +00:00 } }",
            Words(new WorkItemEnvironments(null, null, new WorkDeployment(ProdVersion, ProdAt))));

        // A moment is said with the offset it was written in, which is no part of what the environment means.
        Assert.Equal(
            "WorkItemEnvironments { Tdd = WorkDeployment { Version = 2.4.80, DeployedAt = 11/09/2026 20:30:00 +02:00 }, Uat = , Prod =  }",
            Words(new WorkItemEnvironments(new WorkDeployment(TddVersion, new DateTimeOffset(2026, 11, 9, 20, 30, 0, TimeSpan.FromHours(2))), null, null)));

        foreach (var word in new[] { "Deployed to", " as ", " ago", " · ", "—", "n/a", "<", "$" })
        {
            Assert.DoesNotContain(word, Words(Full), StringComparison.Ordinal);
        }

        Assert.NotEqual(Words(WorkItemEnvironments.None), Words(UnsaidPlaces));
        Assert.Contains("WorkItemEnvironments { Tdd = ", Words(Full), StringComparison.Ordinal);
    }

    /// <summary>The moments in the environment's own words follow whoever reads them, which is what sets them apart
    /// from the places themselves: a German reader of the record is told <c>09.11.2026 16:00:00 +00:00</c> where the
    /// invariant one is told <c>11/09/2026 16:00:00 +00:00</c>, while the names of the three places stay the same
    /// letters to whichever reader - and two environments the two readers see differently worded are one environment
    /// still, for the words are no part of the equality.</summary>
    [Fact]
    public void TheRecordsOwnWordsFollowTheReaderWhileThePartsThemselvesDoNot()
    {
        var environments = new WorkItemEnvironments(new WorkDeployment(TddVersion, TddAt), null, null);

        Assert.Equal(
            "WorkItemEnvironments { Tdd = WorkDeployment { Version = 2.4.80, DeployedAt = 11/09/2026 16:00:00 +00:00 }, Uat = , Prod =  }",
            Words(environments));

        ViewerCulture.German(() => Assert.Equal(
            "WorkItemEnvironments { Tdd = WorkDeployment { Version = 2.4.80, DeployedAt = 09.11.2026 16:00:00 +00:00 }, Uat = , Prod =  }",
            environments.ToString()));

        // The words apart by a slash, the environment the same: whoever reads the record decides how a moment is
        // written; the record only holds the three places, and hashes them the same whichever reader is asked.
        Assert.Equal(environments, new WorkItemEnvironments(new WorkDeployment(TddVersion, TddAt), null, null));
        ViewerCulture.German(() => Assert.Equal(
            environments.GetHashCode(),
            new WorkItemEnvironments(new WorkDeployment(TddVersion, TddAt), null, null).GetHashCode()));
    }

    // --------------------------------------------------------------------------- the machinery

    /// <summary>An environment of the three places, each filled with its own release and its own moment or left at
    /// nothing.</summary>
    private static WorkItemEnvironments Filled(bool tdd, bool uat, bool prod) =>
        new(
            tdd ? new WorkDeployment(TddVersion, TddAt) : null,
            uat ? new WorkDeployment(UatVersion, UatAt) : null,
            prod ? new WorkDeployment(ProdVersion, ProdAt) : null);

    /// <summary>One place of one filling: filled, it stands where it was put and says its own release and its own
    /// arrival; left out, it is the nothing it was given and not the release of the place beside it.</summary>
    private static void AssertPlace(WorkDeployment? place, bool filled, string version, DateTimeOffset at)
    {
        if (filled)
        {
            Assert.NotNull(place);
            Assert.Equal(version, place!.Version);
            Assert.Equal(at, place.DeployedAt);
        }
        else
        {
            Assert.Null(place);
        }
    }

    /// <summary>The eight ways of filling three places, in the order the places are named: none, tdd, uat, prod, and
    /// then the fillings of two, and of three.</summary>
    private static List<WorkItemEnvironments> EveryFilling()
    {
        var fillings = new List<WorkItemEnvironments>(8);
        for (var bits = 0; bits < 8; bits++)
        {
            fillings.Add(Filled((bits & 1) != 0, (bits & 2) != 0, (bits & 4) != 0));
        }

        return fillings;
    }

    /// <summary>Which of the three places an environment fills, told in the order the page says them: nothing at all,
    /// tdd, uat, prod, and the fillings of two and of three.</summary>
    private static string PlacesOf(WorkItemEnvironments environments)
    {
        ArgumentNullException.ThrowIfNull(environments);

        var names = new List<string>(3);
        if (environments.Tdd is not null)
        {
            names.Add("tdd");
        }

        if (environments.Uat is not null)
        {
            names.Add("uat");
        }

        if (environments.Prod is not null)
        {
            names.Add("prod");
        }

        return names.Count == 0 ? "none" : string.Join('+', names);
    }

    /// <summary>The public members the environment declares itself: its three places and its shared nothing, and the
    /// methods that are not the get and the set of one - the two operators kept although the compiler names them
    /// specially, since they are the <c>==</c> and the <c>!=</c> a caller writes - with the record's compiler-named
    /// clone walked past. The constructor is no method, so it is named by the test of the surface rather than by this
    /// list.</summary>
    private static List<string> PublicSurface() =>
        [.. typeof(WorkItemEnvironments)
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is PropertyInfo
                             || member is MethodInfo method
                             && (!method.IsSpecialName || method.Name.StartsWith("op_", StringComparison.Ordinal)))
            .Where(member => !member.Name.Contains('<', StringComparison.Ordinal))
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];

    /// <summary>The words an environment says its three places in, taken in the invariant culture whatever the reader
    /// of this test happens to be: the record's own words follow whoever reads them, which is pinned on its own
    /// further up this file, so the strings there are the point and the culture only the way to say them once.</summary>
    private static string Words(WorkItemEnvironments environments)
    {
        ArgumentNullException.ThrowIfNull(environments);

        var reader = Thread.CurrentThread.CurrentCulture;
        try
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            return environments.ToString()!;
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = reader;
        }
    }
}
