using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppliedResearchAssociates.iAM.DataPersistenceCore.Migrations
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
                ALTER TABLE [dbo].[SimulationYearDetail]             DROP CONSTRAINT [FK_SimulationYearDetail_SimulationOutput_SimulationOutputId];
                ALTER TABLE [dbo].[TargetConditionGoalDetail]         DROP CONSTRAINT [FK_TargetConditionGoalDetail_Attribute_AttributeId];
                ALTER TABLE [dbo].[TargetConditionGoalDetail]         DROP CONSTRAINT [FK_TargetConditionGoalDetail_SimulationYearDetail_SimulationYearDetailId];
                ALTER TABLE [dbo].[TreatmentConsiderationDetail]      DROP CONSTRAINT [FK_TreatmentConsiderationDetail_AssetDetail_AssetDetailId];
                ALTER TABLE [dbo].[TreatmentOptionDetail]             DROP CONSTRAINT [FK_TreatmentOptionDetail_AssetDetail_AssetDetailId];
                ALTER TABLE [dbo].[TreatmentRejectionDetail]          DROP CONSTRAINT [FK_TreatmentRejectionDetail_AssetDetail_AssetDetailId];
                ALTER TABLE [dbo].[TreatmentSchedulingCollisionDetail]DROP CONSTRAINT [FK_TreatmentSchedulingCollisionDetail_AssetDetail_AssetDetailId];
                ");

                            // -----------------------------------------------------------------
                            // 2.  FOR EACH TABLE:
                            //       • Drop the old PK & extra indexes
                            //       • Create the new PK (RunId, Id) CLUSTERED
                            //       • Create ONE NCI on (RunId, <parent‑FK>)
                            // -----------------------------------------------------------------
                            migrationBuilder.Sql(@"
                /* ===== TABLE: Allocation ============================================== */
                ALTER TABLE [dbo].[Allocation] DROP CONSTRAINT [PK_Allocation];
                DROP INDEX IF EXISTS [CI_Allocation] ON [dbo].[Allocation];
                DROP INDEX IF EXISTS [IX_Allocation_FundingCalculationOutputId] ON [dbo].[Allocation];

                ALTER TABLE [dbo].[Allocation]
                    ADD CONSTRAINT [PK_Allocation]
                        PRIMARY KEY CLUSTERED ([RunId] ASC, [Id] ASC)
                        ON [PS_SimulationRun]([RunId]);

                CREATE NONCLUSTERED INDEX [IX_Allocation_FundingCalculationOutputId]
                    ON [dbo].[Allocation] ([RunId] ASC, [FundingCalculationOutputId] ASC)
                    ON [PS_SimulationRun]([RunId]);

                /* ===== TABLE: AssetDetail ============================================= */
                ALTER TABLE [dbo].[AssetDetail] DROP CONSTRAINT [PK_AssetDetail];
                DROP INDEX IF EXISTS [CI_AssetDetail] ON [dbo].[AssetDetail];
                DROP INDEX IF EXISTS [IX_AssetDetail_MaintainableAsset_MaintainableAssetId] ON [dbo].[AssetDetail];
                DROP INDEX IF EXISTS [IX_AssetDetail_MaintainableAssetId] ON [dbo].[AssetDetail];
                DROP INDEX IF EXISTS [IX_AssetDetail_SimulationYearDetail_SimulationYearDetailId] ON [dbo].[AssetDetail];
                DROP INDEX IF EXISTS [IX_AssetDetail_SimulationYearDetailId] ON [dbo].[AssetDetail];

                ALTER TABLE [dbo].[AssetDetail]
                    ADD CONSTRAINT [PK_AssetDetail]
                        PRIMARY KEY CLUSTERED ([RunId] ASC, [Id] ASC)
                        ON [PS_SimulationRun]([RunId]);

                CREATE NONCLUSTERED INDEX [IX_AssetDetail_MaintainableAssetId]
                    ON [dbo].[AssetDetail] ([RunId] ASC, [MaintainableAssetId] ASC)
                    INCLUDE ([AppliedTreatment], [SimulationYearDetailId],
                             [TreatmentCause], [TreatmentFundingIgnoresSpendingLimit], [TreatmentStatus])
                    ON [PS_SimulationRun]([RunId]);

                CREATE NONCLUSTERED INDEX [IX_AssetDetail_SimulationYearDetailId]
                    ON [dbo].[AssetDetail] ([RunId] ASC, [SimulationYearDetailId] ASC)
                    INCLUDE ([AppliedTreatment], [MaintainableAssetId],
                             [TreatmentCause], [TreatmentFundingIgnoresSpendingLimit], [TreatmentStatus])
                    ON [PS_SimulationRun]([RunId]);

                /* ===== TABLE: AssetDetailValueIntId =================================== */
                ALTER TABLE [dbo].[AssetDetailValueIntId] DROP CONSTRAINT [PK_AssetDetailValueIntId];
                DROP INDEX IF EXISTS [CI_AssetDetailValueIntId] ON [dbo].[AssetDetailValueIntId];
                DROP INDEX IF EXISTS [IX_AssetDetailValueIntId_AssetDetailId]     ON [dbo].[AssetDetailValueIntId];

                ALTER TABLE [dbo].[AssetDetailValueIntId]
                    ADD CONSTRAINT [PK_AssetDetailValueIntId]
                        PRIMARY KEY CLUSTERED ([RunId] ASC, [Id] ASC)
                        ON [PS_SimulationRun]([RunId]);

                CREATE NONCLUSTERED INDEX [IX_AssetDetailValueIntId_AssetDetailId]
                    ON [dbo].[AssetDetailValueIntId] ([RunId] ASC, [AssetDetailId] ASC)
                    INCLUDE ([AttributeId], [Discriminator], [NumericValue], [TextValue])
                    ON [PS_SimulationRun]([RunId]);

                /* ===== TABLE: AssetSummaryDetail ====================================== */
                ALTER TABLE [dbo].[AssetSummaryDetail] DROP CONSTRAINT [PK_AssetSummaryDetail];
                DROP INDEX IF EXISTS [CI_AssetSummaryDetail] ON [dbo].[AssetSummaryDetail];
                DROP INDEX IF EXISTS [IX_AssetSummaryDetail_MaintainableAssetId]           ON [dbo].[AssetSummaryDetail];
                DROP INDEX IF EXISTS [IX_AssetSummaryDetail_SimulationOutput_SimulationOutputId] ON [dbo].[AssetSummaryDetail];
                DROP INDEX IF EXISTS [IX_AssetSummaryDetail_SimulationOutputId]            ON [dbo].[AssetSummaryDetail];

                ALTER TABLE [dbo].[AssetSummaryDetail]
                    ADD CONSTRAINT [PK_AssetSummaryDetail]
                        PRIMARY KEY CLUSTERED ([RunId] ASC, [Id] ASC)
                        ON [PS_SimulationRun]([RunId]);

                CREATE NONCLUSTERED INDEX [IX_AssetSummaryDetail_MaintainableAssetId]
                    ON [dbo].[AssetSummaryDetail] ([RunId] ASC, [MaintainableAssetId] ASC)
                    INCLUDE ([SimulationOutputId])
                    ON [PS_SimulationRun]([RunId]);

                CREATE NONCLUSTERED INDEX [IX_AssetSummaryDetail_SimulationOutputId]
                    ON [dbo].[AssetSummaryDetail] ([RunId] ASC, [SimulationOutputId] ASC)
                    INCLUDE ([MaintainableAssetId])
                    ON [PS_SimulationRun]([RunId]);

                /* ===== TABLE: AssetSummaryDetailValueIntId ============================ */
                ALTER TABLE [dbo].[AssetSummaryDetailValueIntId] DROP CONSTRAINT [PK_AssetSummaryDetailValueIntId];
                DROP INDEX IF EXISTS [CI_AssetSummaryDetailValueIntId] ON [dbo].[AssetSummaryDetailValueIntId];
                DROP INDEX IF EXISTS [IX_AssetSummaryDetailValueIntId_AssetSummaryDetailId] ON [dbo].[AssetSummaryDetailValueIntId];
                DROP INDEX IF EXISTS [IX_AssetSummaryDetailValueIntId_AttributeId]         ON [dbo].[AssetSummaryDetailValueIntId];

                ALTER TABLE [dbo].[AssetSummaryDetailValueIntId]
                    ADD CONSTRAINT [PK_AssetSummaryDetailValueIntId]
                        PRIMARY KEY CLUSTERED ([RunId] ASC, [Id] ASC)
                        ON [PS_SimulationRun]([RunId]);

                CREATE NONCLUSTERED INDEX [IX_AssetSummaryDetailValueIntId_AssetSummaryDetailId]
                    ON [dbo].[AssetSummaryDetailValueIntId] ([RunId] ASC, [AssetSummaryDetailId] ASC)
                    INCLUDE ([AttributeId], [Discriminator], [NumericValue], [TextValue])
                    ON [PS_SimulationRun]([RunId]);

                /* ===== TABLE: BudgetDetail =========================================== */
                ALTER TABLE [dbo].[BudgetDetail] DROP CONSTRAINT [PK_BudgetDetail];
                DROP INDEX IF EXISTS [CI_BudgetDetail] ON [dbo].[BudgetDetail];
                DROP INDEX IF EXISTS [IX_BudgetDetail_Id]                  ON [dbo].[BudgetDetail];
                DROP INDEX IF EXISTS [IX_BudgetDetail_SimulationYearDetailId] ON [dbo].[BudgetDetail];

                ALTER TABLE [dbo].[BudgetDetail]
                    ADD CONSTRAINT [PK_BudgetDetail]
                        PRIMARY KEY CLUSTERED ([RunId] ASC, [Id] ASC)
                        ON [PS_SimulationRun]([RunId]);

                CREATE NONCLUSTERED INDEX [IX_BudgetDetail_SimulationYearDetailId]
                    ON [dbo].[BudgetDetail] ([RunId] ASC, [SimulationYearDetailId] ASC)
                    INCLUDE ([AvailableFunding], [BudgetName])
                    ON [PS_SimulationRun]([RunId]);

                /* ===== TABLE: BudgetToSpend ========================================== */
                ALTER TABLE [dbo].[BudgetToSpend] DROP CONSTRAINT [PK_BudgetToSpend];
                DROP INDEX IF EXISTS [CI_BudgetToSpend] ON [dbo].[BudgetToSpend];
                DROP INDEX IF EXISTS [IX_BudgetToSpend_FundingCalculationInputId] ON [dbo].[BudgetToSpend];

                ALTER TABLE [dbo].[BudgetToSpend]
                    ADD CONSTRAINT [PK_BudgetToSpend]
                        PRIMARY KEY CLUSTERED ([RunId] ASC, [Id] ASC)
                        ON [PS_SimulationRun]([RunId]);

                CREATE NONCLUSTERED INDEX [IX_BudgetToSpend_FundingCalculationInputId]
                    ON [dbo].[BudgetToSpend] ([RunId] ASC, [FundingCalculationInputId] ASC)
                    ON [PS_SimulationRun]([RunId]);

                /* ===== TABLE: CashFlowConsiderationDetail ============================ */
                ALTER TABLE [dbo].[CashFlowConsiderationDetail] DROP CONSTRAINT [PK_CashFlowConsiderationDetail];
                DROP INDEX IF EXISTS [CI_CashFlowConsiderationDetail]            ON [dbo].[CashFlowConsiderationDetail];
                DROP INDEX IF EXISTS [IX_CashFlowConsiderationDetail_Id]         ON [dbo].[CashFlowConsiderationDetail];
                DROP INDEX IF EXISTS [IX_CashFlowConsiderationDetail_TreatmentConsiderationDetailId] ON [dbo].[CashFlowConsiderationDetail];

                ALTER TABLE [dbo].[CashFlowConsiderationDetail]
                    ADD CONSTRAINT [PK_CashFlowConsiderationDetail]
                        PRIMARY KEY CLUSTERED ([RunId] ASC, [Id] ASC)
                        ON [PS_SimulationRun]([RunId]);

                CREATE NONCLUSTERED INDEX [IX_CashFlowConsiderationDetail_TreatmentConsiderationDetailId]
                    ON [dbo].[CashFlowConsiderationDetail] ([RunId] ASC, [TreatmentConsiderationDetailId] ASC)
                    INCLUDE ([CashFlowRuleName], [ReasonAgainstCashFlow])
                    ON [PS_SimulationRun]([RunId]);

                /* ===== TABLE: DeficientConditionGoalDetail =========================== */
                ALTER TABLE [dbo].[DeficientConditionGoalDetail] DROP CONSTRAINT [PK_DeficientConditionGoalDetail];
                DROP INDEX IF EXISTS [CI_DeficientConditionGoalDetail] ON [dbo].[DeficientConditionGoalDetail];
                DROP INDEX IF EXISTS [IX_DeficientConditionGoalDetail_Id]            ON [dbo].[DeficientConditionGoalDetail];
                DROP INDEX IF EXISTS [IX_DeficientConditionGoalDetail_AttributeId]   ON [dbo].[DeficientConditionGoalDetail];
                DROP INDEX IF EXISTS [IX_DeficientConditionGoalDetail_SimulationYearDetailId] ON [dbo].[DeficientConditionGoalDetail];

                ALTER TABLE [dbo].[DeficientConditionGoalDetail]
                    ADD CONSTRAINT [PK_DeficientConditionGoalDetail]
                        PRIMARY KEY CLUSTERED ([RunId] ASC, [Id] ASC)
                        ON [PS_SimulationRun]([RunId]);

                CREATE NONCLUSTERED INDEX [IX_DeficientConditionGoalDetail_AttributeId]
                    ON [dbo].[DeficientConditionGoalDetail] ([RunId] ASC, [AttributeId] ASC)
                    ON [PS_SimulationRun]([RunId]);

                CREATE NONCLUSTERED INDEX [IX_DeficientConditionGoalDetail_SimulationYearDetailId]
                    ON [dbo].[DeficientConditionGoalDetail] ([RunId] ASC, [SimulationYearDetailId] ASC)
                    ON [PS_SimulationRun]([RunId]);

                /* ===== TABLE: FundingCalculationInput ================================ */
                ALTER TABLE [dbo].[FundingCalculationInput] DROP CONSTRAINT [PK_FundingCalculationInput];
                DROP INDEX IF EXISTS [CI_FundingCalculationInput] ON [dbo].[FundingCalculationInput];
                DROP INDEX IF EXISTS [IX_FundingCalculationInput_TreatmentConsiderationDetailId] ON [dbo].[FundingCalculationInput];

                ALTER TABLE [dbo].[FundingCalculationInput]
                    ADD CONSTRAINT [PK_FundingCalculationInput]
                        PRIMARY KEY CLUSTERED ([RunId] ASC, [Id] ASC)
                        ON [PS_SimulationRun]([RunId]);

                CREATE NONCLUSTERED INDEX [IX_FundingCalculationInput_TreatmentConsiderationDetailId]
                    ON [dbo].[FundingCalculationInput] ([RunId] ASC, [TreatmentConsiderationDetailId] ASC)
                    ON [PS_SimulationRun]([RunId]);

                /* ===== TABLE: FundingCalculationOutput =============================== */
                ALTER TABLE [dbo].[FundingCalculationOutput] DROP CONSTRAINT [PK_FundingCalculationOutput];
                DROP INDEX IF EXISTS [CI_FundingCalculationOutput] ON [dbo].[FundingCalculationOutput];
                DROP INDEX IF EXISTS [IX_FundingCalculationOutput_TreatmentConsiderationDetailId] ON [dbo].[FundingCalculationOutput];

                ALTER TABLE [dbo].[FundingCalculationOutput]
                    ADD CONSTRAINT [PK_FundingCalculationOutput]
                        PRIMARY KEY CLUSTERED ([RunId] ASC, [Id] ASC)
                        ON [PS_SimulationRun]([RunId]);

                CREATE NONCLUSTERED INDEX [IX_FundingCalculationOutput_TreatmentConsiderationDetailId]
                    ON [dbo].[FundingCalculationOutput] ([RunId] ASC, [TreatmentConsiderationDetailId] ASC)
                    ON [PS_SimulationRun]([RunId]);

                /* ===== TABLE: SimulationYearDetail =================================== */
                ALTER TABLE [dbo].[SimulationYearDetail] DROP CONSTRAINT [PK_SimulationYearDetail];
                DROP INDEX IF EXISTS [CI_SimulationYearDetail] ON [dbo].[SimulationYearDetail];
                DROP INDEX IF EXISTS [IX_SimulationYearDetail_SimulationOutputId] ON [dbo].[SimulationYearDetail];

                ALTER TABLE [dbo].[SimulationYearDetail]
                    ADD CONSTRAINT [PK_SimulationYearDetail]
                        PRIMARY KEY CLUSTERED ([RunId] ASC, [Id] ASC)
                        ON [PS_SimulationRun]([RunId]);

                CREATE NONCLUSTERED INDEX [IX_SimulationYearDetail_SimulationOutputId]
                    ON [dbo].[SimulationYearDetail] ([RunId] ASC, [SimulationOutputId] ASC)
                    ON [PS_SimulationRun]([RunId]);

                /* ===== TABLE: TargetConditionGoalDetail ============================== */
                ALTER TABLE [dbo].[TargetConditionGoalDetail] DROP CONSTRAINT [PK_TargetConditionGoalDetail];
                DROP INDEX IF EXISTS [CI_TargetConditionGoalDetail] ON [dbo].[TargetConditionGoalDetail];
                DROP INDEX IF EXISTS [IX_TargetConditionGoalDetail_Id]                  ON [dbo].[TargetConditionGoalDetail];
                DROP INDEX IF EXISTS [IX_TargetConditionGoalDetail_AttributeId]         ON [dbo].[TargetConditionGoalDetail];
                DROP INDEX IF EXISTS [IX_TargetConditionGoalDetail_SimulationYearDetailId] ON [dbo].[TargetConditionGoalDetail];

                ALTER TABLE [dbo].[TargetConditionGoalDetail]
                    ADD CONSTRAINT [PK_TargetConditionGoalDetail]
                        PRIMARY KEY CLUSTERED ([RunId] ASC, [Id] ASC)
                        ON [PS_SimulationRun]([RunId]);

                CREATE NONCLUSTERED INDEX [IX_TargetConditionGoalDetail_AttributeId]
                    ON [dbo].[TargetConditionGoalDetail] ([RunId] ASC, [AttributeId] ASC)
                    ON [PS_SimulationRun]([RunId]);

                CREATE NONCLUSTERED INDEX [IX_TargetConditionGoalDetail_SimulationYearDetailId]
                    ON [dbo].[TargetConditionGoalDetail] ([RunId] ASC, [SimulationYearDetailId] ASC)
                    ON [PS_SimulationRun]([RunId]);

                /* ===== TABLE: TreatmentConsiderationDetail =========================== */
                ALTER TABLE [dbo].[TreatmentConsiderationDetail] DROP CONSTRAINT [PK_TreatmentConsiderationDetail];
                DROP INDEX IF EXISTS [CI_TreatmentConsiderationDetail] ON [dbo].[TreatmentConsiderationDetail];
                DROP INDEX IF EXISTS [IX_TreatmentConsiderationDetail_Id]          ON [dbo].[TreatmentConsiderationDetail];
                DROP INDEX IF EXISTS [IX_TreatmentConsiderationDetail_AssetDetailId] ON [dbo].[TreatmentConsiderationDetail];

                ALTER TABLE [dbo].[TreatmentConsiderationDetail]
                    ADD CONSTRAINT [PK_TreatmentConsiderationDetail]
                        PRIMARY KEY CLUSTERED ([RunId] ASC, [Id] ASC)
                        ON [PS_SimulationRun]([RunId]);

                CREATE NONCLUSTERED INDEX [IX_TreatmentConsiderationDetail_AssetDetailId]
                    ON [dbo].[TreatmentConsiderationDetail] ([RunId] ASC, [AssetDetailId] ASC)
                    INCLUDE ([BudgetPriorityLevel], [TreatmentName])
                    ON [PS_SimulationRun]([RunId]);

                /* ===== TABLE: TreatmentOptionDetail ================================== */
                ALTER TABLE [dbo].[TreatmentOptionDetail] DROP CONSTRAINT [PK_TreatmentOptionDetail];
                DROP INDEX IF EXISTS [CI_TreatmentOptionDetail] ON [dbo].[TreatmentOptionDetail];
                DROP INDEX IF EXISTS [IX_TreatmentOptionDetail_Id]              ON [dbo].[TreatmentOptionDetail];
                DROP INDEX IF EXISTS [IX_TreatmentOptionDetail_AssetDetailId]   ON [dbo].[TreatmentOptionDetail];

                ALTER TABLE [dbo].[TreatmentOptionDetail]
                    ADD CONSTRAINT [PK_TreatmentOptionDetail]
                        PRIMARY KEY CLUSTERED ([RunId] ASC, [Id] ASC)
                        ON [PS_SimulationRun]([RunId]);

                CREATE NONCLUSTERED INDEX [IX_TreatmentOptionDetail_AssetDetailId]
                    ON [dbo].[TreatmentOptionDetail] ([RunId] ASC, [AssetDetailId] ASC)
                    INCLUDE ([Benefit], [ConditionChange], [Cost], [RemainingLife], [TreatmentName])
                    ON [PS_SimulationRun]([RunId]);

                /* ===== TABLE: TreatmentRejectionDetail =============================== */
                ALTER TABLE [dbo].[TreatmentRejectionDetail] DROP CONSTRAINT [PK_TreatmentRejectionDetail];
                DROP INDEX IF EXISTS [CI_TreatmentRejectionDetail] ON [dbo].[TreatmentRejectionDetail];
                DROP INDEX IF EXISTS [IX_TreatmentRejectionDetail_Id]               ON [dbo].[TreatmentRejectionDetail];
                DROP INDEX IF EXISTS [IX_TreatmentRejectionDetail_AssetDetailId]    ON [dbo].[TreatmentRejectionDetail];

                ALTER TABLE [dbo].[TreatmentRejectionDetail]
                    ADD CONSTRAINT [PK_TreatmentRejectionDetail]
                        PRIMARY KEY CLUSTERED ([RunId] ASC, [Id] ASC)
                        ON [PS_SimulationRun]([RunId]);

                CREATE NONCLUSTERED INDEX [IX_TreatmentRejectionDetail_AssetDetailId]
                    ON [dbo].[TreatmentRejectionDetail] ([RunId] ASC, [AssetDetailId] ASC)
                    INCLUDE ([PotentialConditionChange], [TreatmentName], [TreatmentRejectionReason])
                    ON [PS_SimulationRun]([RunId]);

                /* ===== TABLE: TreatmentSchedulingCollisionDetail ===================== */
                ALTER TABLE [dbo].[TreatmentSchedulingCollisionDetail] DROP CONSTRAINT [PK_TreatmentSchedulingCollisionDetail];
                DROP INDEX IF EXISTS [CI_TreatmentSchedulingCollisionDetail] ON [dbo].[TreatmentSchedulingCollisionDetail];
                DROP INDEX IF EXISTS [IX_TreatmentSchedulingCollisionDetail_Id]            ON [dbo].[TreatmentSchedulingCollisionDetail];
                DROP INDEX IF EXISTS [IX_TreatmentSchedulingCollisionDetail_AssetDetailId] ON [dbo].[TreatmentSchedulingCollisionDetail];

                ALTER TABLE [dbo].[TreatmentSchedulingCollisionDetail]
                    ADD CONSTRAINT [PK_TreatmentSchedulingCollisionDetail]
                        PRIMARY KEY CLUSTERED ([RunId] ASC, [Id] ASC)
                        ON [PS_SimulationRun]([RunId]);

                CREATE NONCLUSTERED INDEX [IX_TreatmentSchedulingCollisionDetail_AssetDetailId]
                    ON [dbo].[TreatmentSchedulingCollisionDetail] ([RunId] ASC, [AssetDetailId] ASC)
                    ON [PS_SimulationRun]([RunId]);
                ");

                            // -----------------------------------------------------------------
                            // 3.  RE‑CREATE all foreign keys in the **new** column order
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
