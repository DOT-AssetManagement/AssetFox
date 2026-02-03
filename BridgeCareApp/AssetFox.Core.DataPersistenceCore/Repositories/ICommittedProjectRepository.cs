using System;
using System.Collections.Generic;
using AssetFox.Core.Analysis;
using AssetFox.Core.DTOs.Abstract;
using AssetFox.Core.DTOs;
using System.IO;
using AssetFox.Core.DataPersistenceCore.Repositories.MSSQL.Models;

namespace AssetFox.Core.DataPersistenceCore.Repositories
{
    public interface ICommittedProjectRepository
    {
        void GetSimulationCommittedProjects(Simulation simulation);

        List<SectionCommittedProjectDTO> GetSectionCommittedProjectDTOs(Guid simulationId);

        Guid GetSimulationId(Guid projectId);

        List<BaseCommittedProjectDTO> GetCommittedProjectsForExport(Guid simulationId);

        void UpsertCommittedProjects(List<SectionCommittedProjectDTO> projects);
        void SaveCommittedProjectChanges(UpsertAndDeleteModel<SectionCommittedProjectDTO> changes, Guid simulationId);

        void SetCommittedProjectTemplate(Stream stream);

        void AddCommittedProjectTemplate(Stream stream, string filename);

        List<String> getUploadedCommittedProjectTemplates();

        string DownloadCommittedProjectTemplate();

        string DownloadSelectedCommittedProjectTemplate(string filename);

        void DeleteSimulationCommittedProjects(Guid simulationId);

        void DeleteSpecificCommittedProjects(List<Guid> projectIds);

        bool ValidateAllCommittedProjects(List<SectionCommittedProjectDTO> sectionCommittedProjectDtos, List<int> budgetYears, Dictionary<string, bool> keyAttributeValuesExists);
    }
}
