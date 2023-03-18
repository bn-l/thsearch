namespace thsearch;


public class SpanCharEqualityComparer : IEqualityComparer<string>
{
    public bool Equals(string x, string y)
    {
        return x.Equals(y, StringComparison.Ordinal);
    }

    public int GetHashCode(string obj)
    {
        return obj.GetHashCode();
    }

    public bool Equals(string x, ReadOnlySpan<char> y)
    {
        return x.AsSpan().SequenceEqual(y);
    }

    public int GetHashCode(ReadOnlySpan<char> obj)
    {
        return obj.GetHashCode();
    }
}