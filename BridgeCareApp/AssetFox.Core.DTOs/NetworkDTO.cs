using System;
using System.Collections.Generic;
using AssetFox.Core.DTOs.Abstract;

namespace AssetFox.Core.DTOs
{
    public class NetworkDTO : BaseDTO
    {
        public string Name { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime LastModifiedDate { get; set; }

        public string Status { get; set; }

        public BenefitQuantifierDTO BenefitQuantifier { get; set; }

        public Guid KeyAttribute { get; set; }

        public List<AttributeDTO> Attributes { get; set; }

        public string DefaultSpatialWeighting { get; set; }
    }
}
