using System.Text.Json;

namespace Dashboard.Health;

/// <summary>
/// The build's test run in numbers: the unit, the integration and the acceptance counts, each the number the
/// build counted or not said, and <see cref="Total" /> the three of them make.
/// <para>
/// What <see cref="Total" /> means when the parts cannot make it is decided here, on purpose, and not left to
/// the arithmetic by inheritance: the parts stay the <c>int?</c> the build endpoint counts them in, and the total
/// adds them in that same <c>int</c> - so past the top of the count the total comes back around, the top and one
/// more being the bottom, the top twice over being <c>-2</c>, and the bottom twice over being nothing again.
/// </para>
/// <para>
/// That is what <see cref="BuildText.Tests" /> says when it puts the total first on the line, and it is what
/// <c>TestCountsEdgeCaseTests</c> and <c>BuildTextEdgeCaseTests</c> pin on purpose in their wrap rows, rather than
/// the saturation or the wider count the parts do not hold.
/// </para>
/// </summary>
public sealed record TestCounts(int? Unit, int? Integration, int? Acceptance)
{
    /// <summary>
    /// The total the counts that are said make, added in the <c>int</c> the parts are held in: past the top of the
    /// count it comes back around, on purpose, the way the record's summary says.
    /// </summary>
    public int Total => (Unit ?? 0) + (Integration ?? 0) + (Acceptance ?? 0);

    internal static TestCounts? Read(JsonElement? section)
    {
        var tests = new TestCounts(JsonRead.Count(section, "unit"), JsonRead.Count(section, "integration"), JsonRead.Count(section, "acceptance"));
        return MeasuredFacts.OrNone(tests, tests.MeasuredNothing);
    }

    internal bool MeasuredNothing => MeasuredFacts.None(Unit, Integration, Acceptance);
}
