using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DTOs.Abstract;

namespace AssetFox.Core.DTOs
{
    public class GetRawDataSpreadsheetColumnHeadersResultDTO: WarningServiceResultDTO
    {
        public List<string> ColumnHeaders { get; set; }
    }
}
