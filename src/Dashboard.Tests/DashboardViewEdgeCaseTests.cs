using System.Globalization;
using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of the two things <c>src/Dashboard/Runtime/ViewAddress.cs</c>
/// declares: the four <see cref="DashboardView" />s a page of the dashboard can be, and <see cref="ViewAddress" />,
/// the record that holds which of the four the page's hash asks for - with the environment it asks it for, the
/// runtime view's two zooms (into its database, into one deployable), and the filter of the work items view.
/// </summary>
/// <remarks>
/// <para>
/// What the words of a hash are worth is pinned where the hashes are read and written:
/// <see cref="ViewAddressTests" /> pins the view each word opens (health, runtime, cluster and work items, in either
/// case, with or without the <c>#</c> and the slashes that only frame a hash), the environment a runtime hash
/// carries, the database view a hash of one environment asks for, the hash each of those three writes back and the
/// round trip of every one of them; <see cref="ApplicationViewTests" /> pins the application view of a deployable -
/// the hash that opens it, the hostile deployable's name that names no view at all, the environment called
/// <c>app</c> and the database word that keeps its meaning beside it, and the hash an address with a deployable
/// writes; and <see cref="WorkAudienceEdgeCaseTests" />, <see cref="WorkItemsTextTests" /> and
/// <see cref="RuntimeTextHelperTests" /> pin the two vocabularies an address leans on for the rest of its parts:
/// the three filters <see cref="WorkAudience" /> holds and the words
/// <see cref="WorkItemsText.AudienceOf(string)" /> settles on them, and the deployable names
/// <see cref="ApplicationText.IsName(string)" /> takes.
/// </para>
/// <para>
/// What all of that leaves open is pinned here: the four views as the numbers, the names and the order they are
/// declared in, the view a value of the type that was never set stands on, and nothing outside the four; the three
/// addresses the page holds fixed and the parts they carry; the hash that says nothing, or nothing but separators,
/// or spaces at its ends; the environment's own spelling, and the markers that mean nothing outside the runtime
/// view; the one segment of a work items hash that is its filter; which parts of an address the hash it writes
/// keeps and which it leaves out; the environment whose name is the empty word, which is an environment still; the
/// view that is no view, which has no address; and the record underneath - the five parts it keeps exactly as it was
/// given them, the five that come out again in order, its equality, its hash, its words and the surface it declares.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: an address is a word the page put in
/// its bar and a word it takes back out of it, so every member of both types is answered here, and there is none
/// left to skip.
/// </para>
/// </remarks>
public class DashboardViewEdgeCaseTests
{
    /// <summary>The longest deployable name the address takes - 64 characters of letters and digits.</summary>
    private const string NameOf64 = "n123456789012345678901234567890123456789012345678901234567890123";

    /// <summary>One character past the longest deployable name the address takes.</summary>
    private const string NameOf65 = NameOf64 + "4";

    // ------------------------------------------------------------------ the four views, as a vocabulary

    /// <summary>
    /// The four views in the order the enum declares them, on the numbers 0 to 3, of an <see cref="int" />
    /// underneath, and a choice and not a set: the enum wears no <see cref="FlagsAttribute" /> and is sealed, so no
    /// view is built on another and none is a part of the ordering beyond the row it stands in.
    /// </summary>
    [Fact]
    public void TheFourViewsAreTheWholeVocabularyInTheirDeclaredOrder()
    {
        Assert.Equal(["Health", "Runtime", "Cluster", "WorkItems"], Enum.GetNames<DashboardView>());
        Assert.Equal([0, 1, 2, 3], Enum.GetValues<DashboardView>().Select(view => (int)view));
        Assert.Equal(4, Enum.GetValues<DashboardView>().Length);
        Assert.Equal(typeof(int), Enum.GetUnderlyingType(typeof(DashboardView)));
        Assert.False(typeof(DashboardView).IsDefined(typeof(FlagsAttribute), false));
    }

    /// <summary>
    /// Each view on its own number and in its own word, both ways round and in every format the enum answers to:
    /// the number is the one the name stands on, the word is the one that number answers to, and a view's hash code
    /// is its number, whatever the view.
    /// </summary>
    [Theory]
    [InlineData(DashboardView.Health, 0, "Health")]
    [InlineData(DashboardView.Runtime, 1, "Runtime")]
    [InlineData(DashboardView.Cluster, 2, "Cluster")]
    [InlineData(DashboardView.WorkItems, 3, "WorkItems")]
    public void EachViewStandsOnItsOwnNumberAndItsOwnWord(DashboardView view, int number, string name)
    {
        Assert.Equal(number, (int)view);
        Assert.Equal(view, (DashboardView)number);
        Assert.Equal(name, view.ToString());
        Assert.Equal(name, Enum.GetName(view));
        Assert.True(Enum.IsDefined(view));
        Assert.Equal(number, view.GetHashCode());

        Assert.Equal(name, view.ToString("G"));
        Assert.Equal(name, view.ToString("F"));
        Assert.Equal(name, view.ToString((string?)null));
        Assert.Equal(name, view.ToString(string.Empty));
        Assert.Equal(number.ToString(CultureInfo.InvariantCulture), view.ToString("D"));
        Assert.Equal(number.ToString("X8", CultureInfo.InvariantCulture), view.ToString("X"));
    }

    /// <summary>
    /// The first view is <see cref="DashboardView.Health" />: the view a value of the type that was never set
    /// stands on, which is the whole point of the page's default - a hash nobody wrote, or a view nobody chose, is
    /// the tiles of every environment. Outside the four there is nothing: neither the number past the last nor the
    /// one below the first nor either end of <see cref="int" /> is a defined view, and a value the enum does not
    /// name says the number it stands on.
    /// </summary>
    [Fact]
    public void TheUnsetViewIsHealthAndNothingOutsideTheFourIsAView()
    {
        Assert.Equal(DashboardView.Health, default);
        Assert.Equal(DashboardView.Health, (DashboardView)0);
        Assert.True(Enum.IsDefined(DashboardView.Health));
        Assert.True(Enum.IsDefined(DashboardView.WorkItems));

        Assert.False(Enum.IsDefined((DashboardView)4));
        Assert.False(Enum.IsDefined((DashboardView)(-1)));
        Assert.False(Enum.IsDefined((DashboardView)int.MinValue));
        Assert.False(Enum.IsDefined((DashboardView)int.MaxValue));

        Assert.Null(Enum.GetName((DashboardView)4));
        Assert.Null(Enum.GetName((DashboardView)(-1)));
        Assert.Equal("4", ((DashboardView)4).ToString());
        Assert.Equal("-1", ((DashboardView)(-1)).ToString());
        Assert.Equal("-2147483648", ((DashboardView)int.MinValue).ToString());
        Assert.Equal("2147483647", ((DashboardView)int.MaxValue).ToString());
    }

