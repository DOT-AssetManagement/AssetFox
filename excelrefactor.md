# Universal Reporting Refactor — Implementation Plan

## Goal

A single, streaming query pipeline shared by the live viewer and Excel export. All Universal Reporting code lives in a new `AssetFox.Core.UniversalReporting` project, fully decoupled from `AssetFox.Core.Reporting` (which retains the legacy BAMS/PAMS hardcoded generators). The report viewer page gets an "Export to Excel" button that streams progress over the existing SignalR/long-poll plumbing and shows up in the global work queue.

---

## 1. Project & namespace layout

Create **`BridgeCareApp/AssetFox.Core.UniversalReporting/`** as a new C# class library (net8.0) referenced by `AssetFoxCore`, the test project, and the work-queue host. It depends on `AssetFox.Core.DataPersistenceCore`, `AssetFox.Core.Analysis`, `AssetFox.Core.DTOs`, `AssetFox.Core.Hubs`, `AssetFox.Core.Common.Logging`, EPPlus, and `Microsoft.Data.SqlClient`. It depends on `AssetFox.Core.Reporting` **only** for the `IReport`/`ReportType` interfaces needed by the work-queue adapter.

Proposed folder/namespace structure:

```
AssetFox.Core.UniversalReporting/
├── AssetFox.Core.UniversalReporting.csproj
├── Schema/                                          // ── input model (persisted)
│   ├── ReportSchemaDefinition.cs
│   ├── ReportPage.cs
│   ├── ReportComponent.cs
│   └── SchemaParser.cs
├── Components/                                      // ── per-component-type query builders
│   ├── IComponentDataSource.cs                      //    Build + Execute (handles single- and multi-query cases)
│   ├── ComponentDataSourceRegistry.cs               //    type → data source lookup
│   ├── AssetDetailDataSource.cs
│   ├── TreatmentTimelineDataSource.cs
│   ├── WorkSummaryDataSource.cs
│   ├── BudgetSummaryDataSource.cs
│   ├── ConditionTrackingDataSource.cs
│   ├── FundedTreatmentListDataSource.cs
│   ├── GoalTrackingDataSource.cs
│   └── TextBlockDataSource.cs                       //    no-SQL special case
├── Query/                                           // ── shared SQL fragments
│   ├── CriteriaSqlEmitter.cs                        //    moved from Reporting
│   ├── ViewerFilterTranslator.cs                    //    extracted helper
│   ├── SortExpressionBuilder.cs                     //    extracted helper
│   ├── AttributePivotCteBuilder.cs                  //    extracted helper
│   ├── AssetKeyResolver.cs                          //    extracted helper
│   └── WorkSummaryHeaderResolver.cs                 //    moved
├── Execution/                                       // ── runs the queries
│   ├── IUniversalQueryEngine.cs
│   ├── UniversalQueryEngine.cs                      //    one entry point used by both consumers
│   ├── ComponentQuery.cs                            //    { Sql, Parameters, Columns, RequiresInMemoryPagination }
│   ├── ComponentResult.cs                           //    { Columns, Rows | RowStream }
│   ├── PagedQueryExecutor.cs                        //    paged path (viewer)
│   └── StreamingQueryExecutor.cs                    //    unbounded streaming path (export)
├── Transforms/                                      // ── column-set transforms applied after execution
│   ├── IRowTransform.cs
│   ├── PercentOfTotalTransform.cs                   //    includePercentages
│   ├── TotalsRowTransform.cs                        //    showTotals
│   └── TransformPipeline.cs
├── Viewer/                                          // ── HTTP-facing service for the live viewer
│   ├── IViewerDataService.cs
│   ├── ViewerDataService.cs                         //    thin wrapper: paged execute + transforms + DTO map
│   ├── ViewerDataRequest.cs
│   ├── PagedViewerResult.cs
│   └── ViewerColumn.cs
├── Export/                                          // ── Excel generation
│   ├── IUniversalReportExporter.cs
│   ├── UniversalReportExcelExporter.cs              //    iterates schema, streams each component to a worksheet
│   ├── ExcelColumnFormatMap.cs                      //    "currency" → "$#,##0.00", etc.
│   ├── WorksheetNamer.cs                            //    moved sanitize/dedupe logic
│   └── ExportProgressReporter.cs                    //    pushes progress to SignalR + work-queue status
└── Pipeline/                                        // ── public façade + IReport adapter
    ├── UniversalReportRunner.cs                     //    Run(simulationId, schemaId, ct, progress) → file path
    └── UniversalReportLegacyAdapter.cs              //    implements IReport so the work-queue still calls it
```

