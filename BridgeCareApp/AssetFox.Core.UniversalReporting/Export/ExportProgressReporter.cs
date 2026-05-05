using AssetFox.Core.Common.Logging;
using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using AssetFox.Core.Hubs;
using AssetFox.Core.Hubs.Interfaces;

namespace AssetFox.Core.UniversalReporting.Export;

public sealed class ExportProgressReporter : IExportProgressReporter
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHubService _hubService;
    private readonly IWorkQueueLog _workQueueLog;
    private readonly string _username;
    private readonly Guid _reportId;
    private readonly Guid _simulationId;
    private readonly string _reportTypeName;

    public ExportProgressReporter(
        IUnitOfWork unitOfWork,
        IHubService hubService,
        IWorkQueueLog workQueueLog,
        string username,
        Guid reportId,
        Guid simulationId,
        string reportTypeName)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _hubService = hubService ?? throw new ArgumentNullException(nameof(hubService));
        _workQueueLog = workQueueLog ?? throw new ArgumentNullException(nameof(workQueueLog));
        _username = username;
        _reportId = reportId;
        _simulationId = simulationId;
        _reportTypeName = reportTypeName;
    }

    public Task ReportAsync(string status, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _workQueueLog.UpdateWorkQueueStatus(status);

        var detail = new SimulationReportDetailDTO
        {
            ReportId = _reportId,
            SimulationId = _simulationId,
            Status = status,
            ReportType = _reportTypeName
        };

        _unitOfWork.SimulationReportDetailRepo.UpsertSimulationReportDetail(detail);
        _hubService.SendRealTimeMessage(
            _username,
            HubConstant.BroadcastReportGenerationStatus,
            detail,
            _simulationId);

        return Task.CompletedTask;
    }
}
