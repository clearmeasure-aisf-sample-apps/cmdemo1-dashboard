namespace Dashboard.Tests;

/// <summary>
/// The edges of <see cref="ClusterStatus.Parse" /> that no other test reaches. <see cref="ClusterStatusEdgeCaseTests" />,
/// <see cref="ParsedEdgeCaseTests" /> and <see cref="ClusterParserTests" /> already pin every other public member of
/// <see cref="ClusterStatus" /> and the null, the spaces, the array, the scalar and the broken file. Pinned here: the
/// empty string and blank text other than spaces, the empty object, names spelled in another case, what strict JSON
/// refuses (comments, trailing commas, a second value, trailing text), the whitespace around a good file, and the
/// nesting depth the reader accepts. Every input is fixed text; nothing reaches the network or the clock.
/// </summary>
public class ClusterStatusParseEdgeCaseTests
{
    /// <summary>Text with nothing in it is the empty file, whatever blank characters it is made of.</summary>
    [Theory]
    [InlineData("")]
    [InlineData("\t")]
    [InlineData("\r\n")]
    [InlineData(" \t\r\n ")]
    public void BlankTextIsTheEmptyFile(string json)
    {
        var parsed = ClusterStatus.Parse(json);

        Assert.Null(parsed.Value);
        Assert.Equal("the file is empty", parsed.Problem);
    }

    /// <summary>The empty object is an object that lists neither nodes nor namespaces, and the names are read in
    /// their exact case: <c>Nodes</c> or <c>NAMESPACES</c> is not a list the file has.</summary>
    [Theory]
    [InlineData("{}")]
    [InlineData("{ }")]
    [InlineData("""{ "Nodes": [], "NAMESPACES": [] }""")]
    [InlineData("""{ "node": [], "namespace": [] }""")]
    public void AnObjectWithoutTheExactNamesHasNeitherList(string json)
    {
        var parsed = ClusterStatus.Parse(json);

        Assert.Null(parsed.Value);
        Assert.Equal("the file has neither nodes nor namespaces", parsed.Problem);
    }

    /// <summary>The reader takes strict JSON: a comment, a trailing comma, a second value or text after the object
    /// makes the file not valid JSON, not a status.</summary>
    [Theory]
    [InlineData("""{ "nodes": [] } // written by the collector""")]
    [InlineData("""/* status */ { "nodes": [] }""")]
    [InlineData("""{ "nodes": [], }""")]
    [InlineData("""{ "nodes": [ { "name": "node-1" }, ] }""")]
    [InlineData("""{ "nodes": [] }{ "nodes": [] }""")]
    [InlineData("""{ "nodes": [] } done""")]
    [InlineData("""{ 'nodes': [] }""")]
    public void WhatStrictJsonRefusesIsNotValidJson(string json)
    {
        var parsed = ClusterStatus.Parse(json);

        Assert.Null(parsed.Value);
        Assert.Equal("the file is not valid JSON", parsed.Problem);
    }

    /// <summary>Whitespace around a good file is no part of it: the file is read with its facts as if it stood
    /// bare.</summary>
    [Fact]
    public void WhitespaceAroundAGoodFileIsRead()
    {
        var parsed = ClusterStatus.Parse("\r\n\t  { \"intervalSeconds\": 30, \"nodes\": [ { \"name\": \"node-1\" } ] }  \n");

        Assert.Null(parsed.Problem);
        var status = Assert.IsType<ClusterStatus>(parsed.Value);
        Assert.Equal(30, status.IntervalSeconds);
        Assert.Equal("node-1", Assert.Single(status.Nodes).Name);
        Assert.Empty(status.Namespaces);
        Assert.Equal(TimeSpan.FromSeconds(120), status.StaleAfter);
    }

    /// <summary>The reader keeps the default depth of 64: an unknown field nested to the limit is ignored and the file
    /// is read, and one level deeper the file is not valid JSON rather than an exception.</summary>
    [Theory]
    [InlineData(63, true)]
    [InlineData(64, false)]
    [InlineData(200, false)]
    public void TheNestingDepthIsSixtyFourLevels(int arrays, bool read)
    {
        // The root object is one level; each array inside the unknown field adds one more.
        var json = $$"""{ "nodes": [], "extra": {{new string('[', arrays)}}{{new string(']', arrays)}} }""";

        var parsed = ClusterStatus.Parse(json);

        if (read)
        {
            Assert.Null(parsed.Problem);
            Assert.Empty(Assert.IsType<ClusterStatus>(parsed.Value).Nodes);
        }
        else
        {
            Assert.Null(parsed.Value);
            Assert.Equal("the file is not valid JSON", parsed.Problem);
        }
    }
}
