namespace Dashboard.Tests;

/// <summary>The plain runtime address: the hashes that parse to it, and the zoomed addresses that still parse to their zoomed views.</summary>
public class ViewAddressPlainRuntimeTests
{
    private static readonly ViewAddress Plain = new(DashboardView.Runtime);

    [Fact]
    public void ParseRuntimeAloneEqualsPlainRuntime()
    {
        Assert.Equal(Plain, ViewAddress.Parse("#runtime"));
    }

    [Fact]
    public void ParseRuntimeWithTrailingSlashesEqualsPlainRuntime()
    {
        // Trim('/') strips the trailing slashes, leaving an empty tail.
        Assert.Equal(Plain, ViewAddress.Parse("#runtime/"));
    }

    [Fact]
    public void ParseRuntimeUatAppWebGivesApplicationAddress()
    {
        Assert.Equal(new ViewAddress(DashboardView.Runtime, "uat", Application: "web"), ViewAddress.Parse("#runtime/uat/app/web"));
    }

    [Fact]
    public void ParseRuntimeUatDatabaseGivesDatabaseAddress()
    {
        Assert.Equal(new ViewAddress(DashboardView.Runtime, "uat", Database: true), ViewAddress.Parse("#runtime/uat/database"));
    }
}
