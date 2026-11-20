using System.Globalization;
using System.Reflection;

namespace Dashboard.Tests;

/// <summary>
/// The boundary, null/empty and fixed-value tests of <see cref="ActivityLine" /> - the one line a node of the
/// runtime view and an environment's name carries from the system's deployments file: what the line is about,
/// the line short, the line in full, and the task in Octopus Deploy.
/// <para>
/// What is pinned where it belongs: <c>DeploymentActivityTests</c> pins the lines the sayers make, word for
/// word, in the order of their urgency, the kinds in their order, and the words a tile of the diagram says of
/// them; <c>ClusterDeliveryHelperTests</c> pins the cut a line of a node takes; <c>DeploymentWaitEdgeCaseTests</c>
/// pins the class's two lengths, a moment at the changes of day, and the arguments the sayers refuse.
/// </para>
/// <para>
/// What is left, and is pinned here, is the record itself, which all of that walks past: the constructor
/// handing each of its four arguments to the property of its name, and the task it names none when it is not
/// there; the three words kept exactly as they are given - empty, padded with whitespace, in their case,
/// trimmed nowhere - including a kind the page does not write; the parts a line may be given as nothing, kept
/// as nothing rather than the empty word; the address held and never asked, one task marked at another place
/// in it being one line and another path being another; two lines being one while their four parts are, and
/// the empty word being a word rather than nothing; the four parts coming back out in the order they went in;
/// the copy that turns the part it names and keeps the other three; the words the record says its parts in,
/// not following whoever reads them; and the public surface, so that the day a fifth part is added the list
/// says so.
/// </para>
/// <para>
/// Nothing here reaches the network and nothing asks a clock what time it is: the line holds words and an
/// address but calls nothing of either - every address below is one the test brought, and the address is held
/// and never fetched.
/// </para>
/// </summary>
public class ActivityLineEdgeCaseTests
{
    private const string Tasks = "https://octopus.example.net/app#/Spaces-356/tasks/ServerTasks-";

    /// <summary>The prod node's line, part for part, brought here without reading the file.</summary>
    private static ActivityLine Full() =>
        new(ActivityKind.Waiting, "2.4.43 waits for demo approvers", "demo-ui 2.4.43 waits for a sign-off in prod", new Uri($"{Tasks}5"));

    /// <summary>A line that names no task: the part a line does without, as the constructor names it none.</summary>
    private static ActivityLine Bare() =>
        new(ActivityKind.Failed, "2.4.43 failed a step and waits", "demo-ui 2.4.43 failed in prod");

    // ----- the constructor, handing each argument to the property of its name

    /// <summary>The constructor's four arguments, position by position, come back as the property of the same
    /// name, and the line holds the very address it was built of. The task is the part the line does without:
    /// a line built of its three words stands, and the constructor names it none. A line built again of the
    /// same four parts is the same line and no longer the same reference - which is what lets a test name a
    /// line read from a file by writing it out, as <see cref="DeploymentActivityTests" /> does.</summary>
    [Fact]
    public void TheConstructorHandsEachArgumentToThePropertyOfItsName()
    {
        var url = new Uri($"{Tasks}5");
        var line = new ActivityLine("waiting", "2.4.43 waits for demo approvers", "demo-ui 2.4.43 waits for a sign-off in prod", url);

        Assert.Equal("waiting", line.Kind);
        Assert.Equal("2.4.43 waits for demo approvers", line.Text);
        Assert.Equal("demo-ui 2.4.43 waits for a sign-off in prod", line.Title);
        Assert.Same(url, line.Url);
        Assert.Equal($"{Tasks}5", line.Url!.AbsoluteUri);

        // The task is the part the line does without: a line built of its three words names no task.
        var bare = new ActivityLine(ActivityKind.Failed, "2.4.43 failed a step and waits", "demo-ui 2.4.43 failed in prod");

        Assert.Null(bare.Url);
        Assert.Equal(Bare(), bare);
        Assert.Equal(Bare().GetHashCode(), bare.GetHashCode());

        var again = Full();

        Assert.Equal(Full(), again);
        Assert.NotSame(Full(), again);
        Assert.Equal(Full().GetHashCode(), again.GetHashCode());
    }