    /// <summary>
    /// The four views are apart from one another and sort by their numbers, as the declaration orders them - and a
    /// view the enum does not name sorts where its number sorts, which is past the last of the four. A boxed
    /// <see cref="int" /> on the same number is no view's equal, neither is a word, and the flag tests follow the
    /// numbers underneath: <see cref="DashboardView.Health" /> stands on 0, so it is a flag of every view.
    /// </summary>
    [Fact]
    public void TheViewsAreApartFromOneAnotherAndSortByTheirNumber()
    {
        Assert.True(DashboardView.Health < DashboardView.Runtime);
        Assert.True(DashboardView.Runtime < DashboardView.Cluster);
        Assert.True(DashboardView.Cluster < DashboardView.WorkItems);
        Assert.Equal(-1, DashboardView.Health.CompareTo(DashboardView.WorkItems));
        Assert.Equal(1, DashboardView.WorkItems.CompareTo(DashboardView.Cluster));
        Assert.Equal(0, DashboardView.Cluster.CompareTo((object)DashboardView.Cluster));
        Assert.Equal(1, DashboardView.Cluster.CompareTo(null));
        Assert.Equal(-1, DashboardView.Health.CompareTo((DashboardView)4));

        Assert.Throws<ArgumentException>(() => DashboardView.Cluster.CompareTo("Cluster"));

        Assert.False(DashboardView.Cluster.Equals(null));
        Assert.False(DashboardView.Cluster.Equals((object)2));
        Assert.True(DashboardView.Cluster.Equals((object)DashboardView.Cluster));

        Assert.Equal(4, Enum.GetValues<DashboardView>().Distinct().Count());
        Assert.Equal(DashboardView.Health, Enum.GetValues<DashboardView>().Min());
        Assert.Equal(DashboardView.WorkItems, Enum.GetValues<DashboardView>().Max());

        Assert.True(DashboardView.Runtime.HasFlag(DashboardView.Health));
        Assert.True(DashboardView.Health.HasFlag(DashboardView.Health));
        Assert.False(DashboardView.Runtime.HasFlag(DashboardView.Cluster));
        // The enum is a choice and not a set, so the operators work on the numbers underneath - and on these four
        // numbers the union of two views happens to land on a third, since 1 with 2 makes 3, which the enum names.
        // What is outside the four is the difference: one below the first, one past the last.
        Assert.Equal(DashboardView.WorkItems, DashboardView.Runtime | DashboardView.Cluster);
        Assert.True(Enum.IsDefined(DashboardView.Runtime | DashboardView.Cluster));
        Assert.Equal(DashboardView.Health, DashboardView.Runtime & DashboardView.Cluster);
        Assert.Equal(DashboardView.Health, DashboardView.Runtime - 1);
        Assert.False(Enum.IsDefined(DashboardView.Health - 1));
        Assert.False(Enum.IsDefined(DashboardView.WorkItems + 1));
    }

    /// <summary>
    /// A word that names a view in the enum's own case, with its surrounding spaces walked past, or holds its
    /// number is that view; a number with a cipher in front of it is still the small number it says. The word of the
    /// address for the work items view is <c>work-items</c> with a dash, which is no word of the enum: the enum's
    /// own spelling is what a parse reads, and the address is turned over by <see cref="ViewAddress.Parse(string)" />.
    /// </summary>
    [Theory]
    [InlineData("Health", DashboardView.Health)]
    [InlineData("Runtime", DashboardView.Runtime)]
    [InlineData("Cluster", DashboardView.Cluster)]
    [InlineData("WorkItems", DashboardView.WorkItems)]
    [InlineData("  Cluster  ", DashboardView.Cluster)]
    [InlineData("0", DashboardView.Health)]
    [InlineData("3", DashboardView.WorkItems)]
    [InlineData("01", DashboardView.Runtime)]
    public void AViewTextIsTheViewItNamesOrTheViewItNumbers(string text, DashboardView expected)
    {
        Assert.True(Enum.TryParse<DashboardView>(text, out var view));
        Assert.Equal(expected, view);
        Assert.True(Enum.IsDefined(view));
        Assert.Equal(expected, Enum.Parse<DashboardView>(text));
    }

    /// <summary>
    /// A view is named in the enum's own case and no other: a word in another case, a word of the address
    /// (<c>work-items</c>, <c>runtime</c>, which the enum spells without the dash), a word with something behind it,
    /// a plural, a fraction and a hexadecimal are refused - and a refused parse leaves its answer standing on
    /// <see cref="DashboardView.Health" />, the first of the four, so a caller that reads the value anyway is shown
    /// the default view rather than nothing.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("health")]
    [InlineData("RUNTIME")]
    [InlineData("Health!")]
    [InlineData("Healthy")]
    [InlineData("work-items")]
    [InlineData("Work Items")]
    [InlineData("1.0")]
    [InlineData("0x3")]
    public void TextThatIsNoViewIsRefusedAndLeavesHealthBehind(string? text)
    {
        Assert.False(Enum.TryParse<DashboardView>(text, out var view));
        Assert.Equal(DashboardView.Health, view);
    }

    /// <summary>The same refusals, said out loud by <see cref="Enum.Parse{TEnum}(string?)" />.</summary>
    [Fact]
    public void ParsingTextThatIsNoViewThrows()
    {
        Assert.Throws<ArgumentNullException>(() => Enum.Parse<DashboardView>(null!));
        Assert.Throws<ArgumentException>(() => Enum.Parse<DashboardView>(string.Empty));
        Assert.Throws<ArgumentException>(() => Enum.Parse<DashboardView>("   "));
        Assert.Throws<ArgumentException>(() => Enum.Parse<DashboardView>("runtime"));
    }

