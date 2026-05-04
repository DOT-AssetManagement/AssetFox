using AssetFox.Core.DataPersistenceCore.UnitOfWork;
using AssetFox.Core.DTOs;
using AssetFox.Core.UniversalReporting.Execution;
using AssetFox.Core.UniversalReporting.Query;
using AssetFox.Core.UniversalReporting.Schema;
using AssetFox.Core.UniversalReporting.Viewer;
using Microsoft.EntityFrameworkCore;

namespace AssetFox.Core.UniversalReporting.Components
{
    public class GoalTrackingDataSource : ComponentDataSourceBase, IComponentDataSource
    {
        public GoalTrackingDataSource(UnitOfDataPersistenceWork unitOfWork, CriteriaSqlEmitter criteriaEmitter)
            : base(unitOfWork, criteriaEmitter)
        {
        }

        public string ComponentType => "goalTracking";

        public ComponentQuery Build(
            ReportComponent component,
            ViewerDataRequest request,
            int runId,
            AttributeLookup attributes)
        {
            var config = component.Config ?? new Dictionary<string, object>();
            var showGoalName = GetConfigBool(config, "showGoalName", true);
            var showActualVsTarget = GetConfigBool(config, "showActualVsTarget", true);
            var showMetStatus = GetConfigBool(config, "showMetStatus", true);

            var columns = new List<ViewerColumnDTO>();
            if (showGoalName)
            {
                columns.Add(new ViewerColumnDTO { Key = "goalName", Header = "Goal", Format = "text" });
            }

            columns.Add(new ViewerColumnDTO { Key = "year", Header = "Year", Format = "year" });
            if (showActualVsTarget)
            {
                columns.Add(new ViewerColumnDTO { Key = "actual", Header = "Actual", Format = "number" });
                columns.Add(new ViewerColumnDTO { Key = "target", Header = "Target", Format = "number" });
            }

            if (showMetStatus)
            {
                columns.Add(new ViewerColumnDTO { Key = "goalIsMet", Header = "Met?", Format = "text" });
            }

            return new ComponentQuery(
                string.Empty,
                Array.Empty<Microsoft.Data.SqlClient.SqlParameter>(),
                columns.Select(ToViewerColumn).ToList(),
                RequiresInMemoryPagination: true,
                cancellationToken => ReadRowsAsync(component, request, runId, columns, cancellationToken));
        }

        private async IAsyncEnumerable<List<object>> ReadRowsAsync(
            ReportComponent component,
            ViewerDataRequest request,
            int runId,
            List<ViewerColumnDTO> columns,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var config = component.Config ?? new Dictionary<string, object>();
            var goalType = GetConfigString(config, "goalType", "target");
            var showGoalName = GetConfigBool(config, "showGoalName", true);
            var showActualVsTarget = GetConfigBool(config, "showActualVsTarget", true);
            var showMetStatus = GetConfigBool(config, "showMetStatus", true);
            var rows = new List<List<object>>();

            if (goalType == "target")
            {
                var goals = await _unitOfWork.Context.TargetConditionGoalDetail
                    .AsNoTracking()
                    .Where(g => g.RunId == runId)
                    .Join(_unitOfWork.Context.SimulationYearDetail.AsNoTracking().Where(y => y.RunId == runId),
                        g => g.SimulationYearDetailId,
                        y => y.Id,
                        (g, y) => new { g.GoalName, y.Year, g.ActualValue, g.TargetValue, g.GoalIsMet })
                    .OrderBy(x => x.GoalName)
                    .ThenBy(x => x.Year)
                    .ToListAsync(cancellationToken);

                foreach (var goal in goals)
                {
                    var row = new List<object>();
                    if (showGoalName) row.Add(goal.GoalName ?? "");
                    row.Add(goal.Year);
                    if (showActualVsTarget) { row.Add(goal.ActualValue); row.Add(goal.TargetValue); }
                    if (showMetStatus) row.Add(goal.GoalIsMet ? "Yes" : "No");
                    rows.Add(row);
                }
            }
            else
            {
                var goals = await _unitOfWork.Context.DeficientConditionGoalDetail
                    .AsNoTracking()
                    .Where(g => g.RunId == runId)
                    .Join(_unitOfWork.Context.SimulationYearDetail.AsNoTracking().Where(y => y.RunId == runId),
                        g => g.SimulationYearDetailId,
                        y => y.Id,
                        (g, y) => new { g.GoalName, y.Year, g.ActualDeficientPercentage, g.AllowedDeficientPercentage, g.GoalIsMet })
                    .OrderBy(x => x.GoalName)
                    .ThenBy(x => x.Year)
                    .ToListAsync(cancellationToken);

                foreach (var goal in goals)
                {
                    var row = new List<object>();
                    if (showGoalName) row.Add(goal.GoalName ?? "");
                    row.Add(goal.Year);
                    if (showActualVsTarget) { row.Add(goal.ActualDeficientPercentage); row.Add(goal.AllowedDeficientPercentage); }
                    if (showMetStatus) row.Add(goal.GoalIsMet ? "Yes" : "No");
                    rows.Add(row);
                }
            }

            foreach (var row in ApplyInMemoryFilterAndSort(columns, rows, request))
            {
                yield return row;
            }
        }
    }
}
