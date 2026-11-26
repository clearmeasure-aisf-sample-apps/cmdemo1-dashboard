using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="DeploymentText" /> - the words the page makes of a
/// deployment: where its mark's link goes, its sentence in each state, the age it says in, its mark's tooltip, what
/// the file says beyond the state, the shape of the mark's dot and the help that says the marks are not live.
/// <para>
/// What is pinned where it belongs, and is not pinned again here: <c>DeploymentTextHelperTests</c> (in
/// <c>HealthTextHelperTests.cs</c>) pins the sentence of each state with no end, the guided failure's own word, the
/// age of what ended at the changes of its units, and the sentence's refusal of a deployment that is not there;
/// <c>DeploymentsTests</c> pins the age at its boundaries of unit - under an hour, under two days, after - with a
/// clock that runs behind, the title's <c>so far</c> and its nothing more for what ended, the dot's shape for every
/// state the enum has, and the help's words up to their first moment; <c>HealthHelperCoverageTests</c> pins the
/// detail - whose desk a question is on, the quoted title, the age of a question at the changes of its units, the
/// started-by and the parts joined in their order - and the detail as the second line of a title; and
/// <c>DeploymentActivityTests</c> the title of what waits and of what runs with what the file says more.
/// </para>
/// <para>
/// Left open by all of that, and pinned here: the tooltip of the mark's link as the fixed value it is, and the field
/// that holds it; a state the page does not know, and a state in flight, each given an end by the file and saying
/// their word without the age the end would give; the age truncated to the whole unit it falls in, a moment past each
/// change of unit, a length past every day it counts, and a length that is no length at all; the title of a mark
/// that is not there, of one that is in flight for no minute, of one that ended at no time the file could read, and
/// of one with a detail, each without the <c>so far</c> that only a count in flight gives; the question after a
/// failed step with its desk, with its title and with its nothing, a title that only says the kind in its own
/// letters, the ask that is not yet a minute old and the one ahead of the clock, and the question that says all of
/// it; and the help word for word to its last word, in a zone of its own, across the date the zone moves, from a
/// moment that carries its own hours, and its refusal of a zone that is not there.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: every moment is one the test brings.
/// </para>
/// </summary>
public class DeploymentTextEdgeCaseTests
{
    /// <summary>The words every help ends with, said once and joined on to its first line.</summary>
    private const string HelpEnd =
        ". A deployment that started shows within about one to six minutes and its end as soon, one that is only queued within half an hour; "
        + "one that ended stays marked for ten minutes.";

    /// <summary>A deployment of uat, started when the page is asked, and ended when the end says it is.</summary>
    private static Deployment One(DeploymentState state, string stateText, DateTimeOffset? finished = null, DeploymentWait? waitsFor = null) =>
        Deployments.InState(state, finished, since: ActivityLines.Now, stateText: stateText, release: "2.4.43", waitsFor: waitsFor);

    /// <summary>A deployment that is waiting, asked five minutes before the page is asked.</summary>
    private static Deployment Waiting(DeploymentWait? wait, string? startedBy = null) =>
        Deployments.InState(DeploymentState.Waiting, since: ActivityLines.Now - TimeSpan.FromMinutes(5), stateText: "waiting", release: "2.4.43", startedBy: startedBy, waitsFor: wait);

    /// <summary>A mark the test builds from its own words, its count and what the file says more.</summary>
    private static DeploymentMark Mark(string sentence, int? minutes, bool inFlight, string? detail = null) =>
        new("demo-ui", "ui", "uat", "2.4.43", DeploymentState.Executing, sentence, minutes, null, inFlight, OfSystem: false, detail);

    // ------------------------------------------------- the link's tooltip, as the fixed value it is

