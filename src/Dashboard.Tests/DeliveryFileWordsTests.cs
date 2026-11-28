namespace Dashboard.Tests;

/// <summary>
/// The four words of <c>delivery.json</c> that two readers each type by hand (<c>name</c>, <c>reason</c>,
/// <c>releaseUrl</c> and <c>at</c>): parsed through the public <see cref="DeliveryReport.Parse" />, one deployable
/// carries <c>reason</c> and <c>releaseUrl</c> both at the entry level and inside its <c>lastPersonSignOff</c> with
/// different values, and the failover test names its <c>at</c>. Each part must come out of its own level, so the
/// rename of a word in one reader cannot silently blank the card line of another.
/// </summary>
public class DeliveryFileWordsTests
{
    private const string Json = """
        {
          "environments": [
            {
              "name": "uat",
              "deployables": [
                {
                  "name": "ui",
                  "reason": "entry-reason",
                  "releaseUrl": "https://octopus.example.net/r/entry",
                  "lastPersonSignOff": {
                    "version": "2.4.11",
                    "by": "pat.morgan",
                    "at": "2026-10-01T15:12:41Z",
                    "reason": "signoff-reason",
                    "releaseUrl": "https://octopus.example.net/r/signoff"
                  }
                }
              ]
            }
          ],
          "failover": {
            "environment": "uat",
            "at": "2026-10-04T05:00:00Z",
            "seconds": 44
          }
        }
        """;

    [Fact]
    public void EachSharedWordComesOutOfItsOwnLevel()
    {
        var report = DeliveryReport.Parse(Json)!;
        var ui = report.Find("uat", "ui")!;

        Assert.Equal("uat", report.Environments[0].Name);
        Assert.Equal("ui", ui.Name);
        Assert.Equal("entry-reason", ui.Reason);
        Assert.Equal(new Uri("https://octopus.example.net/r/entry"), ui.ReleaseUrl);

        var signOff = ui.LastPersonSignOff!;
        Assert.Equal("signoff-reason", signOff.Reason);
        Assert.Equal(new Uri("https://octopus.example.net/r/signoff"), signOff.ReleaseUrl);
        Assert.Equal(new DateTimeOffset(2026, 10, 1, 15, 12, 41, TimeSpan.Zero), signOff.At);

        Assert.Equal(new DateTimeOffset(2026, 10, 4, 5, 0, 0, TimeSpan.Zero), report.Failover!.At);
    }
}
