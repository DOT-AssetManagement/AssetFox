using System.Collections.Generic;
using AssetFox.Core.DTOs;

namespace AssetFox.Core.UnitTestsCore.Tests.Attributes
{
    public static class AttributeDtoLists
    {
        public static List<AttributeDTO> AttributeSetupDtos()
            => new List<AttributeDTO>
            {
                AttributeDtos.ActionType,
                AttributeDtos.AdtTotal,
                AttributeDtos.Age,
                AttributeDtos.BmsId,
                AttributeDtos.BrKey,
                AttributeDtos.ConditionIndex,
                AttributeDtos.CulvSeeded,
                AttributeDtos.DeckArea,
                AttributeDtos.DeckSeeded,
                AttributeDtos.InternetReport,
                AttributeDtos.Interstate,
                AttributeDtos.SubSeeded,
                AttributeDtos.SupSeeded,
                AttributeDtos.CulvDurationN,
                AttributeDtos.DeckDurationN,
                AttributeDtos.RiskScore,
                AttributeDtos.SubDurationN,
                AttributeDtos.SupDurationN,
            };
    }
}
