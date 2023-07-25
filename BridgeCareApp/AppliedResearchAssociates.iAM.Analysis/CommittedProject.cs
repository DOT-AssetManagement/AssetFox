using System;
using System.Collections.Generic;
using System.Linq;
using AppliedResearchAssociates.iAM.Analysis.Engine;
using AppliedResearchAssociates.iAM.DTOs.Enums;
using AppliedResearchAssociates.Validation;

namespace AppliedResearchAssociates.iAM.Analysis
{
    public sealed class CommittedProject : Treatment
    {
        internal CommittedProject(Simulation simulation, AnalysisMaintainableAsset asset, int year)
        {
            Simulation = simulation ?? throw new ArgumentNullException(nameof(simulation));
            Asset = asset ?? throw new ArgumentNullException(nameof(asset));
            Year = year;

            _Budget = Simulation.InvestmentPlan.UnknownBudget;
        }

        public AnalysisMaintainableAsset Asset { get; }

        public Budget Budget
        {
            get => _Budget;
            set => _Budget = value ?? Simulation.InvestmentPlan.UnknownBudget;
        }

        public ICollection<TreatmentConsequence> Consequences { get; } = new SetWithoutNulls<TreatmentConsequence>();

        public double Cost { get; set; }

        public SelectableTreatment TemplateTreatment
        {
            get => _TemplateTreatment;
            set
            {
                _TemplateTreatment = value;

                Name = TemplateTreatment.Name;

                Consequences.Clear();
                foreach (var templateConsequence in TemplateTreatment.Consequences)
                {
                    var consequence = new TreatmentConsequence { Attribute = templateConsequence.Attribute };
                    consequence.Change.Expression = templateConsequence.Change.Expression;
                    Consequences.Add(consequence);
                }

                PerformanceCurveAdjustmentFactors.Clear();
                foreach (var (attribute, factor) in TemplateTreatment.PerformanceCurveAdjustmentFactors)
                {
                    PerformanceCurveAdjustmentFactors.Add(attribute, factor);
                }
            }
        }

        public int Year { get; }

        public DateTime LastModifiedDate { get; set; }

        public TreatmentCategory treatmentCategory { get; set; }

        public override ValidationResultBag GetDirectValidationResults()
        {
            var results = base.GetDirectValidationResults();

            if (Cost < 0)
            {
                results.Add(ValidationStatus.Error, "Cost is less than zero.", this, nameof(Cost));
            }

            if (Consequences.Select(consequence => consequence.Attribute).Distinct().Count() < Consequences.Count)
            {
                results.Add(ValidationStatus.Error, "At least one attribute is acted on by more than one consequence.", this, nameof(Consequences));
            }

            return results;
        }

        public override IEnumerable<TreatmentScheduling> GetSchedulings() => Enumerable.Empty<TreatmentScheduling>();

        internal override bool CanUseBudget(Budget budget) => budget == Budget;

        internal override IReadOnlyCollection<Action> GetConsequenceActions(AssetContext scope) => Consequences.Select(consequence => consequence.GetChangeApplicators(scope, null).Single().Action).ToArray();

        internal override double GetCost(AssetContext scope, bool shouldApplyMultipleFeasibleCosts) => Cost;

        private readonly Simulation Simulation;

        private Budget _Budget;

        private SelectableTreatment _TemplateTreatment;
    }
}
