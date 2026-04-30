using Xunit;

namespace DecisionHelper.Tests;

public class AdminIdParseTests
{
    // Mirror of the parser in Program.cs — kept here so the parsing rules are
    // pinned by tests without needing access to the Program type.
    private static HashSet<long> Parse(string? raw)
    {
        var set = new HashSet<long>();
        if (string.IsNullOrWhiteSpace(raw)) return set;
        foreach (var part in raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (long.TryParse(part, out var id)) set.Add(id);
        }
        return set;
    }

    [Fact]
    public void Empty_returns_empty_set()
    {
        Assert.Empty(Parse(null));
        Assert.Empty(Parse(""));
        Assert.Empty(Parse("   "));
    }

    [Fact]
    public void Single_id_parsed()
    {
        Assert.Equal(new HashSet<long> { 12345L }, Parse("12345"));
    }

    [Fact]
    public void Multiple_ids_with_whitespace_and_dupes()
    {
        Assert.Equal(new HashSet<long> { 1L, 2L, 3L }, Parse(" 1 , 2,3, 2 "));
    }

    [Fact]
    public void Garbage_entries_are_skipped()
    {
        Assert.Equal(new HashSet<long> { 7L }, Parse("not_a_number, 7, also_bad"));
    }
}
