using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssetFox.Core.DataPersistenceCore.Migrations
{
    public partial class SimulationOutputIndexChanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // --- Handle IX_SimulationYearDetail_Id and its potential FK dependencies ---
            migrationBuilder.Sql(@"
                PRINT '--- Safely dropping dependencies and index IX_SimulationYearDetail_Id ---';

                -- Drop potentially dependent FKs IF THEY EXIST
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_DeficientConditionGoalDetail_SimulationYearDetail_SimulationYearDetailId')
                BEGIN
                    PRINT 'Dropping FK FK_DeficientConditionGoalDetail_SimulationYearDetail_SimulationYearDetailId...';
                    ALTER TABLE dbo.DeficientConditionGoalDetail DROP CONSTRAINT FK_DeficientConditionGoalDetail_SimulationYearDetail_SimulationYearDetailId;
                END
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_TargetConditionGoalDetail_SimulationYearDetail_SimulationYearDetailId')
                BEGIN
                    PRINT 'Dropping FK FK_TargetConditionGoalDetail_SimulationYearDetail_SimulationYearDetailId...';
                    ALTER TABLE dbo.TargetConditionGoalDetail DROP CONSTRAINT FK_TargetConditionGoalDetail_SimulationYearDetail_SimulationYearDetailId;
                END
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_BudgetDetail_SimulationYearDetail_SimulationYearDetailId')
                BEGIN
                    PRINT 'Dropping FK FK_BudgetDetail_SimulationYearDetail_SimulationYearDetailId...';
                    ALTER TABLE dbo.BudgetDetail DROP CONSTRAINT FK_BudgetDetail_SimulationYearDetail_SimulationYearDetailId;
                END
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_AssetDetail_SimulationYearDetail_SimulationYearDetailId')
                BEGIN
                    PRINT 'Dropping FK FK_AssetDetail_SimulationYearDetail_SimulationYearDetailId...';
                    ALTER TABLE dbo.AssetDetail DROP CONSTRAINT FK_AssetDetail_SimulationYearDetail_SimulationYearDetailId;
                END

                -- Drop the index IF IT EXISTS
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.SimulationYearDetail') AND name = 'IX_SimulationYearDetail_Id')
                BEGIN
                    PRINT 'Dropping index IX_SimulationYearDetail_Id ON dbo.SimulationYearDetail...';
                    DROP INDEX IX_SimulationYearDetail_Id ON dbo.SimulationYearDetail;
                END
                ELSE
                BEGIN
                     PRINT 'Index IX_SimulationYearDetail_Id does not exist.';
                END

                -- Recreate FKs IF THEY DON'T EXIST (they will now use the PK index), ensuring ON DELETE CASCADE
                PRINT 'Ensuring FKs referencing SimulationYearDetail exist with ON DELETE CASCADE...';
                IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_DeficientConditionGoalDetail_SimulationYearDetail_SimulationYearDetailId')
                   ALTER TABLE dbo.DeficientConditionGoalDetail WITH CHECK ADD CONSTRAINT FK_DeficientConditionGoalDetail_SimulationYearDetail_SimulationYearDetailId FOREIGN KEY (SimulationYearDetailId) REFERENCES dbo.SimulationYearDetail(Id) ON DELETE CASCADE;
                IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_TargetConditionGoalDetail_SimulationYearDetail_SimulationYearDetailId')
                   ALTER TABLE dbo.TargetConditionGoalDetail WITH CHECK ADD CONSTRAINT FK_TargetConditionGoalDetail_SimulationYearDetail_SimulationYearDetailId FOREIGN KEY (SimulationYearDetailId) REFERENCES dbo.SimulationYearDetail(Id) ON DELETE CASCADE;
                IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_BudgetDetail_SimulationYearDetail_SimulationYearDetailId')
                   ALTER TABLE dbo.BudgetDetail WITH CHECK ADD CONSTRAINT FK_BudgetDetail_SimulationYearDetail_SimulationYearDetailId FOREIGN KEY (SimulationYearDetailId) REFERENCES dbo.SimulationYearDetail(Id) ON DELETE CASCADE;
                IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_AssetDetail_SimulationYearDetail_SimulationYearDetailId')
                   ALTER TABLE dbo.AssetDetail WITH CHECK ADD CONSTRAINT FK_AssetDetail_SimulationYearDetail_SimulationYearDetailId FOREIGN KEY (SimulationYearDetailId) REFERENCES dbo.SimulationYearDetail(Id) ON DELETE CASCADE;

                PRINT '--- Finished processing IX_SimulationYearDetail_Id ---';
            ");

            // --- Handle IX_AssetDetail_Id and its potential FK dependencies ---
            // Includes newly identified FKs: TreatmentOption, TreatmentRejection, TreatmentSchedulingCollision, TreatmentConsideration
            migrationBuilder.Sql(@"
                PRINT '--- Safely dropping dependencies and index IX_AssetDetail_Id ---';
                -- Drop potentially dependent FKs IF THEY EXIST
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_AssetDetailValueIntId_AssetDetail_AssetDetailId')
                BEGIN
                    PRINT 'Dropping FK FK_AssetDetailValueIntId_AssetDetail_AssetDetailId...';
                    ALTER TABLE dbo.AssetDetailValueIntId DROP CONSTRAINT FK_AssetDetailValueIntId_AssetDetail_AssetDetailId;
                END
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_TreatmentOptionDetail_AssetDetail_AssetDetailId')
                BEGIN
                    PRINT 'Dropping FK FK_TreatmentOptionDetail_AssetDetail_AssetDetailId...';
                    ALTER TABLE dbo.TreatmentOptionDetail DROP CONSTRAINT FK_TreatmentOptionDetail_AssetDetail_AssetDetailId;
                END
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_TreatmentRejectionDetail_AssetDetail_AssetDetailId')
                BEGIN
                    PRINT 'Dropping FK FK_TreatmentRejectionDetail_AssetDetail_AssetDetailId...';
                    ALTER TABLE dbo.TreatmentRejectionDetail DROP CONSTRAINT FK_TreatmentRejectionDetail_AssetDetail_AssetDetailId;
                END
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_TreatmentSchedulingCollisionDetail_AssetDetail_AssetDetailId')
                BEGIN
                    PRINT 'Dropping FK FK_TreatmentSchedulingCollisionDetail_AssetDetail_AssetDetailId...';
                    ALTER TABLE dbo.TreatmentSchedulingCollisionDetail DROP CONSTRAINT FK_TreatmentSchedulingCollisionDetail_AssetDetail_AssetDetailId;
                END
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_TreatmentConsiderationDetail_AssetDetail_AssetDetailId')
                BEGIN
                    PRINT 'Dropping FK FK_TreatmentConsiderationDetail_AssetDetail_AssetDetailId...';
                    ALTER TABLE dbo.TreatmentConsiderationDetail DROP CONSTRAINT FK_TreatmentConsiderationDetail_AssetDetail_AssetDetailId;
                END

                -- Drop the index IF IT EXISTS
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.AssetDetail') AND name = 'IX_AssetDetail_Id')
                BEGIN
                    PRINT 'Dropping index IX_AssetDetail_Id ON dbo.AssetDetail...';
                    DROP INDEX IX_AssetDetail_Id ON dbo.AssetDetail;
                END
                ELSE
                BEGIN
                     PRINT 'Index IX_AssetDetail_Id does not exist.';
                END

                -- Recreate FKs IF THEY DON'T EXIST (they will now use the PK index), ensuring ON DELETE CASCADE
                PRINT 'Ensuring FKs referencing AssetDetail exist with ON DELETE CASCADE...';
                IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_AssetDetailValueIntId_AssetDetail_AssetDetailId')
                   ALTER TABLE dbo.AssetDetailValueIntId WITH CHECK ADD CONSTRAINT FK_AssetDetailValueIntId_AssetDetail_AssetDetailId FOREIGN KEY (AssetDetailId) REFERENCES dbo.AssetDetail(Id) ON DELETE CASCADE;
                IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_TreatmentOptionDetail_AssetDetail_AssetDetailId')
                   ALTER TABLE dbo.TreatmentOptionDetail WITH CHECK ADD CONSTRAINT FK_TreatmentOptionDetail_AssetDetail_AssetDetailId FOREIGN KEY (AssetDetailId) REFERENCES dbo.AssetDetail(Id) ON DELETE CASCADE;
                IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_TreatmentRejectionDetail_AssetDetail_AssetDetailId')
                   ALTER TABLE dbo.TreatmentRejectionDetail WITH CHECK ADD CONSTRAINT FK_TreatmentRejectionDetail_AssetDetail_AssetDetailId FOREIGN KEY (AssetDetailId) REFERENCES dbo.AssetDetail(Id) ON DELETE CASCADE;
                IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_TreatmentSchedulingCollisionDetail_AssetDetail_AssetDetailId')
                   ALTER TABLE dbo.TreatmentSchedulingCollisionDetail WITH CHECK ADD CONSTRAINT FK_TreatmentSchedulingCollisionDetail_AssetDetail_AssetDetailId FOREIGN KEY (AssetDetailId) REFERENCES dbo.AssetDetail(Id) ON DELETE CASCADE;
                IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_TreatmentConsiderationDetail_AssetDetail_AssetDetailId')
                   ALTER TABLE dbo.TreatmentConsiderationDetail WITH CHECK ADD CONSTRAINT FK_TreatmentConsiderationDetail_AssetDetail_AssetDetailId FOREIGN KEY (AssetDetailId) REFERENCES dbo.AssetDetail(Id) ON DELETE CASCADE;

                PRINT '--- Finished processing IX_AssetDetail_Id ---';
            ");

            // --- Handle IX_AssetSummaryDetail_Id and its potential FK dependency ---
            migrationBuilder.Sql(@"
                PRINT '--- Safely dropping dependencies and index IX_AssetSummaryDetail_Id ---';
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_AssetSummaryDetailValueIntId_AssetSummaryDetail_AssetSummaryDetailId')
                BEGIN
                    PRINT 'Dropping FK FK_AssetSummaryDetailValueIntId_AssetSummaryDetail_AssetSummaryDetailId...';
                    ALTER TABLE dbo.AssetSummaryDetailValueIntId DROP CONSTRAINT FK_AssetSummaryDetailValueIntId_AssetSummaryDetail_AssetSummaryDetailId;
                END

                IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.AssetSummaryDetail') AND name = 'IX_AssetSummaryDetail_Id')
                BEGIN
                    PRINT 'Dropping index IX_AssetSummaryDetail_Id ON dbo.AssetSummaryDetail...';
                    DROP INDEX IX_AssetSummaryDetail_Id ON dbo.AssetSummaryDetail;
                END
                ELSE
                BEGIN
                     PRINT 'Index IX_AssetSummaryDetail_Id does not exist.';
                END

                -- Recreate FK IF IT DOESN'T EXIST, ensuring ON DELETE CASCADE
                PRINT 'Ensuring FK FK_AssetSummaryDetailValueIntId_AssetSummaryDetail_AssetSummaryDetailId exists with ON DELETE CASCADE...';
                IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_AssetSummaryDetailValueIntId_AssetSummaryDetail_AssetSummaryDetailId')
                   ALTER TABLE dbo.AssetSummaryDetailValueIntId WITH CHECK ADD CONSTRAINT FK_AssetSummaryDetailValueIntId_AssetSummaryDetail_AssetSummaryDetailId FOREIGN KEY (AssetSummaryDetailId) REFERENCES dbo.AssetSummaryDetail(Id) ON DELETE CASCADE;
                PRINT '--- Finished processing IX_AssetSummaryDetail_Id ---';
            ");

            // --- Drop other redundant/unnecessary indexes identified previously ---
            // Assuming these don't have complex conditional dependencies and are NOT PKs
            migrationBuilder.DropIndex(
                name: "IX_AssetSummaryDetailValueIntId_Id",
                table: "AssetSummaryDetailValueIntId");

            migrationBuilder.DropIndex(
                name: "IX_AssetDetailValueIntId_Id",
                table: "AssetDetailValueIntId");

            migrationBuilder.DropIndex(
                name: "IX_AssetDetailValueIntId_AttributeId", // Simple index
                table: "AssetDetailValueIntId");

            migrationBuilder.DropIndex(
                name: "IX_AssetDetailValueIntId_Attribute_AttributeId", // Complex index
                table: "AssetDetailValueIntId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // --- Recreate other indexes first ---
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AssetDetailValueIntId_Attribute_AttributeId' AND object_id = OBJECT_ID('dbo.AssetDetailValueIntId'))
                BEGIN
                    PRINT 'Recreating index IX_AssetDetailValueIntId_Attribute_AttributeId...';
                    CREATE NONCLUSTERED INDEX [IX_AssetDetailValueIntId_Attribute_AttributeId]
                    ON [dbo].[AssetDetailValueIntId] ([AttributeId] ASC)
                    INCLUDE ([AssetDetailId],[Discriminator],[TextValue],[NumericValue]) -- Verify included columns
                    WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF,
                            DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON,
                            ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF)
                    ON [PRIMARY];
                END
            ");

            migrationBuilder.CreateIndex(
                name: "IX_AssetDetailValueIntId_AttributeId",
                table: "AssetDetailValueIntId",
                column: "AttributeId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetDetailValueIntId_Id",
                table: "AssetDetailValueIntId",
                column: "Id",
                unique: true); // Assuming unique if dropped

            migrationBuilder.CreateIndex(
               name: "IX_AssetSummaryDetailValueIntId_Id",
               table: "AssetSummaryDetailValueIntId",
               column: "Id",
               unique: true); // Assuming unique if dropped


            // --- Reverse changes for IX_SimulationYearDetail_Id ---
            migrationBuilder.Sql(@"
                PRINT '--- Safely ensuring index IX_SimulationYearDetail_Id and FKs exist with ON DELETE CASCADE (Down) ---';
                -- Ensure index exists (recreate if missing)
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.SimulationYearDetail') AND name = 'IX_SimulationYearDetail_Id')
                BEGIN
                    PRINT 'Recreating index IX_SimulationYearDetail_Id...';
                    CREATE UNIQUE NONCLUSTERED INDEX IX_SimulationYearDetail_Id ON dbo.SimulationYearDetail(Id);
                END
                ELSE
                BEGIN
                     PRINT 'Index IX_SimulationYearDetail_Id already exists.';
                END

                -- Ensure FKs exist with ON DELETE CASCADE (Drop/Create ensures they reference the table)
                PRINT 'Dropping and Recreating FKs referencing SimulationYearDetail with ON DELETE CASCADE...';
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_DeficientConditionGoalDetail_SimulationYearDetail_SimulationYearDetailId')
                    ALTER TABLE dbo.DeficientConditionGoalDetail DROP CONSTRAINT FK_DeficientConditionGoalDetail_SimulationYearDetail_SimulationYearDetailId;
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_TargetConditionGoalDetail_SimulationYearDetail_SimulationYearDetailId')
                    ALTER TABLE dbo.TargetConditionGoalDetail DROP CONSTRAINT FK_TargetConditionGoalDetail_SimulationYearDetail_SimulationYearDetailId;
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_BudgetDetail_SimulationYearDetail_SimulationYearDetailId')
                    ALTER TABLE dbo.BudgetDetail DROP CONSTRAINT FK_BudgetDetail_SimulationYearDetail_SimulationYearDetailId;
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_AssetDetail_SimulationYearDetail_SimulationYearDetailId')
                    ALTER TABLE dbo.AssetDetail DROP CONSTRAINT FK_AssetDetail_SimulationYearDetail_SimulationYearDetailId;

                IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_DeficientConditionGoalDetail_SimulationYearDetail_SimulationYearDetailId')
                   ALTER TABLE dbo.DeficientConditionGoalDetail WITH CHECK ADD CONSTRAINT FK_DeficientConditionGoalDetail_SimulationYearDetail_SimulationYearDetailId FOREIGN KEY (SimulationYearDetailId) REFERENCES dbo.SimulationYearDetail(Id) ON DELETE CASCADE;
                IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_TargetConditionGoalDetail_SimulationYearDetail_SimulationYearDetailId')
                   ALTER TABLE dbo.TargetConditionGoalDetail WITH CHECK ADD CONSTRAINT FK_TargetConditionGoalDetail_SimulationYearDetail_SimulationYearDetailId FOREIGN KEY (SimulationYearDetailId) REFERENCES dbo.SimulationYearDetail(Id) ON DELETE CASCADE;
                IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_BudgetDetail_SimulationYearDetail_SimulationYearDetailId')
                   ALTER TABLE dbo.BudgetDetail WITH CHECK ADD CONSTRAINT FK_BudgetDetail_SimulationYearDetail_SimulationYearDetailId FOREIGN KEY (SimulationYearDetailId) REFERENCES dbo.SimulationYearDetail(Id) ON DELETE CASCADE;
                IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_AssetDetail_SimulationYearDetail_SimulationYearDetailId')
                   ALTER TABLE dbo.AssetDetail WITH CHECK ADD CONSTRAINT FK_AssetDetail_SimulationYearDetail_SimulationYearDetailId FOREIGN KEY (SimulationYearDetailId) REFERENCES dbo.SimulationYearDetail(Id) ON DELETE CASCADE;

                PRINT '--- Finished reversing IX_SimulationYearDetail_Id ---';
            ");


            // --- Reverse changes for IX_AssetDetail_Id ---
            migrationBuilder.Sql(@"
                PRINT '--- Safely ensuring index IX_AssetDetail_Id and FKs exist with ON DELETE CASCADE (Down) ---';
                -- Ensure index exists (recreate if missing)
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.AssetDetail') AND name = 'IX_AssetDetail_Id')
                BEGIN
                    PRINT 'Recreating index IX_AssetDetail_Id...';
                    CREATE UNIQUE NONCLUSTERED INDEX IX_AssetDetail_Id ON dbo.AssetDetail(Id);
                END
                ELSE BEGIN PRINT 'Index IX_AssetDetail_Id already exists.'; END

                -- Ensure FKs exist with ON DELETE CASCADE (Drop/Create ensures they reference the table)
                PRINT 'Dropping and Recreating FKs referencing AssetDetail with ON DELETE CASCADE...';
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_AssetDetailValueIntId_AssetDetail_AssetDetailId')
                    ALTER TABLE dbo.AssetDetailValueIntId DROP CONSTRAINT FK_AssetDetailValueIntId_AssetDetail_AssetDetailId;
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_TreatmentOptionDetail_AssetDetail_AssetDetailId')
                    ALTER TABLE dbo.TreatmentOptionDetail DROP CONSTRAINT FK_TreatmentOptionDetail_AssetDetail_AssetDetailId;
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_TreatmentRejectionDetail_AssetDetail_AssetDetailId')
                    ALTER TABLE dbo.TreatmentRejectionDetail DROP CONSTRAINT FK_TreatmentRejectionDetail_AssetDetail_AssetDetailId;
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_TreatmentSchedulingCollisionDetail_AssetDetail_AssetDetailId')
                    ALTER TABLE dbo.TreatmentSchedulingCollisionDetail DROP CONSTRAINT FK_TreatmentSchedulingCollisionDetail_AssetDetail_AssetDetailId;
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_TreatmentConsiderationDetail_AssetDetail_AssetDetailId')
                    ALTER TABLE dbo.TreatmentConsiderationDetail DROP CONSTRAINT FK_TreatmentConsiderationDetail_AssetDetail_AssetDetailId;

                IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_AssetDetailValueIntId_AssetDetail_AssetDetailId')
                   ALTER TABLE dbo.AssetDetailValueIntId WITH CHECK ADD CONSTRAINT FK_AssetDetailValueIntId_AssetDetail_AssetDetailId FOREIGN KEY (AssetDetailId) REFERENCES dbo.AssetDetail(Id) ON DELETE CASCADE;
                IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_TreatmentOptionDetail_AssetDetail_AssetDetailId')
                   ALTER TABLE dbo.TreatmentOptionDetail WITH CHECK ADD CONSTRAINT FK_TreatmentOptionDetail_AssetDetail_AssetDetailId FOREIGN KEY (AssetDetailId) REFERENCES dbo.AssetDetail(Id) ON DELETE CASCADE;
                IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_TreatmentRejectionDetail_AssetDetail_AssetDetailId')
                   ALTER TABLE dbo.TreatmentRejectionDetail WITH CHECK ADD CONSTRAINT FK_TreatmentRejectionDetail_AssetDetail_AssetDetailId FOREIGN KEY (AssetDetailId) REFERENCES dbo.AssetDetail(Id) ON DELETE CASCADE;
                IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_TreatmentSchedulingCollisionDetail_AssetDetail_AssetDetailId')
                   ALTER TABLE dbo.TreatmentSchedulingCollisionDetail WITH CHECK ADD CONSTRAINT FK_TreatmentSchedulingCollisionDetail_AssetDetail_AssetDetailId FOREIGN KEY (AssetDetailId) REFERENCES dbo.AssetDetail(Id) ON DELETE CASCADE;
                IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_TreatmentConsiderationDetail_AssetDetail_AssetDetailId')
                   ALTER TABLE dbo.TreatmentConsiderationDetail WITH CHECK ADD CONSTRAINT FK_TreatmentConsiderationDetail_AssetDetail_AssetDetailId FOREIGN KEY (AssetDetailId) REFERENCES dbo.AssetDetail(Id) ON DELETE CASCADE;

                PRINT '--- Finished reversing IX_AssetDetail_Id ---';
            ");


            // --- Reverse changes for IX_AssetSummaryDetail_Id ---
            migrationBuilder.Sql(@"
                PRINT '--- Safely ensuring index IX_AssetSummaryDetail_Id and FK exists with ON DELETE CASCADE (Down) ---';
                -- Ensure index exists (recreate if missing)
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('dbo.AssetSummaryDetail') AND name = 'IX_AssetSummaryDetail_Id')
                BEGIN
                    PRINT 'Recreating index IX_AssetSummaryDetail_Id...';
                    CREATE UNIQUE NONCLUSTERED INDEX IX_AssetSummaryDetail_Id ON dbo.AssetSummaryDetail(Id);
                END
                ELSE BEGIN PRINT 'Index IX_AssetSummaryDetail_Id already exists.'; END

                -- Ensure FK exists with ON DELETE CASCADE (Drop/Create ensures it references the table)
                PRINT 'Dropping and Recreating FK referencing AssetSummaryDetail with ON DELETE CASCADE...';
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_AssetSummaryDetailValueIntId_AssetSummaryDetail_AssetSummaryDetailId')
                    ALTER TABLE dbo.AssetSummaryDetailValueIntId DROP CONSTRAINT FK_AssetSummaryDetailValueIntId_AssetSummaryDetail_AssetSummaryDetailId;

                IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_AssetSummaryDetailValueIntId_AssetSummaryDetail_AssetSummaryDetailId')
                   ALTER TABLE dbo.AssetSummaryDetailValueIntId WITH CHECK ADD CONSTRAINT FK_AssetSummaryDetailValueIntId_AssetSummaryDetail_AssetSummaryDetailId FOREIGN KEY (AssetSummaryDetailId) REFERENCES dbo.AssetSummaryDetail(Id) ON DELETE CASCADE;
                PRINT '--- Finished reversing IX_AssetSummaryDetail_Id ---';
            ");
        }
    }
}
