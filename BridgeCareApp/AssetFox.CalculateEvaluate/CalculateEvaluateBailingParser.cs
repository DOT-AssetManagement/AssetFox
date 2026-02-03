using Antlr4.Runtime;

namespace AssetFox.CalculateEvaluate
{
    internal sealed class CalculateEvaluateBailingParser : CalculateEvaluateParser
    {
        public CalculateEvaluateBailingParser(ITokenStream input) : base(input)
        {
            ErrorHandler = new BailErrorStrategy();
            RemoveErrorListeners();
        }
    }
}
