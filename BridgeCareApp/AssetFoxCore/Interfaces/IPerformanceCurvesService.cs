using System;
using System.Collections.Generic;
using AssetFox.Core.Common.Logging;
using System.Threading;
using AssetFox.Core.DTOs;
using AssetFoxCore.Models;
using OfficeOpenXml;

namespace AssetFoxCore.Interfaces
{
    public interface IPerformanceCurvesService
    {
        ScenarioPerformanceCurvesImportResultDTO ImportScenarioPerformanceCurvesFile(Guid simulationId, ExcelPackage excelPackage, UserCriteriaDTO currentUserCriteriaFilter, CancellationToken? cancellationToken = null, IWorkQueueLog queueLog = null);

        PerformanceCurvesImportResultDTO ImportLibraryPerformanceCurvesFile(Guid performanceCurveLibraryId, ExcelPackage excelPackage, UserCriteriaDTO currentUserCriteriaFilter, CancellationToken? cancellationToken = null, IWorkQueueLog queueLog = null);

        FileInfoDTO ExportScenarioPerformanceCurvesFile(Guid simulationId);

        FileInfoDTO ExportLibraryPerformanceCurvesFile(Guid performanceCurveLibraryId);
        
    }
}
