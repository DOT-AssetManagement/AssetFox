using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using AssetFox.Core.Hubs.Interfaces;

namespace AssetFox.Core.Reporting.Interfaces
{
    public interface IReportFactory
    {
        string Name { get; }

        IReport Create(IUnitOfWork uow, ReportIndexDTO results, IHubService hubService, string suffix = "");
    }
}
