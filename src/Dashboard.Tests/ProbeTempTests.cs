using System.Reflection;
using Xunit;

namespace Dashboard.Tests;

public class ProbeTempTests
{
    [Fact]
    public void ProbeDefaults()
    {
        var constructor = typeof(RuntimeIndexResult).GetConstructors().Single();
        var report = string.Join("\n", constructor.GetParameters()
            .Select(p => $"{p.Name}: hasDefault={p.HasDefaultValue}, type={(p.DefaultValue is null ? "NULL" : p.DefaultValue.GetType().FullName)}, str='{p.DefaultValue?.ToString()}'"));
        System.IO.File.WriteAllText("/tmp/probe4.txt", report);
        Assert.True(true);
    }
}
