using System.Collections.Generic;
using AssetFox.Core.DTOs.Abstract;

namespace AssetFox.Core.DTOs
{
    public class TreatmentLibraryDTO : BaseLibraryDTO
    {
        public List<TreatmentDTO> Treatments { get; set; }

        public bool IsModified { get; set; }
    }
}
