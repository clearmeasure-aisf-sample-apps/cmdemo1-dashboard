using System.Globalization;
using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="CostEntry" />: one environment of a cost report - or
/// what the environments share - with its three sums, the services that cost most and the estimate of its part of
/// what they share. It is the unit every other cost type is built of: a report's entries, the shared one found by
/// its name, and the others the page shows.
/// <para>
/// What the file makes of it is already pinned where the reading happens: <see cref="CostReportTests" /> reads a
/// hand-written <c>cost.json</c> and says that an entry Azure gave no number to has no amounts and no services, that
/// an element without a name is no entry, and that the shared entry is found by its name without a care for case;
/// <see cref="CostReportEdgeCaseTests" /> pins that same reading at its ends - the name trimmed of its whitespace,
/// the <c>topServices</c> of a kind that is no array, the estimate's share on its two ends - and says that an entry
/// built without an estimate has none, which is why the estimate the entry does without is not pinned here again;
/// and <see cref="CostTests" /> pins the entries the sample file names and the words the page says of their parts.
/// <see cref="CostAmountsEdgeCaseTests" /> pins the record the entry's sums stand on, at its ends and in its words.
/// What is pinned here is the record itself, which all of that walks past: the constructor handing each of its four
/// arguments to the property of its name, the name kept exactly as it is given - empty, in its whitespace and its
/// case, which no part of the record turns - the amounts, the services and the estimate held as the very instances
/// the entry was built of, at the ends the number runs to; the four parts coming back out in the order they went
/// in; two entries being one while their parts are, the name compared in its case unlike the finding that is not,
/// and the services compared as the list they stand in rather than as their content; the copy that turns one part
/// and keeps the other three; the words the record says its parts in, following whoever reads them; and the public
/// surface, so that the day a fifth part is added the list below says so.
/// <para>
/// Nothing here reaches the network or asks a clock what time it is: the record holds a name, three sums, a list of
/// services and an estimate, no moment and no address, and calls nothing at all - every value below is one the test
/// brought. The members that would need a <c>now</c> are the page's words, and they are pinned on those words where
/// they are pinned.
/// </para>
/// </summary>
public class CostEntryEdgeCaseTests
{
    private static CostAmounts Amounts(double? yesterday, double? last7Days, double? monthToDate) =>
        new(yesterday, last7Days, monthToDate);

    private static CostService Service(string name, double? monthToDate) => new(name, monthToDate);

    // ----- the constructor, handing each argument to the property of its name

    /// <summary>The constructor's four arguments, position by position, come back as the property of the same name:
    /// the environment the entry is of, the three sums of its days, the services that cost most, and the estimate of
    /// its part of what the environments share. The entry holds the very records and the very list it was built of:
    /// a new entry of the same parts is another entry, and the one it was built of stands where it stood.</summary>
    [Fact]
    public void TheConstructorHandsEachArgumentToThePropertyOfItsName()
    {
        var amounts = Amounts(1.52, 9.8, 11.02);
        var services = new List<CostService> { Service("SQL Database", 0.97), Service("Azure Container Apps", 0.62) };
        var estimate = new CostEstimate(0.2048, Amounts(1.17, 2.69, 2.69));
        var entry = new CostEntry("uat", amounts, services, estimate);

        Assert.Equal("uat", entry.Name);
        Assert.Same(amounts, entry.Amounts);
        Assert.Equal(1.52, entry.Amounts.Yesterday);
        Assert.Equal(9.8, entry.Amounts.Last7Days);
        Assert.Equal(11.02, entry.Amounts.MonthToDate);
        Assert.Same(services, entry.TopServices);
        Assert.Equal([Service("SQL Database", 0.97), Service("Azure Container Apps", 0.62)], entry.TopServices);
        Assert.Same(estimate, entry.Estimate);
        Assert.Equal(0.2048, entry.Estimate!.Share);
        Assert.Equal(Amounts(1.17, 2.69, 2.69), entry.Estimate.Amounts);

        // A new entry of the same four parts is another entry, not the one it was built of: the parts are compared
        // where they are compared, and the entry itself is the place they stand in.
        var again = new CostEntry("uat", amounts, services, estimate);
        Assert.Equal(entry, again);
        Assert.NotSame(entry, again);
    }

    // ----- the name, kept exactly as it is given

