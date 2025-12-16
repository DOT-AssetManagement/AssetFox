using System;
using System.Collections.Generic;
using AppliedResearchAssociates.iAM.DTOs;

namespace AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories
{
    public static class IAttributeRepositoryExtensions
    {
        /// <summary>If you don't have a list of attributeDtos to pass in, instead use GetIdNameCache</summary> 
        public static Dictionary<Guid, string> GetAttributeNameLookupDictionary(this IAttributeRepository repository, List<AttributeDTO> attributeDtos)
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
