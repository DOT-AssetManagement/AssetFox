using System;
using System.Collections.Generic;
using AssetFox.Core.Analysis;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Models;
using AssetFox.Core.DTOs;

namespace AssetFox.Core.DataPersistenceCore.Repositories
{
    public interface IPerformanceCurveRepository
    {
        void GetScenarioPerformanceCurves(Simulation simulation);

        List<PerformanceCurveLibraryDTO> GetPerformanceCurveLibraries();
        List<PerformanceCurveLibraryDTO> GetPerformanceCurveLibrariesNoPerformanceCurves();

        void UpsertPerformanceCurveLibrary(PerformanceCurveLibraryDTO dto);

        void UpsertOrDeletePerformanceCurves(List<PerformanceCurveDTO> performanceCurves, Guid libraryId);

        void UpsertOrDeletePerformanceCurveLibraryAndCurves(PerformanceCurveLibraryDTO library, bool isNewLibrary, Guid ownerIdForNewLibrary);

        void DeletePerformanceCurveLibrary(Guid libraryId);

        List<PerformanceCurveDTO> GetScenarioPerformanceCurves(Guid simulationId);

        void UpsertOrDeleteScenarioPerformanceCurves(List<PerformanceCurveDTO> scenarioPerformanceCurves, Guid simulationId);

        void UpsertOrDeleteScenarioPerformanceCurvesNonAtomic(List<PerformanceCurveDTO> scenarioPerformanceCurves, Guid simulationId);
        void SaveScenarioPerformanceCurveChanges(UpsertAndDeleteModel<PerformanceCurveDTO> changes, Guid simulationId);
        void SaveLIbraryPerformanceCurveChanges(PerformanceCurveLibraryDTO libray, UpsertAndDeleteModel<PerformanceCurveDTO> changes);

        public List<PerformanceCurveDTO> GetPerformanceCurvesForLibrary(Guid performanceCurveLibraryId);

        public PerformanceCurveLibraryDTO GetPerformanceCurveLibrary(Guid performanceCurveLibraryId);

        public List<PerformanceCurveDTO> GetScenarioPerformanceCurvesOrderedById(Guid simulationId);

        public List<PerformanceCurveDTO> GetPerformanceCurvesForLibraryOrderedById(Guid performanceCurveLibraryId);

        public List<PerformanceCurveLibraryDTO> GetPerformanceCurveLibrariesNoChildrenAccessibleToUser(Guid userId);

        DateTime GetLibraryModifiedDate(Guid performanceLibraryId);

        public LibraryUserAccessModel GetLibraryAccess(Guid libraryId, Guid userId);

        void UpsertOrDeleteUsers(Guid performanceCurveLibraryId, IList<LibraryUserDTO> libraryUsers);

        List<LibraryUserDTO> GetLibraryUsers(Guid performanceCurveLibraryId);

        List<LibraryUserDTO> GetAccessForUser(Guid performanceCurveLibraryId, Guid userId);
        List<string> GetDistinctScenarioPerformanceFactorAttributeNames();
    }
}
