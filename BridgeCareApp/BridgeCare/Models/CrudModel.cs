using System.Runtime.Serialization;

namespace BridgeCare.Models
{
    public class CrudModel
    {
        public CrudModel()
        {
            matched = false;
        }

        [IgnoreDataMember]
        public bool matched { get; set; }
    }
}