    /// <summary>
    /// Where a mark's link goes is one sentence, written once: the fixed value it is, the words of the link it is
    /// made of, and the field that holds them - a field of the type that is no reference and whose value is known
    /// before the code runs, so a mark's link and a line's link say it the same and a change to the words is a
    /// change the compiler writes here.
    /// </summary>
    [Fact]
    public void WhereAMarksLinkGoesIsSaidOnce()
    {
        Assert.Equal(
            "The deployment's task in Octopus Deploy (opens in a new tab; Octopus Deploy asks you to sign in)",
            DeploymentText.TaskTitle);
        Assert.Equal("The deployment's task in Octopus Deploy" + LinkText.OctopusSuffix, DeploymentText.TaskTitle);
        Assert.EndsWith(LinkText.OctopusSuffix, DeploymentText.TaskTitle, StringComparison.Ordinal);
        Assert.StartsWith("The deployment's task in Octopus Deploy", DeploymentText.TaskTitle, StringComparison.Ordinal);

        var field = typeof(DeploymentText).GetField(nameof(DeploymentText.TaskTitle), BindingFlags.Public | BindingFlags.Static);

        Assert.NotNull(field);
        Assert.Equal(typeof(string), field!.FieldType);
        Assert.True(field.IsPublic);
        Assert.True(field.IsStatic);
        Assert.True(field.IsLiteral);
        Assert.Equal(DeploymentText.TaskTitle, field.GetValue(null));
    }

    // ------------------------------------------- the end the file gives a state that does not say it

    /// <summary>
    /// A state the page does not know is said in the file's own word, and nothing else: an end the file gives it is
    /// no age in the sentence, whether the end was five minutes ago or two hours, while the same end of a state the
    /// page does know says the age - which is pinned where the age itself is pinned, at the changes of its units.
    /// </summary>
    [Theory]
    [InlineData("abandoned", "demo-ui 2.4.43 in uat: abandoned")]
    [InlineData("timed out", "demo-ui 2.4.43 in uat: timed out")]
    public void AStateThePageDoesNotKnowSaysTheFilesWordWithoutAnAge(string stateText, string sentence)
    {
        Assert.Equal(sentence, DeploymentText.Sentence(One(DeploymentState.Unknown, stateText, finished: ActivityLines.Now.AddMinutes(-5)), ActivityLines.Now));
        Assert.Equal(sentence, DeploymentText.Sentence(One(DeploymentState.Unknown, stateText, finished: ActivityLines.Now.AddHours(-2)), ActivityLines.Now));
    }

    /// <summary>
    /// A state in flight is said in its in-flight word, and an end the file gives it is no age in the sentence: the
    /// queued says it is queued, the executing says it is deploying, and the waiting that asks what to do asks it,
    /// each with the end standing in the file five minutes before the page is asked - the end the same moment of a
    /// state the page knows would say as <c>5 min ago</c>, which is pinned where the sentence's verbs are pinned.
    /// </summary>
    [Theory]
    [InlineData(DeploymentState.Queued, "queued", "demo-ui 2.4.43 is queued for uat")]
    [InlineData(DeploymentState.Executing, "executing", "deploying demo-ui 2.4.43 to uat")]
    public void AStateInFlightSaysItsWordWithoutAnAgeEvenWhenTheFileGivesItAnEnd(DeploymentState state, string stateText, string sentence) =>
        Assert.Equal(sentence, DeploymentText.Sentence(One(state, stateText, finished: ActivityLines.Now.AddMinutes(-5)), ActivityLines.Now));

    /// <summary>The question a failed step asks is said without its age, an end the file gives it notwithstanding.</summary>
    [Fact]
    public void AWaitingDeploymentTheFileAlsoEndsSaysTheQuestionWithoutAnAge() =>
        Assert.Equal(
            "demo-ui 2.4.43 asks what to do after a failed step in uat",
            DeploymentText.Sentence(
                One(DeploymentState.Waiting, "waiting", finished: ActivityLines.Now.AddMinutes(-5), waitsFor: new DeploymentWait(DeploymentWait.GuidedFailure, null, null, null)),
                ActivityLines.Now));

    // --------------------------------------------------- the age, truncated to the whole unit it is in

    /// <summary>
    /// The age is the whole unit the length falls in, and nothing of what it falls off: the half-minute off three
    /// minutes is no minute, the half-hour off an hour is no hour, and the day and a night off two days is no day -
    /// each unit counted down from the next, and a length past every day it counts counted in days still.
    /// </summary>
    [Theory]
    [InlineData(210, "3 min")]
    [InlineData(5400, "1 h")]
    [InlineData(259199, "2 d")]
    [InlineData(431999, "4 d")]
    [InlineData(8640000, "100 d")]
    public void AnAgeIsTheWholeUnitTheLengthFallsIn(int seconds, string expected) =>
        Assert.Equal(expected, DeploymentText.Age(TimeSpan.FromSeconds(seconds)));

