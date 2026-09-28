using AssetFox.Core.Common;
using AssetFox.Core.Common.Logging;
using AssetFox.Core.DataPersistenceCore.Repositories;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using AssetFox.Core.Hubs.Interfaces;
using AssetFox.Core.Reporting;
using AssetFoxCore.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace AssetFoxCoreTests.Tests.Report
{
    public class ReportGenerationWorkitemTests
    {
        [Fact]
        public void DoWork_SetsUserOnUnitOfWorkBeforeRunningReport()
        {
            // Reports address their status updates to _unitOfWork.CurrentUser, so the
            // user has to be set before the report runs or the updates are dropped.
            const string username = "report.user@example.com";
            const string reportName = "BAMSSummaryReport";
            var calls = new List<string>();
            var unitOfWork = UnitOfWorkMocksForReports(calls, username);
            var report = new Mock<IReport>();
            report.Setup(r => r.Type).Returns(ReportType.File);
            report.Setup(r => r.Errors).Returns(new List<string>());
            report.Setup(r => r.IsComplete).Returns(true);
            report.Setup(r => r.Run(It.IsAny<string>(), It.IsAny<CancellationToken?>(), It.IsAny<IWorkQueueLog>()))
                .Callback(() => calls.Add("Run"))
                .Returns(Task.CompletedTask);
            var generator = ReportGeneratorMocks.New();
            generator.Setup(g => g.Generate(reportName)).ReturnsAsync(report.Object);
            var serviceProvider = new ServiceCollection()
                .AddSingleton(unitOfWork.Object)
                .AddSingleton(new Mock<IHubService>().Object)
                .AddSingleton(new Mock<ILog>().Object)
                .AddSingleton(generator.Object)
                .BuildServiceProvider();
            var parameters = new JObject { ["scenarioId"] = Guid.NewGuid() };
            var workItem = new ReportGenerationWorkitem(parameters, username, "Test Scenario", reportName);

            workItem.DoWork(serviceProvider, _ => { }, CancellationToken.None);

            Assert.Equal(new[] { "SetUser", "Run" }, calls);
        }

        private static Mock<IUnitOfWork> UnitOfWorkMocksForReports(List<string> calls, string username)
        {
            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.Setup(u => u.SetUser(username)).Callback(() => calls.Add("SetUser"));
            var reportIndexRepo = new Mock<IReportIndexRepository>();
            reportIndexRepo.Setup(r => r.Add(It.IsAny<ReportIndexDTO>())).Returns(true);
            unitOfWork.Setup(u => u.ReportIndexRepository).Returns(reportIndexRepo.Object);
            return unitOfWork;
        }
    }
}
