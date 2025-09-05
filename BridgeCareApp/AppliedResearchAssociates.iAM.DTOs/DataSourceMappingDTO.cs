using System;

namespace AppliedResearchAssociates.iAM.DTOs
{
    public class DataSourceMappingDTO
    {
        public Guid Id { get; set; }

        public string DataField { get; set; }

        public Guid AttributeId { get; set; }

        public Guid DataSourceId { get; set; }
    }
}
