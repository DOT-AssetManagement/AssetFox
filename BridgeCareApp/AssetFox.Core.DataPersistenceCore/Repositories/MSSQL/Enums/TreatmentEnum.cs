using System;
using System.Collections.Generic;
using System.Text;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Enums
{
    public class TreatmentEnum
    {
        // Order is important
        public enum TreatmentCategory
        {
            Preservation,
            CapacityAdding,
            Rehabilitation,
            Replacement,
            Maintenance,
            Other
        }
       // public class AssetCategory
            //{
            //public string Asset { get; set; }
        //}
}
}