    /// <summary>The name is kept in its exact letters: the empty name and the name that is only whitespace stand
    /// where they stand, the whitespace around a name is not trimmed and the case is not turned. The trimming is the
    /// file's reading, pinned where the reading happens, and the name the report is asked for is found without a
    /// care for case, which is the finding and not the name - the entry the record is built of says the name the
    /// builder gave it, nothing more and nothing less.</summary>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("  uat  ")]
    [InlineData("UAT")]
    [InlineData("uAt")]
    public void TheNameIsKeptExactlyAsItIsGiven(string name) =>
        Assert.Equal(name, new CostEntry(name, Amounts(0, 0, 0), []).Name);

    // ----- the amounts, held as they are given

    /// <summary>The entry holds the three sums it was given, as the very record they stand in, at the ends the
    /// number runs to: the zero and a refund below it, the largest there are and the least, and the sum that is no
    /// number at all. The day Azure did not bill stands unsaid, which is not a day at zero: the entry with the three
    /// unsaid sums is an entry, and it is no entry that says zero for them.</summary>
    [Fact]
    public void TheEntryHoldsTheAmountsItIsGivenAndNotAnother()
    {
        var amounts = Amounts(1.52, 9.8, 11.02);
        var entry = new CostEntry("uat", amounts, []);

        Assert.Same(amounts, entry.Amounts);

        var unsaid = new CostEntry("ghost", Amounts(null, null, null), []);
        Assert.Null(unsaid.Amounts.Yesterday);
        Assert.Null(unsaid.Amounts.Last7Days);
        Assert.Null(unsaid.Amounts.MonthToDate);
        Assert.NotEqual(new CostEntry("ghost", Amounts(0, 0, 0), unsaid.TopServices), unsaid);

        var refund = new CostEntry("refund", Amounts(-0.3, null, -1204.8), []);
        Assert.Equal(-0.3, refund.Amounts.Yesterday);
        Assert.Null(refund.Amounts.Last7Days);
        Assert.Equal(-1204.8, refund.Amounts.MonthToDate);

        var large = new CostEntry("large", Amounts(double.MinValue, 1e300, double.MaxValue), []);
        Assert.Equal(double.MinValue, large.Amounts.Yesterday);
        Assert.Equal(1e300, large.Amounts.Last7Days);
        Assert.Equal(double.MaxValue, large.Amounts.MonthToDate);

        var notANumber = new CostEntry("notanumber", Amounts(double.NaN, null, null), []);
        Assert.True(double.IsNaN(notANumber.Amounts.Yesterday!.Value));
    }

    // ----- the services, held as they are given

    /// <summary>The entry holds its services in the order of the list it was given, and the list it was given: the
    /// element the entry is shown is the element the list holds, the amount a service is given stands where it is
    /// given - with its sign - and the amount it is not given is the amount Azure did not give. The entry does not
    /// copy the list and does not order it: the most expensive first is the file's reading, pinned where the
    /// reading happens.</summary>
    [Fact]
    public void TheEntryHoldsItsServicesInTheOrderOfTheList()
    {
        var services = new List<CostService> { Service("SQL Database", 0.97), Service("Azure Container Apps", 0.62) };
        var entry = new CostEntry("uat", Amounts(1.52, 9.8, 11.02), services);

        Assert.Same(services, entry.TopServices);
        Assert.Equal(2, entry.TopServices.Count);
        Assert.Same(services[0], entry.TopServices[0]);
        Assert.Same(services[1], entry.TopServices[1]);
        Assert.Equal("SQL Database", entry.TopServices[0].Name);
        Assert.Equal(0.97, entry.TopServices[0].MonthToDate);
        Assert.Equal("Azure Container Apps", entry.TopServices[1].Name);
        Assert.Equal(0.62, entry.TopServices[1].MonthToDate);
    }

    /// <summary>The month a service cost stands where it stands, at the ends the number runs to: the amount Azure
    /// did not give, the zero, a refund below it, the whole of a month's bill and the largest there are.</summary>
    [Fact]
    public void TheServicesStandAtTheEndsTheNumberRunsTo()
    {
        var services = new List<CostService>
        {
            Service("Azure Front Door Service", null),
            Service("Idle", 0),
            Service("Refund", -0.3),
            Service("Whole", 1204.8),
            Service("Endless", double.MaxValue),
        };
        var entry = new CostEntry("shared", Amounts(5.85, 13.44, 13.44), services);

        Assert.Null(entry.TopServices[0].MonthToDate);
        Assert.Equal(0d, entry.TopServices[1].MonthToDate);
        Assert.Equal(-0.3, entry.TopServices[2].MonthToDate);
        Assert.Equal(1204.8, entry.TopServices[3].MonthToDate);
        Assert.Equal(double.MaxValue, entry.TopServices[4].MonthToDate);
    }

