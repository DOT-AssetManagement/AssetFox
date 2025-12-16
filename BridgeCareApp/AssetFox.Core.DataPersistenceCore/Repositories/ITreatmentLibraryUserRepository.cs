using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DTOs;

namespace AssetFox.Core.DataPersistenceCore.Repositories
{
    public interface ITreatmentLibraryUserRepository
    {
        void UpsertTreatmentLibraryUser(TreatmentLibraryDTO dto, Guid userId);

        LibraryUserAccessModel GetLibraryAccess(Guid libraryId, Guid userId);

        void UpsertOrDeleteUsers(Guid treatmentLibraryId, IList<LibraryUserDTO> libraryUsers);
        List<LibraryUserDTO> GetLibraryUsers(Guid treatmentLibraryId);
    }
}
