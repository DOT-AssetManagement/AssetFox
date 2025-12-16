namespace AssetFox.Core.Analysis.Input.DataTransfer;

public sealed class HistoricalValue<T>
{
    public T Value { get; set; }

    public int Year { get; set; }
}