    /// <summary>The entry without services is the entry with the empty list: the list is the place the services
    /// stand in, and the entry the list is none of keeps that place empty rather than turning it to another.</summary>
    [Fact]
    public void TheEntryWithoutServicesIsTheEntryWithTheEmptyList()
    {
        var none = new CostEntry("ghost", Amounts(null, null, null), Array.Empty<CostService>());
        var alsoNone = new CostEntry("ghost", Amounts(null, null, null), Array.Empty<CostService>());

        Assert.Empty(none.TopServices);
        Assert.Same(Array.Empty<CostService>(), none.TopServices);
        // The empty list is the very instance both entries hold, so the two entries are one.
        Assert.Equal(none, alsoNone);
    }

    // ----- the estimate, held as it is given

    /// <summary>The entry holds the estimate it was given, as the very record it stands in: the share of what all
    /// pods request and the sums of the part it is estimated to carry. The share the estimate does not say stands
    /// unsaid, which is not the share it says of zero - the one is the part Azure did not give and the other the
    /// part it gave of nothing.</summary>
    [Fact]
    public void TheEntryHoldsTheEstimateItIsGiven()
    {
        var estimate = new CostEstimate(0.2048, Amounts(1.17, 2.69, 2.69));
        var entry = new CostEntry("tdd", Amounts(0, 0, 0), [], estimate);

        Assert.Same(estimate, entry.Estimate);
        Assert.Equal(0.2048, entry.Estimate!.Share);
        Assert.Equal(Amounts(1.17, 2.69, 2.69), entry.Estimate.Amounts);

        var noShare = new CostEntry("uat", Amounts(0.4, null, 0.4), [], new CostEstimate(null, Amounts(null, null, 1.5)));
        Assert.Null(noShare.Estimate!.Share);
        Assert.Equal(Amounts(null, null, 1.5), noShare.Estimate.Amounts);
    }

    // ----- the four parts coming out again, in the order they went in

    /// <summary>The four parts of the entry come back out in the order they went in - the name, the sums, the
    /// services and the estimate - as the very instances they were, and what came out goes back in the same way:
    /// the entry the four parts fall apart into is the entry they were. The entry without an estimate falls apart
    /// to the estimate that is none, and the one without services to the list that is empty.</summary>
    [Fact]
    public void TheEntryFallsApartInTheOrderItWasMade()
    {
        var amounts = Amounts(1.52, 9.8, 11.02);
        var services = new List<CostService> { Service("SQL Database", 0.97) };
        var estimate = new CostEstimate(0.2048, Amounts(1.17, 2.69, 2.69));
        var entry = new CostEntry("uat", amounts, services, estimate);

        var (name, amountsOut, servicesOut, estimateOut) = entry;

        Assert.Equal("uat", name);
        Assert.Same(amounts, amountsOut);
        Assert.Same(services, servicesOut);
        Assert.Same(estimate, estimateOut);
        Assert.Equal(entry, new CostEntry(name, amountsOut, servicesOut, estimateOut));

        var (noName, noAmounts, noServices, noEstimate) = new CostEntry("ghost", Amounts(null, null, null), Array.Empty<CostService>());
        Assert.Equal("ghost", noName);
        Assert.Equal(Amounts(null, null, null), noAmounts);
        Assert.Empty(noServices);
        Assert.Null(noEstimate);
    }

    // ----- the equality, the hash, the two

    /// <summary>Two entries are one while their four parts are - the name as the letters it is, the sums as the
    /// three sums they stand on in whatever record they stand in, the services as the very list the two entries
    /// hold, and the estimate as the share and the sums it says in whatever record it stands in - whichever of the
    /// four ways of asking is used. The hash follows the equality, boxed or unboxed and from one call to the next,
    /// so the entries that are one are one entry of a set.</summary>
    [Fact]
    public void TwoEntriesAreOneWhileTheirFourPartsAre()
    {
        var services = new List<CostService> { Service("SQL Database", 0.97) };
        var one = new CostEntry("uat", Amounts(1.52, 9.8, 11.02), services, new CostEstimate(0.2048, Amounts(1.17, 2.69, 2.69)));
        var two = new CostEntry("uat", Amounts(1.52, 9.8, 11.02), services, new CostEstimate(0.2048, Amounts(1.17, 2.69, 2.69)));

        Assert.True(one.Equals(two));
        Assert.True(one.Equals((object)two));
        Assert.True(one == two);
        Assert.True(two == one);
        Assert.False(one != two);
        Assert.Equal(one, two);
        Assert.NotSame(one, two);
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.Equal(one.GetHashCode(), ((object)one).GetHashCode());
        Assert.Equal(one.GetHashCode(), one.GetHashCode());
        Assert.Single(new HashSet<CostEntry> { one, two });
    }

