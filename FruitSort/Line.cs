namespace FruitSort;

[Serializable]
public record Line
{
    public Line(string str)
    {
        var separatorIndex = str.IndexOf('.');
        StringValue = str.AsMemory(separatorIndex + 2);
        var numberSpan = str.AsSpan(0, separatorIndex);
        NumberValue = long.Parse(numberSpan);
    }

    public long NumberValue { get; init; }

    public ReadOnlyMemory<char> StringValue {get; init; }
}