    /// <summary>A view's word in another case is a view as soon as the reader is told to forgive the case, and
    /// nothing until it is; a value outside the four is read as a view that is no view.</summary>
    [Fact]
    public void AViewIsNamedInTheEnumsOwnCaseAndForgivenWhenTheReaderAsks()
    {
        Assert.False(Enum.TryParse<DashboardView>("runtime", out _));
        Assert.True(Enum.TryParse<DashboardView>("runtime", ignoreCase: true, out var forgiven));
        Assert.Equal(DashboardView.Runtime, forgiven);
        Assert.True(Enum.TryParse<DashboardView>("WORKITEMS", ignoreCase: true, out var whole));
        Assert.Equal(DashboardView.WorkItems, whole);

        Assert.True(Enum.TryParse<DashboardView>("4", out var above));
        Assert.Equal((DashboardView)4, above);
        Assert.False(Enum.IsDefined(above));
        Assert.Null(Enum.GetName(above));

        Assert.True(Enum.TryParse<DashboardView>("-1", out var below));
        Assert.False(Enum.IsDefined(below));
    }

    /// <summary>
    /// The whole of what <see cref="DashboardView" /> declares itself, as the running assembly holds it: an
    /// <see cref="int" /> underneath, sealed, a value type and no set, and exactly its four views and nothing else -
    /// each one a public static literal of its own type, in the order the declaration writes them, on the number it
    /// stands on. No method and no property of its own, so a view carries nothing beyond the number it is and every
    /// word of it comes from <see cref="Enum" />.
    /// </summary>
    [Fact]
    public void TheEnumDeclaresItsFourViewsAndNothingElse()
    {
        var type = typeof(DashboardView);

        Assert.True(type.IsEnum);
        Assert.True(type.IsValueType);
        Assert.True(type.IsSealed);
        Assert.False(type.IsPrimitive);
        Assert.Equal(TypeCode.Int32, DashboardView.Health.GetTypeCode());
        Assert.False(type.IsDefined(typeof(FlagsAttribute), false));

        var instance = type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        Assert.Single(instance);
        Assert.Equal("value__", instance[0].Name);
        Assert.Equal(typeof(int), ((FieldInfo)instance[0]).FieldType);

        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

        Assert.Equal(["Health", "Runtime", "Cluster", "WorkItems"], fields.Select(field => field.Name));
        Assert.Equal([0, 1, 2, 3], fields.Select(field => (int)field.GetRawConstantValue()!));
        Assert.All(fields, field =>
        {
            Assert.Equal(type, field.FieldType);
            Assert.True(field.IsPublic);
            Assert.True(field.IsLiteral);
            Assert.False(field.IsInitOnly);
        });

        Assert.Empty(Surface(type));
        Assert.Equal(
            ["IComparable", "IConvertible", "IFormattable", "ISpanFormattable"],
            type.GetInterfaces().Select(iface => iface.Name).Order(StringComparer.Ordinal));
    }

    // ----------------------------------------------------------------- the three addresses the page holds

    /// <summary>
    /// The three addresses that need nothing beyond their view are the same three objects every time they are
    /// asked, and each of them says its own view and nothing else: no environment, no database, no deployable and
    /// the filter that holds the whole file. Each one is equal to a fresh record of the same five parts yet not the
    /// same record, so the page may hand an address about and compare it by what it says; and the three are apart
    /// from one another, so a set of them holds three views.
    /// </summary>
    [Fact]
    public void TheThreeAddressesThePageHoldsFixedSayTheirOwnViewAndNothingElse()
    {
        Assert.Same(ViewAddress.Health, ViewAddress.Health);
        Assert.Same(ViewAddress.Cluster, ViewAddress.Cluster);
        Assert.Same(ViewAddress.WorkItems, ViewAddress.WorkItems);

        Assert.Equal(DashboardView.Health, ViewAddress.Health.View);
        Assert.Equal(DashboardView.Cluster, ViewAddress.Cluster.View);
        Assert.Equal(DashboardView.WorkItems, ViewAddress.WorkItems.View);

        Assert.All(
            [ViewAddress.Health, ViewAddress.Cluster, ViewAddress.WorkItems],
            address =>
            {
                Assert.Null(address.Environment);
                Assert.False(address.Database);
                Assert.Null(address.Application);
                Assert.Equal(WorkAudience.All, address.Audience);
                Assert.Equal(new ViewAddress(address.View), address);
            });

        Assert.NotSame(ViewAddress.Health, new ViewAddress(DashboardView.Health));
        Assert.NotEqual(ViewAddress.Health, ViewAddress.Cluster);
        Assert.NotEqual(ViewAddress.Cluster, ViewAddress.WorkItems);
        Assert.Equal(3, new HashSet<ViewAddress> { ViewAddress.Health, ViewAddress.Cluster, ViewAddress.WorkItems }.Count);
    }

    // ------------------------------------------------------------------------ the hash that says nothing

    /// <summary>A hash that says nothing at all - no text, the empty word, a blank of spaces or of a tab, the
    /// <c>#</c> on its own or doubled, the separators that frame a hash with no word between them, and a dash that
    /// is no view's name - is the health view, with every part of the address left unsaid and its own hash the empty
    /// word: the page's bar ends at the <c>#</c> and nothing is asked for.</summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("  \t ")]
    [InlineData("#")]
    [InlineData("##")]
    [InlineData("/")]
    [InlineData("///")]
    [InlineData("#/")]
    [InlineData(" /# ")]
    [InlineData("-")]
    public void AHashThatSaysNothingOrNothingButSeparatorsIsTheHealthView(string? hash)
    {
        var address = ViewAddress.Parse(hash);

        Assert.Equal(ViewAddress.Health, address);
        Assert.Equal(DashboardView.Health, address.View);
        Assert.Null(address.Environment);
        Assert.False(address.Database);
        Assert.Null(address.Application);
        Assert.Equal(WorkAudience.All, address.Audience);
        Assert.Equal(string.Empty, address.ToHash());
    }

    /// <summary>The spaces at both ends of a hash, the <c>#</c> that opens it - however many of them the page's bar
    /// stacked - and the slashes that only frame it are walked past; a <c>#/</c> in front (the form a router writes
    /// when the address starts with a slash) is walked past too. What is inside is not walked past: a space in the
    /// middle of an environment's name is part of the name, a <c>#</c> behind the first is part of what follows, and
    /// a <c>#</c> where a filter is looked for names no filter.</summary>
    [Theory]
    [InlineData("  runtime/uat  ")]
    [InlineData("\nruntime/uat\r")]
    [InlineData("#runtime/uat")]
    [InlineData("##runtime/uat")]
    [InlineData(" #runtime/uat ")]
    [InlineData("runtime/uat//")]
    [InlineData("///runtime/uat///")]
    [InlineData("#/runtime/uat")]
    public void TheSpacesAndTheMarkersThatOnlyFrameAHashAreWalkedPast(string hash)
    {
        Assert.Equal(new ViewAddress(DashboardView.Runtime, "uat"), ViewAddress.Parse(hash));
    }

