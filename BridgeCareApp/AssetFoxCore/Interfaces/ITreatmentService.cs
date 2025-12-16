using System;
using AssetFox.Core.Common.Logging;
using System.Threading;
using AssetFox.Core.DTOs;
using OfficeOpenXml;

namespace AssetFoxCore.Interfaces
{
    public interface ITreatmentService
    {
        FileInfoDTO ExportLibraryTreatmentsExcelFile(Guid libraryId);

        TreatmentImportResultDTO ImportLibraryTreatmentsFile(
            Guid treatmentLibraryId,
            ExcelPackage excelPackage, CancellationToken? cancellationToken = null, IWorkQueueLog queueLog = null);

        void ImportLibraryTreatmentsFileSingle(
            Guid treatmentLibraryId,
            ExcelPackage excelPackage, CancellationToken? cancellationToken = null, IWorkQueueLog queueLog = null);

        void ImportScenarioTreatmentsFileSingle(
           Guid treatmentLibraryId,
           ExcelPackage excelPackage, CancellationToken? cancellationToken = null, IWorkQueueLog queueLog = null);

        ScenarioTreatmentImportResultDTO ImportScenarioTreatmentsFile(
            Guid simulationId,
            ExcelPackage excelPackage, CancellationToken? cancellationToken = null, IWorkQueueLog queueLog = null);

        FileInfoDTO ExportScenarioTreatmentsExcelFile(Guid simulationId);

        FileInfoDTO ExportScenarioTreatmentSupersedeRuleExcelFile(Guid simulationId);

        TreatmentSupersedeRuleImportResultDTO ImportScenarioTreatmentSupersedeRulesFile(Guid simulationId, ExcelPackage excelPackage, CancellationToken? cancellationToken = null, IWorkQueueLog queueLog = null);

        public FileInfoDTO ExportLibraryTreatmentSupersedeRuleExcelFile(Guid libraryId);

        TreatmentSupersedeRuleImportResultDTO ImportLibraryTreatmentSupersedeRulesFile(Guid libraryId, ExcelPackage excelPackage, CancellationToken? cancellationToken = null, IWorkQueueLog queueLog = null);
    }
}
