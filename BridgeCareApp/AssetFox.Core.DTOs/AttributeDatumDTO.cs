using System;
using System.Collections.Generic;
using System.Text;
using AssetFox.Core.DTOs.Abstract;

namespace AssetFox.Core.DTOs
{
    public class AttributeDatumDTO: BaseDTO
    {
        public string Attribute { get; set; }

        public string TextValue { get; set; }

        public double? NumericValue { get; set; }

        public Guid MaintainableAssetId { get; set; }
    }
}
