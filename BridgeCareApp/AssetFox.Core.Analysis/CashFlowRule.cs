using System;
using System.Collections.Generic;
using System.Linq;
using AssetFox.Validation;

namespace AssetFox.Core.Analysis;

public sealed class CashFlowRule : WeakEntity, IValidator
{
    internal CashFlowRule(Explorer explorer) => Criterion = new Criterion(explorer ?? throw new ArgumentNullException(nameof(explorer)));

    public Criterion Criterion { get; }

    public ICollection<CashFlowDistributionRule> DistributionRules { get; } = new SetWithoutNulls<CashFlowDistributionRule>();

    public string Name { get; set; }

    public string ShortDescription => Name;

    public ValidatorBag Subvalidators => new ValidatorBag { Criterion, DistributionRules };

    public ValidationResultBag GetDirectValidationResults()
    {
        var results = new ValidationResultBag();

        if (string.IsNullOrWhiteSpace(Name))
        {
            results.Add(ValidationStatus.Error, "Name is blank.", this, nameof(Name), "Cash Flow");
        }

        if (DistributionRules.Count == 0)
        {
            results.Add(ValidationStatus.Error, "There are no distribution rules.", this, nameof(DistributionRules), "Cash Flow");
        }

        if (DistributionRules.GroupBy(rule => rule.CostCeiling).Any(group => group.Count() > 1))
        {
            results.Add(ValidationStatus.Error, "There are two or more distribution rules with the same cost ceiling.", this, nameof(DistributionRules), "Cash Flow");
        }

        return results;
    }
}
