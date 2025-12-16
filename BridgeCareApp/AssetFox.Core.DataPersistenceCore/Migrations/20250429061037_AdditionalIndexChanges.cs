using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssetFox.Core.DataPersistenceCore.Migrations
{
    /// <inheritdoc />
    public partial class AdditionalIndexChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // -----------------------------------------------------------------
            // 1.  DROP every FK that references a key we are about to reorder
            // -----------------------------------------------------------------
            migrationBuilder.Sql(@"
                /* ===== FK DROPS ======================================================== */
                ALTER TABLE [dbo].[Allocation]                        DROP CONSTRAINT [FK_Allocation_FundingCalculationOutput_FundingCalculationOutputId];
                ALTER TABLE [dbo].[AssetDetail]                       DROP CONSTRAINT [FK_AssetDetail_MaintainableAsset_MaintainableAssetId];
                ALTER TABLE [dbo].[AssetDetail]                       DROP CONSTRAINT [FK_AssetDetail_SimulationYearDetail_SimulationYearDetailId];
                ALTER TABLE [dbo].[AssetDetailValueIntId]             DROP CONSTRAINT [FK_AssetDetailValueIntId_AssetDetail_AssetDetailId];
                ALTER TABLE [dbo].[AssetDetailValueIntId]             DROP CONSTRAINT [FK_AssetDetailValueIntId_Attribute_AttributeId];
                ALTER TABLE [dbo].[AssetSummaryDetail]                DROP CONSTRAINT [FK_AssetSummaryDetail_MaintainableAsset_MaintainableAssetId];
                ALTER TABLE [dbo].[AssetSummaryDetail]                DROP CONSTRAINT [FK_AssetSummaryDetail_SimulationOutput_SimulationOutputId];
                ALTER TABLE [dbo].[AssetSummaryDetailValueIntId]      DROP CONSTRAINT [FK_AssetSummaryDetailValueIntId_AssetSummaryDetail_AssetSummaryDetailId];
                ALTER TABLE [dbo].[AssetSummaryDetailValueIntId]      DROP CONSTRAINT [FK_AssetSummaryDetailValueIntId_Attribute_AttributeId];
                ALTER TABLE [dbo].[BudgetToSpend]                     DROP CONSTRAINT [FK_BudgetToSpend_FundingCalculationInput_FundingCalculationInputId];
                ALTER TABLE [dbo].[CashFlowConsiderationDetail]       DROP CONSTRAINT [FK_CashFlowConsiderationDetail_TreatmentConsiderationDetail_TreatmentConsiderationDetailId];
                ALTER TABLE [dbo].[DeficientConditionGoalDetail]      DROP CONSTRAINT [FK_DeficientConditionGoalDetail_Attribute_AttributeId];
                ALTER TABLE [dbo].[DeficientConditionGoalDetail]      DROP CONSTRAINT [FK_DeficientConditionGoalDetail_SimulationYearDetail_SimulationYearDetailId];
                ALTER TABLE [dbo].[FundingCalculationInput]           DROP CONSTRAINT [FK_FundingCalculationInput_TreatmentConsiderationDetail_TreatmentConsiderationDetailId];
                ALTER TABLE [dbo].[FundingCalculationOutput]          DROP CONSTRAINT [FK_FundingCalculationOutput_TreatmentConsiderationDetail_TreatmentConsiderationDetailId];
                ALTER TABLE [dbo].[BudgetDetail]                      DROP CONSTRAINT [FK_BudgetDetail_SimulationYearDetail_SimulationYearDetailId];
                ALTER TABLE [dbo].[SimulationYearDetail]             DROP CONSTRAINT [FK_SimulationYearDetail_SimulationOutput_SimulationOutputId];
                ALTER TABLE [dbo].[TargetConditionGoalDetail]         DROP CONSTRAINT [FK_TargetConditionGoalDetail_Attribute_AttributeId];
                ALTER TABLE [dbo].[TargetConditionGoalDetail]         DROP CONSTRAINT [FK_TargetConditionGoalDetail_SimulationYearDetail_SimulationYearDetailId];
                ALTER TABLE [dbo].[TreatmentConsiderationDetail]      DROP CONSTRAINT [FK_TreatmentConsiderationDetail_AssetDetail_AssetDetailId];
                ALTER TABLE [dbo].[TreatmentOptionDetail]             DROP CONSTRAINT [FK_TreatmentOptionDetail_AssetDetail_AssetDetailId];
                ALTER TABLE [dbo].[TreatmentRejectionDetail]          DROP CONSTRAINT [FK_TreatmentRejectionDetail_AssetDetail_AssetDetailId];
                ALTER TABLE [dbo].[TreatmentSchedulingCollisionDetail]DROP CONSTRAINT [FK_TreatmentSchedulingCollisionDetail_AssetDetail_AssetDetailId];

                
                ");

                //drop existing indexes
                migrationBuilder.Sql(@"
                    DROP INDEX IF EXISTS [CI_AssetDetailValueIntId] ON [dbo].[AssetDetailValueIntId];
                    DROP INDEX IF EXISTS [IX_AssetDetailValueIntId_AssetDetailId]     ON [dbo].[AssetDetailValueIntId];
                    DROP INDEX IF EXISTS [CI_AssetSummaryDetailValueIntId] ON [dbo].[AssetSummaryDetailValueIntId];
                    DROP INDEX IF EXISTS [IX_AssetSummaryDetailValueIntId_AssetSummaryDetailId] ON [dbo].[AssetSummaryDetailValueIntId];
                    DROP INDEX IF EXISTS [IX_AssetSummaryDetailValueIntId_AttributeId]         ON [dbo].[AssetSummaryDetailValueIntId];
                    DROP INDEX IF EXISTS [CI_TreatmentRejectionDetail] ON [dbo].[TreatmentRejectionDetail];
                    DROP INDEX IF EXISTS [IX_TreatmentRejectionDetail_Id]               ON [dbo].[TreatmentRejectionDetail];
                    DROP INDEX IF EXISTS [IX_TreatmentRejectionDetail_AssetDetailId]    ON [dbo].[TreatmentRejectionDetail];
                    DROP INDEX IF EXISTS [CI_BudgetToSpend] ON [dbo].[BudgetToSpend];
                    DROP INDEX IF EXISTS [IX_BudgetToSpend_FundingCalculationInputId] ON [dbo].[BudgetToSpend];
                    DROP INDEX IF EXISTS [CI_AssetDetail] ON [dbo].[AssetDetail];
                    DROP INDEX IF EXISTS [IX_AssetDetail_MaintainableAsset_MaintainableAssetId] ON [dbo].[AssetDetail];
                    DROP INDEX IF EXISTS [IX_AssetDetail_MaintainableAssetId] ON [dbo].[AssetDetail];
                    DROP INDEX IF EXISTS [IX_AssetDetail_SimulationYearDetail_SimulationYearDetailId] ON [dbo].[AssetDetail];
                    DROP INDEX IF EXISTS [IX_AssetDetail_SimulationYearDetailId] ON [dbo].[AssetDetail];
                    DROP INDEX IF EXISTS [CI_AssetSummaryDetail] ON [dbo].[AssetSummaryDetail];
                    DROP INDEX IF EXISTS [IX_AssetSummaryDetail_MaintainableAssetId]           ON [dbo].[AssetSummaryDetail];
                    DROP INDEX IF EXISTS [IX_AssetSummaryDetail_SimulationOutput_SimulationOutputId] ON [dbo].[AssetSummaryDetail];
                    DROP INDEX IF EXISTS [IX_AssetSummaryDetail_SimulationOutputId]            ON [dbo].[AssetSummaryDetail];
                    DROP INDEX IF EXISTS [CI_Allocation] ON [dbo].[Allocation];
                    DROP INDEX IF EXISTS [IX_Allocation_FundingCalculationOutputId] ON [dbo].[Allocation];
                    DROP INDEX IF EXISTS [CI_BudgetDetail] ON [dbo].[BudgetDetail];
                    DROP INDEX IF EXISTS [IX_BudgetDetail_Id]                  ON [dbo].[BudgetDetail];
                    DROP INDEX IF EXISTS [IX_BudgetDetail_SimulationYearDetailId] ON [dbo].[BudgetDetail];
                    DROP INDEX IF EXISTS [CI_DeficientConditionGoalDetail] ON [dbo].[DeficientConditionGoalDetail];
                    DROP INDEX IF EXISTS [IX_DeficientConditionGoalDetail_Id]            ON [dbo].[DeficientConditionGoalDetail];
                    DROP INDEX IF EXISTS [IX_DeficientConditionGoalDetail_AttributeId]   ON [dbo].[DeficientConditionGoalDetail];
                    DROP INDEX IF EXISTS [IX_DeficientConditionGoalDetail_SimulationYearDetailId] ON [dbo].[DeficientConditionGoalDetail];
                    DROP INDEX IF EXISTS [CI_CashFlowConsiderationDetail]            ON [dbo].[CashFlowConsiderationDetail];
                    DROP INDEX IF EXISTS [IX_CashFlowConsiderationDetail_Id]         ON [dbo].[CashFlowConsiderationDetail];
                    DROP INDEX IF EXISTS [IX_CashFlowConsiderationDetail_TreatmentConsiderationDetailId] ON [dbo].[CashFlowConsiderationDetail];
                    DROP INDEX IF EXISTS [CI_FundingCalculationInput] ON [dbo].[FundingCalculationInput];
                    DROP INDEX IF EXISTS [IX_FundingCalculationInput_TreatmentConsiderationDetailId] ON [dbo].[FundingCalculationInput];
                    DROP INDEX IF EXISTS [CI_FundingCalculationOutput] ON [dbo].[FundingCalculationOutput];
                    DROP INDEX IF EXISTS [IX_FundingCalculationOutput_TreatmentConsiderationDetailId] ON [dbo].[FundingCalculationOutput];
                    DROP INDEX IF EXISTS [CI_SimulationYearDetail] ON [dbo].[SimulationYearDetail];
                    DROP INDEX IF EXISTS [IX_SimulationYearDetail_SimulationOutputId] ON [dbo].[SimulationYearDetail];
                    DROP INDEX IF EXISTS [CI_TargetConditionGoalDetail] ON [dbo].[TargetConditionGoalDetail];
                    DROP INDEX IF EXISTS [IX_TargetConditionGoalDetail_Id]                  ON [dbo].[TargetConditionGoalDetail];
                    DROP INDEX IF EXISTS [IX_TargetConditionGoalDetail_AttributeId]         ON [dbo].[TargetConditionGoalDetail];
                    DROP INDEX IF EXISTS [IX_TargetConditionGoalDetail_SimulationYearDetailId] ON [dbo].[TargetConditionGoalDetail];
                    DROP INDEX IF EXISTS [CI_TreatmentConsiderationDetail] ON [dbo].[TreatmentConsiderationDetail];
                    DROP INDEX IF EXISTS [IX_TreatmentConsiderationDetail_Id]          ON [dbo].[TreatmentConsiderationDetail];
                    DROP INDEX IF EXISTS [IX_TreatmentConsiderationDetail_AssetDetailId] ON [dbo].[TreatmentConsiderationDetail];
                    DROP INDEX IF EXISTS [CI_TreatmentOptionDetail] ON [dbo].[TreatmentOptionDetail];
                    DROP INDEX IF EXISTS [IX_TreatmentOptionDetail_Id]              ON [dbo].[TreatmentOptionDetail];
                    DROP INDEX IF EXISTS [IX_TreatmentOptionDetail_AssetDetailId]   ON [dbo].[TreatmentOptionDetail];
                    DROP INDEX IF EXISTS [CI_TreatmentSchedulingCollisionDetail] ON [dbo].[TreatmentSchedulingCollisionDetail];
                    DROP INDEX IF EXISTS [IX_TreatmentSchedulingCollisionDetail_Id]            ON [dbo].[TreatmentSchedulingCollisionDetail];
                    DROP INDEX IF EXISTS [IX_TreatmentSchedulingCollisionDetail_AssetDetailId] ON [dbo].[TreatmentSchedulingCollisionDetail];
                ");

                //modify necessary columns for columnstore rules
                migrationBuilder.Sql(@"
                    ALTER TABLE dbo.AssetDetailValueIntId
                    ALTER COLUMN TextValue NVARCHAR(4000) NULL;

                    ALTER TABLE dbo.AssetSummaryDetailValueIntId
                    ALTER COLUMN TextValue NVARCHAR(4000) NULL;

                    ALTER TABLE dbo.TreatmentRejectionDetail
                    ALTER COLUMN TreatmentName NVARCHAR(4000) NULL;

                    ALTER TABLE dbo.BudgetToSpend
                    ALTER COLUMN Name NVARCHAR(4000) NULL;

                    ALTER TABLE dbo.AssetDetail
                    ALTER COLUMN AppliedTreatment NVARCHAR(4000) NULL;

                    ALTER TABLE dbo.AssetDetail
                    ALTER COLUMN ProjectSource NVARCHAR(4000) NULL;

                    ALTER TABLE dbo.Allocation
                    ALTER COLUMN BudgetName NVARCHAR(4000) NULL;

                    ALTER TABLE dbo.Allocation
                    ALTER COLUMN TreatmentName NVARCHAR(4000) NULL;

                    ALTER TABLE dbo.BudgetDetail
                    ALTER COLUMN BudgetName NVARCHAR(4000) NULL;

                    ALTER TABLE dbo.CashFlowConsiderationDetail
                    ALTER COLUMN CashFlowRuleName NVARCHAR(4000) NULL;

                    ALTER TABLE dbo.DeficientConditionGoalDetail
                    ALTER COLUMN GoalName NVARCHAR(4000) NULL;

                    ALTER TABLE dbo.TargetConditionGoalDetail
                    ALTER COLUMN GoalName NVARCHAR(4000) NULL;

                    ALTER TABLE dbo.TreatmentConsiderationDetail
                    ALTER COLUMN TreatmentName NVARCHAR(4000) NULL;

                    ALTER TABLE dbo.TreatmentOptionDetail
                    ALTER COLUMN TreatmentName NVARCHAR(4000) NULL;

                    ALTER TABLE dbo.TreatmentSchedulingCollisionDetail
                    ALTER COLUMN NameOfUnscheduledTreatment NVARCHAR(4000) NULL;
                ");

            //add sequential ids
            /*
                migrationBuilder.Sql(@"
                  DECLARE @df sysname = (
                    SELECT dc.name
                      FROM sys.default_constraints dc
                      JOIN sys.columns    c  ON dc.parent_object_id=c.object_id AND dc.parent_column_id=c.column_id
                     WHERE c.object_id = OBJECT_ID('dbo.TreatmentRejectionDetail')
                       AND c.name      = 'Id'
                  );
                  IF @df IS NOT NULL
                    EXEC('ALTER TABLE dbo.TreatmentRejectionDetail DROP CONSTRAINT ' + @df);
                ");

                migrationBuilder.Sql(@"
                  ALTER TABLE dbo.TreatmentRejectionDetail
                    ADD CONSTRAINT DF_TreatmentRejectionDetail_Id
                    DEFAULT NEWSEQUENTIALID() FOR Id;
                ");

            migrationBuilder.Sql(@"
                  DECLARE @df sysname = (
                    SELECT dc.name
                      FROM sys.default_constraints dc
                      JOIN sys.columns    c  ON dc.parent_object_id=c.object_id AND dc.parent_column_id=c.column_id
                     WHERE c.object_id = OBJECT_ID('dbo.BudgetToSpend')
                       AND c.name      = 'Id'
                  );
                  IF @df IS NOT NULL
                    EXEC('ALTER TABLE dbo.BudgetToSpend DROP CONSTRAINT ' + @df);
                ");

            migrationBuilder.Sql(@"
                  ALTER TABLE dbo.BudgetToSpend
                    ADD CONSTRAINT DF_BudgetToSpend_Id
                    DEFAULT NEWSEQUENTIALID() FOR Id;
                ");

            migrationBuilder.Sql(@"
                  DECLARE @df sysname = (
                    SELECT dc.name
                      FROM sys.default_constraints dc
                      JOIN sys.columns    c  ON dc.parent_object_id=c.object_id AND dc.parent_column_id=c.column_id
                     WHERE c.object_id = OBJECT_ID('dbo.AssetDetail')
                       AND c.name      = 'Id'
                  );
                  IF @df IS NOT NULL
                    EXEC('ALTER TABLE dbo.AssetDetail DROP CONSTRAINT ' + @df);
                ");

            migrationBuilder.Sql(@"
                  ALTER TABLE dbo.AssetDetail
                    ADD CONSTRAINT DF_AssetDetail_Id
                    DEFAULT NEWSEQUENTIALID() FOR Id;
                ");

            migrationBuilder.Sql(@"
                  DECLARE @df sysname = (
                    SELECT dc.name
                      FROM sys.default_constraints dc
                      JOIN sys.columns    c  ON dc.parent_object_id=c.object_id AND dc.parent_column_id=c.column_id
                     WHERE c.object_id = OBJECT_ID('dbo.AssetSummaryDetail')
                       AND c.name      = 'Id'
                  );
                  IF @df IS NOT NULL
                    EXEC('ALTER TABLE dbo.AssetSummaryDetail DROP CONSTRAINT ' + @df);
                ");

            migrationBuilder.Sql(@"
                  ALTER TABLE dbo.AssetSummaryDetail
                    ADD CONSTRAINT DF_AssetSummaryDetail_Id
                    DEFAULT NEWSEQUENTIALID() FOR Id;
                ");

            migrationBuilder.Sql(@"
                  DECLARE @df sysname = (
                    SELECT dc.name
                      FROM sys.default_constraints dc
                      JOIN sys.columns    c  ON dc.parent_object_id=c.object_id AND dc.parent_column_id=c.column_id
                     WHERE c.object_id = OBJECT_ID('dbo.Allocation')
                       AND c.name      = 'Id'
                  );
                  IF @df IS NOT NULL
                    EXEC('ALTER TABLE dbo.Allocation DROP CONSTRAINT ' + @df);
                ");

            migrationBuilder.Sql(@"
                  ALTER TABLE dbo.Allocation
                    ADD CONSTRAINT DF_Allocation_Id
                    DEFAULT NEWSEQUENTIALID() FOR Id;
                ");

            migrationBuilder.Sql(@"
                  DECLARE @df sysname = (
                    SELECT dc.name
                      FROM sys.default_constraints dc
                      JOIN sys.columns    c  ON dc.parent_object_id=c.object_id AND dc.parent_column_id=c.column_id
                     WHERE c.object_id = OBJECT_ID('dbo.BudgetDetail')
                       AND c.name      = 'Id'
                  );
                  IF @df IS NOT NULL
                    EXEC('ALTER TABLE dbo.BudgetDetail DROP CONSTRAINT ' + @df);
                ");

            migrationBuilder.Sql(@"
                  ALTER TABLE dbo.BudgetDetail
                    ADD CONSTRAINT DF_BudgetDetail_Id
                    DEFAULT NEWSEQUENTIALID() FOR Id;
                ");

            migrationBuilder.Sql(@"
                  DECLARE @df sysname = (
                    SELECT dc.name
                      FROM sys.default_constraints dc
                      JOIN sys.columns    c  ON dc.parent_object_id=c.object_id AND dc.parent_column_id=c.column_id
                     WHERE c.object_id = OBJECT_ID('dbo.DeficientConditionGoalDetail')
                       AND c.name      = 'Id'
                  );
                  IF @df IS NOT NULL
                    EXEC('ALTER TABLE dbo.DeficientConditionGoalDetail DROP CONSTRAINT ' + @df);
                ");

            migrationBuilder.Sql(@"
                  ALTER TABLE dbo.DeficientConditionGoalDetail
                    ADD CONSTRAINT DF_DeficientConditionGoalDetail_Id
                    DEFAULT NEWSEQUENTIALID() FOR Id;
                ");

            migrationBuilder.Sql(@"
                  DECLARE @df sysname = (
                    SELECT dc.name
                      FROM sys.default_constraints dc
                      JOIN sys.columns    c  ON dc.parent_object_id=c.object_id AND dc.parent_column_id=c.column_id
                     WHERE c.object_id = OBJECT_ID('dbo.CashFlowConsiderationDetail')
                       AND c.name      = 'Id'
                  );
                  IF @df IS NOT NULL
                    EXEC('ALTER TABLE dbo.CashFlowConsiderationDetail DROP CONSTRAINT ' + @df);
                ");

            migrationBuilder.Sql(@"
                  ALTER TABLE dbo.CashFlowConsiderationDetail
                    ADD CONSTRAINT DF_CashFlowConsiderationDetail_Id
                    DEFAULT NEWSEQUENTIALID() FOR Id;
                ");

            migrationBuilder.Sql(@"
                  DECLARE @df sysname = (
                    SELECT dc.name
                      FROM sys.default_constraints dc
                      JOIN sys.columns    c  ON dc.parent_object_id=c.object_id AND dc.parent_column_id=c.column_id
                     WHERE c.object_id = OBJECT_ID('dbo.FundingCalculationInput')
                       AND c.name      = 'Id'
                  );
                  IF @df IS NOT NULL
                    EXEC('ALTER TABLE dbo.FundingCalculationInput DROP CONSTRAINT ' + @df);
                ");

            migrationBuilder.Sql(@"
                  ALTER TABLE dbo.FundingCalculationInput
                    ADD CONSTRAINT DF_FundingCalculationInput_Id
                    DEFAULT NEWSEQUENTIALID() FOR Id;
                ");

            migrationBuilder.Sql(@"
                  DECLARE @df sysname = (
                    SELECT dc.name
                      FROM sys.default_constraints dc
                      JOIN sys.columns    c  ON dc.parent_object_id=c.object_id AND dc.parent_column_id=c.column_id
                     WHERE c.object_id = OBJECT_ID('dbo.FundingCalculationOutput')
                       AND c.name      = 'Id'
                  );
                  IF @df IS NOT NULL
                    EXEC('ALTER TABLE dbo.FundingCalculationOutput DROP CONSTRAINT ' + @df);
                ");

            migrationBuilder.Sql(@"
                  ALTER TABLE dbo.FundingCalculationOutput
                    ADD CONSTRAINT DF_FundingCalculationOutput_Id
                    DEFAULT NEWSEQUENTIALID() FOR Id;
                ");

            migrationBuilder.Sql(@"
                  DECLARE @df sysname = (
                    SELECT dc.name
                      FROM sys.default_constraints dc
                      JOIN sys.columns    c  ON dc.parent_object_id=c.object_id AND dc.parent_column_id=c.column_id
                     WHERE c.object_id = OBJECT_ID('dbo.SimulationYearDetail')
                       AND c.name      = 'Id'
                  );
                  IF @df IS NOT NULL
                    EXEC('ALTER TABLE dbo.SimulationYearDetail DROP CONSTRAINT ' + @df);
                ");

            migrationBuilder.Sql(@"
                  ALTER TABLE dbo.SimulationYearDetail
                    ADD CONSTRAINT DF_SimulationYearDetail_Id
                    DEFAULT NEWSEQUENTIALID() FOR Id;
                ");

            migrationBuilder.Sql(@"
                  DECLARE @df sysname = (
                    SELECT dc.name
                      FROM sys.default_constraints dc
                      JOIN sys.columns    c  ON dc.parent_object_id=c.object_id AND dc.parent_column_id=c.column_id
                     WHERE c.object_id = OBJECT_ID('dbo.TargetConditionGoalDetail')
                       AND c.name      = 'Id'
                  );
                  IF @df IS NOT NULL
                    EXEC('ALTER TABLE dbo.TargetConditionGoalDetail DROP CONSTRAINT ' + @df);
                ");

            migrationBuilder.Sql(@"
                  ALTER TABLE dbo.TargetConditionGoalDetail
                    ADD CONSTRAINT DF_TargetConditionGoalDetail_Id
                    DEFAULT NEWSEQUENTIALID() FOR Id;
                ");

            migrationBuilder.Sql(@"
                  DECLARE @df sysname = (
                    SELECT dc.name
                      FROM sys.default_constraints dc
                      JOIN sys.columns    c  ON dc.parent_object_id=c.object_id AND dc.parent_column_id=c.column_id
                     WHERE c.object_id = OBJECT_ID('dbo.TreatmentConsiderationDetail')
                       AND c.name      = 'Id'
                  );
                  IF @df IS NOT NULL
                    EXEC('ALTER TABLE dbo.TreatmentConsiderationDetail DROP CONSTRAINT ' + @df);
                ");

            migrationBuilder.Sql(@"
                  ALTER TABLE dbo.TreatmentConsiderationDetail
                    ADD CONSTRAINT DF_TreatmentConsiderationDetail_Id
                    DEFAULT NEWSEQUENTIALID() FOR Id;
                ");

            migrationBuilder.Sql(@"
                  DECLARE @df sysname = (
                    SELECT dc.name
                      FROM sys.default_constraints dc
                      JOIN sys.columns    c  ON dc.parent_object_id=c.object_id AND dc.parent_column_id=c.column_id
                     WHERE c.object_id = OBJECT_ID('dbo.TreatmentOptionDetail')
                       AND c.name      = 'Id'
                  );
                  IF @df IS NOT NULL
                    EXEC('ALTER TABLE dbo.TreatmentOptionDetail DROP CONSTRAINT ' + @df);
                ");

            migrationBuilder.Sql(@"
                  ALTER TABLE dbo.TreatmentOptionDetail
                    ADD CONSTRAINT DF_TreatmentOptionDetail_Id
                    DEFAULT NEWSEQUENTIALID() FOR Id;
                ");

            migrationBuilder.Sql(@"
                  DECLARE @df sysname = (
                    SELECT dc.name
                      FROM sys.default_constraints dc
                      JOIN sys.columns    c  ON dc.parent_object_id=c.object_id AND dc.parent_column_id=c.column_id
                     WHERE c.object_id = OBJECT_ID('dbo.TreatmentSchedulingCollisionDetail')
                       AND c.name      = 'Id'
                  );
                  IF @df IS NOT NULL
                    EXEC('ALTER TABLE dbo.TreatmentSchedulingCollisionDetail DROP CONSTRAINT ' + @df);
                ");

            migrationBuilder.Sql(@"
                  ALTER TABLE dbo.TreatmentSchedulingCollisionDetail
                    ADD CONSTRAINT DF_TreatmentSchedulingCollisionDetail_Id
                    DEFAULT NEWSEQUENTIALID() FOR Id;
                ");*/


            // -----------------------------------------------------------------
            // 2.  FOR EACH TABLE:
            //       • Drop the old PK 
            //       • Create the new PK (RunId,?Id) CLUSTERED
            //       • Create ONE NCCI on (RunId,?<parent-FK>)
            // -----------------------------------------------------------------
            migrationBuilder.Sql(@"
                /* ===== TABLE: Allocation ============================================== */
                ALTER TABLE [dbo].[Allocation] DROP CONSTRAINT [PK_Allocation];
                

                ALTER TABLE [dbo].[Allocation]
                    ADD CONSTRAINT [PK_Allocation]
                        PRIMARY KEY CLUSTERED ([RunId] ASC, [Id] ASC)
                        ON [PS_SimulationRun]([RunId]);

                CREATE NONCLUSTERED COLUMNSTORE INDEX NCCI_Allocation
                  ON dbo.Allocation
                    ([RunId], [Id], [Year], [BudgetName], [TreatmentName], [AllocatedAmount], [FundingCalculationOutputId])
                  ON PS_SimulationRun(RunId);

                /* ===== TABLE: AssetDetail ============================================= */
                ALTER TABLE [dbo].[AssetDetail] DROP CONSTRAINT [PK_AssetDetail];

                ALTER TABLE [dbo].[AssetDetail]
                    ADD CONSTRAINT [PK_AssetDetail]
                        PRIMARY KEY CLUSTERED ([RunId] ASC, [Id] ASC)
                        ON [PS_SimulationRun]([RunId]);

                CREATE NONCLUSTERED COLUMNSTORE INDEX NCCI_AssetDetail
                  ON dbo.AssetDetail
                    ([RunId], [Id], [MaintainableAssetId], [SimulationYearDetailId], [AppliedTreatment], [TreatmentCause], [TreatmentFundingIgnoresSpendingLimit], [TreatmentStatus], [ProjectSource])
                  ON PS_SimulationRun(RunId);

                /* ===== TABLE: AssetDetailValueIntId =================================== */
                ALTER TABLE [dbo].[AssetDetailValueIntId] DROP CONSTRAINT [PK_AssetDetailValueIntId];

                DECLARE @df sysname = (
                    SELECT dc.name
                    FROM   sys.default_constraints dc
                    JOIN   sys.columns c ON dc.parent_object_id = c.object_id
                                         AND dc.parent_column_id = c.column_id
                    WHERE  c.object_id = OBJECT_ID('dbo.AssetDetailValueIntId')
                      AND  c.name      = 'Id');
                IF @df IS NOT NULL EXEC('ALTER TABLE dbo.AssetDetailValueIntId DROP CONSTRAINT '+@df);

                ALTER TABLE dbo.AssetDetailValueIntId DROP COLUMN Id;

                ALTER TABLE dbo.AssetDetailValueIntId
                  ADD Id BIGINT IDENTITY(1,1) NOT NULL;
                
                ALTER TABLE [dbo].[AssetDetailValueIntId]
                    ADD CONSTRAINT [PK_AssetDetailValueIntId]
                        PRIMARY KEY CLUSTERED ([RunId] ASC, [Id] ASC)
                        ON [PS_SimulationRun]([RunId]);

                CREATE NONCLUSTERED COLUMNSTORE INDEX NCCI_AssetDetailValue
                  ON dbo.AssetDetailValueIntId
                    ([RunId], [Id], [AssetDetailId], [AttributeId], [Discriminator], [NumericValue], [TextValue])
                  ON PS_SimulationRun(RunId);

                /* ===== TABLE: AssetSummaryDetail ====================================== */
                ALTER TABLE [dbo].[AssetSummaryDetail] DROP CONSTRAINT [PK_AssetSummaryDetail];

                ALTER TABLE [dbo].[AssetSummaryDetail]
                    ADD CONSTRAINT [PK_AssetSummaryDetail]
                        PRIMARY KEY CLUSTERED ([RunId] ASC, [Id] ASC)
                        ON [PS_SimulationRun]([RunId]);

                CREATE NONCLUSTERED COLUMNSTORE INDEX NCCI_AssetSummarDetail
                  ON dbo.AssetSummaryDetail
                    ([RunId], [Id], [MaintainableAssetId], [SimulationOutputId])
                  ON PS_SimulationRun(RunId);

                /* ===== TABLE: AssetSummaryDetailValueIntId ============================ */
                ALTER TABLE [dbo].[AssetSummaryDetailValueIntId] DROP CONSTRAINT [PK_AssetSummaryDetailValueIntId];

                DECLARE @df2 sysname = (
                    SELECT dc.name
                    FROM   sys.default_constraints dc
                    JOIN   sys.columns c ON dc.parent_object_id = c.object_id
                                         AND dc.parent_column_id = c.column_id
                    WHERE  c.object_id = OBJECT_ID('dbo.AssetSummaryDetailValueIntId')
                      AND  c.name      = 'Id');
                IF @df2 IS NOT NULL EXEC('ALTER TABLE dbo.AssetSummaryDetailValueIntId DROP CONSTRAINT '+@df2);

                ALTER TABLE dbo.AssetSummaryDetailValueIntId DROP COLUMN Id;

                ALTER TABLE dbo.AssetSummaryDetailValueIntId
                  ADD Id BIGINT IDENTITY(1,1) NOT NULL;

                ALTER TABLE [dbo].[AssetSummaryDetailValueIntId]
                    ADD CONSTRAINT [PK_AssetSummaryDetailValueIntId]
                        PRIMARY KEY CLUSTERED ([RunId] ASC, [Id] ASC)
                        ON [PS_SimulationRun]([RunId]);

                CREATE NONCLUSTERED COLUMNSTORE INDEX NCCI_AssetSummaryDetailValue
                  ON dbo.AssetSummaryDetailValueIntId
                    ([RunId], [Id], [AssetSummaryDetailId], [AttributeId], [Discriminator], [NumericValue], [TextValue])
                  ON PS_SimulationRun(RunId);

                /* ===== TABLE: BudgetDetail =========================================== */
                ALTER TABLE [dbo].[BudgetDetail] DROP CONSTRAINT [PK_BudgetDetail];
                

                ALTER TABLE [dbo].[BudgetDetail]
                    ADD CONSTRAINT [PK_BudgetDetail]
                        PRIMARY KEY CLUSTERED ([RunId] ASC, [Id] ASC)
                        ON [PS_SimulationRun]([RunId]);

                CREATE NONCLUSTERED COLUMNSTORE INDEX NCCI_BudgetDetail
                  ON dbo.BudgetDetail
                    ([RunId], [Id], [SimulationYearDetailId], [AvailableFunding], [BudgetName])
                  ON PS_SimulationRun(RunId);

                /* ===== TABLE: BudgetToSpend ========================================== */
                ALTER TABLE [dbo].[BudgetToSpend] DROP CONSTRAINT [PK_BudgetToSpend];

                ALTER TABLE [dbo].[BudgetToSpend]
                    ADD CONSTRAINT [PK_BudgetToSpend]
                        PRIMARY KEY CLUSTERED ([RunId] ASC, [Id] ASC)
                        ON [PS_SimulationRun]([RunId]);

                CREATE NONCLUSTERED COLUMNSTORE INDEX NCCI_BudgetToSpend
                  ON dbo.BudgetToSpend
                    ([RunId], [Id], [Name], [Amount], [Year], [FundingCalculationInputId])
                  ON PS_SimulationRun(RunId);

                /* ===== TABLE: CashFlowConsiderationDetail ============================ */
                ALTER TABLE [dbo].[CashFlowConsiderationDetail] DROP CONSTRAINT [PK_CashFlowConsiderationDetail];
                

                ALTER TABLE [dbo].[CashFlowConsiderationDetail]
                    ADD CONSTRAINT [PK_CashFlowConsiderationDetail]
                        PRIMARY KEY CLUSTERED ([RunId] ASC, [Id] ASC)
                        ON [PS_SimulationRun]([RunId]);

                CREATE NONCLUSTERED COLUMNSTORE INDEX NCCI_CashFlowConsiderationDetail
                  ON dbo.CashFlowConsiderationDetail
                    ([RunId], [Id], [TreatmentConsiderationDetailId], [CashFlowRuleName], [ReasonAgainstCashFlow])
                  ON PS_SimulationRun(RunId);

                /* ===== TABLE: DeficientConditionGoalDetail =========================== */
                ALTER TABLE [dbo].[DeficientConditionGoalDetail] DROP CONSTRAINT [PK_DeficientConditionGoalDetail];
                

                ALTER TABLE [dbo].[DeficientConditionGoalDetail]
                    ADD CONSTRAINT [PK_DeficientConditionGoalDetail]
                        PRIMARY KEY CLUSTERED ([RunId] ASC, [Id] ASC)
                        ON [PS_SimulationRun]([RunId]);

                CREATE NONCLUSTERED COLUMNSTORE INDEX NCCI_DeficientConditionGoalDetail
                  ON dbo.DeficientConditionGoalDetail
                    ([RunId], [Id], [SimulationYearDetailId], [ActualDeficientPercentage], [AllowedDeficientPercentage], [DeficientLimit], [AttributeId], [GoalIsMet], [GoalName])
                  ON PS_SimulationRun(RunId);

                /* ===== TABLE: FundingCalculationInput ================================ */
                ALTER TABLE [dbo].[FundingCalculationInput] DROP CONSTRAINT [PK_FundingCalculationInput];
                

                ALTER TABLE [dbo].[FundingCalculationInput]
                    ADD CONSTRAINT [PK_FundingCalculationInput]
                        PRIMARY KEY CLUSTERED ([RunId] ASC, [Id] ASC)
                        ON [PS_SimulationRun]([RunId]);

                CREATE NONCLUSTERED COLUMNSTORE INDEX NCCI_FundingCalculationInput
                  ON dbo.FundingCalculationInput
                    ([RunId], [Id], [TreatmentConsiderationDetailId])
                  ON PS_SimulationRun(RunId);

                /* ===== TABLE: FundingCalculationOutput =============================== */
                ALTER TABLE [dbo].[FundingCalculationOutput] DROP CONSTRAINT [PK_FundingCalculationOutput];
                

                ALTER TABLE [dbo].[FundingCalculationOutput]
                    ADD CONSTRAINT [PK_FundingCalculationOutput]
                        PRIMARY KEY CLUSTERED ([RunId] ASC, [Id] ASC)
                        ON [PS_SimulationRun]([RunId]);

                CREATE NONCLUSTERED COLUMNSTORE INDEX NCCI_FundingCalculationOutput
                  ON dbo.FundingCalculationOutput
                    ([RunId], [Id], [TreatmentConsiderationDetailId])
                  ON PS_SimulationRun(RunId);

                /* ===== TABLE: SimulationYearDetail =================================== */
                ALTER TABLE [dbo].[SimulationYearDetail] DROP CONSTRAINT [PK_SimulationYearDetail];
                

                ALTER TABLE [dbo].[SimulationYearDetail]
                    ADD CONSTRAINT [PK_SimulationYearDetail]
                        PRIMARY KEY CLUSTERED ([RunId] ASC, [Id] ASC)
                        ON [PS_SimulationRun]([RunId]);

                CREATE NONCLUSTERED INDEX [IX_SimulationYearDetail_SimulationOutputId]
                    ON [dbo].[SimulationYearDetail] ([RunId] ASC, [SimulationOutputId] ASC)
                    ON [PS_SimulationRun]([RunId]);

                /* ===== TABLE: TargetConditionGoalDetail ============================== */
                ALTER TABLE [dbo].[TargetConditionGoalDetail] DROP CONSTRAINT [PK_TargetConditionGoalDetail];
                

                ALTER TABLE [dbo].[TargetConditionGoalDetail]
                    ADD CONSTRAINT [PK_TargetConditionGoalDetail]
                        PRIMARY KEY CLUSTERED ([RunId] ASC, [Id] ASC)
                        ON [PS_SimulationRun]([RunId]);

                CREATE NONCLUSTERED COLUMNSTORE INDEX NCCI_TargetConditionGoalDetail
                  ON dbo.TargetConditionGoalDetail
                    ([RunId], [Id], [SimulationYearDetailId], [ActualValue], [TargetValue], [AttributeId], [GoalIsMet], [GoalName])
                  ON PS_SimulationRun(RunId);

                /* ===== TABLE: TreatmentConsiderationDetail =========================== */
                ALTER TABLE [dbo].[TreatmentConsiderationDetail] DROP CONSTRAINT [PK_TreatmentConsiderationDetail];
                

                ALTER TABLE [dbo].[TreatmentConsiderationDetail]
                    ADD CONSTRAINT [PK_TreatmentConsiderationDetail]
                        PRIMARY KEY CLUSTERED ([RunId] ASC, [Id] ASC)
                        ON [PS_SimulationRun]([RunId]);

                CREATE NONCLUSTERED COLUMNSTORE INDEX NCCI_TreatmentConsiderationDetail
                  ON dbo.TreatmentConsiderationDetail
                    ([RunId], [Id], [AssetDetailId], [BudgetPriorityLevel], [TreatmentName])
                  ON PS_SimulationRun(RunId);

                /* ===== TABLE: TreatmentOptionDetail ================================== */
                ALTER TABLE [dbo].[TreatmentOptionDetail] DROP CONSTRAINT [PK_TreatmentOptionDetail];
                

                ALTER TABLE [dbo].[TreatmentOptionDetail]
                    ADD CONSTRAINT [PK_TreatmentOptionDetail]
                        PRIMARY KEY CLUSTERED ([RunId] ASC, [Id] ASC)
                        ON [PS_SimulationRun]([RunId]);

                CREATE NONCLUSTERED COLUMNSTORE INDEX NCCI_TreatmentOptionDetail
                  ON dbo.TreatmentOptionDetail
                    ([RunId], [Id], [AssetDetailId], [Benefit], [Cost], [RemainingLife], [TreatmentName], [ConditionChange])
                  ON PS_SimulationRun(RunId);

                /* ===== TABLE: TreatmentRejectionDetail =============================== */
                ALTER TABLE [dbo].[TreatmentRejectionDetail] DROP CONSTRAINT [PK_TreatmentRejectionDetail];

                ALTER TABLE [dbo].[TreatmentRejectionDetail]
                    ADD CONSTRAINT [PK_TreatmentRejectionDetail]
                        PRIMARY KEY CLUSTERED ([RunId] ASC, [Id] ASC)
                        ON [PS_SimulationRun]([RunId]);

                CREATE NONCLUSTERED COLUMNSTORE INDEX NCCI_TreatmentRejectionDetail
                  ON dbo.TreatmentRejectionDetail
                    ([RunId], [Id], [AssetDetailId], [TreatmentName], [TreatmentRejectionReason], [PotentialConditionChange])
                  ON PS_SimulationRun(RunId);

                /* ===== TABLE: TreatmentSchedulingCollisionDetail ===================== */
                ALTER TABLE [dbo].[TreatmentSchedulingCollisionDetail] DROP CONSTRAINT [PK_TreatmentSchedulingCollisionDetail];
                

                ALTER TABLE [dbo].[TreatmentSchedulingCollisionDetail]
                    ADD CONSTRAINT [PK_TreatmentSchedulingCollisionDetail]
                        PRIMARY KEY CLUSTERED ([RunId] ASC, [Id] ASC)
                        ON [PS_SimulationRun]([RunId]);

                CREATE NONCLUSTERED COLUMNSTORE INDEX NCCI_TreatmentSchedulingCollisionDetail
                  ON dbo.TreatmentSchedulingCollisionDetail
                    ([RunId], [Id], [AssetDetailId], [NameOfUnscheduledTreatment])
                  ON PS_SimulationRun(RunId);
                ");

                            // -----------------------------------------------------------------
                            // 3.  RE-CREATE all foreign keys in the **new** column order
                            //     (RunId first, then Id / FK column).
                            // -----------------------------------------------------------------
                            migrationBuilder.Sql(@"
                /* ===== FK CREATES (RunId first) ======================================= */
                ALTER TABLE [dbo].[Allocation]
                    ADD CONSTRAINT [FK_Allocation_FundingCalculationOutput_FundingCalculationOutputId]
                        FOREIGN KEY ([RunId], [FundingCalculationOutputId])
                        REFERENCES [dbo].[FundingCalculationOutput] ([RunId], [Id])
                        ON DELETE CASCADE;

                ALTER TABLE [dbo].[AssetDetail]
                    ADD CONSTRAINT [FK_AssetDetail_MaintainableAsset_MaintainableAssetId]
                        FOREIGN KEY ([MaintainableAssetId])  -- MaintainableAsset is not partitioned
                        REFERENCES [dbo].[MaintainableAsset] ([Id]);

                ALTER TABLE [dbo].[AssetDetail]
                    ADD CONSTRAINT [FK_AssetDetail_SimulationYearDetail_SimulationYearDetailId]
                        FOREIGN KEY ([RunId], [SimulationYearDetailId])
                        REFERENCES [dbo].[SimulationYearDetail] ([RunId], [Id])
                        ON DELETE CASCADE;

                ALTER TABLE [dbo].[AssetDetailValueIntId]
                    ADD CONSTRAINT [FK_AssetDetailValueIntId_AssetDetail_AssetDetailId]
                        FOREIGN KEY ([RunId], [AssetDetailId])
                        REFERENCES [dbo].[AssetDetail] ([RunId], [Id])
                        ON DELETE CASCADE;

                ALTER TABLE [dbo].[AssetDetailValueIntId]
                    ADD CONSTRAINT [FK_AssetDetailValueIntId_Attribute_AttributeId]
                        FOREIGN KEY ([AttributeId])
                        REFERENCES [dbo].[Attribute] ([Id]);

                ALTER TABLE [dbo].[AssetSummaryDetail]
                    ADD CONSTRAINT [FK_AssetSummaryDetail_MaintainableAsset_MaintainableAssetId]
                        FOREIGN KEY ([MaintainableAssetId])
                        REFERENCES [dbo].[MaintainableAsset] ([Id]);

                ALTER TABLE [dbo].[AssetSummaryDetail]
                    ADD CONSTRAINT [FK_AssetSummaryDetail_SimulationOutput_SimulationOutputId]
                        FOREIGN KEY ([SimulationOutputId])
                        REFERENCES [dbo].[SimulationOutput] ([Id])
                        ON DELETE CASCADE;

                ALTER TABLE [dbo].[AssetSummaryDetailValueIntId]
                    ADD CONSTRAINT [FK_AssetSummaryDetailValueIntId_AssetSummaryDetail_AssetSummaryDetailId]
                        FOREIGN KEY ([RunId], [AssetSummaryDetailId])
                        REFERENCES [dbo].[AssetSummaryDetail] ([RunId], [Id])
                        ON DELETE CASCADE;

                ALTER TABLE [dbo].[AssetSummaryDetailValueIntId]
                    ADD CONSTRAINT [FK_AssetSummaryDetailValueIntId_Attribute_AttributeId]
                        FOREIGN KEY ([AttributeId])
                        REFERENCES [dbo].[Attribute] ([Id]);

                ALTER TABLE [dbo].[BudgetToSpend]
                    ADD CONSTRAINT [FK_BudgetToSpend_FundingCalculationInput_FundingCalculationInputId]
                        FOREIGN KEY ([RunId], [FundingCalculationInputId])
                        REFERENCES [dbo].[FundingCalculationInput] ([RunId], [Id])
                        ON DELETE CASCADE;

                ALTER TABLE [dbo].[CashFlowConsiderationDetail]
                    ADD CONSTRAINT [FK_CashFlowConsiderationDetail_TreatmentConsiderationDetail_TreatmentConsiderationDetailId]
                        FOREIGN KEY ([RunId], [TreatmentConsiderationDetailId])
                        REFERENCES [dbo].[TreatmentConsiderationDetail] ([RunId], [Id])
                        ON DELETE CASCADE;

                ALTER TABLE [dbo].[DeficientConditionGoalDetail]
                    ADD CONSTRAINT [FK_DeficientConditionGoalDetail_Attribute_AttributeId]
                        FOREIGN KEY ([AttributeId])
                        REFERENCES [dbo].[Attribute] ([Id])
                        ON DELETE CASCADE;

                ALTER TABLE [dbo].[DeficientConditionGoalDetail]
                    ADD CONSTRAINT [FK_DeficientConditionGoalDetail_SimulationYearDetail_SimulationYearDetailId]
                        FOREIGN KEY ([RunId], [SimulationYearDetailId])
                        REFERENCES [dbo].[SimulationYearDetail] ([RunId], [Id])
                        ON DELETE CASCADE;

                ALTER TABLE [dbo].[FundingCalculationInput]
                    ADD CONSTRAINT [FK_FundingCalculationInput_TreatmentConsiderationDetail_TreatmentConsiderationDetailId]
                        FOREIGN KEY ([RunId], [TreatmentConsiderationDetailId])
                        REFERENCES [dbo].[TreatmentConsiderationDetail] ([RunId], [Id])
                        ON DELETE CASCADE;

                ALTER TABLE [dbo].[FundingCalculationOutput]
                    ADD CONSTRAINT [FK_FundingCalculationOutput_TreatmentConsiderationDetail_TreatmentConsiderationDetailId]
                        FOREIGN KEY ([RunId], [TreatmentConsiderationDetailId])
                        REFERENCES [dbo].[TreatmentConsiderationDetail] ([RunId], [Id])
                        ON DELETE CASCADE;

                ALTER TABLE [dbo].[BudgetDetail]
                    ADD CONSTRAINT [FK_BudgetDetail_SimulationYearDetail_SimulationYearDetailId]
                        FOREIGN KEY ([RunId], [SimulationYearDetailId])
                        REFERENCES [dbo].[SimulationYearDetail] ([RunId], [Id])
                        ON DELETE CASCADE;

                ALTER TABLE [dbo].[SimulationYearDetail]
                    ADD CONSTRAINT [FK_SimulationYearDetail_SimulationOutput_SimulationOutputId]
                        FOREIGN KEY ([SimulationOutputId])
                        REFERENCES [dbo].[SimulationOutput] ([Id])
                        ON DELETE CASCADE;

                ALTER TABLE [dbo].[TargetConditionGoalDetail]
                    ADD CONSTRAINT [FK_TargetConditionGoalDetail_Attribute_AttributeId]
                        FOREIGN KEY ([AttributeId])
                        REFERENCES [dbo].[Attribute] ([Id])
                        ON DELETE CASCADE;

                ALTER TABLE [dbo].[TargetConditionGoalDetail]
                    ADD CONSTRAINT [FK_TargetConditionGoalDetail_SimulationYearDetail_SimulationYearDetailId]
                        FOREIGN KEY ([RunId], [SimulationYearDetailId])
                        REFERENCES [dbo].[SimulationYearDetail] ([RunId], [Id])
                        ON DELETE CASCADE;

                ALTER TABLE [dbo].[TreatmentConsiderationDetail]
                    ADD CONSTRAINT [FK_TreatmentConsiderationDetail_AssetDetail_AssetDetailId]
                        FOREIGN KEY ([RunId], [AssetDetailId])
                        REFERENCES [dbo].[AssetDetail] ([RunId], [Id])
                        ON DELETE CASCADE;

                ALTER TABLE [dbo].[TreatmentOptionDetail]
                    ADD CONSTRAINT [FK_TreatmentOptionDetail_AssetDetail_AssetDetailId]
                        FOREIGN KEY ([RunId], [AssetDetailId])
                        REFERENCES [dbo].[AssetDetail] ([RunId], [Id])
                        ON DELETE CASCADE;

                ALTER TABLE [dbo].[TreatmentRejectionDetail]
                    ADD CONSTRAINT [FK_TreatmentRejectionDetail_AssetDetail_AssetDetailId]
                        FOREIGN KEY ([RunId], [AssetDetailId])
                        REFERENCES [dbo].[AssetDetail] ([RunId], [Id])
                        ON DELETE CASCADE;

                ALTER TABLE [dbo].[TreatmentSchedulingCollisionDetail]
                    ADD CONSTRAINT [FK_TreatmentSchedulingCollisionDetail_AssetDetail_AssetDetailId]
                        FOREIGN KEY ([RunId], [AssetDetailId])
                        REFERENCES [dbo].[AssetDetail] ([RunId], [Id])
                        ON DELETE CASCADE;
                /* ====================================================================== */
                ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