    /// <summary>What the trimming walks past is only at the ends: a space inside an environment's name is part of
    /// the name, a <c>#</c> behind the view's word is part of it too rather than the start of a fragment, and a
    /// word that a view's name merely begins is no view - the head of a hash is read whole.</summary>
    [Theory]
    [InlineData("runtime/u at", DashboardView.Runtime, "u at")]
    [InlineData("runtime/uat#frag", DashboardView.Runtime, "uat#frag")]
    [InlineData("runtime#uat", DashboardView.Health, null)]
    [InlineData("runtimes/uat", DashboardView.Health, null)]
    [InlineData("runtime-uat", DashboardView.Health, null)]
    public void AWordInsideAHashIsPartOfWhatItStandsIn(string hash, DashboardView view, string? environment)
    {
        Assert.Equal(new ViewAddress(view, environment), ViewAddress.Parse(hash));
    }

    /// <summary>An environment keeps its own spelling: the view's word is read in any case, the environment's is
    /// not, and a name only has to be a name - capitalised, accented, dotted or dashed, and long. The environment's
    /// name is asked for nothing, so a name the address would refuse of a deployable is an environment still.</summary>
    [Theory]
    [InlineData("runtime/UAT", "UAT")]
    [InlineData("runtime/Uat", "Uat")]
    [InlineData("runtime/café", "café")]
    [InlineData("runtime/uat-database", "uat-database")]
    [InlineData("runtime/uat.database", "uat.database")]
    [InlineData("runtime/uat%20x", "uat%20x")]
    [InlineData("runtime/uat x", "uat x")]
    public void AnEnvironmentKeepsItsOwnSpelling(string hash, string environment)
    {
        var address = ViewAddress.Parse(hash);

        Assert.Equal(new ViewAddress(DashboardView.Runtime, environment), address);
        Assert.Equal(hash, address.ToHash());
    }

    // ------------------------------------------------------------- the segments of a runtime hash, read apart

    /// <summary>A path that runs out of its segments names an environment and no view: the marker word of the
    /// application view with nothing behind it, and either marker word with something else behind it, are part of
    /// the environment's name rather than a zoom - the marker is the last separator of a runtime hash or nothing,
    /// and a name that only ends like a marker's word, with a dash where the slash should be, is a name still - and
    /// an environment's name the address reads with a slash in it is written back with it.</summary>
    [Theory]
    [InlineData("runtime/uat/app", "uat/app")]
    [InlineData("runtime/uat/app/", "uat/app")]
    [InlineData("runtime/uat/APP/ui/x", "uat/APP/ui/x")]
    [InlineData("runtime/uat/database/extra", "uat/database/extra")]
    [InlineData("runtime/uat/app/ui/x", "uat/app/ui/x")]
    [InlineData("runtime/uat-app/app", "uat-app/app")]
    public void APathThatRunsOutOfItsSegmentsNamesAnEnvironmentAndNoView(string hash, string environment)
    {
        var address = ViewAddress.Parse(hash);

        Assert.Equal(new ViewAddress(DashboardView.Runtime, environment), address);
        Assert.Equal(environment, address.Environment);
        Assert.False(address.Database);
        Assert.Null(address.Application);
        Assert.Equal(WorkAudience.All, address.Audience);
        Assert.Equal(hash.TrimEnd('/'), address.ToHash());
    }

    /// <summary>The environment of an address is everything before its marker, however many segments that is, and
    /// the deployable is the one word behind it: a slash before the marker collapses, an environment of one
    /// character is an environment (the marker has to sit behind something), a point is a name, and the database
    /// word in the middle of a path is part of the environment's name rather than a zoom - the zoom is the last
    /// segment or nothing.</summary>
    [Theory]
    [InlineData("runtime/a/app/ui", "a")]
    [InlineData("runtime/./app/ui", ".")]
    [InlineData("runtime/uat//app/ui", "uat")]
    [InlineData("runtime/uat/x/app/ui", "uat/x")]
    [InlineData("runtime/uat/database/app/ui", "uat/database")]
    [InlineData("runtime/uat/app/app/ui", "uat/app")]
    public void TheEnvironmentOfAnAddressIsEverythingBeforeItsMarker(string hash, string environment)
    {
        Assert.Equal(new ViewAddress(DashboardView.Runtime, environment, Application: "ui"), ViewAddress.Parse(hash));
    }

    /// <summary>An environment whose name is nothing but a space is no environment at all: the zoom behind it goes
    /// with it, so a hash that asks for the database, or for a deployable, of an environment nobody named is the
    /// runtime view with no environment - the same address an empty <c>runtime</c> hash opens, and one the page's
    /// own runtime tab keeps, since a name of spaces is nobody's environment.</summary>
    [Theory]
    [InlineData("runtime/ /database")]
    [InlineData("runtime/ /app/ui")]
    public void AnEnvironmentOfSpacesOnlyIsNoEnvironmentAndLosesItsZoom(string hash)
    {
        var address = ViewAddress.Parse(hash);

        Assert.Equal(new ViewAddress(DashboardView.Runtime), address);
        Assert.Equal(DashboardView.Runtime, address.View);
        Assert.Null(address.Environment);
        Assert.False(address.Database);
        Assert.Null(address.Application);
        Assert.Equal("runtime", address.ToHash());
    }

    /// <summary>The two words that mark the runtime view's zooms mean nothing outside it: the cluster's hash and
    /// the work items' hash keep their one view whoever follows them, with no environment, no database, no
    /// deployable and - for the work items view, whose only segment after its name is a filter - the filter that
    /// holds all the work.</summary>
    [Theory]
    [InlineData("cluster/database")]
    [InlineData("cluster/app/ui")]
    [InlineData("cluster/uat/database")]
    [InlineData("work-items/database")]
    [InlineData("work-items/app/ui")]
    [InlineData("work-items/uat")]
    public void TheWordsThatMarkTheRuntimeViewsZoomsMeanNothingOutsideIt(string hash)
    {
        var view = hash.StartsWith("cluster", StringComparison.Ordinal) ? DashboardView.Cluster : DashboardView.WorkItems;
        var address = ViewAddress.Parse(hash);

        Assert.Equal(new ViewAddress(view), address);
        Assert.Null(address.Environment);
        Assert.False(address.Database);
        Assert.Null(address.Application);
        Assert.Equal(WorkAudience.All, address.Audience);
    }

