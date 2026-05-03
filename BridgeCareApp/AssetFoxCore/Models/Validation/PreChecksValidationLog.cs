using System.Collections.Generic;

namespace AssetFoxCore.Models.Validation
{
    public class PreChecksValidationLog
    {
        public List<string> ErrorMessages { get; set; }

        public List<string> WarningMessages { get; set; }
    }
}
