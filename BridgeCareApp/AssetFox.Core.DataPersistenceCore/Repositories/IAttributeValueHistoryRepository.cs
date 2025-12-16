using System;
using System.Collections.Generic;
using AssetFox.Core.Analysis;

namespace AssetFox.Core.DataPersistenceCore.Repositories
{
    public interface IAttributeValueHistoryRepository
    {
        /*void CreateNumericAttributeValueHistories(List<((Guid sectionId, Guid attributeId) sectionIdAttributeId, AttributeValueHistory<double> numericAttributeValueHistory)> numericAttributeValueHistorySectionIdAttributeIdTupleTuples);
        void CreateTextAttributeValueHistories(List<((Guid sectionId, Guid attributeId) sectionIdAttributeId, AttributeValueHistory<string> textAttributeValueHistory)> textAttributeValueHistorySectionIdAttributeIdTupleTuples);*/

        void CreateNumericAttributeValueHistories(
            Dictionary<(Guid sectionId, Guid attributeId), IAttributeValueHistory<double>>
                numericAttributeValueHistoryPerSectionIdAttributeIdTuple);

        void CreateTextAttributeValueHistories(
            Dictionary<(Guid sectionId, Guid attributeId), IAttributeValueHistory<string>>
                numericAttributeValueHistoryPerSectionIdAttributeIdTuple);
    }
}
