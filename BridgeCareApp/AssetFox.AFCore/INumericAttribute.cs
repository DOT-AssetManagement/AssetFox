namespace AssetFox.AFCore
{
    public interface INumericAttribute
    {
        bool IsDecreasingWithDeterioration { get; }

        string Name { get; }
    }
}
