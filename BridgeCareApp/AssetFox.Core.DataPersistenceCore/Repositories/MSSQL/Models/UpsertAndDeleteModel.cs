using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;

namespace AppliedResearchAssociates.iAM.DataPersistenceCore.Repositories.MSSQL.Models
{
    public class UpsertAndDeleteModel<T>
    {
        public UpsertAndDeleteModel()
        {
            RowsForDeletion = new List<Guid>();
            UpdateRows = new List<T>();
            AddedRows = new List<T>();
        }
        public List<Guid> RowsForDeletion { get; set; }
        public List<T> UpdateRows { get; set; }
        public List<T> AddedRows { get; set; }
    }
}
