using AssetFox.Core.DTOs.Abstract;

namespace AssetFox.Core.DTOs
{
    public class SimpleTreatmentDTO : BaseDTO
    {
        public string Name { get; set; }
        public bool IsUnselectable { get; set; }
    }
}
