using System.Collections.Generic;
using AssetFox.Core.DTOs.Abstract;

namespace AssetFox.Core.DTOs
{
    public class CalculatedAttributeLibraryDTO : BaseLibraryDTO
    {
        public bool IsDefault { get; set; } = false;

        public List<CalculatedAttributeDTO> CalculatedAttributes { get; set; } = new List<CalculatedAttributeDTO>();
    }
}
