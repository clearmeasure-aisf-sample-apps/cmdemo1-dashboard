namespace Dashboard.Tests;

/// <summary>
/// The runtime view's hash as one rule in order (#1120): no environment is the plain word whatever zoom the address
/// carries; then an application the address accepts; then the database; then the environment's diagram. A refused
/// deployable's name falls back to the database and then to the diagram.
/// </summary>
public sealed class ViewAddressRuntimeHashTests
{
    [Theory]
    [InlineData(null, false, null, "runtime")]
    [InlineData(null, true, null, "runtime")]
    [InlineData(null, false, "api", "runtime")]
    [InlineData(null, true, "api", "runtime")]
    [InlineData("uat", false, null, "runtime/uat")]
    [InlineData("uat", true, null, "runtime/uat/database")]
    [InlineData("uat", false, "api", "runtime/uat/app/api")]
    [InlineData("uat", true, "api", "runtime/uat/app/api")]
    [InlineData("uat", true, "not a name", "runtime/uat/database")]
    [InlineData("uat", false, "not a name", "runtime/uat")]
    [InlineData("uat", true, "", "runtime/uat/database")]
    [InlineData("", false, null, "runtime/")]
    [InlineData(" ", true, null, "runtime/ /database")]
    public void TheRuntimeHashSaysTheEnvironmentThenTheApplicationThenTheDatabaseThenTheDiagram(
        string? environment, bool database, string? application, string expected)
    {
        var address = new ViewAddress(DashboardView.Runtime, environment, database, application);

        Assert.Equal(expected, address.ToHash());
    }

    [Theory]
    [InlineData("runtime")]
    [InlineData("runtime/uat")]
    [InlineData("runtime/uat/database")]
    [InlineData("runtime/uat/app/api")]
    public void EveryRuntimeHashReadsBackToItself(string hash)
    {
        Assert.Equal(hash, ViewAddress.Parse(hash).ToHash());
    }
}