    /// <summary>
    /// A moment past the change of a unit is the new unit, and one second past it is: past the hour, the age is an
    /// hour; past two days, it is a day - the changes themselves, a second under and a second over, being pinned
    /// where the age's boundaries are pinned.
    /// </summary>
    [Theory]
    [InlineData(3601, "1 h")]
    [InlineData(172801, "2 d")]
    public void AMomentPastTheChangeOfUnitIsTheNewUnit(int seconds, string expected) =>
        Assert.Equal(expected, DeploymentText.Age(TimeSpan.FromSeconds(seconds)));

    /// <summary>
    /// A length that is no length is still an age of one minute, and never of none: the negative the page's own
    /// clock runs behind makes is no less than the minute, and an hour of the negative is the same as a second of
    /// it, since the age's minutes know no sign at all - the end the age counts from being ahead of the moment
    /// asked for, a clock that runs behind the system's, pinned where the marks are pinned.
    /// </summary>
    [Theory]
    [InlineData(-3600)]
    [InlineData(-86400)]
    public void ALengthThatIsNoLengthIsOneMinute(int seconds) => Assert.Equal("1 min", DeploymentText.Age(TimeSpan.FromSeconds(seconds)));

    /// <summary>
    /// An age that has ended is the age and the word for it, in the age's own unit: the sentence, the detail and the
    /// activity line all say it so, from one place.
    /// </summary>
    [Theory]
    [InlineData(0, "1 min ago")]
    [InlineData(300, "5 min ago")]
    [InlineData(18000, "5 h ago")]
    [InlineData(259200, "3 d ago")]
    public void AnAgeThatHasEndedIsSaidAgo(int seconds, string expected) =>
        Assert.Equal(expected, DeploymentText.Ago(TimeSpan.FromSeconds(seconds)));

    /// <summary>
    /// An age still running is the age and the words for it, in the age's own unit and never less than a minute: the
    /// tooltip of a mark in flight says it so.
    /// </summary>
    [Theory]
    [InlineData(0, "1 min so far")]
    [InlineData(3, "3 min so far")]
    [InlineData(90, "1 h so far")]
    [InlineData(2880, "2 d so far")]
    public void AnAgeStillRunningIsSaidSoFar(int minutes, string expected) =>
        Assert.Equal(expected, DeploymentText.SoFar(minutes));

    // ------------------------------------------------------------ the title, and what it is not told

    /// <summary>The title is the mark's tooltip, and a tooltip needs a mark: one that is not there is refused by the
    /// name the guard keeps.</summary>
    [Fact]
    public void WithoutAMarkThereIsNoTitle() =>
        Assert.Equal("mark", Assert.Throws<ArgumentNullException>(() => DeploymentText.Title(null!)).ParamName);

    /// <summary>
    /// A mark that is in flight for no minute is one: the count of its so far is the age of the minutes it has, and
    /// the age of no minutes is a minute, so the tooltip of a deployment started the moment the page is asked says
    /// <c>1 min so far</c> and never <c>0</c> of anything.
    /// </summary>
    [Fact]
    public void AMarkThatIsInFlightForNoMinuteSaysOneMinuteSoFar() =>
        Assert.Equal(
            "deploying demo-ui 2.4.43 to uat (1 min so far)",
            DeploymentText.Title(Mark("deploying demo-ui 2.4.43 to uat", 0, inFlight: true)));

    /// <summary>
    /// What ended at no time the file could read is said without a count, and the title is the sentence and nothing
    /// more: the sentence of one says no age, since there is no end to count from, and the title has no count to
    /// add the <c>so far</c> to - the in flight of no count being pinned where the file is read, and what ended with
    /// a count saying the sentence and nothing more where the file is read alike.
    /// </summary>
    [Fact]
    public void WhatEndedAtNoTimeTheFileCouldReadIsSaidWithoutACount() =>
        Assert.Equal(
            "demo-ui 2.4.43 reached uat",
            DeploymentText.Title(Mark("demo-ui 2.4.43 reached uat", null, inFlight: false)));

