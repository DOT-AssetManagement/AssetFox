using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppliedResearchAssociates.iAM.DataPersistenceCore
{
    public static class AttributeNameDictionaryLookupExtensions
    {
        /// <summary>If the dictionary contains the key, returns the string value.
        /// Otherwise, returns the empty string. Will never throw. Usage inside
        /// of ToDto calls will impact sql-side virtualization.</summary> 
        public static String GetAttributeNameOrEmptyString(
            this IReadOnlyDictionary<Guid, string> attributeNameCacheDictionary, Guid? key)
        {
            if (key!=null)
            {
                if (attributeNameCacheDictionary.ContainsKey(key.Value))
                {
                    return attributeNameCacheDictionary[key.Value];
                }
            }
            return String.Empty;
        }
    }
}
