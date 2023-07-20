namespace FruitSort;

class LineComparer : IComparer<string?>
{
    /// <summary>
    /// Compares two lines in a file, first by the string part and then by the number part.
    /// </summary>
    /// <param name="a">The first string.</param>
    /// <param name="b">The second string.</param>
    /// <remarks>
    /// Uses to StringComparison.Ordinal as recommended in the documentation:
    /// https://learn.microsoft.com/en-us/dotnet/standard/base-types/best-practices-strings#recommendations-for-string-usage
    /// </remarks>
    public int Compare(string? a, string? b)
    {
        if (a == null && b == null)
        {
            return 0;
        }
        else if (a == null)
        {
            return -1;
        }
        else if (b == null)
        {
            return 1;
        }

        var aSeparatorIndex = a.IndexOf('.');
        var aString = a.AsSpan(aSeparatorIndex + 2);

        var bSeparatorIndex = b.IndexOf('.');
        var bString = b.AsSpan(bSeparatorIndex + 2);

        if (aString.Equals(bString, StringComparison.Ordinal))
        {
            var aNumberSpan = a.AsSpan(0, aSeparatorIndex);
            var aNumber = long.Parse(aNumberSpan);

            var bNumberSpan = b.AsSpan(0, bSeparatorIndex);
            var bNumber = long.Parse(bNumberSpan);
    
            if (aNumber == bNumber)
            {
                return 0;
            }
            else if (aNumber < bNumber)
            {
                return -1;
            }

            return 1;
        }
        
        return aString.CompareTo(bString, StringComparison.Ordinal);
    }
}