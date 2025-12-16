using System;

namespace AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Entities
{
    public class BudgetDetailEntity
    {
        public Guid Id { get; set; }

        public int RunId { get; set; }

        public Guid SimulationYearDetailId { get; set; }

        public virtual SimulationYearDetailEntity SimulationYearDetail { get; set; }    

        public decimal AvailableFunding { get; set; }
        
        public string BudgetName { get; set; }
    }
}