    /// <summary>
    /// The <c>so far</c> is the count's, and the count is in flight's: what ended with a count and a detail says the
    /// detail on its own line and no <c>so far</c>, and what is in flight without a time to count from says the
    /// detail alike and no <c>so far</c> - the detail as the second line of the title being pinned where the detail
    /// is pinned, and the <c>so far</c> with a count where the title is pinned.
    /// </summary>
    [Fact]
    public void ATitleSaysTheDetailWithoutSoFarWhereTheCountIsNotInFlight()
    {
        Assert.Equal(
            "demo-ui 2.4.43 failed in uat 5 min ago\nstarted by jeffrey",
            DeploymentText.Title(Mark("demo-ui 2.4.43 failed in uat 5 min ago", 5, inFlight: false, detail: "started by jeffrey")));

        Assert.Equal(
            "demo-ui 2.4.43 waits for a sign-off in uat\nthe sign-off is with demo approvers",
            DeploymentText.Title(Mark("demo-ui 2.4.43 waits for a sign-off in uat", null, inFlight: true, detail: "the sign-off is with demo approvers")));
    }

    // ---------------------------------------------------------- the detail, at the ends its tests leave open

    /// <summary>
    /// The question a failed step asks is named as what it is, and says its desk and its title and nothing it was
    /// not given: with its desk and nothing else, with its title and nothing else, and with neither - the desk and
    /// the title and the ask together being pinned where the detail's parts are pinned.
    /// </summary>
    [Fact]
    public void AQuestionAfterAFailedStepSaysItsDeskItsTitleAndNothingItWasNotGiven()
    {
        Assert.Equal(
            "the question after a failed step is with jeffrey",
            DeploymentText.Detail(Waiting(new DeploymentWait(DeploymentWait.GuidedFailure, null, null, "jeffrey")), ActivityLines.Now));

        Assert.Equal(
            "the question after a failed step \"Database is unreachable\"",
            DeploymentText.Detail(Waiting(new DeploymentWait(DeploymentWait.GuidedFailure, "Database is unreachable", null, null)), ActivityLines.Now));

        Assert.Null(DeploymentText.Detail(Waiting(new DeploymentWait(DeploymentWait.GuidedFailure, null, null, null)), ActivityLines.Now));
    }

    /// <summary>
    /// A title that only says the kind is the kind, and is not quoted for saying it: the guided failure titled
    /// <c>Guided Failure</c>, in the letters the kind wears, and <c>guided-failure</c>, in the letters the file
    /// writes it with its dash, are titles that say nothing more, and the sign-off titled <c>SIGN-OFF</c> is its
    /// kind in capitals - each said as the kind and not in the file's letters, whatever those are.
    /// </summary>
    [Fact]
    public void ATitleThatOnlySaysTheKindInItsOwnLettersIsNotQuoted()
    {
        Assert.Equal(
            "the question after a failed step is with ops",
            DeploymentText.Detail(Waiting(new DeploymentWait(DeploymentWait.GuidedFailure, "Guided Failure", null, "ops")), ActivityLines.Now));

        Assert.Equal(
            "the question after a failed step is with ops",
            DeploymentText.Detail(Waiting(new DeploymentWait(DeploymentWait.GuidedFailure, "guided-failure", null, "ops")), ActivityLines.Now));

        Assert.Equal(
            "the sign-off is with ops",
            DeploymentText.Detail(Waiting(new DeploymentWait(DeploymentWait.SignOff, "SIGN-OFF", null, "ops")), ActivityLines.Now));
    }

    /// <summary>
    /// The ask that is not yet a minute old is a minute, and the ask ahead of the clock is one alike: the age of the
    /// question is the age the page says in, and the age of no minutes - the question asked the moment the page is
    /// asked - is a minute, and a question the page's clock runs behind the system's asks is no more than that -
    /// the age at the changes of its units being pinned where the detail's parts are pinned.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-2)]
    public void AnAskOfNoWholeMinuteIsSaidAsOneMinuteAgo(int minutesAgo)
    {
        var wait = new DeploymentWait(DeploymentWait.SignOff, null, ActivityLines.Now - TimeSpan.FromMinutes(minutesAgo), null);

        Assert.Equal("asked 1 min ago", DeploymentText.Detail(Waiting(wait), ActivityLines.Now));
    }

