using System;
using System.Collections.Generic;
using AppliedResearchAssociates.CalculateEvaluate;

namespace AppliedResearchAssociates.iAM.Analysis;

public sealed class Criterion : CompilableExpression
{
    internal Criterion(Explorer explorer) => Explorer = explorer ?? throw new ArgumentNullException(nameof(explorer));

    public IReadOnlyCollection<string> ReferencedParameters
    {
        get
        {
            try
            {
                EnsureCompiled();
            }
            catch (MalformedInputException)
            {
                return Array.Empty<string>();
            }
            return Evaluator?.ReferencedParameters ?? Array.Empty<string>();
        }
    }

    public bool? Evaluate(CalculateEvaluateScope scope)
    {
        EnsureCompiled();
        return Evaluator?.Delegate(scope);
    }

    protected override void Compile()
    {
        if (ExpressionIsBlank)
        {
            Evaluator = null;
            return;
        }

        try
        {
            Evaluator = Explorer.Compiler.GetEvaluator(Expression);
        }
        catch (CalculateEvaluateException e)
        {
            throw ExpressionCouldNotBeCompiled(e);
        }
    }

    private readonly Explorer Explorer;

    private Evaluator Evaluator;
}
