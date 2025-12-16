using System.Runtime.Serialization;

namespace AssetFox.Core.DTOs
{
    public class CrudDTO
    {
        public CrudDTO()
        {
            matched = false;
        }

        [IgnoreDataMember]
        public bool matched { get; set; }
    }
}