    /// <summary>The filter of the work items view is the one segment that follows its name, and nothing deeper:
    /// what stands behind that segment is walked past, an empty segment between the two is walked over, the word is
    /// read in any case and with its spaces trimmed by the file's own helper, and a word that is no filter - a
    /// <c>#</c> and a filter behind it, or an environment's name - is the filter that holds all the work.</summary>
    [Theory]
    [InlineData("work-items/bot/team", WorkAudience.Bot)]
    [InlineData("work-items//bot", WorkAudience.Bot)]
    [InlineData("work-items/bot/", WorkAudience.Bot)]
    [InlineData("work-items/BOT", WorkAudience.Bot)]
    [InlineData("work-items/ team ", WorkAudience.Team)]
    [InlineData("work-items/#bot", WorkAudience.All)]
    [InlineData("work-items/all", WorkAudience.All)]
    [InlineData("work-items/uat", WorkAudience.All)]
    public void TheFilterOfTheWorkItemsViewIsTheSegmentBehindItsName(string hash, WorkAudience audience)
    {
        var address = ViewAddress.Parse(hash);

        Assert.Equal(new ViewAddress(DashboardView.WorkItems, Audience: audience), address);
        Assert.Equal(audience, address.Audience);
        Assert.Null(address.Environment);
        Assert.Equal(DashboardView.WorkItems, address.View);
    }

    // ------------------------------------------------------------------- the hash an address writes back

    /// <summary>An address writes the word of its view and keeps only the parts that mean something in it: the
    /// health view writes the empty word and nothing else, whatever else it was given; the cluster's address is its
    /// one word; the runtime view writes its environment and its deployable and has no use for a filter; and the
    /// work items view writes its filter and has no use for an environment, a database or a deployable.</summary>
    [Theory]
    [InlineData(DashboardView.Health, "")]
    [InlineData(DashboardView.Runtime, "runtime/uat/app/ui")]
    [InlineData(DashboardView.Cluster, "cluster")]
    [InlineData(DashboardView.WorkItems, "work-items/bot")]
    public void AnAddressWritesTheWordOfItsViewAndKeepsOnlyThePartsThatMeanSomethingInIt(DashboardView view, string hash)
    {
        Assert.Equal(hash, Everything(view).ToHash());
    }

    /// <summary>A filter means nothing to the runtime view and an environment nothing to the work items view: the
    /// whole of the database address of an environment is its two words, whichever filter came along with it, and
    /// the work items view of a filter with an environment on it is the filter's own hash, the environment left
    /// out.</summary>
    [Fact]
    public void AFilterMeansNothingToTheRuntimeViewAndAnEnvironmentNothingToTheWorkItemsView()
    {
        Assert.Equal("runtime/uat/database", new ViewAddress(DashboardView.Runtime, "uat", Database: true, Audience: WorkAudience.Bot).ToHash());
        Assert.Equal("runtime/uat", new ViewAddress(DashboardView.Runtime, "uat", Audience: WorkAudience.Team).ToHash());
        Assert.Equal("work-items/team", new ViewAddress(DashboardView.WorkItems, "uat", Audience: WorkAudience.Team).ToHash());
        Assert.Equal("work-items", new ViewAddress(DashboardView.WorkItems, "uat", Audience: WorkAudience.All).ToHash());

        // A filter the three are not is no filter, and leaves the word of the view alone behind it.
        Assert.Equal("work-items", new ViewAddress(DashboardView.WorkItems, Audience: (WorkAudience)7).ToHash());
    }

    /// <summary>A deployable is written only when its name is one the address takes, and it is written before the
    /// database: an address that says both shows the one zoom the runtime view can show at a time, an address whose
    /// deployable's name the address refuses falls back to the database it also asked for, and a name of nothing, of
    /// a blank or of one character past the longest is no deployable at all. The name that is written keeps its own
    /// case, and holds the two marks nothing else of a hash may: the dash and the understore.</summary>
    [Fact]
    public void AnApplicationIsWrittenOnlyWhenItsNameIsOneTheAddressTakes()
    {
        Assert.Equal("runtime/uat/app/ui", new ViewAddress(DashboardView.Runtime, "uat", Database: true, Application: "ui").ToHash());
        Assert.Equal("runtime/uat/database", new ViewAddress(DashboardView.Runtime, "uat", Database: true, Application: "ui/x").ToHash());
        Assert.Equal("runtime/uat", new ViewAddress(DashboardView.Runtime, "uat", Application: "ui.server").ToHash());
        Assert.Equal("runtime/uat", new ViewAddress(DashboardView.Runtime, "uat", Application: string.Empty).ToHash());
        Assert.Equal("runtime/uat", new ViewAddress(DashboardView.Runtime, "uat", Application: " ").ToHash());
        Assert.Equal("runtime/uat", new ViewAddress(DashboardView.Runtime, "uat", Application: NameOf65).ToHash());
        Assert.Equal($"runtime/uat/app/{NameOf64}", new ViewAddress(DashboardView.Runtime, "uat", Application: NameOf64).ToHash());
        Assert.Equal("runtime/uat/app/UI", new ViewAddress(DashboardView.Runtime, "uat", Application: "UI").ToHash());
        Assert.Equal("runtime/uat/app/-_", new ViewAddress(DashboardView.Runtime, "uat", Application: "-_").ToHash());
    }

    /// <summary>The environment the page holds is an environment even when its name is the empty word or a blank:
    /// <see langword="null" /> is no environment and writes <c>runtime</c>, the empty word writes <c>runtime/</c>,
    /// and a blank writes the space it holds. The empty word is not nothing, and the hash it writes reads back as
    /// something else: <c>runtime/</c> opens the runtime view of nobody's environment, and the two double-slash
    /// hashes it writes open one environment called after the words that came after it - the database word, and the
    /// marker with its deployable. A record the page built by hand can therefore write a hash that opens another
    /// record, which is what the two ways round are for rather than the other way about.</summary>
    [Fact]
    public void AnEnvironmentThatIsTheEmptyWordIsAnEnvironmentStill()
    {
        Assert.Equal("runtime", new ViewAddress(DashboardView.Runtime).ToHash());
        Assert.Equal("runtime/", new ViewAddress(DashboardView.Runtime, string.Empty).ToHash());
        Assert.Equal("runtime/ ", new ViewAddress(DashboardView.Runtime, " ").ToHash());

        Assert.Equal(new ViewAddress(DashboardView.Runtime), ViewAddress.Parse(new ViewAddress(DashboardView.Runtime, string.Empty).ToHash()));
        Assert.Equal(new ViewAddress(DashboardView.Runtime), ViewAddress.Parse(new ViewAddress(DashboardView.Runtime, " ").ToHash()));
        Assert.Equal(new ViewAddress(DashboardView.Runtime, "database"), ViewAddress.Parse(new ViewAddress(DashboardView.Runtime, string.Empty, Database: true).ToHash()));
        Assert.Equal(new ViewAddress(DashboardView.Runtime, "app/ui"), ViewAddress.Parse(new ViewAddress(DashboardView.Runtime, string.Empty, Application: "ui").ToHash()));
    }

