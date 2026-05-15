using AssetFox.Core.DTOs.Abstract;
using System;
using System.Collections.Generic;

namespace AssetFox.Core.DTOs
{
    public class TreatmentRuleOverrideCriterionDTO : BaseDTO
    {
        public string LogicalOperator { get; set; } = "AND";

        public int SortOrder { get; set; }

        public string CriterionExpression { get; set; }

        public List<Guid> TreatmentIds { get; set; } = new List<Guid>();

        public List<SimpleTreatmentDTO> Treatments { get; set; } = new List<SimpleTreatmentDTO>();
    }
}
