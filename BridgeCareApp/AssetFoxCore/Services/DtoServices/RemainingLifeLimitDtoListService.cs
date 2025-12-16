using AssetFox.Core.DTOs;
using System.Collections.Generic;
using System;

namespace AssetFoxCore.Services
{
    public static class RemainingLifeLimitDtoListService
    {
        public static void AddLibraryIdToScenarioRemainingLifeLimit(List<RemainingLifeLimitDTO> remainingLifeLimitDTOs, Guid? libraryId)
        {
            if (libraryId == null) return;
            foreach (var dto in remainingLifeLimitDTOs)
            {
                dto.LibraryId = (Guid)libraryId;
            }
        }
        public static void AddModifiedToScenarioRemainingLifeLimit(List<RemainingLifeLimitDTO> remainingLifeLimitDTOs, bool IsModified)
        {
            foreach (var dto in remainingLifeLimitDTOs)
            {
                dto.IsModified = IsModified;
            }
        }
    }
}