    /// <summary>A view that is no view has no address: the switch of the hash comes to the end it declares for
    /// values outside the four and says so, naming the parameter it refused and the value it was handed, whichever
    /// of the parts came with it and at either end of the count - and the refusal is of the view alone, since an
    /// undefined filter of the work items view is answered by the word of the view above.</summary>
    [Theory]
    [InlineData((DashboardView)4)]
    [InlineData((DashboardView)(-1))]
    [InlineData((DashboardView)100)]
    [InlineData((DashboardView)int.MinValue)]
    [InlineData((DashboardView)int.MaxValue)]
    public void AViewThatIsNoViewHasNoAddress(DashboardView view)
    {
        var error = Assert.Throws<ArgumentOutOfRangeException>(() => Everything(view).ToHash());

        Assert.Equal("View", error.ParamName);
        Assert.Equal(view, error.ActualValue);
        Assert.StartsWith("A view has no address.", error.Message, StringComparison.Ordinal);
    }

    /// <summary>The hash an address writes opens that same address again, and writes the same hash for whoever
    /// reads it back: whatever a page's bar holds - the empty word, a stack of separators, a zoom, a deployable, an
    /// environment with a slash in its name, a word inside a name, a hostile name the address refuses - the address
    /// it settles on is an address that survives a second trip through the two ways round, and the hash it writes
    /// has no <c>#</c> at its front (the page's own to write), never runs to a slash and never doubles one, so the
    /// bar that holds it is the bar the link asked for.</summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  \t ")]
    [InlineData("///")]
    [InlineData("runtime")]
    [InlineData("RUNTIME")]
    [InlineData("runtime/UAT")]
    [InlineData("runtime/u at")]
    [InlineData("runtime/uat#frag")]
    [InlineData("runtime/uat-database")]
    [InlineData("runtime/café")]
    [InlineData("runtime/database")]
    [InlineData("runtime/app")]
    [InlineData("runtime/app/database")]
    [InlineData("runtime/uat/app")]
    [InlineData("runtime/uat/x/app/ui")]
    [InlineData("runtime/uat/database/app/ui")]
    [InlineData("runtime/uat/app/database")]
    [InlineData("runtime/uat/app/ui<script>")]
    [InlineData("runtime/uat/app/aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
    [InlineData("cluster/database")]
    [InlineData("work-items/BOT")]
    [InlineData("work-items/bot/team")]
    [InlineData("work-items/#bot")]
    public void TheHashAnAddressWritesOpensThatAddressAgain(string? hash)
    {
        var once = ViewAddress.Parse(hash);
        var written = once.ToHash();
        var twice = ViewAddress.Parse(written);

        Assert.Equal(once, twice);
        Assert.Equal(written, twice.ToHash());
        Assert.False(written.StartsWith('#'));
        Assert.False(written.EndsWith('/'));
        Assert.False(written.Contains("//", StringComparison.Ordinal));
    }

    // ------------------------------------------------------------------- the record underneath the two ways

    /// <summary>An address keeps the five parts it is given and decides nothing about them: an environment of
    /// slashes, of spaces or of the empty word, a deployable's name the address would refuse, a filter the three
    /// are not and a view the four are not are all held as they were handed over, and a part the caller leaves out
    /// stands on the default the declaration writes - no environment, no database, no deployable and the filter
    /// that holds all the work. The record is the page's own; only the two ways round read anything into it.</summary>
    [Fact]
    public void AnAddressKeepsTheFivePartsItIsGivenAndDecidesNothingAboutThem()
    {
        var address = new ViewAddress(DashboardView.Cluster, "uat/x/ ", Database: true, Application: "../ui", Audience: (WorkAudience)7);

        Assert.Equal(DashboardView.Cluster, address.View);
        Assert.Equal("uat/x/ ", address.Environment);
        Assert.True(address.Database);
        Assert.Equal("../ui", address.Application);
        Assert.Equal((WorkAudience)7, address.Audience);

        var undefined = new ViewAddress((DashboardView)4, string.Empty, Application: string.Empty);

        Assert.Equal((DashboardView)4, undefined.View);
        Assert.Equal(string.Empty, undefined.Environment);
        Assert.False(undefined.Database);
        Assert.Equal(string.Empty, undefined.Application);
        Assert.Equal(WorkAudience.All, undefined.Audience);

        var onlyAView = new ViewAddress(DashboardView.Runtime);

        Assert.Null(onlyAView.Environment);
        Assert.False(onlyAView.Database);
        Assert.Null(onlyAView.Application);
        Assert.Equal(WorkAudience.All, onlyAView.Audience);
    }

    /// <summary>The five parts come out of an address in the order they went in - the view, the environment, the
    /// database, the deployable, the filter - whether they are asked for all at once or one after the other, and
    /// what came out goes back in the same way: the address they fall apart into is the address they were, of one
    /// hash and one entry of a set. The address the page holds fixed falls apart to its view and four nothings, and
    /// an address at the top of the count of names falls apart to it whole.</summary>
    [Fact]
    public void TheFivePartsComeOutOfAnAddressInTheOrderTheyWentIn()
    {
        var address = new ViewAddress(DashboardView.Runtime, "uat", Database: true, Application: NameOf64, Audience: WorkAudience.Team);

        var (view, environment, database, application, audience) = address;

        Assert.Equal(DashboardView.Runtime, view);
        Assert.Equal("uat", environment);
        Assert.True(database);
        Assert.Equal(NameOf64, application);
        Assert.Equal(WorkAudience.Team, audience);
        Assert.Equal(address, new ViewAddress(view, environment, database, application, audience));
        Assert.Equal(address.GetHashCode(), new ViewAddress(view, environment, database, application, audience).GetHashCode());

        address.Deconstruct(out var again, out var againEnvironment, out var againDatabase, out var againApplication, out var againAudience);

        Assert.Equal(address.View, again);
        Assert.Equal(address.Environment, againEnvironment);
        Assert.Equal(address.Database, againDatabase);
        Assert.Equal(address.Application, againApplication);
        Assert.Equal(address.Audience, againAudience);

        ViewAddress.Health.Deconstruct(out var health, out var noEnvironment, out var noDatabase, out var noApplication, out var allWork);

        Assert.Equal(DashboardView.Health, health);
        Assert.Null(noEnvironment);
        Assert.False(noDatabase);
        Assert.Null(noApplication);
        Assert.Equal(WorkAudience.All, allWork);
    }

    /// <summary>Two addresses are one address while their five parts are, whichever of the four ways of asking is
    /// used: the record's own <c>Equals</c>, the one of <see cref="object" />, <c>==</c> and <c>!=</c>. The hash
    /// follows the equality, boxed or unboxed and from one call to the next, the parts are compared rather than the
    /// calls that gathered them, an address read out of a hash is one with the record the same parts make, and a set
    /// of them holds one entry.</summary>
    [Fact]
    public void TwoAddressesAreOneAddressWhileTheirFivePartsAre()
    {
        var one = new ViewAddress(DashboardView.Runtime, "uat", Database: true, Application: "ui", Audience: WorkAudience.Bot);
        var two = new ViewAddress(DashboardView.Runtime, "uat", Database: true, Application: "ui", Audience: WorkAudience.Bot);

        Assert.True(one.Equals(two));
        Assert.True(one.Equals(one));
        Assert.True(one.Equals((object)two));
        Assert.True(one == two);
        Assert.True(two == one);
        Assert.False(one != two);
        Assert.Equal(one, two);
        Assert.NotSame(one, two);
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.Equal(one.GetHashCode(), ((object)one).GetHashCode());
        Assert.Equal(one.GetHashCode(), one.GetHashCode());
        Assert.Single(new HashSet<ViewAddress> { one, two });
        Assert.Contains(two, new HashSet<ViewAddress> { one });

        // The address a hash opens is one with the record of the same five parts; an address that says both zooms is
        // not one with the address its own hash writes, since the hash can only show the one zoom the page has.
        Assert.Equal(ViewAddress.Parse("runtime/uat/app/ui"), new ViewAddress(DashboardView.Runtime, "uat", Application: "ui"));
        Assert.NotEqual(one, ViewAddress.Parse(one.ToHash()));
        Assert.Equal(ViewAddress.Parse("runtime/uat/database"), new ViewAddress(DashboardView.Runtime, "uat", Database: true));
    }

    /// <summary>A difference in one part is another address: the view, the environment, the database, the
    /// deployable and the filter, each one turned on its own while the other four stay - and the environment turned
    /// from nothing to the empty word and from there to a blank, and the deployable turned from nothing to the empty
    /// word, which is a name the address refuses but a part of the record still. A set of addresses that differ in
    /// one part each holds them all apart.</summary>
    [Fact]
    public void AnAddressThatDiffersInOnePartIsAnotherAddress()
    {
        var one = new ViewAddress(DashboardView.Runtime, "uat");

        Assert.NotEqual(one, one with { View = DashboardView.Health });
        Assert.NotEqual(one, one with { View = (DashboardView)4 });
        Assert.NotEqual(one, one with { Environment = "prod" });
        Assert.NotEqual(one, one with { Environment = "UAT" });
        Assert.NotEqual(one, one with { Environment = string.Empty });
        Assert.NotEqual(one, one with { Environment = " " });
        Assert.NotEqual(one, one with { Database = true });
        Assert.NotEqual(one, one with { Application = "ui" });
        Assert.NotEqual(one, one with { Application = string.Empty });
        Assert.NotEqual(one, one with { Audience = WorkAudience.Bot });
        Assert.NotEqual(one, new ViewAddress(DashboardView.Runtime));

        // The empty environment, a blank one and none at all are three addresses, and three entries of a set.
        Assert.NotEqual(new ViewAddress(DashboardView.Runtime, string.Empty), new ViewAddress(DashboardView.Runtime, " "));
        Assert.NotEqual(new ViewAddress(DashboardView.Runtime, string.Empty), new ViewAddress(DashboardView.Runtime));

        ViewAddress[] apart =
        [
            one,
            one with { View = DashboardView.WorkItems },
            one with { Environment = "prod" },
            one with { Database = true },
            one with { Application = "ui" },
            one with { Audience = WorkAudience.Team },
        ];

        Assert.Equal(apart.Length, new HashSet<ViewAddress>(apart).Count);
    }

    /// <summary>No address is one with no address, and none is one with anything else: a null address on either
    /// side of <c>==</c> is refused by the operator the record was built with, the record's own <c>Equals</c> says
    /// the same, and a word, a view, a number or a view's hash code is no address's equal - while two addresses that
    /// are nothing at all are one with each other, which is how a page that has settled on no view compares.</summary>
    [Fact]
    public void NoAddressIsOneWithNoAddressOrWithAnythingElse()
    {
        var one = new ViewAddress(DashboardView.Runtime, "uat", Database: true);
        ViewAddress? nothing = null;
        ViewAddress? alsoNothing = null;

        Assert.False(one.Equals(nothing));
        Assert.False(one == nothing);
        Assert.False(nothing == one);
        Assert.True(one != nothing);
        Assert.True(nothing == alsoNothing);
        Assert.False(nothing != alsoNothing);
        Assert.False(one.Equals((object)"runtime/uat/database"));
        Assert.False(one.Equals((object)DashboardView.Runtime));
        Assert.False(one.Equals((object)DashboardView.Runtime.GetHashCode()));
        Assert.False(ViewAddress.Health.Equals(ViewAddress.Cluster));
    }

    /// <summary>An address says its five parts in the order the record declares them, in the words the record
    /// builds for itself: the view by its enum word, the two zooms by <c>True</c> and <c>False</c>, the environment
    /// and the deployable by their own text and, when they are nothing, by nothing at all - the empty space between
    /// the <c>=</c> and the comma, which is how a record says a part nobody filled in. The words are the same for
    /// whichever reader looks: an address holds no number to turn.</summary>
    [Fact]
    public void AnAddressSaysItsFivePartsInTheOrderTheRecordDeclaresThem()
    {
        Assert.Equal("ViewAddress { View = Health, Environment = , Database = False, Application = , Audience = All }", ViewAddress.Health.ToString());
        Assert.Equal("ViewAddress { View = Runtime, Environment = uat, Database = False, Application = , Audience = All }", new ViewAddress(DashboardView.Runtime, "uat").ToString());
        Assert.Equal("ViewAddress { View = Runtime, Environment = uat, Database = True, Application = ui, Audience = Team }", new ViewAddress(DashboardView.Runtime, "uat", Database: true, Application: "ui", Audience: WorkAudience.Team).ToString());
        Assert.Equal("ViewAddress { View = WorkItems, Environment = , Database = False, Application = , Audience = Bot }", new ViewAddress(DashboardView.WorkItems, Audience: WorkAudience.Bot).ToString());

        // The view that is no view says its number, and the part that is nothing says nothing.
        Assert.Equal("ViewAddress { View = 4, Environment = , Database = False, Application = , Audience = All }", new ViewAddress((DashboardView)4).ToString());

        ViewerCulture.German(() =>
        {
            Assert.Equal("ViewAddress { View = Health, Environment = , Database = False, Application = , Audience = All }", ViewAddress.Health.ToString());
            Assert.Equal("ViewAddress { View = Runtime, Environment = uat, Database = True, Application = ui, Audience = Team }", new ViewAddress(DashboardView.Runtime, "uat", Database: true, Application: "ui", Audience: WorkAudience.Team).ToString());
        });
    }

    /// <summary>The address's whole public surface, as the running assembly holds it: its five parts, named in the
    /// order the constructor names them and of the type the constructor says, each read and the place a
    /// <c>with</c> may turn it; the three addresses the page holds fixed, of its own type and read-only; the two
    /// ways round - <see cref="ViewAddress.Parse(string)" /> and <see cref="ViewAddress.ToHash" /> - and nothing
    /// else to call but the members every record is asked (falling apart, comparing, hashing and saying itself) and
    /// the two operators. One constructor of the five parts, four of them optional, and the equality of a record
    /// with no rule of its own: a sixth part added here has to be written into this list as well, which is the
    /// point.</summary>
    [Fact]
    public void TheAddressDeclaresFivePartsThreeFixedAddressesAndTheTwoWaysRound()
    {
        var type = typeof(ViewAddress);

        Assert.True(type.IsSealed);
        Assert.False(type.IsValueType);
        Assert.Equal([typeof(IEquatable<ViewAddress>)], type.GetInterfaces());
        Assert.Equal(
            ["Application", "Audience", "Cluster", "Database", "Deconstruct", "Environment", "Equals", "GetHashCode", "Health", "Parse", "ToHash", "ToString", "View", "WorkItems", "op_Equality", "op_Inequality"],
            Surface(type));

        Assert.Equal(
            ["View", "Environment", "Database", "Application", "Audience"],
            [.. type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Select(property => property.Name)]);

        var properties = type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .ToDictionary(property => property.Name, StringComparer.Ordinal);

        Assert.Equal(typeof(DashboardView), properties["View"].PropertyType);
        Assert.Equal(typeof(string), properties["Environment"].PropertyType);
        Assert.Equal(typeof(bool), properties["Database"].PropertyType);
        Assert.Equal(typeof(string), properties["Application"].PropertyType);
        Assert.Equal(typeof(WorkAudience), properties["Audience"].PropertyType);
        Assert.All(properties.Values, property =>
        {
            Assert.True(property.CanRead);
            Assert.True(property.CanWrite);
        });

        Assert.Equal(
            ["Health", "Cluster", "WorkItems"],
            [.. type.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).Select(property => property.Name)]);
        Assert.All(
            type.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly),
            property =>
            {
                Assert.Equal(type, property.PropertyType);
                Assert.True(property.CanRead);
                Assert.False(property.CanWrite);
            });

        var constructors = type.GetConstructors();

        Assert.Single(constructors);

        var parameters = constructors[0].GetParameters();

        Assert.Equal(["View", "Environment", "Database", "Application", "Audience"], parameters.Select(parameter => parameter.Name!));
        Assert.Equal(
            [typeof(DashboardView), typeof(string), typeof(bool), typeof(string), typeof(WorkAudience)],
            parameters.Select(parameter => parameter.ParameterType));
        Assert.False(parameters[0].HasDefaultValue);
        Assert.True(parameters[1].HasDefaultValue);
        Assert.True(parameters[2].HasDefaultValue);
        Assert.True(parameters[3].HasDefaultValue);
        Assert.True(parameters[4].HasDefaultValue);
        Assert.Equal(false, parameters[2].DefaultValue);
        Assert.Equal(WorkAudience.All, parameters[4].DefaultValue);
        Assert.Null(parameters[1].DefaultValue);
        Assert.Null(parameters[3].DefaultValue);

        // The two ways round, and the one of them that answers any text at all rather than refusing it.
        var parse = type.GetMethod(nameof(ViewAddress.Parse))!;

        Assert.True(parse.IsStatic);
        Assert.Equal(type, parse.ReturnType);
        Assert.Single(parse.GetParameters());
        Assert.Equal(typeof(string), parse.GetParameters()[0].ParameterType);
        Assert.False(parse.GetParameters()[0].HasDefaultValue);

        var toHash = type.GetMethod(nameof(ViewAddress.ToHash))!;

        Assert.False(toHash.IsStatic);
        Assert.Equal(typeof(string), toHash.ReturnType);
        Assert.Empty(toHash.GetParameters());
    }

    // ------------------------------------------------------------------- the helpers the rest of the file leans on

    /// <summary>An address with every part said: an environment, both of the runtime view's two zooms at once, and
    /// the work items filter the runtime view has no use for.</summary>
    private static ViewAddress Everything(DashboardView view) =>
        new(view, "uat", Database: true, Application: "ui", Audience: WorkAudience.Bot);

    /// <summary>The public members a type declares itself: its properties and the methods that are not the get and
    /// the set of one - the two operators kept although the compiler names them specially, since they are the
    /// <c>==</c> and the <c>!=</c> a caller writes - with the record's compiler-named clone and backing members
    /// walked past.</summary>
    private static List<string> Surface(Type type) =>
        [.. type
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is PropertyInfo
                             || member is MethodInfo method
                             && (!method.IsSpecialName || method.Name.StartsWith("op_", StringComparison.Ordinal)))
            .Where(member => !member.Name.Contains('<', StringComparison.Ordinal))
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];
}