    // ----- the three words, kept exactly as they are given

    /// <summary>Every word a line carries - the kind, the line short and the line in full - is kept as letters
    /// and nothing more: the empty word is said and not left unsaid, a word of nothing but whitespace is a word
    /// of its own, and a word of one letter is a line's word. Two lines built of the same letters are one line
    /// of one hash, whatever the letters are.</summary>
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("  ")]
    [InlineData("a")]
    [InlineData(" waiting ")]
    [InlineData("WAITING")]
    [InlineData("waiting")]
    public void EachWordIsKeptExactlyAsItIsGiven(string word)
    {
        var line = new ActivityLine(word, word, word, null);

        Assert.Equal(word, line.Kind);
        Assert.Equal(word, line.Text);
        Assert.Equal(word, line.Title);

        var same = new ActivityLine(word, word, word, null);

        Assert.Equal(line, same);
        Assert.Equal(line.GetHashCode(), same.GetHashCode());
        Assert.True(line == same);
        Assert.False(line != same);
    }

    /// <summary>Trimming and casing are the sayers' and not the line's: a line padded is no line trimmed, one in
    /// another case is a line in its own letters, and a word said empty is a word said - which is the difference
    /// between a line that says nothing and a line that says the empty word, and why the page keeps the letters
    /// it was given rather than the words it would say of them.</summary>
    [Fact]
    public void TheLineTrimsNothingAndCasesNothing()
    {
        var padded = new ActivityLine(" waiting ", " 2.4.43 waits for demo approvers ", " demo-ui 2.4.43 waits for a sign-off in prod ");

        Assert.NotEqual(padded, padded with { Kind = "waiting" });
        Assert.NotEqual(padded, padded with { Text = "2.4.43 waits for demo approvers" });
        Assert.NotEqual(padded, padded with { Title = "demo-ui 2.4.43 waits for a sign-off in prod" });
        Assert.Equal(padded, padded with { Kind = " waiting " });

        var cased = Full() with { Kind = "WAITING" };
        Assert.NotEqual(Full(), cased);
        Assert.Equal("WAITING", cased.Kind);

        // A word said empty stands; the part said as nothing is another part.
        var empty = Full() with { Text = string.Empty };
        Assert.Equal(string.Empty, empty.Text);
        Assert.NotEqual(empty, Full() with { Text = null! });
        Assert.NotEqual(empty, Full());
    }

    /// <summary>The line's kind is the word it is given, whatever the page knows: the page's own words are the
    /// <see cref="ActivityKind" /> ones, in the order of their urgency, and a kind the page does not write is
    /// the line's word like another - it is the sayers that name only the kinds they know, and not the line,
    /// which keeps the letters it was given.</summary>
    [Fact]
    public void TheKindIsTheWordItIsGivenWhateverThePageKnows()
    {
        var line = new ActivityLine("review", "2.4.43 waits for pat", "demo-ui 2.4.43 waits for a review in prod", null);

        Assert.Equal("review", line.Kind);
        Assert.DoesNotContain("review", ActivityKind.All);
        Assert.Equal(line, new ActivityLine("review", "2.4.43 waits for pat", "demo-ui 2.4.43 waits for a review in prod", null));
        Assert.NotEqual(line, Full());
    }

    // ----- the parts a line may be given as nothing

    /// <summary>The three words and the task may each be given as nothing, and the line keeps nothing where it is
    /// given nothing: the record asks nothing of its parts, and it is the sayers that give words or nothing,
    /// never the line. Nothing in a part is not the empty word in the part, as the line keeps the difference.</summary>
    [Fact]
    public void ALineGivenNothingKeepsNothing()
    {
        var nothing = new ActivityLine(null!, null!, null!, null);

        Assert.Null(nothing.Kind);
        Assert.Null(nothing.Text);
        Assert.Null(nothing.Title);
        Assert.Null(nothing.Url);

        Assert.NotEqual(nothing, nothing with { Kind = string.Empty });
        Assert.NotEqual(nothing, nothing with { Text = string.Empty });
        Assert.NotEqual(nothing, nothing with { Title = string.Empty });
        Assert.NotEqual(Bare(), nothing);
    }

    // ----- the address, held and never asked

    /// <summary>The task in Octopus Deploy is held as the very instance it was given and asked nothing of: no
    /// lookup, no request, and no refusal of an address the sayers would never let through - a scheme other
    /// than http or https, and an address that is no address but a path - since it is the reading of the file
    /// that decides what the page may link to and not the line. Two spellings of one task are one address,
    /// what follows the mark in it being a mark rather than another task, and the line is one line with them;
    /// another path is another address and another line. The words the line says itself keep the spelling.</summary>
    [Fact]
    public void TheAddressIsHeldAndNeverAsked()
    {
        var url = new Uri($"{Tasks}5");
        var line = Full() with { Url = url };

        Assert.Same(url, line.Url);
        Assert.True(line.Url!.IsAbsoluteUri);
        Assert.Equal("https", line.Url.Scheme);
        Assert.Equal(line, Full());
        Assert.Equal(line.GetHashCode(), Full().GetHashCode());
        Assert.NotEqual(line, Bare());

        // The same task, marked at another place in it: one address, one line, one hash - and other words.
        var marked = Full() with { Url = new Uri($"{Tasks}4") };
        Assert.True(line == marked);
        Assert.Equal(line.GetHashCode(), marked.GetHashCode());
        Assert.Single(new HashSet<ActivityLine> { line, marked });
        Assert.NotEqual(Words(line), Words(marked));

        // Another task, in another space of the same instance, is another address and another line.
        var elsewhere = Full() with { Url = new Uri("https://octopus.example.net/spaces/356/tasks/ServerTasks-5") };
        Assert.NotEqual(line, elsewhere);
        Assert.True(line != elsewhere);

        // An address of a host nothing could ever answer for is held the same way, host and all: holding an
        // address asks nothing of it, and no request is made of any of these.
        var far = Full() with { Url = new Uri("https://octopus.invalid/app#/task/5") };
        Assert.Equal("octopus.invalid", far.Url!.DnsSafeHost);
        Assert.Contains("Url = https://octopus.invalid/app#/task/5", Words(far), StringComparison.Ordinal);

        // What the page would not link to is a part like any other: the line knows no scheme and no shape.
        var relative = Full() with { Url = new Uri("ServerTasks-5", UriKind.Relative) };
        Assert.False(relative.Url!.IsAbsoluteUri);
        Assert.NotEqual(line, relative);
        Assert.NotEqual(Bare(), relative);

        var otherScheme = Full() with { Url = new Uri("ftp://octopus.example.net/task/5") };
        Assert.Equal("ftp", otherScheme.Url!.Scheme);
        Assert.NotEqual(line, otherScheme);
    }

    // ----- the equality, the hash, the two operators

    /// <summary>Two lines are one while their four parts are - each word in the case it is written in, and the
    /// address as the address it is, down to the mark in it that the address does not count - and the hash
    /// follows the equality, boxed or unboxed and from one call to the next, so that the lines that are one are
    /// one line of a set and a lookup meets them.</summary>
    [Fact]
    public void TwoLinesAreOneWhileTheirFourPartsAre()
    {
        var one = Full();
        var two = Full();

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
        Assert.Single(new HashSet<ActivityLine> { one, two });
        Assert.Contains(one, new HashSet<ActivityLine> { two });

        var keyed = new Dictionary<ActivityLine, string> { [one] = "the first line" };
        keyed[two] = "still the first line";

        Assert.Single(keyed);
        Assert.Equal("still the first line", keyed[Full()]);
    }

    /// <summary>A difference in one part is another line, and so is the same part in another place: the kind in
    /// another word, another case or nothing, the line short padded or empty or said as nothing, the line in
    /// full of another environment or in capitals, the task of another path and no task at all. A set of the
    /// lines that differ in one part each holds them all apart, and the parts handed over in another order are
    /// other parts.</summary>
    [Fact]
    public void ALineThatDiffersInOnePartIsAnotherLine()
    {
        var one = Full();

        ActivityLine[] different =
        [
            one with { Kind = "deploying" },
            one with { Kind = "WAITING" },
            one with { Kind = string.Empty },
            one with { Kind = null! },
            one with { Text = "2.4.43 waits for pat" },
            one with { Text = " 2.4.43 waits for demo approvers" },
            one with { Text = string.Empty },
            one with { Text = null! },
            one with { Title = "demo-ui 2.4.43 waits for a sign-off in uat" },
            one with { Title = "DEMO-UI 2.4.43 WAITS FOR A SIGN-OFF IN PROD" },
            one with { Title = string.Empty },
            one with { Title = null! },
            one with { Url = new Uri("https://octopus.example.net/spaces/356/tasks/ServerTasks-5") },
            one with { Url = null },
        ];

        foreach (var other in different)
        {
            Assert.NotEqual(one, other);
            Assert.False(one.Equals(other));
            Assert.False(one.Equals((object)other));
            Assert.False(one == other);
            Assert.True(one != other);
        }

        Assert.Equal(1 + different.Length, new HashSet<ActivityLine>([one, .. different]).Count);

        // The kind is no text and the text is no title: the parts stand in their places.
        Assert.NotEqual(one, new ActivityLine(one.Text, one.Kind, one.Title, one.Url));
        Assert.NotEqual(one, new ActivityLine(one.Kind, one.Title, one.Text, one.Url));
    }

    /// <summary>The empty word is a word and nothing is not: the line that says the empty word in a part and the
    /// line that says nothing in it are two lines in that part, in the kind, the line short and the line in full
    /// alike - which is the difference between a line that says no more than its kind does and a line that says
    /// nothing at all, and why the page's words for a part nobody gave are the empty place and not the word.</summary>
    [Fact]
    public void TheEmptyWordIsAWordAndNothingIsNotAWord()
    {
        Assert.NotEqual(Full() with { Kind = string.Empty }, Full() with { Kind = null! });
        Assert.NotEqual(Full() with { Text = string.Empty }, Full() with { Text = null! });
        Assert.NotEqual(Full() with { Title = string.Empty }, Full() with { Title = null! });
    }

    // ----- the four parts coming back out, in the order they went in

    /// <summary>The four parts of a line fall out in the order the constructor names them - the kind, the line
    /// short, the line in full, and the task - as the very address it was, and what came out goes back in the
    /// same way. The line that names no task falls apart to its three words and the part a line does without.</summary>
    [Fact]
    public void TheLineFallsApartInTheOrderItWasMade()
    {
        var url = new Uri($"{Tasks}5");
        var line = new ActivityLine("waiting", "2.4.43 waits for demo approvers", "demo-ui 2.4.43 waits for a sign-off in prod", url);

        var (kind, text, title, task) = line;

        Assert.Equal("waiting", kind);
        Assert.Equal("2.4.43 waits for demo approvers", text);
        Assert.Equal("demo-ui 2.4.43 waits for a sign-off in prod", title);
        Assert.Same(url, task);
        Assert.Equal(line, new ActivityLine(kind, text, title, task));

        var (bareKind, bareText, bareTitle, bareTask) = Bare();

        Assert.Equal(ActivityKind.Failed, bareKind);
        Assert.Equal("2.4.43 failed a step and waits", bareText);
        Assert.Equal("demo-ui 2.4.43 failed in prod", bareTitle);
        Assert.Null(bareTask);
        Assert.Equal(Bare(), new ActivityLine(bareKind, bareText, bareTitle));
    }

    // ----- the copy, turning the part it names

    /// <summary>Any of the four parts may be turned as the line is copied - the kind to another word, the task
    /// to nothing - and stands where it is turned to, while the parts the turn does not name stay the very
    /// values the line was built of, the task as the very address it was. The copy is another line and never
    /// the same reference, the original stands as it stood, and a copy that names every part its own value is
    /// the line it was copied from.</summary>
    [Fact]
    public void ALineCopiedTurnsThePartItNamesAndKeepsTheOtherThree()
    {
        var url = new Uri($"{Tasks}5");
        var line = Full() with { Url = url };

        var moved = line with { Kind = ActivityKind.Failed };

        Assert.Equal(ActivityKind.Failed, moved.Kind);
        Assert.Equal("2.4.43 waits for demo approvers", moved.Text);
        Assert.Equal("demo-ui 2.4.43 waits for a sign-off in prod", moved.Title);
        Assert.Same(url, moved.Url);
        Assert.NotSame(line, moved);
        Assert.NotEqual(line, moved);

        var silent = line with { Url = null };

        Assert.Null(silent.Url);
        Assert.Equal(ActivityKind.Waiting, silent.Kind);
        Assert.Equal("2.4.43 waits for demo approvers", silent.Text);
        Assert.Equal("demo-ui 2.4.43 waits for a sign-off in prod", silent.Title);
        Assert.NotEqual(line, silent);

        var unchanged = line with { };

        Assert.Equal(line, unchanged);
        Assert.NotSame(line, unchanged);
        Assert.Equal(line.GetHashCode(), unchanged.GetHashCode());
        Assert.Same(url, unchanged.Url);

        Assert.Equal(ActivityKind.Waiting, line.Kind);
        Assert.Same(url, line.Url);
    }

    // ----- no line is one with no line

    /// <summary>The nothing on either side of the members every record is asked with: two lines that are no line
    /// at all are one, a line and no line are two - the equality and the inequality each saying it, from either
    /// side - and the line asked about itself says no to the things it is not: a word, an address, a wait of the
    /// same release, another line that differs in a single part, and the anonymous thing that happens to carry
    /// two of its parts.</summary>
    [Fact]
    public void NoLineIsOneWithNoLineAndNotWithAnyOtherThing()
    {
        ActivityLine? nothing = null;
        ActivityLine? alsoNothing = null;
        var there = Full();

        Assert.Null(default(ActivityLine));
        Assert.True(nothing == alsoNothing);
        Assert.False(nothing != alsoNothing);
        Assert.False(nothing == there);
        Assert.True(nothing != there);
        Assert.False(there == (ActivityLine?)null);
        Assert.True(there != (ActivityLine?)null);
        Assert.False(there.Equals(nothing));
        Assert.False(there.Equals((object?)null));
        Assert.True(there.Equals(there));

        Assert.False(there.Equals("waiting"));
        Assert.False(there.Equals(new Uri($"{Tasks}5")));
        Assert.False(there.Equals(new DeploymentWait("sign-off", null, null, null)));
        Assert.False(there.Equals(new { Kind = "waiting", Text = "2.4.43 waits for demo approvers" }));
        Assert.False(there.Equals(Full() with { Text = "2.4.43 waits for pat" }));
    }

    // ----- the words the record says its parts in

    /// <summary>The words the line says its four parts in: its own name at the front, the parts in the order the
    /// constructor names them, each word in its own letters and the address as the address is spelled - the part
    /// nobody gave being the empty place before the mark's close. These are the record's words and not the
    /// page's: no urgency, no cut, no sentence, which are the words the sayers make of the parts.</summary>
    [Fact]
    public void TheLineSaysItsFourPartsInTheOrderTheyAreNamed()
    {
        const string Spoken =
            "ActivityLine { Kind = waiting, Text = 2.4.43 waits for demo approvers, "
                + "Title = demo-ui 2.4.43 waits for a sign-off in prod, "
                + "Url = https://octopus.example.net/app#/Spaces-356/tasks/ServerTasks-5 }";

        Assert.Equal(Spoken, Words(Full()));
        Assert.StartsWith("ActivityLine { Kind = waiting, ", Words(Full()), StringComparison.Ordinal);
        Assert.EndsWith("ServerTasks-5 }", Words(Full()), StringComparison.Ordinal);

        // The task the line does without is the empty place, and a line of nothing says nothing of its parts.
        Assert.Equal(
            "ActivityLine { Kind = failed, Text = 2.4.43 failed a step and waits, Title = demo-ui 2.4.43 failed in prod, Url =  }",
            Words(Bare()));
        Assert.Equal(
            "ActivityLine { Kind = , Text = , Title = , Url =  }",
            Words(new ActivityLine(null!, null!, null!, null)));
    }

    /// <summary>The line's own words are letters and follow no reader: a reader whose culture writes dates and
    /// numbers differently is told the same line, the words in their letters and the address in its spelling,
    /// and no word of the page's is said of them - no cut, no urgency and no sentence, which are the sayers'.</summary>
    [Fact]
    public void TheLinesOwnWordsDoNotFollowTheReader()
    {
        var line = Full();

        ViewerCulture.German(() => Assert.Equal(Words(line), line.ToString()!));

        Assert.Contains("Kind = waiting, Text = 2.4.43 waits for demo approvers", Words(line), StringComparison.Ordinal);
        foreach (var word in new[] { " ago", "In force", "Deployment freeze", "The last deployment" })
        {
            Assert.DoesNotContain(word, Words(line), StringComparison.Ordinal);
        }
    }

    // ----- the record's own shape

    /// <summary>The line's whole public surface, as it stands: its four parts, named in the order the constructor
    /// names them and of the type the constructor says, the parts falling apart into them, and the members every
    /// record is asked - comparing, hashing and saying itself - and nothing else to call. A fifth part added to
    /// the line has to be written here as well, which is the point: the page cannot be shown a part nobody
    /// decided what it means.</summary>
    [Fact]
    public void TheLineHasFourPartsAndTheMembersEveryRecordIsAsked()
    {
        Assert.Equal(
            ["Kind", "Text", "Title", "Url"],
            [.. typeof(ActivityLine)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(property => property.Name)]);

        Assert.Equal(
            ["Deconstruct", "Equals", "GetHashCode", "Kind", "Text", "Title", "ToString", "Url", "op_Equality", "op_Inequality"],
            PublicSurface());

        Assert.Equal([typeof(IEquatable<ActivityLine>)], typeof(ActivityLine).GetInterfaces());

        var properties = typeof(ActivityLine)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .ToDictionary(property => property.Name, StringComparer.Ordinal);

        Assert.Equal(typeof(string), properties["Kind"].PropertyType);
        Assert.Equal(typeof(string), properties["Text"].PropertyType);
        Assert.Equal(typeof(string), properties["Title"].PropertyType);
        Assert.Equal(typeof(Uri), properties["Url"].PropertyType);

        Assert.All(properties.Values, property => Assert.True(property.CanRead));
        Assert.All(properties.Values, property => Assert.True(property.GetMethod!.IsPublic));
        Assert.All(properties.Values, property => Assert.Empty(property.GetIndexParameters()));

        var arguments = typeof(ActivityLine).GetConstructors().Single().GetParameters();

        Assert.Equal(4, arguments.Length);
        Assert.Equal(
            ["Kind", "Text", "Title", "Url"],
            [.. arguments.Select(argument => argument.Name!)]);
        var defaults = arguments.Select(argument => argument.HasDefaultValue).ToArray();
        Assert.Equal([false, false, false, true], defaults);
        Assert.Null(arguments.Single(argument => argument.Name == "Url").DefaultValue);
        Assert.Equal(
            [typeof(string), typeof(string), typeof(string), typeof(Uri)],
            [.. arguments.Select(argument => argument.ParameterType)]);
    }

    /// <summary>The public members the line declares itself: its properties and the methods that are not the get
    /// and the set of one - the two operators kept although the compiler names them specially, since they are
    /// the <c>==</c> and the <c>!=</c> a caller writes - with the record's compiler-named clone walked past. The
    /// constructor is no method, so it is named by the test above rather than by this list, and the reading of a
    /// file is no public member at all.</summary>
    private static List<string> PublicSurface() =>
        [.. typeof(ActivityLine)
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(member => member is PropertyInfo
                             || member is MethodInfo method
                             && (!method.IsSpecialName || method.Name.StartsWith("op_", StringComparison.Ordinal)))
            .Where(member => !member.Name.Contains('<', StringComparison.Ordinal))
            .Select(member => member.Name)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];

    /// <summary>The words a line says its four parts in, taken in the invariant culture whatever the reader of
    /// this test happens to be: the line's own words follow no reader, which is pinned on the line's own further
    /// up this file, so the strings there are the point and the culture only the way to say them once.</summary>
    private static string Words(ActivityLine line)
    {
        ArgumentNullException.ThrowIfNull(line);

        var reader = Thread.CurrentThread.CurrentCulture;
        try
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            return line.ToString()!;
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = reader;
        }
    }
}
