using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetFox.Core.Common;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.Hubs.Interfaces;
using AssetFox.Core.Reporting.Logging;
using AssetFoxCore.Interfaces;
using AssetFoxCore.Services;

namespace AssetFoxCoreTests
{
    public static class TestServices
    {
        public static ILog Logger()
        {
            var logger = new LogNLog();
            return logger;
        }

        public static ExpressionValidationService ExpressionValidation(IUnitOfWork unitOfWork)
        {
            var log = Logger();
            var service = new ExpressionValidationService(unitOfWork, log);
            return service;
        }

        public static IPerformanceCurvesService PerformanceCurves(IUnitOfWork unitOfWork, IHubService hubService)
        {
            var logger = new LogNLog();
            var expressionValidation = ExpressionValidation(unitOfWork);
            var service = new PerformanceCurvesService(unitOfWork, hubService, expressionValidation);
            return service;
        }
        public static ExcelRawDataImportService CreateExcelSpreadsheetImportService(IUnitOfWork unitOfWork)
        {
            var returnValue = new ExcelRawDataImportService(unitOfWork);
            return returnValue;
        }
    }
}