Namespace root: `AssetFox.Core.UniversalReporting`, with sub-namespaces matching folders.

**Strict rule:** `AssetFox.Core.UniversalReporting` does not reference any FlexibleReport / UniversalReport types in `AssetFox.Core.Reporting`. The reverse is also banned. The only shared touch points are `AssetFox.Core.Reporting`'s `IReport` / `ReportType` interfaces (consumed by the legacy adapter), `AssetFox.Core.DataPersistenceCore` for the `DbContext`, and `AssetFox.Core.Hubs` for SignalR push.

---

## 2. The streaming abstraction (heart of the change)

```csharp
public interface IComponentDataSource
{
    string ComponentType { get; }              // matches ReportComponent.Type, case-insensitive

    // Builder produces the SQL + columns. For single-query components this is straightforward;
    // for components that need orchestration across multiple queries (budget summary,
    // work summary), the data source owns its own composition internally and returns
    // a virtual ComponentQuery whose execution is delegated back to the data source.
    ComponentQuery Build(
        ReportComponent component,
        ViewerDataRequest request,
        int runId,
        AttributeLookup attributes);

    // For multi-query components, the data source produces the final result set itself.
    // For single-query components this is null and the engine takes the default path.
    Func<CancellationToken, IAsyncEnumerable<List<object>>> CustomExecutor => null;
}

public sealed record ComponentQuery(
    string Sql,
    IReadOnlyList<SqlParameter> Parameters,
    IReadOnlyList<ViewerColumn> Columns,
    bool RequiresInMemoryPagination);            // true for work summary (wide pivot)
```

Splitting "build" from "execute" is what unlocks both consumers. Each data source returns a `ComponentQuery` parameterized for whatever request shape it got (`pageSize = ALL` for export, `pageSize = N, page = P` for viewer). The viewer wraps `OFFSET/FETCH` around the query; export executes it raw and streams.

```csharp
public interface IUniversalQueryEngine
{
    Task<PagedViewerResult> ExecutePagedAsync(
        ReportComponent component, ViewerDataRequest request, int runId, CancellationToken ct);

    Task<(IReadOnlyList<ViewerColumn> Columns, IAsyncEnumerable<List<object>> Rows)> StreamAllAsync(
        ReportComponent component, ViewerDataRequest request, int runId, CancellationToken ct);
}
```

`StreamAllAsync` opens one connection per component, executes once, and yields rows as they come off the reader. The exporter writes them straight to an EPPlus worksheet without ever holding the full result set in C#-managed memory at once. For wide-pivot components like work summary that need full materialization in C#, the engine falls back internally to "fully materialize, then yield" — the consumer can't tell the difference, and the doc-comment is honest about the memory profile.

The `IComponentDataSource.CustomExecutor` seam is what handles the two-query components (budget summary needs `BudgetDetail` + an `Allocation` aggregation; work summary needs the long-format SQL plus an in-memory pivot). These data sources own their orchestration and surface a unified row stream to the engine.

---

## 3. Implementation order

### Phase 1 — Stand up the new project (no behavior change)

1. Create `AssetFox.Core.UniversalReporting.csproj` with the dependencies listed above. Add it to `AssetFoxCore.sln`.
2. Move the schema model (`ReportSchemaDefinition`, `ReportPage`, `ReportComponent`, `SchemaParser`) from `AssetFox.Core.Reporting/Models/FlexibleReport/` and `AssetFox.Core.Reporting/Services/FlexibleReport/SchemaParser.cs` into `AssetFox.Core.UniversalReporting/Schema/`. Update namespaces. Update the legacy adapter callsite to import from the new location.
3. Create `ViewerDataRequest`, `PagedViewerResult`, `ViewerColumn` in `Viewer/`. Keep `AssetFox.Core.DTOs.PagedViewerResultDTO` and `ViewerColumnDTO` as wire-compat DTOs at the controller boundary, with a small mapper. The frontend's `report-schema.ts` and `viewer.ts` shapes do not change.
4. Move shared query helpers (`CriteriaSqlEmitter`, the inline `Build*` methods currently embedded in `ViewerDataService`) to `Query/`, renamed to standalone public classes. This is mechanical extraction with no behavior change.

### Phase 2 — Split `ViewerDataService` into per-component data sources

