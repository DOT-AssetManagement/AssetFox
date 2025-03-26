using System;
using System.Collections.Generic;
using AppliedResearchAssociates.iAM.DTOs;

namespace AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories
{
    public static class IAttributeRepositoryExtensions
    {
        public static Dictionary<Guid, string> GetAttributeNameLookupDictionary(this IAttributeRepository repository, List<AttributeDTO> attributeDtos = null)
        {

            var allAttributes = attributeDtos ?? repository.GetAttributes();
            var attributeNameLookup = new Dictionary<Guid, string>();
            foreach (var attribute in allAttributes)
            {
                attributeNameLookup[attribute.Id] = attribute.Name;
            }
            return attributeNameLookup;
        }
    }
}
