using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using AssetFox.Core.UnitTestsCore.Tests.SelectableTreatment;

namespace AssetFox.Core.UnitTestsCore.Tests
{
    public static class TreatmentLibraryTestSetup
    {
        public static TreatmentLibraryDTO ModelForEntityInDb(IUnitOfWork unitOfWork, Guid? id = null)
        {
            var resolveId = id ?? Guid.NewGuid();
            var dto = TreatmentLibraryDtos.Empty(resolveId);
            unitOfWork.SelectableTreatmentRepo.UpsertTreatmentLibrary(dto);
            return dto;
        }
    }
}