5. For each `GetXxxData` method in the legacy `ViewerDataService`:
   - Extract the SQL-construction logic into a new `XxxDataSource.Build(component, request, runId, attributes)` returning a `ComponentQuery`.
   - The execution half (open connection, execute, read, paginate) collapses into the shared `PagedQueryExecutor`.
   - For two-query components (work summary's pivot, budget summary's spending lookup), the `XxxDataSource` implements `CustomExecutor` and orchestrates internally.
6. Wire all data sources into `ComponentDataSourceRegistry` keyed by `ComponentType`.
7. Replace the legacy `ViewerDataService` with the new thin orchestrator: `var query = registry.For(component.Type).Build(...); return await pagedExecutor.RunAsync(query, request, ct);`. Update DI in `Startup.cs`.
8. Verify viewer behavior end-to-end against the existing test suite. Fix divergences before moving on.

### Phase 3 — Build the streaming exporter

9. Implement `StreamingQueryExecutor` that takes a `ComponentQuery` with `pageSize = int.MaxValue` and yields `IAsyncEnumerable<List<object>>` from the data reader (no `OFFSET/FETCH` wrapper, no `ROW_NUMBER` paging — but the underlying ordering inside the builder's CTEs stays).
10. Implement `UniversalReportExcelExporter`:
    - Iterate `schema.Pages.SelectMany(p => p.Components)` ordered by `Order`.
    - Skip non-visible components.
    - Text blocks → `WorksheetNamer.Name(...)` + write `content` with `style`.
    - All other types → call `engine.StreamAllAsync(component, request, runId, ct)`, get back `(columns, rowStream)`, write headers (with `ExcelColumnFormatMap.Resolve(col.Format)` applied as the column's number format), then `await foreach` rows, writing each one to the worksheet.
    - One worksheet per component. `WorksheetNamer` handles dedupe.
    - Emit progress through the injected `IExportProgressReporter` ("Exporting component 3 of 7: Work Summary").
11. Implement transforms (`PercentOfTotalTransform`, `TotalsRowTransform`) as drain-then-transform passes — they require the full set in memory, so they are explicitly opt-in via component config and the doc-comments call out the memory cost.

### Phase 4 — Wire up the runner and the work-queue adapter

12. `UniversalReportRunner.RunAsync(simulationId, schemaId, ct, progress)` becomes the new public entry: parse schema → resolve sim → exporter → return file path. It accepts an `IExportProgressReporter` so progress flows the same way regardless of who invoked it.
13. `UniversalReportLegacyAdapter : IReport` wraps `UniversalReportRunner` so the existing work-queue and `BroadcastReportGenerationStatus` SignalR plumbing keeps working without changes. Register this adapter under whatever name `FlexibleSchemaReport` currently uses, in the report registration in `Startup.cs`.
14. Delete `FlexibleSchemaReport.cs`, `UniversalReportBuilder.cs`, `DataExtractor.cs`, `UniversalReportModel.cs`, `AggregationEngine.cs` from `AssetFox.Core.Reporting`. Their tests move to a new `AssetFox.Core.UniversalReporting.Tests` project.

### Phase 5 — Frontend: Excel export button on the viewer page

15. **`report-schema.module.ts`** — add a new Vuex action `requestUniversalReportExport({ schemaId, simulationId })` that POSTs to a new backend endpoint and returns the `reportId` it gets back. The action also dispatches the existing `notification.module` channel so the toast/work-queue pipeline lights up the same way as for any other report.
16. **Backend endpoint** in `ReportController` (or a new `UniversalReportController` if cleaner — pick one consistent with how other reports register): `POST /api/report/universal/export`, body `{ schemaId, simulationId }`. Internally it enqueues a work item that runs the legacy adapter, identical to how `FlexibleSchemaReport` was triggered before. It returns the `reportId` immediately so the UI can subscribe to its progress.
17. **`ReportViewer.vue`** — add an "Export to Excel" button to the toolbar. On click:
    - Disable the button and show a small inline spinner.
    - Dispatch `requestUniversalReportExport`.
    - Subscribe to the existing report-progress SignalR channel (`HubConstant.BroadcastReportGenerationStatus`) filtered to the returned `reportId`. Update the inline progress copy ("Building component 3 of 7…") on each push.
    - When `IsComplete && !Errors.Any()`, surface a toast with a download link (mirroring how `BAMSSummary` and `PAMSSummary` exports do it today — find the pattern in `report-builder` or `scenarios/run-output/` and copy it verbatim).
    - On failure, surface the error list from the work item.
18. Confirm the new export item appears in the global **Work Queue** sidebar (`work-queue/` components). Since we route through the same `IReport` + work-queue plumbing as the other reports, this should be free — but verify on the actual UI. If the work-queue list filters by report type name, register the universal-export adapter's `ReportTypeName` in whatever lookup table drives the sidebar.
19. Long-poll fallback: the existing notification pipeline already long-polls when SignalR is unavailable; nothing extra needed as long as the work-queue endpoint returns the same shape.

### Phase 6 — Cleanup

20. Audit `AssetFox.Core.Reporting` for stragglers — anything URM-flavored (`FlexibleAuditReport`, the test fixtures that referenced URM types) gets evaluated. If it's also schema-driven, it moves; if it's a separate hardcoded thing, it stays.
21. Update `overview.md` with the new architecture diagram: the live viewer and Excel export both go through `AssetFox.Core.UniversalReporting`, sharing per-component data sources. Legacy `AssetFox.Core.Reporting` retains BAMS/PAMS hardcoded generators only.

---

## 4. Things that need a decision before code

1. **Where does the export endpoint live?** New `UniversalReportController` or extend existing `ReportController`? Default: extend `ReportController` to keep the surface consistent — one place where reports are kicked off.
2. **Streaming write format with EPPlus.** EPPlus's standard API materializes the workbook in memory before saving. For phase 1 we stick with row-by-row `Cells[r,c].Value = ...` writes; the *workbook* is in memory, but individual *queries* stream. If we hit memory limits on real-world reports, swap to `OpenXmlWriter` from `DocumentFormat.OpenXml` — the swap is local to `UniversalReportExcelExporter`.
3. **`ReportComponent.CriteriaExpression` parity.** Once both consumers share data sources, criteria filtering automatically applies on both paths — but verify per-component-type that the existing data source actually honors `CriteriaExpression`. It's used today on asset detail and treatment timeline; confirm `goalTracking`, `budgetSummary`, `workSummary`, `conditionTracking`, `fundedTreatmentList` either honor it or document why they don't.
4. **Cancellation.** All data sources accept `CancellationToken`; both consumers thread it through. Work-queue integration ties the token to the existing `cancellationToken.Value.IsCancellationRequested` polling. The new runner calls `ct.ThrowIfCancellationRequested()` between components, not just inside SQL execution.
5. **Test seam.** Unit tests at `IComponentDataSource.Build()` (assert SQL/params/columns shape) and at `UniversalReportExcelExporter` (mock `IUniversalQueryEngine`, assert worksheet contents). Integration tests for the full path against a small fixture DB — same fixture shape the existing `ViewerDataServiceTests` use.

---

## 5. Risks

- **`ViewerDataService` is ~1,900 lines.** Splitting it into per-type data sources is the largest single chunk of work. Lean on the existing test suite as a safety net; if the suite has gaps for a given component type, write characterization tests *before* splitting.
- **Two-query components.** `IComponentDataSource.CustomExecutor` is the seam that handles them. Get this interface right before fanning out to all eight component types — once one data source goes through `CustomExecutor`, the others can copy the pattern.
- **Schema JSON in the DB references no namespace** — Newtonsoft just reads structure. Old persisted schemas continue to deserialize fine against the new types in the new namespace.
- **The legacy `IReport` adapter is the only lock-in point.** As long as the adapter exists, nothing in legacy `AssetFox.Core.Reporting` needs to know about UniversalReporting. The dependency arrow points one way: `UniversalReporting → Reporting (interface only) → nothing`.
- **Work-queue UI assumptions.** The sidebar may filter by known report-type names. Verify the new universal-export type registers correctly there before declaring victory.

---

## 6. End state

- `AssetFox.Core.UniversalReporting` is the single home for schema-driven reports — the live viewer's data path, the Excel exporter, the schema model, the per-component query builders, and the runner/adapter.
- `AssetFox.Core.Reporting` shrinks to the legacy hardcoded BAMS/PAMS/audit generators plus the shared `IReport` interface contract.
- The viewer page has an "Export to Excel" button that produces the same file the work-queue would produce, surfaces progress live, and shows up in the global work-queue sidebar like every other report.
- Adding a new component type means: write one `IComponentDataSource`, register it. Both the viewer and the exporter pick it up automatically.
- Adding a new column-level transform (totals, percentages, future cumulative-sum) means: write one `IRowTransform`, opt-in via component config. Applies to both consumers.