    /// <summary>The estimate is the part the entry may do without: the entry built without it is the entry built
    /// with the one that is none, one with one hash, and it is no entry that was given an estimate at all, however
    /// small the part of what is shared it is estimated to carry.</summary>
    [Fact]
    public void TheEntryWithoutAnEstimateIsTheOneItsEstimateIsNoneOn()
    {
        var services = new List<CostService> { Service("SQL Database", 0.97) };
        var without = new CostEntry("uat", Amounts(1.52, 9.8, 11.02), services);
        var withNone = new CostEntry("uat", Amounts(1.52, 9.8, 11.02), services, null);
        var withOne = new CostEntry("uat", Amounts(1.52, 9.8, 11.02), services, new CostEstimate(0.2048, Amounts(1.17, 2.69, 2.69)));

        Assert.Null(without.Estimate);
        Assert.Equal(without, withNone);
        Assert.True(without == withNone);
        Assert.False(without != withNone);
        Assert.Equal(without.GetHashCode(), withNone.GetHashCode());
        Assert.NotEqual(without, withOne);
    }

    /// <summary>A difference in one part is another entry, and so is the same part in another place: the name in
    /// its case, which the finding of an entry is not, a sum turned by a cent and the sum the file never gave
    /// turned to the zero it is not, the same two services in another list - the services are compared as the list
    /// they stand in, not as their content - and the estimate with another share. A set of the entries that differ
    /// in one part each holds them all apart.</summary>
    [Fact]
    public void AnEntryThatDiffersInOnePartIsAnotherEntry()
    {
        var services = new List<CostService> { Service("SQL Database", 0.97) };
        var estimate = new CostEstimate(0.2048, Amounts(1.17, 2.69, 2.69));
        var one = new CostEntry("uat", Amounts(1.52, 9.8, 11.02), services, estimate);

        Assert.NotEqual(one, new CostEntry("UAT", Amounts(1.52, 9.8, 11.02), services, estimate));
        Assert.NotEqual(one, new CostEntry("uat", Amounts(1.53, 9.8, 11.02), services, estimate));
        Assert.NotEqual(one, new CostEntry("uat", Amounts(null, 9.8, 11.02), services, estimate));
        Assert.NotEqual(one, new CostEntry("uat", Amounts(1.52, 9.8, 11.02), new List<CostService> { Service("SQL Database", 0.97) }, estimate));
        Assert.NotEqual(one, new CostEntry("uat", Amounts(1.52, 9.8, 11.02), services, new CostEstimate(0.4, Amounts(1.17, 2.69, 2.69))));
        Assert.NotEqual(one, new CostEntry("uat", Amounts(1.52, 9.8, 11.02), services, null));

        Assert.Equal(
            4,
            new HashSet<CostEntry>
            {
                one,
                new CostEntry("UAT", Amounts(1.52, 9.8, 11.02), services, estimate),
                new CostEntry("uat", Amounts(1.53, 9.8, 11.02), services, estimate),
                new CostEntry("uat", Amounts(1.52, 9.8, 11.02), new List<CostService> { Service("SQL Database", 0.97) }, estimate),
            }.Count);
    }