    /// <summary>
    /// A question that says all of it says it in its order: the title that says more than the kind does, quoted; the
    /// desk it is on; the age of the ask; and the account that started the task - four parts, three of them the
    /// question's and one the deployment's, in the order the file's words stand.
    /// </summary>
    [Fact]
    public void AQuestionThatSaysAllOfItSaysItInItsOrder()
    {
        var wait = new DeploymentWait(DeploymentWait.SignOff, "Approve the release", ActivityLines.Now - TimeSpan.FromHours(2), "demo approvers");

        Assert.Equal(
            "the sign-off \"Approve the release\" is with demo approvers; asked 2 h ago; started by jeffrey",
            DeploymentText.Detail(Waiting(wait, "jeffrey"), ActivityLines.Now));
    }

    // ------------------------------------------------------------- the help, word for word to its last word

    /// <summary>
    /// The help says every word it has: the marks are not live, the moment the system read them where the file says
    /// one, and the length a mark stands - to the last word, the <c>as of</c> and its words being pinned where the
    /// help's first line is pinned, and the full sentence here, with and without the moment.
    /// </summary>
    [Fact]
    public void TheHelpSaysEveryWordItHas()
    {
        Assert.Equal(
            "Not live: what the system itself read from Octopus Deploy, as of 2026-10-08 04:22 +00:00" + HelpEnd,
            DeploymentText.Help(new DateTimeOffset(2026, 10, 8, 4, 22, 24, TimeSpan.Zero), TimeZoneInfo.Utc));

        Assert.Equal(
            "Not live: what the system itself read from Octopus Deploy" + HelpEnd,
            DeploymentText.Help(null, TimeZoneInfo.Utc));
    }

    /// <summary>
    /// The moment in the help is the instant it is, in the hours the zone it is given wears: the same moment five
    /// and a half hours east of the meridian, the same instant written as the next day in the east where the
    /// meridian keeps the day, the same instant the hour before the day turns in the west, and a moment that
    /// carries its own hours said in the zone's hours at the very instant it is.
    /// </summary>
    [Fact]
    public void TheMomentInTheHelpFollowsTheZoneItIsGiven()
    {
        var east = TimeZoneInfo.CreateCustomTimeZone("IST", TimeSpan.FromMinutes(330), "IST", "IST");
        var west = TimeZoneInfo.CreateCustomTimeZone("EST", TimeSpan.FromHours(-5), "EST", "EST");
        var moment = new DateTimeOffset(2026, 10, 8, 4, 22, 24, TimeSpan.Zero);

        Assert.Contains(", as of 2026-10-08 09:52 +05:30", DeploymentText.Help(moment, east), StringComparison.Ordinal);
        Assert.Contains(", as of 2026-10-09 05:00 +05:30", DeploymentText.Help(new DateTimeOffset(2026, 10, 8, 23, 30, 0, TimeSpan.Zero), east), StringComparison.Ordinal);
        Assert.Contains(", as of 2026-10-08 18:30 -05:00", DeploymentText.Help(new DateTimeOffset(2026, 10, 8, 23, 30, 0, TimeSpan.Zero), west), StringComparison.Ordinal);
        Assert.Contains(
            ", as of 2026-10-08 04:22 +00:00",
            DeploymentText.Help(new DateTimeOffset(2026, 10, 8, 9, 52, 24, TimeSpan.FromMinutes(330)), TimeZoneInfo.Utc),
            StringComparison.Ordinal);
    }

    /// <summary>The help says its moment in the zone it is given, and a zone that is not there is no help: the
    /// moment the guard keeps is the zone's, and it is refused by the name the guard keeps.</summary>
    [Fact]
    public void WithoutAZoneThereIsNoHelp() =>
        Assert.Equal("zone", Assert.Throws<ArgumentNullException>(() => DeploymentText.Help(null, null!)).ParamName);
}
