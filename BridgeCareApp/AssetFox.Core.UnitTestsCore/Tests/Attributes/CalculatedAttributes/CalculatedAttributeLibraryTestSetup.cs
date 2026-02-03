using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using AssetFox.Core.TestHelpers;

namespace AssetFox.Core.UnitTestsCore.Tests.Attributes.CalculatedAttributes
{
    public static class CalculatedAttributeLibraryTestSetup
    {
        public static CalculatedAttributeLibraryDTO ModelForEntityInDb(IUnitOfWork unitOfWork)
        {
            var dto = CalculatedAttributeLibraryDtos.Dto();
            unitOfWork.CalculatedAttributeRepo.UpsertCalculatedAttributeLibrary(dto);
            var dtoAfter = unitOfWork.CalculatedAttributeRepo.GetCalculatedAttributeLibraries().FirstOrDefault(x => x.Id == dto.Id);
            return dtoAfter;
        }
    }
}
