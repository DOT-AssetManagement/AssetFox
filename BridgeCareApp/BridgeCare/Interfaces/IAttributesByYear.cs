using System.Collections.Generic;
using BridgeCare.Models;

namespace BridgeCare.Interfaces
{
    public interface IAttributesByYear
    {
        List<AttributeByYearModel> GetHistoricalAttributes(SectionModel sectionModel, BridgeCareContext db);

        List<AttributeByYearModel> GetProjectedAttributes(SimulatedSegmentIdsModel segmentAddressModel, BridgeCareContext db);
    }
}