    /// <summary>The nothing on either side of the members every record is asked with: two entries that are no
    /// entry at all are one, an entry and no entry are two - the equality and the inequality each saying it, from
    /// either side - and the entry asked about itself says no to the things it is not: a name, the sums, a service,
    /// an estimate and the anonymous thing that happens to carry two of its parts.</summary>
    [Fact]
    public void NoEntryIsOneWithNoEntryAndNotWithAnyOtherThing()
    {
        CostEntry? nothing = null;
        CostEntry? alsoNothing = null;
        var there = new CostEntry("uat", Amounts(1.52, 9.8, 11.02), []);

        Assert.Null(default(CostEntry));
        Assert.True(nothing == alsoNothing);
        Assert.False(nothing != alsoNothing);
        Assert.False(nothing == there);
        Assert.True(nothing != there);
        Assert.False(there == (CostEntry?)null);
        Assert.True(there != (CostEntry?)null);
        Assert.False(there.Equals(nothing));
        Assert.False(there.Equals((object?)null));
        Assert.True(there.Equals(there));

        Assert.False(there.Equals("CostEntry { Name = uat, Amounts = , TopServices = , Estimate =  }"));
        Assert.False(there.Equals(Amounts(1.52, 9.8, 11.02)));
        Assert.False(there.Equals(Service("SQL Database", 0.97)));
        Assert.False(there.Equals(new CostEstimate(0.2048, Amounts(1.17, 2.69, 2.69))));
        Assert.False(there.Equals(new { Name = "uat", Amounts = Amounts(1.52, 9.8, 11.02) }));
    }

    // ----- the copy, turning the part it names

    /// <summary>Each part may be turned as the entry is copied - the name to another name, the estimate to the one
    /// that is none, the services to another list - and stands where it is turned to, while the parts the turn does
    /// not name stay the very instances the entry was built of: the copy is another entry and never the same
    /// reference, and the original stands as it stood.</summary>
    [Fact]
    public void ACopyTurnsThePartItNamesAndKeepsTheOtherThree()
    {
        var amounts = Amounts(1.52, 9.8, 11.02);
        var services = new List<CostService> { Service("SQL Database", 0.97) };
        var estimate = new CostEstimate(0.2048, Amounts(1.17, 2.69, 2.69));
        var entry = new CostEntry("uat", amounts, services, estimate);

        var renamed = entry with { Name = "tdd" };
        Assert.Equal("tdd", renamed.Name);
        Assert.Same(amounts, renamed.Amounts);
        Assert.Same(services, renamed.TopServices);
        Assert.Same(estimate, renamed.Estimate);
        Assert.NotSame(entry, renamed);
        Assert.NotEqual(entry, renamed);

        var estimateTurnedOff = entry with { Estimate = null };
        Assert.Null(estimateTurnedOff.Estimate);
        Assert.Same(amounts, estimateTurnedOff.Amounts);
        Assert.Same(services, estimateTurnedOff.TopServices);
        Assert.Equal(new CostEntry("uat", amounts, services), estimateTurnedOff);

        var servicesTurned = entry with { TopServices = new List<CostService> { Service("SQL Database", 0.97) } };
        Assert.NotSame(services, servicesTurned.TopServices);
        Assert.NotEqual(entry, servicesTurned);

        var unchanged = entry with { };
        Assert.Equal(entry, unchanged);
        Assert.NotSame(entry, unchanged);
        Assert.Equal("uat", entry.Name);
        Assert.Equal(Amounts(1.52, 9.8, 11.02), entry.Amounts);
        Assert.Same(services, entry.TopServices);
        Assert.Same(estimate, entry.Estimate);
    }

    // ----- the words the record says itself

    /// <summary>The words the entry says its four parts in: its own name at the front, the parts in the order they
    /// are named, the sums as the plain numbers they stand on, the list of services as the list it is and the
    /// estimate as the record it is - and the part nobody said as the empty place between the mark and the comma or
    /// the closing brace. These are the record's words and not the page's: no dash and no dollar sign, which are
    /// the words the page says of the parts it is shown.</summary>
    [Fact]
    public void TheEntrySaysItsFourPartsInTheOrderTheyAreNamed()
    {
        var full = new CostEntry(
            "uat",
            Amounts(1.52, 9.8, 11.02),
            new List<CostService> { Service("SQL Database", 0.97), Service("Azure Container Apps", 0.62) },
            new CostEstimate(0.2048, Amounts(1.17, 2.69, 2.69)));

        Assert.Equal(
            "CostEntry { Name = uat, Amounts = CostAmounts { Yesterday = 1.52, Last7Days = 9.8, MonthToDate = 11.02 }, "
                + "TopServices = System.Collections.Generic.List`1[Dashboard.Health.CostService], "
                + "Estimate = CostEstimate { Share = 0.2048, Amounts = CostAmounts { Yesterday = 1.17, Last7Days = 2.69, MonthToDate = 2.69 } } }",
            Words(full));

        // The sums that are unsaid and the estimate that is none are the empty places, and the words of the entry
        // without them carry no dash and no dollar sign.
        var none = new CostEntry("ghost", Amounts(null, null, null), new List<CostService>());
        Assert.Equal(
            "CostEntry { Name = ghost, Amounts = CostAmounts { Yesterday = , Last7Days = , MonthToDate =  }, "
                + "TopServices = System.Collections.Generic.List`1[Dashboard.Health.CostService], Estimate =  }",
            Words(none));
        Assert.DoesNotContain("—", Words(none), StringComparison.Ordinal);
        Assert.DoesNotContain("$", Words(full), StringComparison.Ordinal);
        Assert.DoesNotContain("$", Words(none), StringComparison.Ordinal);
    }

