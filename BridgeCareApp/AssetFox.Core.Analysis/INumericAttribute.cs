namespace AssetFox.Core.Analysis;

public interface INumericAttribute
{
    bool IsDecreasingWithDeterioration { get; }

    string Name { get; }
}
