using System;
using System.Collections.Generic;
using AssetFox.Core.Common.Logging;
using System.Threading;
using AssetFox.Core.DTOs;
using AssetFox.Core.DTOs.Abstract;
using AssetFoxCore.Models;
using OfficeOpenXml;

namespace AssetFoxCore.Interfaces
{
    public interface IInvestmentBudgetsService
    {
        FileInfoDTO ExportScenarioInvestmentBudgetsFile(Guid simulationId);

        FileInfoDTO ExportLibraryInvestmentBudgetsFile(Guid budgetLibraryId);

        ScenarioBudgetImportResultDTO ImportScenarioInvestmentBudgetsFile(Guid simulationId, ExcelPackage excelPackage, UserCriteriaDTO currentUserCriteriaFilter,
            bool overwriteBudgets, CancellationToken? cancellationToken = null, IWorkQueueLog queueLog = null);

        BudgetImportResultDTO ImportLibraryInvestmentBudgetsFile(Guid budgetLibraryId, ExcelPackage excelPackage, UserCriteriaDTO currentUserCriteriaFilter,
            bool overwriteBudgets, CancellationToken? cancellationToken = null, IWorkQueueLog queueLog = null);
    }
}