    /// <summary>The numbers of these words follow whoever reads them, which is what sets the record's own words
    /// apart from the page's: a reader of German is told <c>1,52</c> by the entry and <c>$1.52</c> by
    /// <see cref="CostText.Money" />, whose words are written for the dashboard and not for whoever looks.</summary>
    [Fact]
    public void TheEntriesOwnWordsFollowTheReaderWhileThePagesWordsDoNot()
    {
        var tdd = new CostEntry("tdd", Amounts(0, 1.5, 1.59), new List<CostService>());

        ViewerCulture.German(() =>
        {
            Assert.Equal(
                "CostEntry { Name = tdd, Amounts = CostAmounts { Yesterday = 0, Last7Days = 1,5, MonthToDate = 1,59 }, "
                    + "TopServices = System.Collections.Generic.List`1[Dashboard.Health.CostService], Estimate =  }",
                tdd.ToString());
            Assert.Equal("$1.50", CostText.Money(tdd.Amounts.Last7Days, "USD"));
        });

        Assert.Equal(
            "CostEntry { Name = tdd, Amounts = CostAmounts { Yesterday = 0, Last7Days = 1.5, MonthToDate = 1.59 }, "
                + "TopServices = System.Collections.Generic.List`1[Dashboard.Health.CostService], Estimate =  }",
            Words(tdd));
    }

    // ----- the record's own shape

    /// <summary>The entry's whole public surface, as it stands: the four parts named in the order the constructor
    /// names them - the name, the sums, the services and the estimate - and the members every record is asked,
    /// falling apart, comparing, hashing and saying itself - and nothing else to call. The parts are what they are
    /// said to be, and a fifth part added to the entry has to be written here as well, which is the point: the page
    /// cannot be shown a part nobody decided what it means.</summary>
    [Fact]
    public void TheEntryHasFourPartsAndTheMembersEveryRecordIsAsked()
    {
        Assert.Equal(
            ["Name", "Amounts", "TopServices", "Estimate"],
            [.. typeof(CostEntry)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(property => property.Name)]);

        Assert.Equal(
            ["Amounts", "Deconstruct", "Equals", "Estimate", "GetHashCode", "Name", "ToString", "TopServices", "op_Equality", "op_Inequality"],
            PublicSurface());

        var properties = typeof(CostEntry)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .ToDictionary(property => property.Name, StringComparer.Ordinal);

        Assert.Equal(typeof(string), properties["Name"].PropertyType);
        Assert.Equal(typeof(CostAmounts), properties["Amounts"].PropertyType);
        Assert.Equal(typeof(IReadOnlyList<CostService>), properties["TopServices"].PropertyType);
        Assert.Equal(typeof(CostEstimate), properties["Estimate"].PropertyType);
    }

    /// <summary>The public members the entry declares itself: its properties and the methods that are not the get
    /// and the set of one - the two operators kept although the compiler names them specially, since they are the
    /// <c>==</c> and the <c>!=</c> a caller writes - with the record's compiler-named clone walked past. The
    /// constructor is no method, so it is named by the test above rather than by this list.</summary>
    private static List<string> PublicSurface() =>
        [.. typeof(CostEntry)
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is PropertyInfo
                             || member is MethodInfo method
                             && (!method.IsSpecialName || method.Name.StartsWith("op_", StringComparison.Ordinal)))
            .Where(member => !member.Name.Contains('<', StringComparison.Ordinal))
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];

    /// <summary>The words an entry says its four parts in, taken in the invariant culture whatever the reader of
    /// this test happens to be: the record's own words follow whoever reads them, which is pinned on its own further
    /// up this file, so the strings there are the point and the culture only the way to say them once.</summary>
    private static string Words(CostEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        var reader = Thread.CurrentThread.CurrentCulture;
        try
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            return entry.ToString()!;
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = reader;
        }
    }
}
