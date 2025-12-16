using Antlr4.Runtime;

namespace AssetFox.CalculateEvaluate
{
    internal sealed class CalculateEvaluateBailingLexer : CalculateEvaluateLexer
    {
        public CalculateEvaluateBailingLexer(ICharStream input) : base(input) => RemoveErrorListeners();

        public override void Recover(LexerNoViableAltException e) => throw new CalculateEvaluateLexingException(null, e);
    }
}
