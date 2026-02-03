using AssetFox.Core.DTOs.Abstract;

namespace AssetFoxCore.Models
{
    public class CreateAttributeRequest
    {
        public AllAttributeDTO Attribute { get; set; }
        public bool SetForAllAttributes { get; set; }
    }
}
