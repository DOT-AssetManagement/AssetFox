using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppliedResearchAssociates.iAM.DataPersistenceCore.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOldStoredProcs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var createAlterProcSql = @"EXEC('CREATE OR ALTER PROCEDURE dbo.usp_delete_simulation(@SimGuidList NVARCHAR(MAX)=NULL,@RetMessage VARCHAR(250)=NULL OUTPUT)
	          AS
	          BEGIN 
 	            DECLARE @CustomErrorMessage NVARCHAR(MAX),
	            @ErrorNumber int,
	            @ErrorSeverity int,
	            @ErrorState int,
	            @ErrorProcedure nvarchar(126),
	            @ErrorLine int,
	            @ErrorMessage nvarchar(4000);
	            Set  @RetMessage = ''Success'';
	            DECLARE @CurrentDateTime DATETIME;
	            DECLARE @BatchSize INT = 100000;  -- Adjust batch size as needed
	            DECLARE @RowsDeleted INT = 1;

             ---------------------------------------------
             CREATE TABLE #SimTempGuids
                (
                   -- Guid UNIQUEIDENTIFIER
		            Guid NVARCHAR(36)
                );
	
	            IF @SimGuidList IS NULL OR LEN(@SimGuidList) = 0
	            BEGIN
		              PRINT ''String is NULL or empty'';
		              Set  @SimGuidList = ''00000000-0000-0000-0000-000000000000'';
	            END

                INSERT INTO #SimTempGuids (Guid)
	            SELECT LEFT(LTRIM(RTRIM(value)), 36)
                FROM STRING_SPLIT(@SimGuidList, '','');

	            --Select *, '''' as ''aaa'' from #SimTempGuids;

	            UPDATE #SimTempGuids
	            SET Guid = ''00000000-0000-0000-0000-000000000000''
	            WHERE TRY_CAST(Guid AS UNIQUEIDENTIFIER) IS NULL OR Guid = '''';
	
	            --Select *, '''' as ''bbb'' from #SimTempGuids;

		            Begin Transaction
	            BEGIN TRY

            -----------------------------------------------------------------------------


                        BEGIN TRY
 
                        ALTER TABLE Benefit NOCHECK CONSTRAINT all

			            Delete l3 
			            FROM  Simulation  AS l1
			            JOIN AnalysisMethod AS l2 ON l2.SimulationId = l1.Id
			            JOIN Benefit AS l3 ON l3.AnalysisMethodId = l2.Id
			            WHERE l1.Id IN (SELECT Guid FROM #SimTempGuids);

                        ALTER TABLE Benefit WITH CHECK CHECK CONSTRAINT all

                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in Benefit''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            -----------------------------------------------------------------------

                        BEGIN TRY


                        ALTER TABLE CriterionLibrary_AnalysisMethod NOCHECK CONSTRAINT all

			            Delete l3 
			            FROM  Simulation  AS l1
			            JOIN AnalysisMethod AS l2 ON l2.SimulationId = l1.Id
			            JOIN CriterionLibrary_AnalysisMethod AS l3 ON l3.AnalysisMethodId = l2.Id
			            WHERE l1.Id IN (SELECT Guid FROM #SimTempGuids);

                        ALTER TABLE CriterionLibrary_AnalysisMethod WITH CHECK CHECK CONSTRAINT all

                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in CriterionLibrary_AnalysisMethod''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH
			            ------------------------------------------------------------------

                        BEGIN TRY

                        ALTER TABLE AnalysisMethod NOCHECK CONSTRAINT all

			            Delete l2 
			            FROM  Simulation  AS l1
			            JOIN AnalysisMethod AS l2 ON l2.SimulationId = l1.Id
			            WHERE l1.Id IN (SELECT Guid FROM #SimTempGuids);

			            ALTER TABLE AnalysisMethod WITH CHECK CHECK CONSTRAINT all
 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in AnalysisMethod''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH
 
 			            -----------------------------------------------------------------------

                     BEGIN TRY

                        ALTER TABLE CommittedProjectConsequence NOCHECK CONSTRAINT all

			            Delete l3 
			            FROM  Simulation  AS l1
			            JOIN CommittedProject AS l2 ON l2.SimulationId = l1.Id
			            JOIN CommittedProjectConsequence AS l3 ON l3.CommittedProjectId = l2.Id
			            WHERE l1.Id IN (SELECT Guid FROM #SimTempGuids);

			            ALTER TABLE CommittedProjectConsequence WITH CHECK CHECK CONSTRAINT all
 	
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in CommittedProjectConsequence''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            -------------------------------------

			             BEGIN TRY

                        ALTER TABLE CommittedProjectLocation NOCHECK CONSTRAINT all

			            Delete l3 
			            FROM  Simulation  AS l1
			            JOIN CommittedProject AS l2 ON l2.SimulationId = l1.Id
			            JOIN CommittedProjectLocation AS l3 ON l3.CommittedProjectId = l2.Id
			            WHERE l1.Id IN (SELECT Guid FROM #SimTempGuids);

			            ALTER TABLE CommittedProjectLocation WITH CHECK CHECK CONSTRAINT all
 	
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in CommittedProjectLocation''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            --------------------------------------

                        BEGIN TRY

			            ALTER TABLE CommittedProject NOCHECK CONSTRAINT all

			            Delete l2 
			            FROM  Simulation  AS l1
			            JOIN CommittedProject AS l2 ON l2.SimulationId = l1.Id
			            WHERE l1.Id IN (SELECT Guid FROM #SimTempGuids);

			            ALTER TABLE CommittedProject WITH CHECK CHECK CONSTRAINT all

                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in CommittedProject''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH 

			            -------------------------------------

                        BEGIN TRY

                        ALTER TABLE CommittedProjectConsequence NOCHECK CONSTRAINT all

			            Delete l4 
			            FROM  Simulation  AS l1
			            JOIN ScenarioBudget AS l2 ON l2.SimulationId = l1.Id
			            JOIN CommittedProject AS l3 ON l3.ScenarioBudgetId = l2.Id
			            JOIN CommittedProjectConsequence AS l4 ON l4.CommittedProjectId = l3.Id
			            WHERE l1.Id IN (SELECT Guid FROM #SimTempGuids);

			            ALTER TABLE CommittedProjectConsequence WITH CHECK CHECK CONSTRAINT all
 	
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in CommittedProjectConsequence''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            -------------------------------------

			             BEGIN TRY

                        ALTER TABLE CommittedProjectLocation NOCHECK CONSTRAINT all

			            Delete l4 
			            FROM  Simulation  AS l1
			            JOIN ScenarioBudget AS l2 ON l2.SimulationId = l1.Id
			            JOIN CommittedProject AS l3 ON l3.ScenarioBudgetId = l2.Id
			            JOIN CommittedProjectLocation AS l4 ON l4.CommittedProjectId = l3.Id
			            WHERE l1.Id IN (SELECT Guid FROM #SimTempGuids);

			            ALTER TABLE CommittedProjectLocation WITH CHECK CHECK CONSTRAINT all
 	
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in CommittedProjectLocation''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            -----------------------------------------

                        BEGIN TRY

			            ALTER TABLE ScenarioBudgetAmount NOCHECK CONSTRAINT all

			            Delete l3 
			            FROM  Simulation  AS l1
			            JOIN ScenarioBudget AS l2 ON l2.SimulationId = l1.Id
			            JOIN ScenarioBudgetAmount AS l3 ON l3.ScenarioBudgetId = l2.Id
			            WHERE l1.Id IN (SELECT Guid FROM #SimTempGuids);

			            ALTER TABLE ScenarioBudgetAmount WITH CHECK CHECK CONSTRAINT all

 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in ScenarioBudgetAmount''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            ----------------------------------------------

                        BEGIN TRY

			            ALTER TABLE BudgetPercentagePair NOCHECK CONSTRAINT all

			            Delete l3 
			            FROM  Simulation  AS l1
			            JOIN ScenarioBudget AS l2 ON l2.SimulationId = l1.Id
			            JOIN BudgetPercentagePair AS l3 ON l3.ScenarioBudgetId = l2.Id
			            WHERE l1.Id IN (SELECT Guid FROM #SimTempGuids);

			            ALTER TABLE BudgetPercentagePair WITH CHECK CHECK CONSTRAINT all

 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in BudgetPercentagePair''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            -------------------------------------------------------------------------

                        BEGIN TRY

			            ALTER TABLE ScenarioBudget NOCHECK CONSTRAINT all

			            Delete l2 
			            FROM  Simulation  AS l1
			            JOIN ScenarioBudget AS l2 ON l2.SimulationId = l1.Id
			            WHERE l1.Id IN (SELECT Guid FROM #SimTempGuids);

			            ALTER TABLE ScenarioBudget WITH CHECK CHECK CONSTRAINT all

 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in ScenarioBudget''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            ---------------------------------------------

                        BEGIN TRY

			            ALTER TABLE CriterionLibrary_ScenarioBudgetPriority NOCHECK CONSTRAINT all

			            Delete l3 
			            FROM  Simulation  AS l1
			            JOIN ScenarioBudgetPriority AS l2 ON l2.SimulationId = l1.Id
			            JOIN CriterionLibrary_ScenarioBudgetPriority AS l3 ON l3.ScenarioBudgetPriorityId = l2.Id
			            WHERE l1.Id IN (SELECT Guid FROM #SimTempGuids);

			            ALTER TABLE CriterionLibrary_ScenarioBudgetPriority WITH CHECK CHECK CONSTRAINT all

 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in CriterionLibrary_ScenarioBudgetPriority''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            ------------------------------------------------------------

                        BEGIN TRY

			            ALTER TABLE BudgetPercentagePair NOCHECK CONSTRAINT all

			            Delete l3 
			            FROM  Simulation  AS l1
			            JOIN ScenarioBudgetPriority AS l2 ON l2.SimulationId = l1.Id
			            JOIN BudgetPercentagePair AS l3 ON l3.ScenarioBudgetPriorityId = l2.Id
			            WHERE l1.Id IN (SELECT Guid FROM #SimTempGuids);

			            ALTER TABLE BudgetPercentagePair WITH CHECK CHECK CONSTRAINT all

 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in BudgetPercentagePair''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH


			            ------------------------------------------------------------

                        BEGIN TRY

			            ALTER TABLE ScenarioBudgetPriority NOCHECK CONSTRAINT all

			            Delete l2 
			            FROM  Simulation  AS l1
			            JOIN ScenarioBudgetPriority AS l2 ON l2.SimulationId = l1.Id
			            WHERE l1.Id IN (SELECT Guid FROM #SimTempGuids);

			            ALTER TABLE ScenarioBudgetPriority WITH CHECK CHECK CONSTRAINT all
 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in ScenarioBudgetPriority''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            ----------------------------------------------------------------------

                        BEGIN TRY

			            ALTER TABLE ScenarioCalculatedAttributePair_Criteria NOCHECK CONSTRAINT all

			            Delete l4 
			            FROM  Simulation  AS l1
			            JOIN ScenarioCalculatedAttribute AS l2 ON l2.SimulationId = l1.Id
			            JOIN ScenarioCalculatedAttributePair AS l3 ON l3.ScenarioCalculatedAttributeId = l2.Id
			            JOIN ScenarioCalculatedAttributePair_Criteria AS l4 ON l4.ScenarioCalculatedAttributePairId = l3.Id
			            WHERE l1.Id IN (SELECT Guid FROM #SimTempGuids);

			            ALTER TABLE ScenarioCalculatedAttributePair_Criteria WITH CHECK CHECK CONSTRAINT all

 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in ScenarioCalculatedAttributePair_Criteria''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            ------------------------------------------------------------

                        BEGIN TRY

			            ALTER TABLE ScenarioCalculatedAttributePair_Equation NOCHECK CONSTRAINT all

			            Delete l4 
			            FROM  Simulation  AS l1
			            JOIN ScenarioCalculatedAttribute AS l2 ON l2.SimulationId = l1.Id
			            JOIN ScenarioCalculatedAttributePair AS l3 ON l3.ScenarioCalculatedAttributeId = l2.Id
			            JOIN ScenarioCalculatedAttributePair_Equation AS l4 ON l4.ScenarioCalculatedAttributePairId = l3.Id
			            WHERE l1.Id IN (SELECT Guid FROM #SimTempGuids);

			            ALTER TABLE ScenarioCalculatedAttributePair_Equation WITH CHECK CHECK CONSTRAINT all
 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in ScenarioCalculatedAttributePair_Equation''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            ------------------------------------------------------------

			            BEGIN TRY

			            ALTER TABLE ScenarioCalculatedAttributePair NOCHECK CONSTRAINT all

			            Delete l3 
			            FROM  Simulation  AS l1
			            JOIN ScenarioCalculatedAttribute AS l2 ON l2.SimulationId = l1.Id
			            JOIN ScenarioCalculatedAttributePair AS l3 ON l3.ScenarioCalculatedAttributeId = l2.Id
			            WHERE l1.Id IN (SELECT Guid FROM #SimTempGuids);

			            ALTER TABLE ScenarioCalculatedAttributePair WITH CHECK CHECK CONSTRAINT all
 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in ScenarioCalculatedAttributePair''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            ---------------------------------------------------------------------------

                        BEGIN TRY

			            ALTER TABLE ScenarioCalculatedAttribute NOCHECK CONSTRAINT all

			            Delete l2 
			            FROM  Simulation  AS l1
			            JOIN ScenarioCalculatedAttribute AS l2 ON l2.SimulationId = l1.Id
			            WHERE l1.Id IN (SELECT Guid FROM #SimTempGuids);

			            ALTER TABLE ScenarioCalculatedAttribute WITH CHECK CHECK CONSTRAINT all
 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in ScenarioCalculatedAttribute''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            --------------------------------------------------------------------

                        BEGIN TRY

			            ALTER TABLE CriterionLibrary_ScenarioCashFlowRule NOCHECK CONSTRAINT all

			            Delete l3 
			            FROM  Simulation  AS l1
			            JOIN ScenarioCashFlowRule AS l2 ON l2.SimulationId = l1.Id
			            JOIN CriterionLibrary_ScenarioCashFlowRule AS l3 ON l3.ScenarioCashFlowRuleId = l2.Id
			            WHERE l1.Id IN (SELECT Guid FROM #SimTempGuids);

			            ALTER TABLE CriterionLibrary_ScenarioCashFlowRule WITH CHECK CHECK CONSTRAINT all
 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in CriterionLibrary_ScenarioCashFlowRule''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            ------------------------------------------------------------

			            BEGIN TRY

			            ALTER TABLE ScenarioCashFlowDistributionRule NOCHECK CONSTRAINT all

			            Delete l3 
			            FROM  Simulation  AS l1
			            JOIN ScenarioCashFlowRule AS l2 ON l2.SimulationId = l1.Id
			            JOIN ScenarioCashFlowDistributionRule AS l3 ON l3.ScenarioCashFlowRuleId = l2.Id
			            WHERE l1.Id IN (SELECT Guid FROM #SimTempGuids);

			            ALTER TABLE ScenarioCashFlowDistributionRule WITH CHECK CHECK CONSTRAINT all
 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in ScenarioCashFlowDistributionRule''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            ---------------------------------------------------------------------------

                        BEGIN TRY

			            ALTER TABLE ScenarioCashFlowRule NOCHECK CONSTRAINT all

			            Delete l2 
			            FROM  Simulation  AS l1
			            JOIN ScenarioCashFlowRule AS l2 ON l2.SimulationId = l1.Id
			            WHERE l1.Id IN (SELECT Guid FROM #SimTempGuids);

			            ALTER TABLE ScenarioCashFlowRule WITH CHECK CHECK CONSTRAINT all
 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in ScenarioCashFlowRule''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            --------------------------------------------------------------------

                        BEGIN TRY

			            ALTER TABLE CriterionLibrary_ScenarioDeficientConditionGoal NOCHECK CONSTRAINT all

			            Delete l3 
			            FROM  Simulation  AS l1
			            JOIN ScenarioDeficientConditionGoal AS l2 ON l2.SimulationId = l1.Id
			            JOIN CriterionLibrary_ScenarioDeficientConditionGoal AS l3 ON l3.ScenarioDeficientConditionGoalId = l2.Id
			            WHERE l1.Id IN (SELECT Guid FROM #SimTempGuids);

			            ALTER TABLE CriterionLibrary_ScenarioDeficientConditionGoal WITH CHECK CHECK CONSTRAINT all
 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in CriterionLibrary_ScenarioDeficientConditionGoal''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            ------------------------------------------------------------

                        BEGIN TRY

			            ALTER TABLE ScenarioDeficientConditionGoal NOCHECK CONSTRAINT all

			            Delete l2 
			            FROM  Simulation  AS l1
			            JOIN ScenarioDeficientConditionGoal AS l2 ON l2.SimulationId = l1.Id
			            WHERE l1.Id IN (SELECT Guid FROM #SimTempGuids);

			            ALTER TABLE ScenarioDeficientConditionGoal WITH CHECK CHECK CONSTRAINT all
 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in ScenarioDeficientConditionGoal''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            ---------------------------------------------------------------------

                        BEGIN TRY

			            ALTER TABLE CriterionLibrary_ScenarioPerformanceCurve NOCHECK CONSTRAINT all

			            Delete l3 
			            FROM  Simulation  AS l1
			            JOIN ScenarioPerformanceCurve AS l2 ON l2.SimulationId = l1.Id
			            JOIN CriterionLibrary_ScenarioPerformanceCurve AS l3 ON l3.ScenarioPerformanceCurveId = l2.Id
			            WHERE l1.Id IN (SELECT Guid FROM #SimTempGuids);

			            ALTER TABLE CriterionLibrary_ScenarioPerformanceCurve WITH CHECK CHECK CONSTRAINT all
 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in CriterionLibrary_ScenarioPerformanceCurve''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            ------------------------------------------------------------

                        BEGIN TRY

			            ALTER TABLE ScenarioPerformanceCurve_Equation NOCHECK CONSTRAINT all

			            Delete l3 
			            FROM  Simulation  AS l1
			            JOIN ScenarioPerformanceCurve AS l2 ON l2.SimulationId = l1.Id
			            JOIN ScenarioPerformanceCurve_Equation AS l3 ON l3.ScenarioPerformanceCurveId = l2.Id
			            WHERE l1.Id IN (SELECT Guid FROM #SimTempGuids);

			            ALTER TABLE ScenarioPerformanceCurve_Equation WITH CHECK CHECK CONSTRAINT all
 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in ScenarioPerformanceCurve_Equation''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            ---------------------------------------------------------------------------

                        BEGIN TRY

			            ALTER TABLE ScenarioPerformanceCurve NOCHECK CONSTRAINT all

			            Delete l2 
			            FROM  Simulation  AS l1
			            JOIN ScenarioPerformanceCurve AS l2 ON l2.SimulationId = l1.Id
			            WHERE l1.Id IN (SELECT Guid FROM #SimTempGuids);

			            ALTER TABLE ScenarioPerformanceCurve WITH CHECK CHECK CONSTRAINT all
 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in ScenarioPerformanceCurve''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            ---------------------------------------------------------------------

                        BEGIN TRY

			            ALTER TABLE CriterionLibrary_ScenarioRemainingLifeLimit NOCHECK CONSTRAINT all

			            Delete l3 
			            FROM  Simulation  AS l1
			            JOIN ScenarioRemainingLifeLimit AS l2 ON l2.SimulationId = l1.Id
			            JOIN CriterionLibrary_ScenarioRemainingLifeLimit AS l3 ON l3.ScenarioRemainingLifeLimitId = l2.Id
			            WHERE l1.Id IN (SELECT Guid FROM #SimTempGuids);

			            ALTER TABLE CriterionLibrary_ScenarioRemainingLifeLimit WITH CHECK CHECK CONSTRAINT all
 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in CriterionLibrary_ScenarioRemainingLifeLimit''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            ---------------------------------------------------------------------------

                        BEGIN TRY

			            ALTER TABLE ScenarioRemainingLifeLimit NOCHECK CONSTRAINT all

			            Delete l2 
			            FROM  Simulation  AS l1
			            JOIN ScenarioRemainingLifeLimit AS l2 ON l2.SimulationId = l1.Id
			            WHERE l1.Id IN (SELECT Guid FROM #SimTempGuids);

			            ALTER TABLE ScenarioRemainingLifeLimit WITH CHECK CHECK CONSTRAINT all
 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in ScenarioRemainingLifeLimit''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            --------------------------------------------------------------------

			            BEGIN TRY

			            ALTER TABLE CriterionLibrary_ScenarioTreatmentSupersedeRule NOCHECK CONSTRAINT all

			            Delete l4
			            FROM Simulation AS l1
			            JOIN ScenarioSelectableTreatment AS l2 ON l2.SimulationId = l1.Id
			            JOIN ScenarioTreatmentSupersedeRule AS l3 ON l3.TreatmentId = l2.Id
			            JOIN CriterionLibrary_ScenarioTreatmentSupersedeRule AS l4  ON l4.ScenarioTreatmentSupersedeRuleId = l3.Id
			            WHERE l1.Id IN (SELECT Guid FROM #SimTempGuids);

			            ALTER TABLE CriterionLibrary_ScenarioTreatmentSupersedeRule WITH CHECK CHECK CONSTRAINT all
 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in CriterionLibrary_ScenarioTreatmentSupersedeRule''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            ---------------------------------------------------------------------------

               --Network --> Simulation --> ScenarioSelectableTreatment --> ScenarioTreatmentSupersedeRule  

                     BEGIN TRY

			            ALTER TABLE ScenarioTreatmentSupersedeRule NOCHECK CONSTRAINT all

			            Delete l3
			            FROM Simulation AS l1
			            JOIN ScenarioSelectableTreatment AS l2 ON l2.SimulationId = l1.Id
			            JOIN ScenarioTreatmentSupersedeRule AS l3 ON l3.TreatmentId = l2.Id
			            WHERE l1.Id IN (SELECT Guid FROM #SimTempGuids);

			            ALTER TABLE ScenarioTreatmentSupersedeRule WITH CHECK CHECK CONSTRAINT all
 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in ScenarioTreatmentSupersedeRule''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            ---------------------------------------------------------------------------

		             BEGIN TRY

			            ALTER TABLE CriterionLibrary_ScenarioTreatmentConsequence NOCHECK CONSTRAINT all

			            Delete l4 
			            FROM  Simulation  AS l1
			            JOIN ScenarioSelectableTreatment AS l2 ON l2.SimulationId = l1.Id
			            JOIN ScenarioConditionalTreatmentConsequences AS l3 ON l3.ScenarioSelectableTreatmentId = l2.Id
			            JOIN CriterionLibrary_ScenarioTreatmentConsequence AS l4 ON l4.ScenarioConditionalTreatmentConsequenceId = l3.Id
			            WHERE l1.Id IN (SELECT Guid FROM #SimTempGuids);

			            ALTER TABLE CriterionLibrary_ScenarioTreatmentConsequence WITH CHECK CHECK CONSTRAINT all
 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in CriterionLibrary_ScenarioTreatmentConsequence''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH
		
		            ---------------------------------------------------------------------------

                        BEGIN TRY

			            --ALTER TABLE ScenarioTreatmentConsequence_Equation NOCHECK CONSTRAINT all

			            Delete l4 
			            FROM  Simulation  AS l1
			            JOIN ScenarioSelectableTreatment AS l2 ON l2.SimulationId = l1.Id
			            JOIN ScenarioConditionalTreatmentConsequences AS l3 ON l3.ScenarioSelectableTreatmentId = l2.Id
			            JOIN  ScenarioTreatmentConsequence_Equation AS l4 ON l4.ScenarioConditionalTreatmentConsequenceId = l3.Id
			            WHERE l1.Id IN (SELECT Guid FROM #SimTempGuids);

			            --ALTER TABLE ScenarioTreatmentConsequence_Equation WITH CHECK CHECK CONSTRAINT all
 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in ScenarioTreatmentConsequence_Equation''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            ---------------------------------------------------------------------------

                        BEGIN TRY

			            ALTER TABLE ScenarioConditionalTreatmentConsequences NOCHECK CONSTRAINT all

			            Delete l3 
			            FROM  Simulation  AS l1
			            JOIN ScenarioSelectableTreatment AS l2 ON l2.SimulationId = l1.Id
			            JOIN ScenarioConditionalTreatmentConsequences AS l3 ON l3.ScenarioSelectableTreatmentId = l2.Id
			            WHERE l1.Id IN (SELECT Guid FROM #SimTempGuids);

			            ALTER TABLE ScenarioConditionalTreatmentConsequences WITH CHECK CHECK CONSTRAINT all

 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in ScenarioConditionalTreatmentConsequences''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

	  	            --------------------------------------------------------------------------

                        BEGIN TRY

			            --ALTER TABLE ScenarioTreatmentCost_Equation NOCHECK CONSTRAINT all

			            Delete l4 
			            FROM  Simulation  AS l1
			            JOIN ScenarioSelectableTreatment AS l2 ON l2.SimulationId = l1.Id
			            JOIN ScenarioTreatmentCost AS l3 ON l3.ScenarioSelectableTreatmentId = l2.Id
			            JOIN ScenarioTreatmentCost_Equation AS l4 ON l4.ScenarioTreatmentCostId = l3.Id
			            WHERE l1.Id IN (SELECT Guid FROM #SimTempGuids);

			            --ALTER TABLE ScenarioTreatmentCost_Equation WITH CHECK CHECK CONSTRAINT all
 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in ScenarioTreatmentCost_Equation''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH


		            ---------------------------------------------------------------------------

                        BEGIN TRY

			            ALTER TABLE CriterionLibrary_ScenarioTreatmentCost NOCHECK CONSTRAINT all

			            Delete l4 
			            FROM  Simulation  AS l1
			            JOIN ScenarioSelectableTreatment AS l2 ON l2.SimulationId = l1.Id
			            JOIN ScenarioTreatmentCost AS l3 ON l3.ScenarioSelectableTreatmentId = l2.Id
			            JOIN  CriterionLibrary_ScenarioTreatmentCost AS l4 ON l4.ScenarioTreatmentCostId = l3.Id
			            WHERE l1.Id IN (SELECT Guid FROM #SimTempGuids);

			            ALTER TABLE CriterionLibrary_ScenarioTreatmentCost WITH CHECK CHECK CONSTRAINT all

 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in CriterionLibrary_ScenarioTreatmentCost''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            ---------------------------------------------------------------------------

                        BEGIN TRY

			            ALTER TABLE ScenarioTreatmentCost NOCHECK CONSTRAINT all

			            Delete l3 
			            FROM  Simulation  AS l1
			            JOIN ScenarioSelectableTreatment AS l2 ON l2.SimulationId = l1.Id
			            JOIN ScenarioTreatmentCost AS l3 ON l3.ScenarioSelectableTreatmentId = l2.Id
			            WHERE l1.Id IN (SELECT Guid FROM #SimTempGuids);

			            ALTER TABLE ScenarioTreatmentCost WITH CHECK CHECK CONSTRAINT all

 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in ScenarioTreatmentCost''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH


		            ---------------------------------------------------------------------------

                        BEGIN TRY

			            ALTER TABLE ScenarioSelectableTreatment_ScenarioBudget NOCHECK CONSTRAINT all

			            Delete l3 
			            FROM  Simulation  AS l1
			            JOIN ScenarioSelectableTreatment AS l2 ON l2.SimulationId = l1.Id
			            JOIN ScenarioSelectableTreatment_ScenarioBudget AS l3 ON l3.ScenarioSelectableTreatmentId = l2.Id
			            WHERE l1.Id IN (SELECT Guid FROM #SimTempGuids);

			            ALTER TABLE ScenarioSelectableTreatment_ScenarioBudget WITH CHECK CHECK CONSTRAINT all
 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in ScenarioSelectableTreatment_ScenarioBudget''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            ---------------------------------------------------------------------------

                        BEGIN TRY

			            ALTER TABLE CriterionLibrary_ScenarioTreatment NOCHECK CONSTRAINT all

			            Delete l3 
			            FROM  Simulation  AS l1
			            JOIN ScenarioSelectableTreatment AS l2 ON l2.SimulationId = l1.Id
			            JOIN CriterionLibrary_ScenarioTreatment AS l3 ON l3.ScenarioSelectableTreatmentId = l2.Id
			            WHERE l1.Id IN (SELECT Guid FROM #SimTempGuids);

			            ALTER TABLE CriterionLibrary_ScenarioTreatment WITH CHECK CHECK CONSTRAINT all

 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in CriterionLibrary_ScenarioTreatment''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            ---------------------------------------------------------------------------

                        BEGIN TRY

			            ALTER TABLE ScenarioTreatmentScheduling NOCHECK CONSTRAINT all

			            Delete l3 
			            FROM  Simulation  AS l1
			            JOIN ScenarioSelectableTreatment AS l2 ON l2.SimulationId = l1.Id
			            JOIN ScenarioTreatmentScheduling AS l3 ON l3.TreatmentId = l2.Id
			            WHERE l1.Id IN (SELECT Guid FROM #SimTempGuids);

			            ALTER TABLE ScenarioTreatmentScheduling WITH CHECK CHECK CONSTRAINT all

 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in ScenarioTreatmentScheduling''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            ---------------------------------------------------------------------------

                        BEGIN TRY

			            ALTER TABLE ScenarioSelectableTreatment NOCHECK CONSTRAINT all

			            Delete l2 
			            FROM  Simulation  AS l1
			            JOIN ScenarioSelectableTreatment AS l2 ON l2.SimulationId = l1.Id
			            WHERE l1.Id IN (SELECT Guid FROM #SimTempGuids);

			            ALTER TABLE ScenarioSelectableTreatment WITH CHECK CHECK CONSTRAINT all
 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in ScenarioSelectableTreatment''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH


			            ---------------------------------------------------------------------------

                        BEGIN TRY

			            ALTER TABLE CriterionLibrary_ScenarioTargetConditionGoal NOCHECK CONSTRAINT all

			            Delete l3 
			            FROM  Simulation  AS l1
			            JOIN ScenarioTargetConditionGoals AS l2 ON l2.SimulationId = l1.Id
			            JOIN CriterionLibrary_ScenarioTargetConditionGoal AS l3 ON l3.ScenarioTargetConditionGoalId = l2.Id
			            WHERE l1.Id IN (SELECT Guid FROM #SimTempGuids);

			            ALTER TABLE CriterionLibrary_ScenarioTargetConditionGoal WITH CHECK CHECK CONSTRAINT all

 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in CriterionLibrary_ScenarioTargetConditionGoal''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            ---------------------------------------------------------------------------

                        BEGIN TRY

			            ALTER TABLE ScenarioTargetConditionGoals NOCHECK CONSTRAINT all

			            Delete l2 
			            FROM  Simulation  AS l1
			            JOIN ScenarioTargetConditionGoals AS l2 ON l2.SimulationId = l1.Id
			            WHERE l1.Id IN (SELECT Guid FROM #SimTempGuids);

			            ALTER TABLE ScenarioTargetConditionGoals WITH CHECK CHECK CONSTRAINT all
 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in ScenarioTargetConditionGoals''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            ---------------------------------------------------------------------------

                        BEGIN TRY

			            ALTER TABLE InvestmentPlan NOCHECK CONSTRAINT all

			            Delete l2 
			            FROM  Simulation  AS l1
			            JOIN InvestmentPlan AS l2 ON l2.SimulationId = l1.Id
			            WHERE l1.Id IN (SELECT Guid FROM #SimTempGuids);

			            ALTER TABLE InvestmentPlan WITH CHECK CHECK CONSTRAINT all

 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in InvestmentPlan''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            ---------------------------------------------------------------------------

                        BEGIN TRY

			            ALTER TABLE ReportIndex NOCHECK CONSTRAINT all

			            Delete l2 
			            FROM  Simulation  AS l1
			            JOIN ReportIndex AS l2 ON l2.SimulationId = l1.Id
			            WHERE l1.Id IN (SELECT Guid FROM #SimTempGuids);

			            ALTER TABLE ReportIndex WITH CHECK CHECK CONSTRAINT all

 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in ReportIndex''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            ---------------------------------------------------------------------------

                        BEGIN TRY

			            ALTER TABLE Simulation_User NOCHECK CONSTRAINT all

			            Delete l2 
			            FROM  Simulation  AS l1
			            JOIN Simulation_User AS l2 ON l2.SimulationId = l1.Id
			            WHERE l1.Id IN (SELECT Guid FROM #SimTempGuids);

			            ALTER TABLE Simulation_User WITH CHECK CHECK CONSTRAINT all

 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in Simulation_User''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

                        ---------------------------------------------------------------------------

                        BEGIN TRY
								ALTER TABLE SimulationAnalysisDetail NOCHECK CONSTRAINT ALL 

								DELETE l2
								FROM Simulation AS l1
                                JOIN SimulationAnalysisDetail AS l2 on l2.SimulationId = l1.Id
								WHERE l1.Id IN (SELECT Guid FROM #SimTempGuids); 

								ALTER TABLE SimulationAnalysisDetail WITH CHECK CHECK CONSTRAINT ALL 

							END TRY
							BEGIN CATCH
								SELECT ERROR_NUMBER() AS ErrorNumber
									  ,ERROR_SEVERITY() AS ErrorSeverity
									  ,ERROR_STATE() AS ErrorState
									  ,ERROR_PROCEDURE() AS ErrorProcedure
									  ,ERROR_LINE() AS ErrorLine
									  ,ERROR_MESSAGE() AS ErrorMessage;

								SELECT @CustomErrorMessage = ''Query Error in SimulationAnalysisDetail''
								RAISERROR (@CustomErrorMessage, 16, 1);
								Set @RetMessage = @CustomErrorMessage;
							END CATCH

			            ---------------------------------------------------------------------------

                        BEGIN TRY
								PRINT ''Starting new logic for SimulationOutput data deletion via partition switching.'';

								-- 1. Create a temporary table to hold RunIds and their corresponding SimulationOutput.Id
								--    for the simulations being deleted.
								IF OBJECT_ID(''tempdb..#SimulationOutputsToProcess'') IS NOT NULL
									DROP TABLE #SimulationOutputsToProcess;
								CREATE TABLE #SimulationOutputsToProcess
								(
									SimulationOutputId UNIQUEIDENTIFIER PRIMARY KEY,
									RunId INT NOT NULL,
									SimulationId UNIQUEIDENTIFIER NOT NULL -- For reference/logging if needed
								);

								INSERT INTO #SimulationOutputsToProcess (SimulationOutputId, RunId, SimulationId)
								SELECT DISTINCT
									   so.Id,
									   so.RunId,
									   s.Guid
								FROM #SimTempGuids s
								JOIN dbo.SimulationOutput so ON s.Guid = so.SimulationId
								WHERE so.RunId IS NOT NULL; -- Process only if RunId exists (implies it''s part of partitioning scheme)

								PRINT ''Found '' + CAST(@@ROWCOUNT AS VARCHAR(10)) + '' SimulationOutput records with RunIds to process.'';

								-- Declare variables for the loop
								DECLARE @CurrentProcessingOutputId UNIQUEIDENTIFIER;
								DECLARE @CurrentProcessingRunId INT;

								-- Cursor to iterate through each SimulationOutput that needs partition processing
								DECLARE sim_output_cursor CURSOR LOCAL FAST_FORWARD FOR
									SELECT SimulationOutputId, RunId FROM #SimulationOutputsToProcess;

								OPEN sim_output_cursor;
								FETCH NEXT FROM sim_output_cursor INTO @CurrentProcessingOutputId, @CurrentProcessingRunId;

								WHILE @@FETCH_STATUS = 0
								BEGIN
									PRINT N''-------------------------------------------------------------------------------'';
									PRINT N''Processing SimulationOutputId: '' + CAST(@CurrentProcessingOutputId AS VARCHAR(36)) + N'', RunId: '' + CAST(@CurrentProcessingRunId AS VARCHAR(10));

									-- 2. Delete from SimulationOutputJson for the current SimulationOutputId
									--    (This table is related via SimulationOutputId, not directly partitioned by RunId in the same way as others in C# example)
									DELETE FROM dbo.SimulationOutputJson WHERE SimulationOutputId = @CurrentProcessingOutputId;
									PRINT N''  Deleted from SimulationOutputJson for OutputId: '' + CAST(@CurrentProcessingOutputId AS VARCHAR(36)) + N'' (Rows: '' + CAST(@@ROWCOUNT AS VARCHAR(10)) + N'')'';

									-- 3. Define the list of partitioned tables (Order: Children before Parents as in C#)
									--    This order is important for disabling/enabling FKs if they were not schema-bound or if switching could somehow be affected.
									--    SWITCH operation itself bypasses FK checks, but subsequent re-enabling relies on consistent data.
									DECLARE @PartitionedTableList TABLE (
										OrderId INT IDENTITY(1,1) PRIMARY KEY,
										SchemaName SYSNAME,
										TableName SYSNAME
									);
									INSERT INTO @PartitionedTableList (SchemaName, TableName) VALUES
										(''dbo'', ''BudgetDetail''),
										(''dbo'', ''AssetDetailValueIntId''),
										(''dbo'', ''AssetSummaryDetailValueIntId''), -- Child of AssetSummaryDetail
										(''dbo'', ''TreatmentOptionDetail''),
										(''dbo'', ''TreatmentRejectionDetail''),
										(''dbo'', ''TreatmentSchedulingCollisionDetail''),
										(''dbo'', ''BudgetToSpend''),                -- Child of FundingCalculationInput
										(''dbo'', ''Allocation''),                   -- Child of FundingCalculationOutput
										(''dbo'', ''CashFlowConsiderationDetail''),  -- Child of TreatmentConsiderationDetail
										(''dbo'', ''TargetConditionGoalDetail''),
										(''dbo'', ''DeficientConditionGoalDetail''),
										-- Parents
										(''dbo'', ''FundingCalculationInput''),      -- Parent of BudgetToSpend
										(''dbo'', ''FundingCalculationOutput''),     -- Parent of Allocation
										(''dbo'', ''TreatmentConsiderationDetail''), -- Parent of Funding*, CashFlow*
										(''dbo'', ''AssetDetail''),                  -- Parent of ValueIntId, Treatment*
										(''dbo'', ''AssetSummaryDetail''),           -- Parent of ValueIntId (AssetSummaryDetailValueIntId)
										(''dbo'', ''SimulationYearDetail'');         -- Parent of AssetDetail & others

									DECLARE @CurrentSchemaName SYSNAME;
									DECLARE @CurrentTableName SYSNAME;
									DECLARE @FullyQualifiedTableForObjectLookup NVARCHAR(257); -- Format: schema.table
									DECLARE @FullyQualifiedTableForSPCall NVARCHAR(260);       -- Format: [schema].[table]

									-- Check from C# logic: if (_unitOfWork.Context.AssetSummaryDetail.Where(_ => _.RunId == simulationRunId).Any())
									-- This ensures we only attempt to switch if data is expected to exist for this RunId in a key table.
									IF EXISTS (SELECT 1 FROM dbo.AssetSummaryDetail WHERE RunId = @CurrentProcessingRunId)
									BEGIN
										PRINT N''  Partitioned data found in AssetSummaryDetail for RunId: '' + CAST(@CurrentProcessingRunId AS VARCHAR(10)) + N''. Proceeding with switch out for related tables.'';

										DECLARE table_partition_cursor CURSOR LOCAL FAST_FORWARD FOR
											SELECT SchemaName, TableName FROM @PartitionedTableList ORDER BY OrderId ASC; -- Process in defined order

										OPEN table_partition_cursor;
										FETCH NEXT FROM table_partition_cursor INTO @CurrentSchemaName, @CurrentTableName;

										WHILE @@FETCH_STATUS = 0
										BEGIN
											SET @FullyQualifiedTableForObjectLookup = @CurrentSchemaName + N''.'' + @CurrentTableName;
											SET @FullyQualifiedTableForSPCall = QUOTENAME(@CurrentSchemaName) + N''.'' + QUOTENAME(@CurrentTableName);

											PRINT N''    Processing partitioned table: '' + @FullyQualifiedTableForSPCall + N'' for RunId: '' + CAST(@CurrentProcessingRunId AS VARCHAR(10));

											-- 3a. Disable foreign key constraints referencing this table
											DECLARE @SqlDisableFK NVARCHAR(MAX) = N'''';
											SELECT @SqlDisableFK = @SqlDisableFK +
												N''IF OBJECT_ID(N'''''' + QUOTENAME(OBJECT_SCHEMA_NAME(fk.parent_object_id)) + N''.'' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N'''''') IS NOT NULL AND OBJECT_ID(N'''''' + QUOTENAME(fk.name) + N'''''', N''''F'''') IS NOT NULL '' +
												N''ALTER TABLE '' + QUOTENAME(OBJECT_SCHEMA_NAME(fk.parent_object_id)) +
												N''.'' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) +
												N'' NOCHECK CONSTRAINT '' + QUOTENAME(fk.name) + N'';'' + CHAR(13)
											FROM sys.foreign_keys fk
											JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
											WHERE fk.referenced_object_id = OBJECT_ID(@FullyQualifiedTableForObjectLookup);

											IF LEN(ISNULL(@SqlDisableFK, N'''')) > 0
											BEGIN
												-- PRINT N''      Disabling FKs: '' + @SqlDisableFK; -- Can be verbose
												EXEC sp_executesql @SqlDisableFK;
												PRINT N''      Disabled FKs pointing to '' + @FullyQualifiedTableForObjectLookup;
											END
											ELSE
											BEGIN
												PRINT N''      No FKs to disable pointing to '' + @FullyQualifiedTableForObjectLookup;
											END

											-- 3b. Execute partition switch out
											DECLARE @SwitchOutSql NVARCHAR(MAX);
											SET @SwitchOutSql = N''EXEC dbo.usp_PurgePartitionViaSwitchOut @SourceTable = N'''''' + @FullyQualifiedTableForSPCall + '''''', @PartitionValue = '' + CAST(@CurrentProcessingRunId AS NVARCHAR(10)) + '';'';
											-- PRINT N''      Executing switch out: '' + @SwitchOutSql; -- Can be verbose
											EXEC sp_executesql @SwitchOutSql;
											PRINT N''      Executed partition switch out for '' + @FullyQualifiedTableForSPCall;

											-- 3c. Re-enable (and re-trust) the same foreign keys
											DECLARE @SqlEnableFK NVARCHAR(MAX) = N'''';
											SELECT @SqlEnableFK = @SqlEnableFK +
												N''IF OBJECT_ID(N'''''' + QUOTENAME(OBJECT_SCHEMA_NAME(fk.parent_object_id)) + N''.'' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N'''''') IS NOT NULL AND OBJECT_ID(N'''''' + QUOTENAME(fk.name) + N'''''', N''''F'''') IS NOT NULL '' +
												N''ALTER TABLE '' + QUOTENAME(OBJECT_SCHEMA_NAME(fk.parent_object_id)) +
												N''.'' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) +
												N'' WITH CHECK CHECK CONSTRAINT '' + QUOTENAME(fk.name) + N'';'' + CHAR(13)
											FROM sys.foreign_keys fk
											JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
											WHERE fk.referenced_object_id = OBJECT_ID(@FullyQualifiedTableForObjectLookup);

											IF LEN(ISNULL(@SqlEnableFK, N'''')) > 0
											BEGIN
												-- PRINT N''      Enabling FKs: '' + @SqlEnableFK; -- Can be verbose
												EXEC sp_executesql @SqlEnableFK;
												PRINT N''      Enabled FKs pointing to '' + @FullyQualifiedTableForObjectLookup;
											END
											ELSE
											BEGIN
												 PRINT N''      No FKs to enable pointing to '' + @FullyQualifiedTableForObjectLookup;
											END

											FETCH NEXT FROM table_partition_cursor INTO @CurrentSchemaName, @CurrentTableName;
										END

										CLOSE table_partition_cursor;
										DEALLOCATE table_partition_cursor;
									END
									ELSE
									BEGIN
										PRINT N''  No partitioned data found in AssetSummaryDetail for RunId: '' + CAST(@CurrentProcessingRunId AS VARCHAR(10)) + N''. Skipping partition switch-out loop for this RunId.'';
									END

									-- Clear the table variable for the next iteration of the outer loop (if any, though here it''s single use per RunId)
									DELETE FROM @PartitionedTableList;

									-- 4. Remove the root SimulationOutput row for this RunId
									DELETE FROM dbo.SimulationOutput WHERE RunId = @CurrentProcessingRunId;
									PRINT N''  Deleted from SimulationOutput for RunId: '' + CAST(@CurrentProcessingRunId AS VARCHAR(10)) + N'' (Rows: '' + CAST(@@ROWCOUNT AS VARCHAR(10)) + N'')'';

									-- 5. Recycle the freed partition boundary
									DECLARE @RecycleSql NVARCHAR(MAX);
									SET @RecycleSql = N''EXEC dbo.usp_RecycleFreedRunPartition @OldRunId = '' + CAST(@CurrentProcessingRunId AS NVARCHAR(10)) + '';'';
									-- PRINT N''  Executing partition recycle: '' + @RecycleSql; -- Can be verbose
									EXEC sp_executesql @RecycleSql;
									PRINT N''  Executed partition recycle for RunId: '' + CAST(@CurrentProcessingRunId AS VARCHAR(10));

									FETCH NEXT FROM sim_output_cursor INTO @CurrentProcessingOutputId, @CurrentProcessingRunId;
								END

								CLOSE sim_output_cursor;
								DEALLOCATE sim_output_cursor;

								IF OBJECT_ID(''tempdb..#SimulationOutputsToProcess'') IS NOT NULL
									DROP TABLE #SimulationOutputsToProcess;

								PRINT N''Finished processing SimulationOutput data via partition switching.'';
								PRINT N''-------------------------------------------------------------------------------'';

							END TRY
							BEGIN CATCH
								-- Clean up cursors if they are open
								IF CURSOR_STATUS(''local'', ''sim_output_cursor'') >= 0
								BEGIN
									CLOSE sim_output_cursor;
									DEALLOCATE sim_output_cursor;
								END
								IF CURSOR_STATUS(''local'', ''table_partition_cursor'') >= 0
								BEGIN
									CLOSE table_partition_cursor;
									DEALLOCATE table_partition_cursor;
								END

								IF OBJECT_ID(''tempdb..#SimulationOutputsToProcess'') IS NOT NULL
									DROP TABLE #SimulationOutputsToProcess;

								-- Capture error details (using variables already declared in your main SP)
								SELECT @ErrorNumber = ERROR_NUMBER(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE(),
									   @ErrorProcedure = ERROR_PROCEDURE(), @ErrorLine = ERROR_LINE(), @ErrorMessage = ERROR_MESSAGE();

								SET @CustomErrorMessage = N''Error during SimulationOutput partition deletion for RunId: '' + ISNULL(CAST(@CurrentProcessingRunId AS VARCHAR(10)), ''N/A'') +
														  N''. Table: '' + ISNULL(@FullyQualifiedTableForSPCall, ''N/A'') +
														  N''. Procedure: '' + @ErrorProcedure + N'', Line: '' + CAST(@ErrorLine AS VARCHAR(10)) + N'', Message: '' + @ErrorMessage;
								RAISERROR (@CustomErrorMessage, 16, 1);
								Set @RetMessage = @CustomErrorMessage;
								-- This will allow the main transactions''s CATCH block to handle the rollback.
							END CATCH;

			            ------------------------------------------------------------------
			
			            --SimulationOutputJson Delete records where Simulation is the parent

                        BEGIN TRY

			            ALTER TABLE SimulationOutputJson NOCHECK CONSTRAINT all

			            BEGIN TRY
				            Begin Transaction

					            Delete l2 
					            FROM  Simulation AS l1
					            JOIN SimulationOutputJson AS l2 ON l2.SimulationId = l1.Id
					            WHERE l1.Id IN (SELECT Guid FROM #SimTempGuids);

 					            SET @RowsDeleted = @@ROWCOUNT;
					            COMMIT TRANSACTION
					            Print ''Rows Affected SimulationOutputJson: '' +  convert(NVARCHAR(50), @RowsDeleted);
					            END TRY
					            BEGIN CATCH
							            ALTER TABLE SimulationOutputJson WITH CHECK CHECK CONSTRAINT all
  							            Set @RetMessage = ''Failed'';
							            Set @ErrorMessage =  ERROR_PROCEDURE() + '' (Error At Line: '' + cast( ERROR_LINE() as Varchar(5)) + '' ): '' + char(13) + char(10)  + ERROR_MESSAGE()  -- AS ErrorMessage;
							            Print ''Rolled Back SimulationOutputJson Delete Transaction in Simulation SP  '' + @ErrorMessage;
							            ROLLBACK TRANSACTION;
							            RAISERROR  (@RetMessage, 16, 1); 
							            Return -1;
					            END CATCH;

			            ALTER TABLE SimulationOutputJson WITH CHECK CHECK CONSTRAINT all
 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in SimulationOutputJson''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            ---------------------------------------------------------------------------

                        BEGIN TRY

			            ALTER TABLE Simulation NOCHECK CONSTRAINT all

			            Delete l1 
			            FROM  Simulation  AS l1
			            WHERE l1.Id IN (SELECT Guid FROM #SimTempGuids);

			            ALTER TABLE Simulation WITH CHECK CHECK CONSTRAINT all

 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in Simulation''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH
		
		            -------------------------------------------------------------
	
	            DROP TABLE #SimTempGuids;
                COMMIT TRANSACTION
                Print ''Simulation  Delete Transaction Committed'';
   	            --RAISERROR (@RetMessage, 0, 1);
	            END TRY
	            BEGIN CATCH
  			            Set @RetMessage = ''Failed'';
			            Set @ErrorMessage =  ERROR_PROCEDURE() + '' (Error At Line: '' + cast( ERROR_LINE() as Varchar(5)) + '' ): '' + char(13) + char(10)  + ERROR_MESSAGE()  -- AS ErrorMessage;
			            Print ''Rolled Back Simulation Delete Transaction in Simulation SP:  '' + @ErrorMessage;
			            ROLLBACK TRANSACTION;
			            RAISERROR  (@RetMessage, 16, 1);  
	            END CATCH;

            END')";

            migrationBuilder.Sql(createAlterProcSql);

            var createAlterProcSql2 = @"EXEC('CREATE OR ALTER PROCEDURE dbo.usp_delete_network(@NetworkId AS uniqueidentifier=NULL,@RetMessage VARCHAR(250) OUTPUT)
	                      AS
                BEGIN 
	               BEGIN TRY

 	            DECLARE @CustomErrorMessage NVARCHAR(MAX),
	            @ErrorNumber int,
	            @ErrorSeverity int,
	            @ErrorState int,
	            @ErrorProcedure nvarchar(126),
	            @ErrorLine int,
	            @ErrorMessage nvarchar(4000);
	            Set  @RetMessage = ''Success'';
	            DECLARE @CurrentDateTime DATETIME;
	            DECLARE @BatchSize INT = 100000;
	            DECLARE @RowsDeleted INT = 0;

                        -- ***************************************************************************************
                        -- BEGIN: New Logic for Deleting SimulationOutput data using Partition Switching (for Network Delete)
                        -- This replaces the old DELETE statements for SimulationOutput-related partitioned tables
                        -- when deleting data for a whole Network.
                        -- ***************************************************************************************
                        BEGIN TRY
                            PRINT N''Starting new logic for SimulationOutput data deletion (via NetworkId) using partition switching.'';

                            -- 1. Create a temporary table to hold RunIds and their corresponding SimulationOutput.Id
                            --    for all simulations within the given NetworkId.
                            IF OBJECT_ID(''tempdb..#SimulationOutputsToProcess'') IS NOT NULL
                                DROP TABLE #SimulationOutputsToProcess;
                            CREATE TABLE #SimulationOutputsToProcess
                            (
                                SimulationOutputId UNIQUEIDENTIFIER PRIMARY KEY,
                                RunId INT NOT NULL,
                                SimulationId UNIQUEIDENTIFIER NOT NULL -- For reference/logging
                            );

                            INSERT INTO #SimulationOutputsToProcess (SimulationOutputId, RunId, SimulationId)
                            SELECT DISTINCT
                                   so.Id,
                                   so.RunId,
                                   s.Id -- Simulation.Id
                            FROM dbo.Simulation s
                            JOIN dbo.SimulationOutput so ON s.Id = so.SimulationId
                            WHERE s.NetworkId = @NetworkId -- Filter by the input NetworkId
                              AND so.RunId IS NOT NULL;     -- Process only if RunId exists

                            PRINT N''Found '' + CAST(@@ROWCOUNT AS VARCHAR(10)) + N'' SimulationOutput records with RunIds to process for NetworkId: '' + CAST(@NetworkId AS VARCHAR(36));

                            -- Declare variables for the loop (ensure these don''t conflict with existing vars if this is nested deeper)
                            DECLARE @CurrentProcessingOutputId_Part UNIQUEIDENTIFIER;
                            DECLARE @CurrentProcessingRunId_Part INT;
                            DECLARE @FullyQualifiedTableForObjectLookup_Part NVARCHAR(257); 
                            DECLARE @FullyQualifiedTableForSPCall_Part NVARCHAR(260);  

                            -- Cursor to iterate through each SimulationOutput that needs partition processing
                            DECLARE sim_output_cursor CURSOR LOCAL FAST_FORWARD FOR
                                SELECT SimulationOutputId, RunId FROM #SimulationOutputsToProcess;

                            OPEN sim_output_cursor;
                            FETCH NEXT FROM sim_output_cursor INTO @CurrentProcessingOutputId_Part, @CurrentProcessingRunId_Part;

                            WHILE @@FETCH_STATUS = 0
                            BEGIN
                                PRINT N''-------------------------------------------------------------------------------'';
                                PRINT N''Processing SimulationOutputId: '' + CAST(@CurrentProcessingOutputId_Part AS VARCHAR(36)) + N'', RunId: '' + CAST(@CurrentProcessingRunId_Part AS VARCHAR(10));
                                SET @FullyQualifiedTableForObjectLookup_Part = NULL; -- Reset for error message context
                                SET @FullyQualifiedTableForSPCall_Part = NULL;       -- Reset for error message context

                                -- 2. Delete from SimulationOutputJson for the current SimulationOutputId
                                DELETE FROM dbo.SimulationOutputJson WHERE SimulationOutputId = @CurrentProcessingOutputId_Part;
                                PRINT N''  Deleted from SimulationOutputJson for OutputId: '' + CAST(@CurrentProcessingOutputId_Part AS VARCHAR(36)) + N'' (Rows: '' + CAST(@@ROWCOUNT AS VARCHAR(10)) + N'')'';

                                -- 3. Define the list of partitioned tables (Order: Children before Parents)
                                DECLARE @PartitionedTableList_Part TABLE (
                                    OrderId INT IDENTITY(1,1) PRIMARY KEY,
                                    SchemaName SYSNAME,
                                    TableName SYSNAME
                                );
                                INSERT INTO @PartitionedTableList_Part (SchemaName, TableName) VALUES
                                    (''dbo'', ''BudgetDetail''),
                                    (''dbo'', ''AssetDetailValueIntId''),
                                    (''dbo'', ''AssetSummaryDetailValueIntId''),
                                    (''dbo'', ''TreatmentOptionDetail''),
                                    (''dbo'', ''TreatmentRejectionDetail''),
                                    (''dbo'', ''TreatmentSchedulingCollisionDetail''),
                                    (''dbo'', ''BudgetToSpend''),
                                    (''dbo'', ''Allocation''),
                                    (''dbo'', ''CashFlowConsiderationDetail''),
                                    (''dbo'', ''TargetConditionGoalDetail''),
                                    (''dbo'', ''DeficientConditionGoalDetail''),
                                    (''dbo'', ''FundingCalculationInput''),
                                    (''dbo'', ''FundingCalculationOutput''),
                                    (''dbo'', ''TreatmentConsiderationDetail''),
                                    (''dbo'', ''AssetDetail''),
                                    (''dbo'', ''AssetSummaryDetail''),
                                    (''dbo'', ''SimulationYearDetail'');

                                DECLARE @CurrentSchemaName_Part SYSNAME;
                                DECLARE @CurrentTableName_Part SYSNAME;

                                IF EXISTS (SELECT 1 FROM dbo.AssetSummaryDetail WHERE RunId = @CurrentProcessingRunId_Part)
                                BEGIN
                                    PRINT N''  Partitioned data found in AssetSummaryDetail for RunId: '' + CAST(@CurrentProcessingRunId_Part AS VARCHAR(10)) + N''. Proceeding with switch out.'';

                                    DECLARE table_partition_cursor CURSOR LOCAL FAST_FORWARD FOR
                                        SELECT SchemaName, TableName FROM @PartitionedTableList_Part ORDER BY OrderId ASC;

                                    OPEN table_partition_cursor;
                                    FETCH NEXT FROM table_partition_cursor INTO @CurrentSchemaName_Part, @CurrentTableName_Part;

                                    WHILE @@FETCH_STATUS = 0
                                    BEGIN
                                        SET @FullyQualifiedTableForObjectLookup_Part = @CurrentSchemaName_Part + N''.'' + @CurrentTableName_Part;
                                        SET @FullyQualifiedTableForSPCall_Part = QUOTENAME(@CurrentSchemaName_Part) + N''.'' + QUOTENAME(@CurrentTableName_Part);

                                        PRINT N''    Processing partitioned table: '' + @FullyQualifiedTableForSPCall_Part + N'' for RunId: '' + CAST(@CurrentProcessingRunId_Part AS VARCHAR(10));

                                        DECLARE @SqlDisableFK_Part NVARCHAR(MAX) = N'''';
                                        SELECT @SqlDisableFK_Part = @SqlDisableFK_Part +
                                            N''IF OBJECT_ID(N'''''' + QUOTENAME(OBJECT_SCHEMA_NAME(fk.parent_object_id)) + N''.'' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N'''''') IS NOT NULL AND OBJECT_ID(N'''''' + QUOTENAME(fk.name) + N'''''', N''''F'''') IS NOT NULL '' +
                                            N''ALTER TABLE '' + QUOTENAME(OBJECT_SCHEMA_NAME(fk.parent_object_id)) +
                                            N''.'' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) +
                                            N'' NOCHECK CONSTRAINT '' + QUOTENAME(fk.name) + N'';'' + CHAR(13)
                                        FROM sys.foreign_keys fk
                                        JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
                                        WHERE fk.referenced_object_id = OBJECT_ID(@FullyQualifiedTableForObjectLookup_Part);

                                        IF LEN(ISNULL(@SqlDisableFK_Part, N'''')) > 0 EXEC sp_executesql @SqlDisableFK_Part;
                                        PRINT N''      Disabled/Checked FKs pointing to '' + @FullyQualifiedTableForObjectLookup_Part;
                
                                        DECLARE @SwitchOutSql_Part NVARCHAR(MAX);
                                        SET @SwitchOutSql_Part = N''EXEC dbo.usp_PurgePartitionViaSwitchOut @SourceTable = N'''''' + @FullyQualifiedTableForSPCall_Part + N'''''', @PartitionValue = '' + CAST(@CurrentProcessingRunId_Part AS NVARCHAR(10)) + '';'';
                                        EXEC sp_executesql @SwitchOutSql_Part;
                                        PRINT N''      Executed partition switch out for '' + @FullyQualifiedTableForSPCall_Part;

                                        DECLARE @SqlEnableFK_Part NVARCHAR(MAX) = N'''';
                                        SELECT @SqlEnableFK_Part = @SqlEnableFK_Part +
                                            N''IF OBJECT_ID(N'''''' + QUOTENAME(OBJECT_SCHEMA_NAME(fk.parent_object_id)) + N''.'' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) + N'''''') IS NOT NULL AND OBJECT_ID(N'''''' + QUOTENAME(fk.name) + N'''''', N''''F'''') IS NOT NULL '' +
                                            N''ALTER TABLE '' + QUOTENAME(OBJECT_SCHEMA_NAME(fk.parent_object_id)) +
                                            N''.'' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) +
                                            N'' WITH CHECK CHECK CONSTRAINT '' + QUOTENAME(fk.name) + N'';'' + CHAR(13)
                                        FROM sys.foreign_keys fk
                                        JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
                                        WHERE fk.referenced_object_id = OBJECT_ID(@FullyQualifiedTableForObjectLookup_Part);

                                        IF LEN(ISNULL(@SqlEnableFK_Part, N'''')) > 0 EXEC sp_executesql @SqlEnableFK_Part;
                                        PRINT N''      Enabled/Checked FKs pointing to '' + @FullyQualifiedTableForObjectLookup_Part;

                                        FETCH NEXT FROM table_partition_cursor INTO @CurrentSchemaName_Part, @CurrentTableName_Part;
                                    END
                                    CLOSE table_partition_cursor;
                                    DEALLOCATE table_partition_cursor;
                                END
                                ELSE
                                BEGIN
                                    PRINT N''  No partitioned data found in AssetSummaryDetail for RunId: '' + CAST(@CurrentProcessingRunId_Part AS VARCHAR(10)) + N''. Skipping partition switch-out loop.'';
                                END

                                DELETE FROM @PartitionedTableList_Part;

                                -- 4. Remove the root SimulationOutput row for this RunId
                                DELETE FROM dbo.SimulationOutput WHERE RunId = @CurrentProcessingRunId_Part;
                                PRINT N''  Deleted from SimulationOutput for RunId: '' + CAST(@CurrentProcessingRunId_Part AS VARCHAR(10)) + N'' (Rows: '' + CAST(@@ROWCOUNT AS VARCHAR(10)) + N'')'';

                                -- 5. Recycle the freed partition boundary
                                DECLARE @RecycleSql_Part NVARCHAR(MAX);
                                SET @RecycleSql_Part = N''EXEC dbo.usp_RecycleFreedRunPartition @OldRunId = '' + CAST(@CurrentProcessingRunId_Part AS NVARCHAR(10)) + '';'';
                                EXEC sp_executesql @RecycleSql_Part;
                                PRINT N''  Executed partition recycle for RunId: '' + CAST(@CurrentProcessingRunId_Part AS VARCHAR(10));

                                FETCH NEXT FROM sim_output_cursor INTO @CurrentProcessingOutputId_Part, @CurrentProcessingRunId_Part;
                            END

                            CLOSE sim_output_cursor;
                            DEALLOCATE sim_output_cursor;

                            IF OBJECT_ID(''tempdb..#SimulationOutputsToProcess'') IS NOT NULL
                                DROP TABLE #SimulationOutputsToProcess;

                            PRINT N''Finished processing SimulationOutput data (via NetworkId) using partition switching.'';
                            PRINT N''-------------------------------------------------------------------------------'';

                        END TRY
                        BEGIN CATCH
                            IF CURSOR_STATUS(''local'', ''sim_output_cursor'') >= 0 BEGIN CLOSE sim_output_cursor; DEALLOCATE sim_output_cursor; END
                            IF CURSOR_STATUS(''local'', ''table_partition_cursor'') >= 0 BEGIN CLOSE table_partition_cursor; DEALLOCATE table_partition_cursor; END
                            IF OBJECT_ID(''tempdb..#SimulationOutputsToProcess'') IS NOT NULL DROP TABLE #SimulationOutputsToProcess;

                            -- Use existing error variables from the main SP scope
                            SELECT @ErrorNumber = ERROR_NUMBER(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE(),
                                   @ErrorProcedure = ERROR_PROCEDURE(), @ErrorLine = ERROR_LINE(), @ErrorMessage = ERROR_MESSAGE();
    
                            SET @CustomErrorMessage = N''Error during SimulationOutput partition deletion (NetworkId: '' + CAST(@NetworkId AS VARCHAR(36)) + 
                                                      N'', Current RunId: '' + ISNULL(CAST(@CurrentProcessingRunId_Part AS VARCHAR(10)), ''N/A'') +
                                                      N'', Table: '' + ISNULL(@FullyQualifiedTableForSPCall_Part, ''N/A'') +
                                                      N''). Proc: '' + @ErrorProcedure + N'', Line: '' + CAST(@ErrorLine AS VARCHAR(10)) + N'', Msg: '' + @ErrorMessage;
                            RAISERROR (@CustomErrorMessage, 16, 1);
                            Set @RetMessage = @CustomErrorMessage; 
                            -- This RAISERROR will be caught by the outer CATCH block of usp_delete_network
                        END CATCH;
                        -- ***************************************************************************************
                        -- END: New Logic for Deleting SimulationOutput data using Partition Switching (for Network Delete)
                        -- ***************************************************************************************

            -----Start BenefitQuantifier Path-----------------------------------------

			            --Network --> BenefitQuantifier

                        BEGIN TRY

                      ALTER TABLE BenefitQuantifier NOCHECK CONSTRAINT all

		  	            Print ''BenefitQuantifier '';

			            Delete l2 
			            FROM Network AS l1
			            JOIN BenefitQuantifier AS l2 ON l2.NetworkId = l1.Id
			            WHERE l1.Id IN (@NetworkId);

                        ALTER TABLE BenefitQuantifier WITH CHECK CHECK CONSTRAINT all

                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in Network --> BenefitQuantifier''
		                     RAISERROR (@CustomErrorMessage, 16, 1);
				             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            ------End BenefitQuantifier------------------------------------------

			            -----Start NetworkRollupDetail Path-----------------------------------------

			            --Network --> NetworkRollupDetail

                       BEGIN TRY

			            ALTER TABLE NetworkRollupDetail NOCHECK CONSTRAINT all

			            Print ''NetworkRollupDetail '';

			            Delete l2 
			            FROM Network AS l1
			            JOIN NetworkRollupDetail AS l2 ON l2.NetworkId = l1.Id
			            WHERE l1.Id IN (@NetworkId);

                        ALTER TABLE NetworkRollupDetail WITH CHECK CHECK CONSTRAINT all

                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in Network -->  NetworkRollupDetail''
		                     RAISERROR (@CustomErrorMessage, 16, 1);
				             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            ------End NetworkRollupDetail------------------------------------------
			            -----Start NetworkAttribute Path-----------------------------------------

			            --Network --> NetworkAttribute

                        BEGIN TRY

                       ALTER TABLE NetworkAttribute NOCHECK CONSTRAINT all

		               Print ''NetworkAttribute '';

			            Delete l2 
			            FROM Network AS l1
			            JOIN NetworkAttribute AS l2 ON l2.NetworkId = l1.Id
			            WHERE l1.Id IN (@NetworkId);

                        ALTER TABLE NetworkAttribute WITH CHECK CHECK CONSTRAINT all

                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in Network --> NetworkAttribute''
		                     RAISERROR (@CustomErrorMessage, 16, 1);
				             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            ------End NetworkAttribute------------------------------------------
			            -----Start AnalysisMaintainableAsset Path-----------------------------------------

			            --Network --> AnalysisMaintainableAsset

                        BEGIN TRY

                      ALTER TABLE AnalysisMaintainableAsset NOCHECK CONSTRAINT all

		              Print ''AnalysisMaintainableAsset '';

			            Delete l2 
			            FROM Network AS l1
			            JOIN AnalysisMaintainableAsset AS l2 ON l2.NetworkId = l1.Id
			            WHERE l1.Id IN (@NetworkId);

                        ALTER TABLE AnalysisMaintainableAsset WITH CHECK CHECK CONSTRAINT all;

                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in Network --> AnalysisMaintainableAsset''
		                     RAISERROR (@CustomErrorMessage, 16, 1);
				             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

               ------End AnalysisMaintainableAsset------------------------------------------
	            ----- Start MaintainableAsset Path
	            -----Start AggregatedResult Path-----------------------------------------

			            --MaintainableAsset --> AggregatedResult

	            BEGIN TRY

		            Set @RowsDeleted = 1;

		            ALTER TABLE AggregatedResult NOCHECK CONSTRAINT all;
		            --ALTER INDEX ALL ON AggregatedResult DISABLE;
		
		            Print ''AggregatedResult '';

				            WHILE @RowsDeleted > 0
				            BEGIN
					            BEGIN TRY
						            --Begin Transaction
					
						            --Delete TOP (@BatchSize) l3
						            SELECT TOP (@BatchSize) l3.Id  INTO #tempAggregatedResult
						            FROM Network AS l1
						            Join MaintainableAsset AS l2 ON l2.NetworkId = l1.Id
						            JOIN AggregatedResult AS l3 ON l3.MaintainableAssetId = l2.Id
						            WHERE l1.Id IN (@NetworkId);
						
						            DELETE FROM AggregatedResult WHERE Id in (SELECT Id FROM #tempAggregatedResult);

						            SET @RowsDeleted = @@ROWCOUNT;

						            DROP TABLE #tempAggregatedResult;
						            --WAITFOR DELAY ''00:00:01'';
						
						            --COMMIT TRANSACTION
						
						            Print ''Rows Affected Network --> MaintainableAsset-->AggregatedResult: '' +  convert(NVARCHAR(50), @RowsDeleted);
					            END TRY
					            BEGIN CATCH
							            ALTER TABLE AggregatedResult WITH CHECK CHECK CONSTRAINT all
  							            Set @RetMessage = ''Failed'';
							            Set @ErrorMessage =  ERROR_PROCEDURE() + '' (Error At Line: '' + cast( ERROR_LINE() as Varchar(5)) + '' ): '' + char(13) + char(10)  + ERROR_MESSAGE()  -- AS ErrorMessage;
							            --Print ''Rolled Back AggregatedResult Delete Transaction in NetworkDelete SP:  '' + @ErrorMessage;
							            --ROLLBACK TRANSACTION;
							            RAISERROR  (@RetMessage, 16, 1); 
							            Return -1;
					            END CATCH;
				            END

                        ALTER TABLE AggregatedResult WITH CHECK CHECK CONSTRAINT all
					
	            END TRY
	            BEGIN CATCH
		            ALTER TABLE AggregatedResult WITH CHECK CHECK CONSTRAINT all;
		            --ALTER INDEX ALL ON AggregatedResult REBUILD;
		            Print ''Query Error in  Network --> MaintainableAsset-->AggregatedResult ***Failed***'';
		            SELECT ERROR_NUMBER() AS ErrorNumber
		            ,ERROR_SEVERITY() AS ErrorSeverity
		            ,ERROR_STATE() AS ErrorState
		            ,ERROR_PROCEDURE() AS ErrorProcedure
		            ,ERROR_LINE() AS ErrorLine
		            ,ERROR_MESSAGE() AS ErrorMessage;

		            SELECT @CustomErrorMessage = ''Query Error in  Network --> MaintainableAsset-->AggregatedResult''
		            RAISERROR (@CustomErrorMessage, 16, 1);
		            Set @RetMessage = @CustomErrorMessage;
	            END CATCH;

            -----End AggregatedResult -----------------------------------------------------------------


            -------Start AttributeDatum Path-

            --MaintainableAsset --> AttributeDatum --> AttributeDatumLocation -->  -->  --> 

	            BEGIN TRY

                      ALTER TABLE AttributeDatumLocation NOCHECK CONSTRAINT all
		              SET @RowsDeleted = 1;
		              Print ''AttributeDatumLocation '';

		              WHILE @RowsDeleted > 0
				            BEGIN
					            BEGIN TRY
						            --Begin Transaction

							            SELECT TOP (@BatchSize) l4.Id  INTO #tempAttributeDatumLocation
							            FROM Network AS l1
							            Join MaintainableAsset AS l2 ON l2.NetworkId = l1.Id
							            JOIN AttributeDatum AS l3 ON l3.MaintainableAssetId = l2.Id
							            Join AttributeDatumLocation As l4 ON l4.AttributeDatumId = l3.Id
							            WHERE l1.Id IN (@NetworkId);

						            DELETE FROM AttributeDatumLocation WHERE Id in (SELECT Id FROM #tempAttributeDatumLocation);

						            SET @RowsDeleted = @@ROWCOUNT;

						            DROP TABLE #tempAttributeDatumLocation;
						            --WAITFOR DELAY ''00:00:01'';

						            --COMMIT TRANSACTION
						
						            Print ''Rows Affected Network --> MaintainableAsset-->AttributeDatumLocation: '' +  convert(NVARCHAR(50), @RowsDeleted);
					            END TRY
					            BEGIN CATCH
							            ALTER TABLE AttributeDatumLocation WITH CHECK CHECK CONSTRAINT all
  							            Set @RetMessage = ''Failed'';
							            Set @ErrorMessage =  ERROR_PROCEDURE() + '' (Error At Line: '' + cast( ERROR_LINE() as Varchar(5)) + '' ): '' + char(13) + char(10)  + ERROR_MESSAGE()  -- AS ErrorMessage;
							            --Print ''Rolled Back AttributeDatumLocation Delete Transaction in NetworkDelete SP:  '' + @ErrorMessage;
							            --ROLLBACK TRANSACTION;
							            RAISERROR  (@RetMessage, 16, 1); 
							            Return -1;
					            END CATCH;
				            END

                        ALTER TABLE AttributeDatumLocation WITH CHECK CHECK CONSTRAINT all

                        END TRY 
	            BEGIN CATCH
			            ALTER TABLE AttributeDatumLocation WITH CHECK CHECK CONSTRAINT all;
			            Print ''Query Error in Network --> MaintainableAsset-->AttributeDatum-->AttributeDatumLocationt ***Failed***'';
			            SELECT ERROR_NUMBER() AS ErrorNumber
			            ,ERROR_SEVERITY() AS ErrorSeverity
			            ,ERROR_STATE() AS ErrorState
			            ,ERROR_PROCEDURE() AS ErrorProcedure
			            ,ERROR_LINE() AS ErrorLine
			            ,ERROR_MESSAGE() AS ErrorMessage;

			            SELECT @CustomErrorMessage = ''Query Error in  Network --> MaintainableAsset-->AttributeDatum-->AttributeDatumLocation''
			            RAISERROR (@CustomErrorMessage, 16, 1);
			            Set @RetMessage = @CustomErrorMessage;
	            END CATCH;



            --------End AttributeDatumLocation---------------------------------------------------------------

            --------MaintainableAsset --> AttributeDatum--
          
	            BEGIN TRY

			            ALTER TABLE AttributeDatum NOCHECK CONSTRAINT all
			            SET @RowsDeleted = 1;
			            Print ''AttributeDatum '';

			            WHILE @RowsDeleted > 0
				            BEGIN
					            BEGIN TRY
					            --Begin Transaction

						            Select  TOP (@BatchSize) l3.Id  INTO #tempAttributeDatum
						            FROM Network AS l1
						            Join MaintainableAsset AS l2 ON l2.NetworkId = l1.Id
						            JOIN AttributeDatum AS l3 ON l3.MaintainableAssetId = l2.Id
						            WHERE l1.Id IN (@NetworkId);

						            DELETE FROM AttributeDatum WHERE Id in (SELECT Id FROM #tempAttributeDatum);

						            SET @RowsDeleted = @@ROWCOUNT;

						            DROP TABLE #tempAttributeDatum;
						            --WAITFOR DELAY ''00:00:01'';

						            --COMMIT TRANSACTION
						
						            Print ''Rows Affected Network --> MaintainableAsset-->AttributeDatum: '' +  convert(NVARCHAR(50), @RowsDeleted);
					            END TRY
					            BEGIN CATCH
						            ALTER TABLE AttributeDatum WITH CHECK CHECK CONSTRAINT all
						            Set @RetMessage = ''Failed'';
						            Set @ErrorMessage =  ERROR_PROCEDURE() + '' (Error At Line: '' + cast( ERROR_LINE() as Varchar(5)) + '' ): '' + char(13) + char(10)  + ERROR_MESSAGE()  -- AS ErrorMessage;
						            --Print ''Rolled Back AttributeDatum Delete Transaction in NetworkDelete SP:  '' + @ErrorMessage;
						            --ROLLBACK TRANSACTION;
						            RAISERROR  (@RetMessage, 16, 1); 
						            Return -1;
					            END CATCH;
				            END

                        ALTER TABLE AttributeDatum WITH CHECK CHECK CONSTRAINT all

                        END TRY 
	            BEGIN CATCH
		            ALTER TABLE AttributeDatum WITH CHECK CHECK CONSTRAINT all;
		            --ALTER INDEX ALL ON AttributeDatum REBUILD
		            Print ''Query Error in Network --> MaintainableAsset-->AttributeDatum ***Failed***'';
		            SELECT ERROR_NUMBER() AS ErrorNumber
		            ,ERROR_SEVERITY() AS ErrorSeverity
		            ,ERROR_STATE() AS ErrorState
		            ,ERROR_PROCEDURE() AS ErrorProcedure
		            ,ERROR_LINE() AS ErrorLine
		            ,ERROR_MESSAGE() AS ErrorMessage;

		            SELECT @CustomErrorMessage = ''Query Error in  Network --> MaintainableAsset-->AttributeDatum''
		            RAISERROR (@CustomErrorMessage, 16, 1);
		            Set @RetMessage = @CustomErrorMessage;
	            END CATCH

	            ----------------------------------------------------------------------
	            -----End AttributeDatum Path--------------------------------------

	            
	              -----Start CommittedProject Path-----------------------------------------
			
		            --Network --> MaintainableAsset --> CommittedProject --> CommittedProjectConsequence -->  -->  -->  -->  -->  --> 

                     BEGIN TRY

                        ALTER TABLE CommittedProjectConsequence NOCHECK CONSTRAINT all

			            Print ''CommittedProjectConsequence '';

			            Delete l4 
			            FROM Network AS l1
			            Join MaintainableAsset AS l2 ON l2.NetworkId = l1.Id
			            Join CommittedProject  AS l3 ON l3.MaintainableAssetEntityId = l2.Id
			            JOIN CommittedProjectConsequence AS l4 ON l4.CommittedProjectId = l3.Id
			            WHERE l1.Id IN (@NetworkId);

			            SET @RowsDeleted = @@ROWCOUNT;
			            --Print ''Rows Affected Network Delete CommittedProjectConsequence: '' +  convert(NVARCHAR(50), @RowsDeleted);

			            ALTER TABLE CommittedProjectConsequence WITH CHECK CHECK CONSTRAINT all
 	
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in CommittedProjectConsequence''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            -------------------------------------

			            --Network --> MaintainableAsset --> CommittedProject --> CommittedProjectLocation -->  -->  -->  -->  -->  -->

			             BEGIN TRY

                        ALTER TABLE CommittedProjectLocation NOCHECK CONSTRAINT all

			            Print ''CommittedProjectLocation '';

			            Delete l4 
			            FROM Network AS l1
			            Join MaintainableAsset AS l2 ON l2.NetworkId = l1.Id
			            Join CommittedProject  AS l3 ON l3.MaintainableAssetEntityId = l2.Id
			            JOIN CommittedProjectLocation AS l4 ON l4.CommittedProjectId = l3.Id
			            WHERE l1.Id IN (@NetworkId);

			            SET @RowsDeleted = @@ROWCOUNT;
			            --Print ''Rows Affected Network Delete CommittedProjectLocation: '' +  convert(NVARCHAR(50), @RowsDeleted);

			            ALTER TABLE CommittedProjectLocation WITH CHECK CHECK CONSTRAINT all
 	
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in CommittedProjectLocation''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            --------------------------------------

			            --Network --> MaintainableAsset --> CommittedProject 

                        BEGIN TRY

			            ALTER TABLE CommittedProject NOCHECK CONSTRAINT all

			            Print ''CommittedProject '';

			            Delete l3 
			            FROM Network AS l1
			            Join MaintainableAsset AS l2 ON l2.NetworkId = l1.Id
			            Join CommittedProject AS l3 ON l3.MaintainableAssetEntityId = l2.Id
			            WHERE l1.Id IN (@NetworkId);

			            SET @RowsDeleted = @@ROWCOUNT;
			            --Print ''Rows Affected Network Delete CommittedProject: '' +  convert(NVARCHAR(50), @RowsDeleted);

			            ALTER TABLE CommittedProject WITH CHECK CHECK CONSTRAINT all

                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in CommittedProject''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH 

			            -------------------------------------


			            -----End CommittedProject Path-----------------------------------------

		            ----Start Network --> MaintainableAsset --> MaintainableAssetLocation  Path
			
			            BEGIN TRY

                        ALTER TABLE MaintainableAssetLocation NOCHECK CONSTRAINT all

		  	            Print ''MaintainableAssetLocation '';

			            Delete l3
			            FROM Network AS l1
			            Join MaintainableAsset AS l2 ON l2.NetworkId = l1.Id
			            JOIN MaintainableAssetLocation AS l3 ON l3.MaintainableAssetId = l2.Id
			            WHERE l1.Id IN (@NetworkId);

			            SET @RowsDeleted = @@ROWCOUNT;
			            --Print ''Rows Affected Network --> MaintainableAsset-->MaintainableAssetLocation: '' +  convert(NVARCHAR(50), @RowsDeleted);

                        ALTER TABLE MaintainableAssetLocation WITH CHECK CHECK CONSTRAINT all

                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in Network --> MaintainableAsset-->MaintainableAssetLocation''
		                     RAISERROR (@CustomErrorMessage, 16, 1);
				             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

	            -------End -------MaintainableAssetLocation----------------------------------------------------

			            -- Start MaintainableAsset

 			            BEGIN TRY

                        ALTER TABLE MaintainableAsset NOCHECK CONSTRAINT all

			            Print ''MaintainableAsset'';

			            Delete l2 
			            FROM Network AS l1
			            Join MaintainableAsset AS l2 ON l2.NetworkId = l1.Id
			            WHERE l1.Id IN (@NetworkId);

			            SET @RowsDeleted = @@ROWCOUNT;
			            --Print ''Rows Affected Network --> MaintainableAsset: '' +  convert(NVARCHAR(50), @RowsDeleted);

                        ALTER TABLE MaintainableAsset WITH CHECK CHECK CONSTRAINT all

                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in Network --> MaintainableAsset''
		                     RAISERROR (@CustomErrorMessage, 16, 1);
				             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            ------Network --> MaintainableAsset Path--------------------------------------------------------------------
					   			 
		            ----- Start Simulation Path
			
		                --SET @CurrentDateTime = GETDATE();
			            --PRINT ''Start Simulation Delete: '' + CONVERT(NVARCHAR, @CurrentDateTime, 120);
			            -----Start AnalysisMethod Path-----------------------------------------

                        BEGIN TRY
 
                        ALTER TABLE Benefit NOCHECK CONSTRAINT all

			            Print ''Benefit '';
			            --AnalysisMethod	Benefit							FK_Benefit_AnalysisMethod_AnalysisMethodId
			            --AnalysisMethod	CriterionLibrary_AnalysisMethod	FK_CriterionLibrary_AnalysisMethod_AnalysisMethod_AnalysisMethodId

			            Delete l4 
			            FROM Network AS l1
			            JOIN Simulation  AS l2 ON l2.NetworkId = l1.Id
			            JOIN AnalysisMethod AS l3 ON l3.SimulationId = l2.Id
			            JOIN Benefit AS l4 ON l4.AnalysisMethodId = l3.Id
			            WHERE l1.Id IN (@NetworkId);

			            SET @RowsDeleted = @@ROWCOUNT;
			            --Print ''Rows Affected Network --> Benefit: '' +  convert(NVARCHAR(50), @RowsDeleted);

                        ALTER TABLE Benefit WITH CHECK CHECK CONSTRAINT all

                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in Benefit''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            -----------------------------------------------------------------------

                        BEGIN TRY


                        ALTER TABLE CriterionLibrary_AnalysisMethod NOCHECK CONSTRAINT all

			            Print ''CriterionLibrary_AnalysisMethod '';

			            Delete l4 
			            FROM Network AS l1
			            JOIN Simulation  AS l2 ON l2.NetworkId = l1.Id
			            JOIN AnalysisMethod AS l3 ON l3.SimulationId = l2.Id
			            JOIN CriterionLibrary_AnalysisMethod AS l4 ON l4.AnalysisMethodId = l3.Id
			            WHERE l1.Id IN (@NetworkId);

			            SET @RowsDeleted = @@ROWCOUNT;
			            --Print ''Rows Affected Network --> CriterionLibrary_AnalysisMethod: '' +  convert(NVARCHAR(50), @RowsDeleted);

                        ALTER TABLE CriterionLibrary_AnalysisMethod WITH CHECK CHECK CONSTRAINT all

                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in CriterionLibrary_AnalysisMethod''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH
			            ------------------------------------------------------------------

                        BEGIN TRY

                        ALTER TABLE AnalysisMethod NOCHECK CONSTRAINT all

			            Print ''AnalysisMethod '';

			            Delete l3 
			            FROM Network AS l1
			            JOIN Simulation  AS l2 ON l2.NetworkId = l1.Id
			            JOIN AnalysisMethod AS l3 ON l3.SimulationId = l2.Id
			            WHERE l1.Id IN (@NetworkId);

			            SET @RowsDeleted = @@ROWCOUNT;
			            --Print ''Rows Affected Network --> AnalysisMethod: '' +  convert(NVARCHAR(50), @RowsDeleted);

			            ALTER TABLE AnalysisMethod WITH CHECK CHECK CONSTRAINT all
 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in AnalysisMethod''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH
 
 			            -----End Simulation --> AnalysisMethod Path-----------------------------------------
 			            -----------------------------------------------------------------------
			            -----Start Simulation --> CommittedProject Path-----------------------------------------

			            --Simulation --> CommittedProject --> CommittedProjectConsequence 

                     BEGIN TRY

                        ALTER TABLE CommittedProjectConsequence NOCHECK CONSTRAINT all

			            Print ''CommittedProjectConsequence '';

			            Delete l4 
			            FROM Network AS l1
			            JOIN Simulation  AS l2 ON l2.NetworkId = l1.Id
			            JOIN CommittedProject AS l3 ON l3.SimulationId = l2.Id
			            JOIN CommittedProjectConsequence AS l4 ON l4.CommittedProjectId = l3.Id
			            WHERE l1.Id IN (@NetworkId);

			            SET @RowsDeleted = @@ROWCOUNT;
			            --Print ''Rows Affected Network --> CommittedProjectConsequence: '' +  convert(NVARCHAR(50), @RowsDeleted);

			            ALTER TABLE CommittedProjectConsequence WITH CHECK CHECK CONSTRAINT all
 	
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in CommittedProjectConsequence''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            -------------------------------------

			            --Simulation --> CommittedProject --> CommittedProjectLocation 

			             BEGIN TRY

                        ALTER TABLE CommittedProjectLocation NOCHECK CONSTRAINT all

			            Print ''CommittedProjectLocation '';

			            Delete l4 
			            FROM Network AS l1
			            JOIN Simulation  AS l2 ON l2.NetworkId = l1.Id
			            JOIN CommittedProject AS l3 ON l3.SimulationId = l2.Id
			            JOIN CommittedProjectLocation AS l4 ON l4.CommittedProjectId = l3.Id
			            WHERE l1.Id IN (@NetworkId);

			            SET @RowsDeleted = @@ROWCOUNT;
			            --Print ''Rows Affected Network --> CommittedProjectLocation: '' +  convert(NVARCHAR(50), @RowsDeleted);

			            ALTER TABLE CommittedProjectLocation WITH CHECK CHECK CONSTRAINT all
 	
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in CommittedProjectLocation''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            --------------------------------------

			            --Simulation --> CommittedProject

                        BEGIN TRY

			            ALTER TABLE CommittedProject NOCHECK CONSTRAINT all

			            Print ''CommittedProject '';

			            Delete l3 
			            FROM Network AS l1
			            JOIN Simulation  AS l2 ON l2.NetworkId = l1.Id
			            JOIN CommittedProject AS l3 ON l3.SimulationId = l2.Id
			            WHERE l1.Id IN (@NetworkId)

			            SET @RowsDeleted = @@ROWCOUNT;
			            --Print ''Rows Affected Network --> CommittedProject: '' +  convert(NVARCHAR(50), @RowsDeleted);

			            ALTER TABLE CommittedProject WITH CHECK CHECK CONSTRAINT all

                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in CommittedProject''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH 


			            -----End CommittedProject Path-----------------------------------------
		               -----Start Simulation --> InvestmentPlan 


                        BEGIN TRY

			            ALTER TABLE InvestmentPlan NOCHECK CONSTRAINT all

			            Print ''InvestmentPlan'';

			            Delete l3 
			            FROM Network AS l1
			            JOIN Simulation  AS l2 ON l2.NetworkId = l1.Id
			            JOIN InvestmentPlan AS l3 ON l3.SimulationId = l2.Id
			            WHERE l1.Id IN (@NetworkId);

			            ALTER TABLE InvestmentPlan WITH CHECK CHECK CONSTRAINT all
 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in InvestmentPlan''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            -----End Simulation --> InvestmentPlan 

		            -----Start Simulation --> ReportIndex 


                        BEGIN TRY
			            Print ''ReportIndex'';

			            ALTER TABLE ReportIndex NOCHECK CONSTRAINT all

			            Delete l3 
			            FROM Network AS l1
			            JOIN Simulation  AS l2 ON l2.NetworkId = l1.Id
			            JOIN ReportIndex AS l3 ON l3.SimulationId = l2.Id
			            WHERE l1.Id IN (@NetworkId);

			            ALTER TABLE ReportIndex WITH CHECK CHECK CONSTRAINT all
 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in ReportIndex''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            -----End Simulation --> ReportIndex ---------------------------
			            -- Start Network --> Simulation --> ScenarioBudget --> BudgetPercentagePair -->

                        BEGIN TRY

			            ALTER TABLE BudgetPercentagePair NOCHECK CONSTRAINT all

			            Print ''BudgetPercentagePair'';

			            Delete l4 
			            FROM Network AS l1
			            JOIN Simulation  AS l2 ON l2.NetworkId = l1.Id
			            JOIN ScenarioBudget AS l3 ON l3.SimulationId = l2.Id
			            JOIN BudgetPercentagePair AS l4 ON l4.ScenarioBudgetId = l3.Id
			            WHERE l1.Id IN (@NetworkId);

			            ALTER TABLE BudgetPercentagePair WITH CHECK CHECK CONSTRAINT all
 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in BudgetPercentagePair''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            -- End Simulation --> ScenarioBudget --> BudgetPercentagePair -->

			            -- Start Simulation --> ScenarioBudget --> CommittedProjectConsequence -->

                        BEGIN TRY

                        ALTER TABLE CommittedProjectConsequence NOCHECK CONSTRAINT all

			            Print ''CommittedProjectConsequence'';

			            Delete l5 
			            FROM Network AS l1
			            JOIN Simulation  AS l2 ON l2.NetworkId = l1.Id
			            JOIN ScenarioBudget AS l3 ON l3.SimulationId = l2.Id
			            JOIN CommittedProject AS l4 ON l4.ScenarioBudgetId = l3.Id
			            JOIN CommittedProjectConsequence AS l5 ON l5.CommittedProjectId = l4.Id
			            WHERE l1.Id IN (@NetworkId);

			            ALTER TABLE CommittedProjectConsequence WITH CHECK CHECK CONSTRAINT all
 	
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in CommittedProjectConsequence''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            -------------------------------------

			            --Simulation --> ScenarioBudget --> CommittedProject --> CommittedProjectLocation

			             BEGIN TRY

                        ALTER TABLE CommittedProjectLocation NOCHECK CONSTRAINT all

			            Print ''CommittedProjectLocation '';

			            Delete l5 
			            FROM Network AS l1
			            JOIN Simulation  AS l2 ON l2.NetworkId = l1.Id
			            JOIN ScenarioBudget AS l3 ON l3.SimulationId = l2.Id
			            JOIN CommittedProject AS l4 ON l4.ScenarioBudgetId = l3.Id
			            JOIN CommittedProjectLocation AS l5 ON l5.CommittedProjectId = l4.Id
			            WHERE l1.Id IN (@NetworkId);

			            ALTER TABLE CommittedProjectLocation WITH CHECK CHECK CONSTRAINT all
 	
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in CommittedProjectLocation''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            --------------------------------------

		            --Simulation --> ScenarioBudget --> CommittedProject

                        BEGIN TRY

			            ALTER TABLE CommittedProject NOCHECK CONSTRAINT all

			            Print ''CommittedProject '';

			            Delete l4 
			            FROM Network AS l1
			            JOIN Simulation  AS l2 ON l2.NetworkId = l1.Id
			            JOIN ScenarioBudget AS l3 ON l3.SimulationId = l2.Id
			            JOIN CommittedProject AS l4 ON l4.ScenarioBudgetId = l3.Id
			            WHERE l1.Id IN (@NetworkId);

			            ALTER TABLE CommittedProject WITH CHECK CHECK CONSTRAINT all

                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in CommittedProject''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH 

			            -----End ScenarioBudget -CommittedProject Path------------

		            -----Start Network --> Simulation --> ScenarioBudget --> CriterionLibrary_ScenarioBudget

                        BEGIN TRY

			            ALTER TABLE CriterionLibrary_ScenarioBudget NOCHECK CONSTRAINT all

			            Print ''CriterionLibrary_ScenarioBudget '';

			            Delete l4 
			            FROM Network AS l1
			            JOIN Simulation  AS l2 ON l2.NetworkId = l1.Id
			            JOIN ScenarioBudget AS l3 ON l3.SimulationId = l2.Id
			            JOIN CriterionLibrary_ScenarioBudget AS l4 ON l4.ScenarioBudgetId = l3.Id
			            WHERE l1.Id IN (@NetworkId);

			            ALTER TABLE CriterionLibrary_ScenarioBudget WITH CHECK CHECK CONSTRAINT all

 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in CriterionLibrary_ScenarioBudget''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH
			            -----------------------------------------

			            --Network --> Simulation --> ScenarioBudget --> ScenarioBudgetAmount --> 

                        BEGIN TRY

			            ALTER TABLE ScenarioBudgetAmount NOCHECK CONSTRAINT all

			            Print ''ScenarioBudgetAmount'';

			            Delete l4 
			            FROM Network AS l1
			            JOIN Simulation  AS l2 ON l2.NetworkId = l1.Id
			            JOIN ScenarioBudget AS l3 ON l3.SimulationId = l2.Id
			            JOIN ScenarioBudgetAmount AS l4 ON l4.ScenarioBudgetId = l3.Id
			            WHERE l1.Id IN (@NetworkId);

			            ALTER TABLE ScenarioBudgetAmount WITH CHECK CHECK CONSTRAINT all

 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in ScenarioBudgetAmount''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            ----------------------------------------------

		            --Network --> Simulation --> ScenarioBudget --> ScenarioSelectableTreatment_ScenarioBudget --> 

                        BEGIN TRY

			            Print ''ScenarioSelectableTreatment_ScenarioBudget'';

			            ALTER TABLE ScenarioSelectableTreatment_ScenarioBudget NOCHECK CONSTRAINT all

			            Delete l4 
			            FROM Network AS l1
			            JOIN Simulation  AS l2 ON l2.NetworkId = l1.Id
			            JOIN ScenarioBudget AS l3 ON l3.SimulationId = l2.Id
			            JOIN ScenarioSelectableTreatment_ScenarioBudget AS l4 ON l4.ScenarioBudgetId = l3.Id
			            WHERE l1.Id IN (@NetworkId);

			            ALTER TABLE ScenarioSelectableTreatment_ScenarioBudget WITH CHECK CHECK CONSTRAINT all
 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in ScenarioSelectableTreatment_ScenarioBudget''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH
  
			            ------------------------------------------------------------

			            --Network --> Simulation --> ScenarioBudget 

                        BEGIN TRY

			            Print ''ScenarioBudget'';

			            ALTER TABLE ScenarioBudget NOCHECK CONSTRAINT all

			            Delete l3 
			            FROM Network AS l1
			            JOIN Simulation  AS l2 ON l2.NetworkId = l1.Id
			            JOIN ScenarioBudget AS l3 ON l3.SimulationId = l2.Id
			            WHERE l1.Id IN (@NetworkId);

			            ALTER TABLE ScenarioBudget WITH CHECK CHECK CONSTRAINT all
 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in ScenarioBudget''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            -----End --Network --> Simulation --> ScenarioBudget Path-----------------------------------------

			            ------Start  --Network --> Simulation --> ScenarioBudgetPriority -----------

			            --Network --> Simulation --> ScenarioBudgetPriority --> CriterionLibrary_ScenarioBudgetPriority --> 

                        BEGIN TRY

			            ALTER TABLE CriterionLibrary_ScenarioBudgetPriority NOCHECK CONSTRAINT all

			            Print ''CriterionLibrary_ScenarioBudgetPriority '';

			            Delete l4 
			            FROM Network AS l1
			            JOIN Simulation  AS l2 ON l2.NetworkId = l1.Id
			            JOIN ScenarioBudgetPriority AS l3 ON l3.SimulationId = l2.Id
			            JOIN CriterionLibrary_ScenarioBudgetPriority AS l4 ON l4.ScenarioBudgetPriorityId = l3.Id
			            WHERE l1.Id IN (@NetworkId);

			            ALTER TABLE CriterionLibrary_ScenarioBudgetPriority WITH CHECK CHECK CONSTRAINT all

 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in CriterionLibrary_ScenarioBudgetPriority''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            ------------------------------------------------------------

			            --Network --> Simulation --> ScenarioBudgetPriority --> BudgetPercentagePair --> 

                        BEGIN TRY

			            Print ''BudgetPercentagePair'';

			            ALTER TABLE BudgetPercentagePair NOCHECK CONSTRAINT all

			            Delete l4 
			            FROM Network AS l1
			            JOIN Simulation  AS l2 ON l2.NetworkId = l1.Id
			            JOIN ScenarioBudgetPriority AS l3 ON l3.SimulationId = l2.Id
			            JOIN BudgetPercentagePair AS l4 ON l4.ScenarioBudgetPriorityId = l3.Id
			            WHERE l1.Id IN (@NetworkId);

			            ALTER TABLE BudgetPercentagePair WITH CHECK CHECK CONSTRAINT all

 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in BudgetPercentagePair''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            ------------------------------------------------------------

			            --Network --> Simulation --> ScenarioBudgetPriority

                        BEGIN TRY

			            ALTER TABLE ScenarioBudgetPriority NOCHECK CONSTRAINT all

			            Print ''ScenarioBudgetPriority '';

			            Delete l3 
			            FROM Network AS l1
			            JOIN Simulation  AS l2 ON l2.NetworkId = l1.Id
			            JOIN ScenarioBudgetPriority AS l3 ON l3.SimulationId = l2.Id
			            WHERE l1.Id IN (@NetworkId);

			            ALTER TABLE ScenarioBudgetPriority WITH CHECK CHECK CONSTRAINT all
 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in ScenarioBudgetPriority''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            ------End Network -->  Simulation --> ScenarioBudgetPriority-------------------------------------------------------------------

			            ------Start  ScenarioCalculatedAttribute-------------------------------------------------------------------
			            --Network --> Simulation --> ScenarioCalculatedAttribute --> ScenarioCalculatedAttributePair --> ScenarioCalculatedAttributePair_Criteria
			            --Network --> Simulation --> ScenarioCalculatedAttribute --> ScenarioCalculatedAttributePair --> ScenarioCalculatedAttributePair_Equation


                        BEGIN TRY

			            ALTER TABLE ScenarioCalculatedAttributePair_Criteria NOCHECK CONSTRAINT all

			            Print ''ScenarioCalculatedAttributePair_Criteria'';

			            Delete l5 
			            FROM Network AS l1
			            JOIN Simulation  AS l2 ON l2.NetworkId = l1.Id
			            JOIN ScenarioCalculatedAttribute AS l3 ON l3.SimulationId = l2.Id
			            JOIN ScenarioCalculatedAttributePair AS l4 ON l4.ScenarioCalculatedAttributeId = l3.Id
			            JOIN ScenarioCalculatedAttributePair_Criteria AS l5 ON l5.ScenarioCalculatedAttributePairId = l4.Id
			            WHERE l1.Id IN (@NetworkId);

			            ALTER TABLE ScenarioCalculatedAttributePair_Criteria WITH CHECK CHECK CONSTRAINT all

 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in ScenarioCalculatedAttributePair_Criteria''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            ------------------------------------------------------------

			            --Network --> Simulation --> ScenarioCalculatedAttribute --> ScenarioCalculatedAttributePair --> ScenarioCalculatedAttributePair_Equation

                        BEGIN TRY

			            ALTER TABLE ScenarioCalculatedAttributePair_Equation NOCHECK CONSTRAINT all

			            Print ''ScenarioCalculatedAttributePair_Equation'';

			            Delete l5 
			            FROM Network AS l1
			            JOIN Simulation  AS l2 ON l2.NetworkId = l1.Id
			            JOIN ScenarioCalculatedAttribute AS l3 ON l3.SimulationId = l2.Id
			            JOIN ScenarioCalculatedAttributePair AS l4 ON l4.ScenarioCalculatedAttributeId = l3.Id
			            JOIN ScenarioCalculatedAttributePair_Equation AS l5 ON l5.ScenarioCalculatedAttributePairId = l4.Id
			            WHERE l1.Id IN (@NetworkId);

			            ALTER TABLE ScenarioCalculatedAttributePair_Equation WITH CHECK CHECK CONSTRAINT all
 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in ScenarioCalculatedAttributePair_Equation''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            ------------------------------------------------------------
			            --Network --> Simulation --> ScenarioCalculatedAttribute --> ScenarioCalculatedAttributePair 

			            BEGIN TRY

			            ALTER TABLE ScenarioCalculatedAttributePair NOCHECK CONSTRAINT all

			            Print ''ScenarioCalculatedAttributePair'';

			            Delete l4 
			            FROM Network AS l1
			            JOIN Simulation  AS l2 ON l2.NetworkId = l1.Id
			            JOIN ScenarioCalculatedAttribute AS l3 ON l3.SimulationId = l2.Id
			            JOIN ScenarioCalculatedAttributePair AS l4 ON l4.ScenarioCalculatedAttributeId = l3.Id
			            WHERE l1.Id IN (@NetworkId);

			            ALTER TABLE ScenarioCalculatedAttributePair WITH CHECK CHECK CONSTRAINT all
 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in ScenarioCalculatedAttributePair''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            ---------------------------------------------------------------------------
			            --Network --> Simulation --> ScenarioCalculatedAttribute 

                        BEGIN TRY

			            Print ''ScenarioCalculatedAttribute '';

			            ALTER TABLE ScenarioCalculatedAttribute NOCHECK CONSTRAINT all

			            Delete l3 
			            FROM Network AS l1
			            JOIN Simulation  AS l2 ON l2.NetworkId = l1.Id
			            JOIN ScenarioCalculatedAttribute AS l3 ON l3.SimulationId = l2.Id
			            WHERE l1.Id IN (@NetworkId);

			            ALTER TABLE ScenarioCalculatedAttribute WITH CHECK CHECK CONSTRAINT all
 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in ScenarioCalculatedAttribute''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            ----End --Network --> Simulation --> ScenarioCalculatedAttribute-------------------------------------------------------------------

			            ------Start  Network --> Simulation --> ScenarioCashFlowRule-------------------------------------------------------------------
			            ----Network --> Simulation --> ScenarioCashFlowRule --> CriterionLibrary_ScenarioCashFlowRule --> 

                        BEGIN TRY

			            ALTER TABLE CriterionLibrary_ScenarioCashFlowRule NOCHECK CONSTRAINT all

			            Print ''CriterionLibrary_ScenarioCashFlowRule '';

			            Delete l4 
			            FROM Network AS l1
			            JOIN Simulation  AS l2 ON l2.NetworkId = l1.Id
			            JOIN ScenarioCashFlowRule AS l3 ON l3.SimulationId = l2.Id
			            JOIN CriterionLibrary_ScenarioCashFlowRule AS l4 ON l4.ScenarioCashFlowRuleId = l3.Id
			            WHERE l1.Id IN (@NetworkId);

			            ALTER TABLE CriterionLibrary_ScenarioCashFlowRule WITH CHECK CHECK CONSTRAINT all
 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in CriterionLibrary_ScenarioCashFlowRule''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            ------------------------------------------------------------

			            --Network --> Simulation --> ScenarioCashFlowRule --> ScenarioCashFlowDistributionRule --> 

			            BEGIN TRY

			            ALTER TABLE ScenarioCashFlowDistributionRule NOCHECK CONSTRAINT all

			            Print ''ScenarioCashFlowDistributionRule '';

			            Delete l4 
			            FROM Network AS l1
			            JOIN Simulation  AS l2 ON l2.NetworkId = l1.Id
			            JOIN ScenarioCashFlowRule AS l3 ON l3.SimulationId = l2.Id
			            JOIN ScenarioCashFlowDistributionRule AS l4 ON l4.ScenarioCashFlowRuleId = l3.Id
			            WHERE l1.Id IN (@NetworkId);

			            ALTER TABLE ScenarioCashFlowDistributionRule WITH CHECK CHECK CONSTRAINT all
 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in ScenarioCashFlowDistributionRule''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            ---------------------------------------------------------------------------
			            --Network --> Simulation --> ScenarioCashFlowRule 

                        BEGIN TRY

			            ALTER TABLE ScenarioCashFlowRule NOCHECK CONSTRAINT all

			            Print ''ScenarioCashFlowRule '';

			            Delete l3 
			            FROM Network AS l1
			            JOIN Simulation  AS l2 ON l2.NetworkId = l1.Id
			            JOIN ScenarioCashFlowRule AS l3 ON l3.SimulationId = l2.Id
			            WHERE l1.Id IN (@NetworkId);

			            ALTER TABLE ScenarioCashFlowRule WITH CHECK CHECK CONSTRAINT all
 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in ScenarioCashFlowRule''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            ------End ScenarioCashFlowRule-------------------------------------------------------------------
			            ------Start  ScenarioDeficientConditionGoal-------------------------------------------------------------------
			            --Network --> Simulation --> ScenarioDeficientConditionGoal --> CriterionLibrary_ScenarioDeficientConditionGoal --> 

                        BEGIN TRY

			            Print ''CriterionLibrary_ScenarioDeficientConditionGoal'';

			            ALTER TABLE CriterionLibrary_ScenarioDeficientConditionGoal NOCHECK CONSTRAINT all

			            Delete l4 
			            FROM Network AS l1
			            JOIN Simulation  AS l2 ON l2.NetworkId = l1.Id
			            JOIN ScenarioDeficientConditionGoal AS l3 ON l3.SimulationId = l2.Id
			            JOIN CriterionLibrary_ScenarioDeficientConditionGoal AS l4 ON l4.ScenarioDeficientConditionGoalId = l3.Id
			            WHERE l1.Id IN (@NetworkId);

			            ALTER TABLE CriterionLibrary_ScenarioDeficientConditionGoal WITH CHECK CHECK CONSTRAINT all
 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in CriterionLibrary_ScenarioDeficientConditionGoal''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            ------------------------------------------------------------

			            --Network --> Simulation --> ScenarioDeficientConditionGoal 

                        BEGIN TRY

			            ALTER TABLE ScenarioDeficientConditionGoal NOCHECK CONSTRAINT all

			            Print ''ScenarioDeficientConditionGoal '';

			            Delete l3 
			            FROM Network AS l1
			            JOIN Simulation  AS l2 ON l2.NetworkId = l1.Id
			            JOIN ScenarioDeficientConditionGoal AS l3 ON l3.SimulationId = l2.Id
			            WHERE l1.Id IN (@NetworkId);

			            ALTER TABLE ScenarioDeficientConditionGoal WITH CHECK CHECK CONSTRAINT all
 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in ScenarioDeficientConditionGoal''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            ------End ScenarioDeficientConditionGoal-------------------------------------------------------------------
			            ------Start  ScenarioPerformanceCurve-------------------------------------------------------------------
			            --Network --> Simulation --> ScenarioPerformanceCurve --> CriterionLibrary_ScenarioPerformanceCurve --> 
			            --Network --> Simulation --> ScenarioPerformanceCurve --> ScenarioPerformanceCurve_Equation --> 

                        BEGIN TRY

			            Print ''CriterionLibrary_ScenarioPerformanceCurve'';

			            ALTER TABLE CriterionLibrary_ScenarioPerformanceCurve NOCHECK CONSTRAINT all

			            Delete l4 
			            FROM Network AS l1
			            JOIN Simulation  AS l2 ON l2.NetworkId = l1.Id
			            JOIN ScenarioPerformanceCurve AS l3 ON l3.SimulationId = l2.Id
			            JOIN CriterionLibrary_ScenarioPerformanceCurve AS l4 ON l4.ScenarioPerformanceCurveId = l3.Id
			            WHERE l1.Id IN (@NetworkId);

			            ALTER TABLE CriterionLibrary_ScenarioPerformanceCurve WITH CHECK CHECK CONSTRAINT all
 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in CriterionLibrary_ScenarioPerformanceCurve''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            ------------------------------------------------------------
			            --Network --> Simulation --> ScenarioPerformanceCurve --> ScenarioPerformanceCurve_Equation --> 

                        BEGIN TRY

			            ALTER TABLE ScenarioPerformanceCurve_Equation NOCHECK CONSTRAINT all

			            Print ''ScenarioPerformanceCurve_Equation '';

			            Delete l4 
			            FROM Network AS l1
			            JOIN Simulation  AS l2 ON l2.NetworkId = l1.Id
			            JOIN ScenarioPerformanceCurve AS l3 ON l3.SimulationId = l2.Id
			            JOIN ScenarioPerformanceCurve_Equation AS l4 ON l4.ScenarioPerformanceCurveId = l3.Id
			            WHERE l1.Id IN (@NetworkId);

			            ALTER TABLE ScenarioPerformanceCurve_Equation WITH CHECK CHECK CONSTRAINT all
 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in ScenarioPerformanceCurve_Equation''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            ---------------------------------------------------------------------------

			            --Network --> Simulation --> ScenarioPerformanceCurve

                        BEGIN TRY

			            Print ''ScenarioPerformanceCurve'';

			            ALTER TABLE ScenarioPerformanceCurve NOCHECK CONSTRAINT all

			            Delete l3 
			            FROM Network AS l1
			            JOIN Simulation  AS l2 ON l2.NetworkId = l1.Id
			            JOIN ScenarioPerformanceCurve AS l3 ON l3.SimulationId = l2.Id
			            WHERE l1.Id IN (@NetworkId);

			            ALTER TABLE ScenarioPerformanceCurve WITH CHECK CHECK CONSTRAINT all
 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in ScenarioPerformanceCurve''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            ------End ScenarioPerformanceCurve-------------------------------------------------------------------
			            ------Start ScenarioRemainingLifeLimit-------------------------------------------------------------------

			            --Network --> Simulation --> ScenarioRemainingLifeLimit --> CriterionLibrary_ScenarioRemainingLifeLimit --> 

                        BEGIN TRY

			            ALTER TABLE CriterionLibrary_ScenarioRemainingLifeLimit NOCHECK CONSTRAINT all

			            Print ''CriterionLibrary_ScenarioRemainingLifeLimit'';

			            Delete l4 
			            FROM Network AS l1
			            JOIN Simulation  AS l2 ON l2.NetworkId = l1.Id
			            JOIN ScenarioRemainingLifeLimit AS l3 ON l3.SimulationId = l2.Id
			            JOIN CriterionLibrary_ScenarioRemainingLifeLimit AS l4 ON l4.ScenarioRemainingLifeLimitId = l3.Id
			            WHERE l1.Id IN (@NetworkId);

			            ALTER TABLE CriterionLibrary_ScenarioRemainingLifeLimit WITH CHECK CHECK CONSTRAINT all
 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in CriterionLibrary_ScenarioRemainingLifeLimit''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            ---------------------------------------------------------------------------

			            --Network --> Simulation --> ScenarioRemainingLifeLimit 

                        BEGIN TRY

			            ALTER TABLE ScenarioRemainingLifeLimit NOCHECK CONSTRAINT all

			            Print ''ScenarioRemainingLifeLimit'';

			            Delete l3 
			            FROM Network AS l1
			            JOIN Simulation  AS l2 ON l2.NetworkId = l1.Id
			            JOIN ScenarioRemainingLifeLimit AS l3 ON l3.SimulationId = l2.Id
			            WHERE l1.Id IN (@NetworkId);

			            ALTER TABLE ScenarioRemainingLifeLimit WITH CHECK CHECK CONSTRAINT all
 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in ScenarioRemainingLifeLimit''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            ------End ScenarioRemainingLifeLimit----------------------------------------------------------
			            ------Start ScenarioSelectableTreatment--------------------------------------------------------

 
              --Network --> Simulation --> ScenarioSelectableTreatment --> ScenarioTreatmentSupersedeRule --> CriterionLibrary_ScenarioTreatmentSupersedeRule --> 

			            BEGIN TRY

			            ALTER TABLE CriterionLibrary_ScenarioTreatmentSupersedeRule NOCHECK CONSTRAINT all

			            Print ''CriterionLibrary_ScenarioTreatmentSupersedeRule'';

			            Delete l5
			            FROM Network AS l1
			            JOIN Simulation  AS l2 ON l2.NetworkId = l1.Id
			            JOIN ScenarioSelectableTreatment AS l3 ON l3.SimulationId = l2.Id
			            JOIN ScenarioTreatmentSupersedeRule AS l4 ON l4.TreatmentId = l3.Id
			            JOIN CriterionLibrary_ScenarioTreatmentSupersedeRule AS l5  ON l5.ScenarioTreatmentSupersedeRuleId = l4.Id
			            WHERE l1.Id IN (@NetworkId);

			            ALTER TABLE CriterionLibrary_ScenarioTreatmentSupersedeRule WITH CHECK CHECK CONSTRAINT all
 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in CriterionLibrary_ScenarioTreatmentSupersedeRule''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            ---------------------------------------------------------------------------

               --Network --> Simulation --> ScenarioSelectableTreatment --> ScenarioTreatmentSupersedeRule  

                     BEGIN TRY

			            ALTER TABLE ScenarioTreatmentSupersedeRule NOCHECK CONSTRAINT all

			            Print ''ScenarioTreatmentSupersedeRule '';

			            Delete l4
			            FROM Network AS l1
			            JOIN Simulation  AS l2 ON l2.NetworkId = l1.Id
			            JOIN ScenarioSelectableTreatment AS l3 ON l3.SimulationId = l2.Id
			            JOIN ScenarioTreatmentSupersedeRule AS l4 ON l4.TreatmentId = l3.Id
			            WHERE l1.Id IN (@NetworkId);

			            ALTER TABLE ScenarioTreatmentSupersedeRule WITH CHECK CHECK CONSTRAINT all
 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in ScenarioTreatmentSupersedeRule''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            ---------------------------------------------------------------------------

		            --Simulation --> ScenarioSelectableTreatment --> ScenarioConditionalTreatmentConsequences   --> CriterionLibrary_ScenarioTreatmentConsequence

                        BEGIN TRY

			            Print ''CriterionLibrary_ScenarioTreatmentConsequence'';

			            ALTER TABLE CriterionLibrary_ScenarioTreatmentConsequence NOCHECK CONSTRAINT all

			            Delete l5 
			            FROM Network AS l1
			            JOIN Simulation  AS l2 ON l2.NetworkId = l1.Id
			            JOIN ScenarioSelectableTreatment AS l3 ON l3.SimulationId = l2.Id
			            JOIN ScenarioConditionalTreatmentConsequences AS l4 ON l4.ScenarioSelectableTreatmentId = l3.Id
			            JOIN  CriterionLibrary_ScenarioTreatmentConsequence AS l5 ON l5.ScenarioConditionalTreatmentConsequenceId = l4.Id
			            WHERE l1.Id IN (@NetworkId);

			            ALTER TABLE CriterionLibrary_ScenarioTreatmentConsequence WITH CHECK CHECK CONSTRAINT all
 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in CriterionLibrary_ScenarioTreatmentConsequence''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH
		
		            ---------------------------------------------------------------------------

		            --Simulation --> ScenarioSelectableTreatment --> ScenarioConditionalTreatmentConsequences   --> ScenarioTreatmentConsequence_Equation

                        BEGIN TRY

			            --ALTER TABLE ScenarioTreatmentConsequence_Equation NOCHECK CONSTRAINT all

			            Print ''ScenarioTreatmentConsequence_Equation'';

			            Delete l5 
			            FROM Network AS l1
			            JOIN Simulation  AS l2 ON l2.NetworkId = l1.Id
			            JOIN ScenarioSelectableTreatment AS l3 ON l3.SimulationId = l2.Id
			            JOIN ScenarioConditionalTreatmentConsequences AS l4 ON l4.ScenarioSelectableTreatmentId = l3.Id
			            JOIN  ScenarioTreatmentConsequence_Equation AS l5 ON l5.ScenarioConditionalTreatmentConsequenceId = l4.Id
			            WHERE l1.Id IN (@NetworkId);

			            --ALTER TABLE ScenarioTreatmentConsequence_Equation WITH CHECK CHECK CONSTRAINT all
 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in ScenarioTreatmentConsequence_Equation''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            ---------------------------------------------------------------------------
			
			            --Simulation --> ScenarioSelectableTreatment --> ScenarioConditionalTreatmentConsequences

                        BEGIN TRY

			            ALTER TABLE ScenarioConditionalTreatmentConsequences NOCHECK CONSTRAINT all

			            Print ''ScenarioConditionalTreatmentConsequences'';

			            Delete l4 
			            FROM Network AS l1
			            JOIN Simulation  AS l2 ON l2.NetworkId = l1.Id
			            JOIN ScenarioSelectableTreatment AS l3 ON l3.SimulationId = l2.Id
			            JOIN ScenarioConditionalTreatmentConsequences AS l4 ON l4.ScenarioSelectableTreatmentId = l3.Id
			            WHERE l1.Id IN (@NetworkId);

			            ALTER TABLE ScenarioConditionalTreatmentConsequences WITH CHECK CHECK CONSTRAINT all

 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in ScenarioConditionalTreatmentConsequences''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

	  	            --------------------------------------------------------------------------

			            --Simulation --> ScenarioSelectableTreatment --> ScenarioTreatmentCost --> ScenarioTreatmentCost_Equation

                        BEGIN TRY

			            --ALTER TABLE ScenarioTreatmentCost_Equation NOCHECK CONSTRAINT all

			            Print ''ScenarioTreatmentCost_Equation'';

			            Delete l5 
			            FROM Network AS l1
			            JOIN Simulation  AS l2 ON l2.NetworkId = l1.Id
			            JOIN ScenarioSelectableTreatment AS l3 ON l3.SimulationId = l2.Id
			            JOIN ScenarioTreatmentCost AS l4 ON l4.ScenarioSelectableTreatmentId = l3.Id
			            JOIN ScenarioTreatmentCost_Equation AS l5 ON l5.ScenarioTreatmentCostId = l4.Id
			            WHERE l1.Id IN (@NetworkId);

			            --ALTER TABLE ScenarioTreatmentCost_Equation WITH CHECK CHECK CONSTRAINT all
 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in ScenarioTreatmentCost_Equation''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH


		            ---------------------------------------------------------------------------

		            --Simulation --> ScenarioSelectableTreatment --> ScenarioTreatmentCost --> CriterionLibrary_ScenarioTreatmentCost

                        BEGIN TRY

			            Print ''CriterionLibrary_ScenarioTreatmentCost '';

			            ALTER TABLE CriterionLibrary_ScenarioTreatmentCost NOCHECK CONSTRAINT all

			            Delete l5 
			            FROM Network AS l1
			            JOIN Simulation  AS l2 ON l2.NetworkId = l1.Id
			            JOIN ScenarioSelectableTreatment AS l3 ON l3.SimulationId = l2.Id
			            JOIN ScenarioTreatmentCost AS l4 ON l4.ScenarioSelectableTreatmentId = l3.Id
			            JOIN CriterionLibrary_ScenarioTreatmentCost AS l5 ON l5.ScenarioTreatmentCostId = l4.Id
			            WHERE l1.Id IN (@NetworkId);

			            ALTER TABLE CriterionLibrary_ScenarioTreatmentCost WITH CHECK CHECK CONSTRAINT all
 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in CriterionLibrary_ScenarioTreatmentCost''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            ---------------------------------------------------------------------------
		            --simulation --> ScenarioSelectableTreatment --> ScenarioTreatmentCost

                        BEGIN TRY

			            ALTER TABLE ScenarioTreatmentCost NOCHECK CONSTRAINT all

			            Print ''ScenarioTreatmentCost '';

			            Delete l4 
			            FROM Network AS l1
			            JOIN Simulation  AS l2 ON l2.NetworkId = l1.Id
			            JOIN ScenarioSelectableTreatment AS l3 ON l3.SimulationId = l2.Id
			            JOIN ScenarioTreatmentCost AS l4 ON l4.ScenarioSelectableTreatmentId = l3.Id
			            WHERE l1.Id IN (@NetworkId);

			            ALTER TABLE ScenarioTreatmentCost WITH CHECK CHECK CONSTRAINT all
 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in ScenarioTreatmentCost''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH


		            ---------------------------------------------------------------------------

		            --Simulation --> ScenarioSelectableTreatment --> ScenarioSelectableTreatment_ScenarioBudget --> 

                        BEGIN TRY

			            ALTER TABLE ScenarioSelectableTreatment_ScenarioBudget NOCHECK CONSTRAINT all

			            Print ''ScenarioSelectableTreatment_ScenarioBudget '';

			            Delete l4 
			            FROM Network AS l1
			            JOIN Simulation  AS l2 ON l2.NetworkId = l1.Id
			            JOIN ScenarioSelectableTreatment AS l3 ON l3.SimulationId = l2.Id
			            JOIN ScenarioSelectableTreatment_ScenarioBudget AS l4 ON l4.ScenarioSelectableTreatmentId = l3.Id
			            WHERE l1.Id IN (@NetworkId);

			            ALTER TABLE ScenarioSelectableTreatment_ScenarioBudget WITH CHECK CHECK CONSTRAINT all
 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in ScenarioSelectableTreatment_ScenarioBudget''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            ---------------------------------------------------------------------------

			            --Simulation --> ScenarioSelectableTreatment --> ScenarioTreatmentPerformanceFactor --> 

                        BEGIN TRY

			            Print ''ScenarioTreatmentPerformanceFactor'';

			            ALTER TABLE ScenarioTreatmentPerformanceFactor NOCHECK CONSTRAINT all

			            Delete l4 
			            FROM Network AS l1
			            JOIN Simulation  AS l2 ON l2.NetworkId = l1.Id
			            JOIN ScenarioSelectableTreatment AS l3 ON l3.SimulationId = l2.Id
			            JOIN ScenarioTreatmentPerformanceFactor AS l4 ON l4.ScenarioSelectableTreatmentId = l3.Id
			            WHERE l1.Id IN (@NetworkId);

			            ALTER TABLE ScenarioTreatmentPerformanceFactor WITH CHECK CHECK CONSTRAINT all
 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in ScenarioTreatmentPerformanceFactor''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            ---------------------------------------------------------------------------
			            --Simulation --> ScenarioSelectableTreatment --> CriterionLibrary_ScenarioTreatment --> 

                        BEGIN TRY

			            ALTER TABLE CriterionLibrary_ScenarioTreatment NOCHECK CONSTRAINT all

			            Print ''CriterionLibrary_ScenarioTreatment '';

			            Delete l4 
			            FROM Network AS l1
			            JOIN Simulation  AS l2 ON l2.NetworkId = l1.Id
			            JOIN ScenarioSelectableTreatment AS l3 ON l3.SimulationId = l2.Id
			            JOIN CriterionLibrary_ScenarioTreatment AS l4 ON l4.ScenarioSelectableTreatmentId = l3.Id
			            WHERE l1.Id IN (@NetworkId);

			            ALTER TABLE CriterionLibrary_ScenarioTreatment WITH CHECK CHECK CONSTRAINT all
 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in CriterionLibrary_ScenarioTreatment''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            ---------------------------------------------------------------------------

			            --Simulation --> ScenarioSelectableTreatment --> ScenarioTreatmentScheduling -->

                        BEGIN TRY

			            ALTER TABLE ScenarioTreatmentScheduling NOCHECK CONSTRAINT all

			            Print ''ScenarioTreatmentScheduling '';

			            Delete l4 
			            FROM Network AS l1
			            JOIN Simulation  AS l2 ON l2.NetworkId = l1.Id
			            JOIN ScenarioSelectableTreatment AS l3 ON l3.SimulationId = l2.Id
			            JOIN ScenarioTreatmentScheduling AS l4 ON l4.TreatmentId = l3.Id
			            WHERE l1.Id IN (@NetworkId);

			            ALTER TABLE ScenarioTreatmentScheduling WITH CHECK CHECK CONSTRAINT all
 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in ScenarioTreatmentScheduling''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            ---------------------------------------------------------------------------

			            --Simulation --> ScenarioSelectableTreatment 15

                        BEGIN TRY

			            ALTER TABLE ScenarioSelectableTreatment NOCHECK CONSTRAINT all

			            Print ''ScenarioSelectableTreatment '';

			            Delete l3 
			            FROM Network AS l1
			            JOIN Simulation  AS l2 ON l2.NetworkId = l1.Id
			            JOIN ScenarioSelectableTreatment AS l3 ON l3.SimulationId = l2.Id
			            WHERE l1.Id IN (@NetworkId);

			            ALTER TABLE ScenarioSelectableTreatment WITH CHECK CHECK CONSTRAINT all
 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in ScenarioSelectableTreatment''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            ---End ScenarioSelectableTreatment --------------------------------------------
			            ---------------------------------------------------------------------------
			            ---Start ScenarioTargetConditionGoals --------------------------------------------

			            --Simulation --> ScenarioTargetConditionGoals --> CriterionLibrary_ScenarioTargetConditionGoal --> 

                        BEGIN TRY

			            Print ''CriterionLibrary_ScenarioTargetConditionGoal'';

			            ALTER TABLE CriterionLibrary_ScenarioTargetConditionGoal NOCHECK CONSTRAINT all

			            Delete l4 
			            FROM Network AS l1
			            JOIN Simulation  AS l2 ON l2.NetworkId = l1.Id
			            JOIN ScenarioTargetConditionGoals AS l3 ON l3.SimulationId = l2.Id
			            JOIN CriterionLibrary_ScenarioTargetConditionGoal AS l4 ON l4.ScenarioTargetConditionGoalId = l3.Id
			            WHERE l1.Id IN (@NetworkId);

			            ALTER TABLE CriterionLibrary_ScenarioTargetConditionGoal WITH CHECK CHECK CONSTRAINT all

 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in CriterionLibrary_ScenarioTargetConditionGoal''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            ---------------------------------------------------------------------------

		            --Simulation --> ScenarioTargetConditionGoals 

                        BEGIN TRY

			            ALTER TABLE ScenarioTargetConditionGoals NOCHECK CONSTRAINT all

			            Print ''ScenarioTargetConditionGoals '';

			            Delete l3 
			            FROM Network AS l1
			            JOIN Simulation  AS l2 ON l2.NetworkId = l1.Id
			            JOIN ScenarioTargetConditionGoals AS l3 ON l3.SimulationId = l2.Id
			            WHERE l1.Id IN (@NetworkId);

			            ALTER TABLE ScenarioTargetConditionGoals WITH CHECK CHECK CONSTRAINT all
 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in ScenarioTargetConditionGoals''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            ---End ScenarioTargetConditionGoals --------------------------------------------
			            ---------------------------------------------------------------------------
		            --Simulation --> Simulation_User 

                        BEGIN TRY

			            ALTER TABLE Simulation_User NOCHECK CONSTRAINT all

			            Print ''Simulation_User '';

			            Delete l3 
			            FROM Network AS l1
			            JOIN Simulation  AS l2 ON l2.NetworkId = l1.Id
			            JOIN Simulation_User AS l3 ON l3.SimulationId = l2.Id
			            WHERE l1.Id IN (@NetworkId);

			            ALTER TABLE Simulation_User WITH CHECK CHECK CONSTRAINT all

 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in Simulation_User''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            ---------------------------------------------------------------------------

		            --Simulation --> SimulationAnalysisDetail 

                        BEGIN TRY

			            Print ''SimulationAnalysisDetail'';

			            ALTER TABLE SimulationAnalysisDetail NOCHECK CONSTRAINT all

			            Delete l3 
			            FROM Network AS l1
			            JOIN Simulation  AS l2 ON l2.NetworkId = l1.Id
			            JOIN SimulationAnalysisDetail AS l3 ON l3.SimulationId = l2.Id
			            WHERE l1.Id IN (@NetworkId);

			            ALTER TABLE SimulationAnalysisDetail WITH CHECK CHECK CONSTRAINT all

 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in SimulationAnalysisDetail''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            ---------------------------------------------------------------------------

                        BEGIN TRY

			            Print ''SimulationLog'';

			            ALTER TABLE SimulationLog NOCHECK CONSTRAINT all

			            Delete l3 
			            FROM Network AS l1
			            JOIN Simulation  AS l2 ON l2.NetworkId = l1.Id
			            JOIN SimulationLog AS l3 ON l3.SimulationId = l2.Id
			            WHERE l1.Id IN (@NetworkId);

			            ALTER TABLE SimulationLog WITH CHECK CHECK CONSTRAINT all

 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in SimulationLog''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            -------End SimulationLog-----------------------------------------
			
			            
			            ---------------------------------------------------------------------------

			            --SimulationReportDetail

                        BEGIN TRY

			            Print ''SimulationReportDetail'';

			            ALTER TABLE SimulationReportDetail NOCHECK CONSTRAINT all

			            Delete l3 
			            FROM Network AS l1
			            JOIN Simulation  AS l2 ON l2.NetworkId = l1.Id
			            JOIN SimulationReportDetail AS l3 ON l3.SimulationId = l2.Id
			            WHERE l1.Id IN (@NetworkId);

			            ALTER TABLE SimulationReportDetail WITH CHECK CHECK CONSTRAINT all

 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in SimulationReportDetail''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

			            ---------------------------------------------------------------------------
			            --Simulation

                        BEGIN TRY

			            Print ''Simulation'';

			            ALTER TABLE Simulation NOCHECK CONSTRAINT all

			            Delete l2 
			            FROM Network AS l1
			            JOIN Simulation  AS l2 ON l2.NetworkId = l1.Id
			            WHERE l1.Id IN (@NetworkId);

			            SET @RowsDeleted = @@ROWCOUNT;
			            --Print ''Rows Affected  Network --> Simulation: '' +  convert(NVARCHAR(50), @RowsDeleted);

			            ALTER TABLE Simulation WITH CHECK CHECK CONSTRAINT all;
 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in Simulation''
                             RAISERROR  (@CustomErrorMessage, 16, 1);  
                             Set @RetMessage = @CustomErrorMessage;

                        END CATCH
		
		             --   SET @CurrentDateTime = GETDATE();
			            --PRINT ''End Simulation Delete: '' + CONVERT(NVARCHAR, @CurrentDateTime, 120);
			            ------End Simulation Delete-------------------------------------------------------------------
			            -----End  ----Network --> Simulation Path---------

			            -- Start Network

                        BEGIN TRY

			            Print ''Network'';

                        ALTER TABLE Network NOCHECK CONSTRAINT all

			            Delete l1 
			            FROM  Network AS l1
			            WHERE l1.Id IN (@NetworkId);

			            SET @RowsDeleted = @@ROWCOUNT;
			            --Print ''Rows Affected  Network: '' +  convert(NVARCHAR(50), @RowsDeleted);

			            ALTER TABLE Network WITH CHECK CHECK CONSTRAINT all
 
                        END TRY 
			            BEGIN CATCH
                             SELECT ERROR_NUMBER() AS ErrorNumber
                                   ,ERROR_SEVERITY() AS ErrorSeverity
                                   ,ERROR_STATE() AS ErrorState
                                   ,ERROR_PROCEDURE() AS ErrorProcedure
                                   ,ERROR_LINE() AS ErrorLine
                                   ,ERROR_MESSAGE() AS ErrorMessage;

 		                     SELECT @CustomErrorMessage = ''Query Error in Network''
		                     RAISERROR (@CustomErrorMessage, 16, 1);
				             Set @RetMessage = @CustomErrorMessage;

                        END CATCH

		               ---------------------------------------------------------------------------
			            ------End Network Delete-------------------------------------------------------------------
                Print ''Delete Network Committed End'';
 	            RAISERROR (@RetMessage, 0, 1);
	            END TRY
	            BEGIN CATCH
  			            Set @RetMessage = ''Failed '' + @RetMessage;
			            Set @ErrorMessage =  ERROR_PROCEDURE() + '' (Error At Line: '' + cast( ERROR_LINE() as Varchar(5)) + '' ): '' + char(13) + char(10)  + ERROR_MESSAGE()  -- AS ErrorMessage;
			            Print ''Overall Catch in Network SP:  '' + @ErrorMessage;

			            RAISERROR  (@RetMessage, 16, 1);  
	            END CATCH;

            END')";

            migrationBuilder.Sql(createAlterProcSql2);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
