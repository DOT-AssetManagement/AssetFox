using System;
using System.Collections.Generic;
using System.Text;

namespace AssetFox.Core.Data.ExcelDatabaseStorage.Serializers
{
    public class ExcelRawDataSpreadsheetDeserializationResult
    {
        public ExcelRawDataSpreadsheet Worksheet { get; set; }
        public string Message { get; set; }
    }
}
