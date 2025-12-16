using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DTOs.Abstract;

namespace AssetFox.Core.DTOs
{
    public class TreatmentImportResultDTO: WarningServiceResultDTO
    {
        public TreatmentLibraryDTO TreatmentLibrary { get; set; }
    }
}
