using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppliedResearchAssociates.iAM.DataPersistenceCore.Migrations
{
    public partial class Modify_usp_delete_simulationoutput : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var createProcSql = @"EXEC('CREATE OR ALTER PROCEDURE dbo.usp_delete_simulationoutput(@SimOutputGuidList NVARCHAR(MAX)=NULL,@RetMessage VARCHAR(250) OUTPUT)
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
    	            DECLARE @BatchSize INT = 200000;  -- Adjust batch size as needed
    	            DECLARE @RowsDeleted INT = 1;

                    CREATE TABLE #SimOutputTempGuids
                    (
                        --Guid UNIQUEIDENTIFIER
    		             Guid NVARCHAR(36)
                    );


    	            IF @SimOutputGuidList IS NULL OR LEN(@SimOutputGuidList) = 0
    	            BEGIN
    		              PRINT ''String is NULL or empty'';
    		              Set  @SimOutputGuidList = ''00000000-0000-0000-0000-000000000000'';
    	            END

                    INSERT INTO #SimOutputTempGuids (Guid)
    	            SELECT LEFT(LTRIM(RTRIM(value)), 36)
                    FROM STRING_SPLIT(@SimOutputGuidList, '','');

    	            UPDATE #SimOutputTempGuids
    	            SET Guid = ''00000000-0000-0000-0000-000000000000''
    	            WHERE TRY_CAST(Guid AS UNIQUEIDENTIFIER) IS NULL OR Guid = '''';
    	
    	            Begin Transaction
    	            BEGIN TRY

    			            -----Start AssetSummaryDetail Path-----------------------------------------

    			            -------SimulationOutput --> AssetSummaryDetail --> AssetSummaryDetailValueIntI----

                            BEGIN TRY

                          ALTER TABLE AssetSummaryDetailValueIntId NOCHECK CONSTRAINT all
    		  
    		              SET @RowsDeleted = 1;
    		              --Print ''AssetSummaryDetailValueIntId '';

    		               WHILE @RowsDeleted > 0
    				            BEGIN
    					            BEGIN TRY
    						            Begin Transaction

    							            Delete TOP (@BatchSize) l3
    							            FROM SimulationOutput AS l1
    							            JOIN AssetSummaryDetail AS l2 ON l2.SimulationOutputId = l1.Id
    							            Join AssetSummaryDetailValueIntId As l3 ON l3.AssetSummaryDetailId = l2.Id
    							            WHERE l1.Id IN (SELECT Guid FROM #SimOutputTempGuids);

    							            SET @RowsDeleted = @@ROWCOUNT;
    						            COMMIT TRANSACTION
    						            Print ''Rows Affected AssetSummaryDetailValueIntId: '' +  convert(NVARCHAR(50), @RowsDeleted);
    					            END TRY
    					            BEGIN CATCH
    							            ALTER TABLE AssetSummaryDetailValueIntId WITH CHECK CHECK CONSTRAINT all
      							            Set @RetMessage = ''Failed'';
    							            Set @ErrorMessage =  ERROR_PROCEDURE() + '' (Error At Line: '' + cast( ERROR_LINE() as Varchar(5)) + '' ): '' + char(13) + char(10)  + ERROR_MESSAGE()  -- AS ErrorMessage;
    							            Print ''Rolled Back AssetSummaryDetailValueIntId Delete Transaction in SimulationOutput SP:  '' + @ErrorMessage;
    							            ROLLBACK TRANSACTION;
    							            RAISERROR  (@RetMessage, 16, 1); 
    							            Return -1;
    					            END CATCH;
    				            END

                            ALTER TABLE AssetSummaryDetailValueIntId WITH CHECK CHECK CONSTRAINT all

                            END TRY 
    			            BEGIN CATCH
                                 SELECT ERROR_NUMBER() AS ErrorNumber
                                       ,ERROR_SEVERITY() AS ErrorSeverity
                                       ,ERROR_STATE() AS ErrorState
                                       ,ERROR_PROCEDURE() AS ErrorProcedure
                                       ,ERROR_LINE() AS ErrorLine
                                       ,ERROR_MESSAGE() AS ErrorMessage;

     		                     SELECT @CustomErrorMessage = ''Query Error in AssetSummaryDetailValueIntId''
    		                     RAISERROR (@CustomErrorMessage, 16, 1);
    				             Set @RetMessage = @CustomErrorMessage;

                            END CATCH

    			            -----------------------------------------------------------------------

        		            -------SimulationOutput --> AssetSummaryDetail -----

                            BEGIN TRY

                          ALTER TABLE AssetSummaryDetail NOCHECK CONSTRAINT all

    			            Delete l2 
    			            FROM SimulationOutput AS l1
    			            JOIN AssetSummaryDetail AS l2 ON l2.SimulationOutputId = l1.Id
    			            WHERE l1.Id IN (SELECT Guid FROM #SimOutputTempGuids);

                            ALTER TABLE AssetSummaryDetail WITH CHECK CHECK CONSTRAINT all

                            END TRY 
    			            BEGIN CATCH
                                 SELECT ERROR_NUMBER() AS ErrorNumber
                                       ,ERROR_SEVERITY() AS ErrorSeverity
                                       ,ERROR_STATE() AS ErrorState
                                       ,ERROR_PROCEDURE() AS ErrorProcedure
                                       ,ERROR_LINE() AS ErrorLine
                                       ,ERROR_MESSAGE() AS ErrorMessage;

     		                     SELECT @CustomErrorMessage = ''Query Error in AssetSummaryDetail''
    		                     RAISERROR (@CustomErrorMessage, 16, 1);
    				             Set @RetMessage = @CustomErrorMessage;

                            END CATCH



    			            ----------------------------------------------------------------------
    			            -----End AssetSummaryDetail Path--------------------------------------

    			            -----Start SimulationYearDetail Path-----------------------------------------

    			            ----SimulationOutput --> SimulationYearDetail --> AssetDetail --> TreatmentConsiderationDetail --> BudgetUsageDetail -- REMOVED PER SCHEMA UPDATES
    	
    	            ------------------------------------------------------------------

    			            --SimulationOutput --> SimulationYearDetail --> AssetDetail --> TreatmentConsiderationDetail --> CashFlowConsiderationDetail

    			            BEGIN TRY

                            ALTER TABLE CashFlowConsiderationDetail NOCHECK CONSTRAINT all

    			            Delete l5 
    			            FROM SimulationOutput AS l1
    			            JOIN SimulationYearDetail AS l2 ON l2.SimulationOutputId = l1.Id
    			            JOIN AssetDetail AS l3 ON l3.SimulationYearDetailId = l2.Id
    			            JOIN TreatmentConsiderationDetail AS l4 ON l4.AssetDetailId = l3.Id
    			            JOIN CashFlowConsiderationDetail AS l5 ON l5.TreatmentConsiderationDetailId = l4.Id
    			            WHERE l1.Id IN (SELECT Guid FROM #SimOutputTempGuids);

                            ALTER TABLE CashFlowConsiderationDetail WITH CHECK CHECK CONSTRAINT all

                            END TRY 
    			            BEGIN CATCH
                                 SELECT ERROR_NUMBER() AS ErrorNumber
                                       ,ERROR_SEVERITY() AS ErrorSeverity
                                       ,ERROR_STATE() AS ErrorState
                                       ,ERROR_PROCEDURE() AS ErrorProcedure
                                       ,ERROR_LINE() AS ErrorLine
                                       ,ERROR_MESSAGE() AS ErrorMessage;

     		                     SELECT @CustomErrorMessage = ''Query Error in CashFlowConsiderationDetail''
    		                     RAISERROR (@CustomErrorMessage, 16, 1);
    				             Set @RetMessage = @CustomErrorMessage;

                            END CATCH

					------------------------------------------------------------------

    						--SimulationOutput --> SimulationYearDetail --> AssetDetail --> TreatmentConsiderationDetail --> FundingCalculationOutput --> Allocation  
						
						BEGIN TRY

							ALTER TABLE Allocation NOCHECK CONSTRAINT all

    						SET @RowsDeleted = 1;

							WHILE @RowsDeleted > 0
    							BEGIN
    								BEGIN TRY
    									Begin Transaction

    									Delete TOP (@BatchSize) l6
    									FROM SimulationOutput AS l1
    									JOIN SimulationYearDetail AS l2 ON l2.SimulationOutputId = l1.Id
    									JOIN AssetDetail AS l3 ON l3.SimulationYearDetailId = l2.Id
    									JOIN TreatmentConsiderationDetail AS l4 ON l4.AssetDetailId = l3.Id    										
										JOIN FundingCalculationOutput AS l5 ON l5.TreatmentConsiderationDetailId = l4.Id
										JOIN Allocation AS l6 ON l6.FundingCalculationOutputId = l5.Id
    									WHERE l1.Id IN (SELECT Guid FROM #SimOutputTempGuids);

     									SET @RowsDeleted = @@ROWCOUNT;
    									COMMIT TRANSACTION
    									Print ''Rows Affected Allocation: '' +  convert(NVARCHAR(50), @RowsDeleted);
    								END TRY
    								BEGIN CATCH
    										ALTER TABLE Allocation WITH CHECK CHECK CONSTRAINT all
      										Set @RetMessage = ''Failed'';
    										Set @ErrorMessage =  ERROR_PROCEDURE() + '' (Error At Line: '' + cast( ERROR_LINE() as Varchar(5)) + '' ): '' + char(13) + char(10)  + ERROR_MESSAGE()  -- AS ErrorMessage;
    										Print ''Rolled Back Allocation Delete Transaction in SimulationOutput SP:  '' + @ErrorMessage;
    										ROLLBACK TRANSACTION;
    										RAISERROR  (@RetMessage, 16, 1); 
    										Return -1;
    								END CATCH;
    							END

							ALTER TABLE Allocation WITH CHECK CHECK CONSTRAINT all

                        END TRY 
    			        BEGIN CATCH

                                SELECT ERROR_NUMBER() AS ErrorNumber
                                    ,ERROR_SEVERITY() AS ErrorSeverity
                                    ,ERROR_STATE() AS ErrorState
                                    ,ERROR_PROCEDURE() AS ErrorProcedure
                                    ,ERROR_LINE() AS ErrorLine
                                    ,ERROR_MESSAGE() AS ErrorMessage;

     		                    SELECT @CustomErrorMessage = ''Query Error in Allocation''
								Set @RetMessage = @CustomErrorMessage;
    		                    RAISERROR (@CustomErrorMessage, 16, 1);    				             

                        END CATCH
						
					------------------------------------------------------------------

    					--SimulationOutput --> SimulationYearDetail --> AssetDetail --> TreatmentConsiderationDetail --> FundingCalculationInput --> BudgetToSpend
							
						BEGIN TRY

							ALTER TABLE BudgetToSpend NOCHECK CONSTRAINT all

    						SET @RowsDeleted = 1;

							WHILE @RowsDeleted > 0
    							BEGIN
    								BEGIN TRY
    									Begin Transaction

    									Delete TOP (@BatchSize) l6
    									FROM SimulationOutput AS l1
    									JOIN SimulationYearDetail AS l2 ON l2.SimulationOutputId = l1.Id
    									JOIN AssetDetail AS l3 ON l3.SimulationYearDetailId = l2.Id
    									JOIN TreatmentConsiderationDetail AS l4 ON l4.AssetDetailId = l3.Id
    									JOIN FundingCalculationInput AS l5 ON l5.TreatmentConsiderationDetailId = l4.Id
										JOIN BudgetToSpend AS l6 ON l6.FundingCalculationInputId = l5.Id
    									WHERE l1.Id IN (SELECT Guid FROM #SimOutputTempGuids);

     									SET @RowsDeleted = @@ROWCOUNT;
    									COMMIT TRANSACTION
    									Print ''Rows Affected BudgetToSpend: '' +  convert(NVARCHAR(50), @RowsDeleted);
    								END TRY
    								BEGIN CATCH
    										ALTER TABLE BudgetToSpend WITH CHECK CHECK CONSTRAINT all
      										Set @RetMessage = ''Failed'';
    										Set @ErrorMessage =  ERROR_PROCEDURE() + '' (Error At Line: '' + cast( ERROR_LINE() as Varchar(5)) + '' ): '' + char(13) + char(10)  + ERROR_MESSAGE()  -- AS ErrorMessage;
    										Print ''Rolled Back BudgetToSpend Delete Transaction in SimulationOutput SP:  '' + @ErrorMessage;
    										ROLLBACK TRANSACTION;
    										RAISERROR  (@RetMessage, 16, 1); 
    										Return -1;
    								END CATCH;
    							END

							ALTER TABLE BudgetToSpend WITH CHECK CHECK CONSTRAINT all

                        END TRY
    			        BEGIN CATCH

                                SELECT ERROR_NUMBER() AS ErrorNumber
                                    ,ERROR_SEVERITY() AS ErrorSeverity
                                    ,ERROR_STATE() AS ErrorState
                                    ,ERROR_PROCEDURE() AS ErrorProcedure
                                    ,ERROR_LINE() AS ErrorLine
                                    ,ERROR_MESSAGE() AS ErrorMessage;

     		                    SELECT @CustomErrorMessage = ''Query Error in BudgetToSpend''
								Set @RetMessage = @CustomErrorMessage;
    		                    RAISERROR (@CustomErrorMessage, 16, 1);    				             

                        END CATCH
    						
					------------------------------------------------------------------
					--SimulationOutput --> SimulationYearDetail --> AssetDetail --> TreatmentConsiderationDetail --> FundingCalculationOutput

    					BEGIN TRY

                        ALTER TABLE FundingCalculationOutput NOCHECK CONSTRAINT all

    			        Delete l5 
    			        FROM SimulationOutput AS l1
    			        JOIN SimulationYearDetail AS l2 ON l2.SimulationOutputId = l1.Id
    			        JOIN AssetDetail AS l3 ON l3.SimulationYearDetailId = l2.Id
    			        JOIN TreatmentConsiderationDetail AS l4 ON l4.AssetDetailId = l3.Id
    			        JOIN FundingCalculationOutput AS l5 ON l5.TreatmentConsiderationDetailId = l4.Id
    			        WHERE l1.Id IN (SELECT Guid FROM #SimOutputTempGuids);

                        ALTER TABLE FundingCalculationOutput WITH CHECK CHECK CONSTRAINT all

                        END TRY 
    			        BEGIN CATCH

                                SELECT ERROR_NUMBER() AS ErrorNumber
                                    ,ERROR_SEVERITY() AS ErrorSeverity
                                    ,ERROR_STATE() AS ErrorState
                                    ,ERROR_PROCEDURE() AS ErrorProcedure
                                    ,ERROR_LINE() AS ErrorLine
                                    ,ERROR_MESSAGE() AS ErrorMessage;

     		                    SELECT @CustomErrorMessage = ''Query Error in FundingCalculationOutput''
								Set @RetMessage = @CustomErrorMessage;
    		                    RAISERROR (@CustomErrorMessage, 16, 1);    				             

                        END CATCH

    	            ------------------------------------------------------------------

    					--SimulationOutput --> SimulationYearDetail --> AssetDetail --> TreatmentConsiderationDetail --> FundingCalculationInput
    						
						BEGIN TRY

						ALTER TABLE FundingCalculationInput NOCHECK CONSTRAINT all

    					Delete l5 
    					FROM SimulationOutput AS l1
    					JOIN SimulationYearDetail AS l2 ON l2.SimulationOutputId = l1.Id
    					JOIN AssetDetail AS l3 ON l3.SimulationYearDetailId = l2.Id
    					JOIN TreatmentConsiderationDetail AS l4 ON l4.AssetDetailId = l3.Id
    					JOIN FundingCalculationInput AS l5 ON l5.TreatmentConsiderationDetailId = l4.Id
    					WHERE l1.Id IN (SELECT Guid FROM #SimOutputTempGuids);

						ALTER TABLE FundingCalculationInput WITH CHECK CHECK CONSTRAINT all

						END TRY 
    					BEGIN CATCH

								SELECT ERROR_NUMBER() AS ErrorNumber
									,ERROR_SEVERITY() AS ErrorSeverity
									,ERROR_STATE() AS ErrorState
									,ERROR_PROCEDURE() AS ErrorProcedure
									,ERROR_LINE() AS ErrorLine
									,ERROR_MESSAGE() AS ErrorMessage;

     							SELECT @CustomErrorMessage = ''Query Error in FundingCalculationInput''
								Set @RetMessage = @CustomErrorMessage;
    							RAISERROR (@CustomErrorMessage, 16, 1);    				             

						END CATCH

					------------------------------------------------------------------    						    					
    	
    		            --SimulationOutput --> SimulationYearDetail --> AssetDetail --> TreatmentConsiderationDetail 

    			            BEGIN TRY

                            ALTER TABLE TreatmentConsiderationDetail NOCHECK CONSTRAINT all

    			            Delete l4 
    			            FROM SimulationOutput AS l1
    			            JOIN SimulationYearDetail AS l2 ON l2.SimulationOutputId = l1.Id
    			            JOIN AssetDetail AS l3 ON l3.SimulationYearDetailId = l2.Id
    			            JOIN TreatmentConsiderationDetail AS l4 ON l4.AssetDetailId = l3.Id
    			            WHERE l1.Id IN (SELECT Guid FROM #SimOutputTempGuids);

                            ALTER TABLE TreatmentConsiderationDetail WITH CHECK CHECK CONSTRAINT all

                            END TRY 
    			            BEGIN CATCH
                                 SELECT ERROR_NUMBER() AS ErrorNumber
                                       ,ERROR_SEVERITY() AS ErrorSeverity
                                       ,ERROR_STATE() AS ErrorState
                                       ,ERROR_PROCEDURE() AS ErrorProcedure
                                       ,ERROR_LINE() AS ErrorLine
                                       ,ERROR_MESSAGE() AS ErrorMessage;

     		                     SELECT @CustomErrorMessage = ''Query Error in TreatmentConsiderationDetail''
                                 RAISERROR (@CustomErrorMessage, 16, 1);
								 Set @RetMessage = @CustomErrorMessage;    		                         				             

                            END CATCH
    	
    	            ------------------------------------------------------------------

                --SimulationOutput --> SimulationYearDetail --> AssetDetail --> TreatmentOptionDetail -->  -->  -->  -->  -->  --> 

    			            BEGIN TRY

                            ALTER TABLE TreatmentOptionDetail NOCHECK CONSTRAINT all

    			            Delete l4 
    			            FROM SimulationOutput AS l1
    			            JOIN SimulationYearDetail AS l2 ON l2.SimulationOutputId = l1.Id
    			            JOIN AssetDetail AS l3 ON l3.SimulationYearDetailId = l2.Id
    			            JOIN TreatmentOptionDetail AS l4 ON l4.AssetDetailId = l3.Id
    			            WHERE l1.Id IN (SELECT Guid FROM #SimOutputTempGuids);

                            ALTER TABLE TreatmentOptionDetail WITH CHECK CHECK CONSTRAINT all

                            END TRY 
    			            BEGIN CATCH
                                 SELECT ERROR_NUMBER() AS ErrorNumber
                                       ,ERROR_SEVERITY() AS ErrorSeverity
                                       ,ERROR_STATE() AS ErrorState
                                       ,ERROR_PROCEDURE() AS ErrorProcedure
                                       ,ERROR_LINE() AS ErrorLine
                                       ,ERROR_MESSAGE() AS ErrorMessage;

     		                     SELECT @CustomErrorMessage = ''Query Error in TreatmentOptionDetail''
    		                     RAISERROR (@CustomErrorMessage, 16, 1);
    				             Set @RetMessage = @CustomErrorMessage;

                            END CATCH
    	
    	            -----------------------------------------------------------------

    	            --SimulationOutput --> SimulationYearDetail --> AssetDetail --> AssetDetailValueIntId -->  -->  -->  -->  -->  --> 
    	
    			            BEGIN TRY

                            ALTER TABLE AssetDetailValueIntId NOCHECK CONSTRAINT all

    			            SET @RowsDeleted = 1;
    		  	            --Print ''AssetDetailValueIntId '';

    		               WHILE @RowsDeleted > 0
    				            BEGIN
    					            BEGIN TRY
    						            Begin Transaction
    						            Delete TOP (@BatchSize) l4 
    						            FROM SimulationOutput AS l1
    						            JOIN SimulationYearDetail AS l2 ON l2.SimulationOutputId = l1.Id
    						            JOIN AssetDetail AS l3 ON l3.SimulationYearDetailId = l2.Id
    						            JOIN AssetDetailValueIntId AS l4 ON l4.AssetDetailId = l3.Id
    						            WHERE l1.Id IN (SELECT Guid FROM #SimOutputTempGuids);

    						            SET @RowsDeleted = @@ROWCOUNT;
    						            COMMIT TRANSACTION
    						            Print ''Rows Affected AssetDetailValueIntId: '' +  convert(NVARCHAR(50), @RowsDeleted);
    					            END TRY
    					            BEGIN CATCH
    							            ALTER TABLE AssetDetailValueIntId WITH CHECK CHECK CONSTRAINT all
      							            Set @RetMessage = ''Failed'';
    							            Set @ErrorMessage =  ERROR_PROCEDURE() + '' (Error At Line: '' + cast( ERROR_LINE() as Varchar(5)) + '' ): '' + char(13) + char(10)  + ERROR_MESSAGE()  -- AS ErrorMessage;
    							            Print ''Rolled Back AssetDetailValueIntId Delete Transaction in SimulationOutput SP  '' + @ErrorMessage;
    							            ROLLBACK TRANSACTION;
    							            RAISERROR  (@RetMessage, 16, 1); 
    							            Return -1;
    					            END CATCH;
    				            END

                            ALTER TABLE AssetDetailValueIntId WITH CHECK CHECK CONSTRAINT all

                            END TRY 
    			            BEGIN CATCH
                                 SELECT ERROR_NUMBER() AS ErrorNumber
                                       ,ERROR_SEVERITY() AS ErrorSeverity
                                       ,ERROR_STATE() AS ErrorState
                                       ,ERROR_PROCEDURE() AS ErrorProcedure
                                       ,ERROR_LINE() AS ErrorLine
                                       ,ERROR_MESSAGE() AS ErrorMessage;

     		                     SELECT @CustomErrorMessage = ''Query Error in AssetDetailValueIntId''
    		                     RAISERROR (@CustomErrorMessage, 16, 1);
    				             Set @RetMessage = @CustomErrorMessage;

                            END CATCH
    	
    		            -----------------------------------------------------------------

    		            --SimulationOutput --> SimulationYearDetail --> AssetDetail --> TreatmentRejectionDetail -->  -->  -->  -->  -->  --> 

    			            BEGIN TRY

                            ALTER TABLE TreatmentRejectionDetail NOCHECK CONSTRAINT all
    		  
    		               SET @RowsDeleted = 1;
    		              --Print ''TreatmentRejectionDetail '';

    		               WHILE @RowsDeleted > 0
    				            BEGIN
    					            BEGIN TRY
    						            Begin Transaction

    						            Delete TOP (@BatchSize) l4
    						            FROM SimulationOutput AS l1
    						            JOIN SimulationYearDetail AS l2 ON l2.SimulationOutputId = l1.Id
    						            JOIN AssetDetail AS l3 ON l3.SimulationYearDetailId = l2.Id
    						            JOIN TreatmentRejectionDetail AS l4 ON l4.AssetDetailId = l3.Id
    						            WHERE l1.Id IN (SELECT Guid FROM #SimOutputTempGuids);

     						            SET @RowsDeleted = @@ROWCOUNT;
    						            COMMIT TRANSACTION
    						            Print ''Rows Affected TreatmentRejectionDetail: '' +  convert(NVARCHAR(50), @RowsDeleted);
    					            END TRY
    					            BEGIN CATCH
    							            ALTER TABLE TreatmentRejectionDetail WITH CHECK CHECK CONSTRAINT all
      							            Set @RetMessage = ''Failed'';
    							            Set @ErrorMessage =  ERROR_PROCEDURE() + '' (Error At Line: '' + cast( ERROR_LINE() as Varchar(5)) + '' ): '' + char(13) + char(10)  + ERROR_MESSAGE()  -- AS ErrorMessage;
    							            Print ''Rolled Back TreatmentRejectionDetail Delete Transaction in SimulationOutput SP  '' + @ErrorMessage;
    							            ROLLBACK TRANSACTION;
    							            RAISERROR  (@RetMessage, 16, 1); 
    							            Return -1;
    					            END CATCH;
    				            END		
    			
    			            ALTER TABLE TreatmentRejectionDetail WITH CHECK CHECK CONSTRAINT all

                            END TRY 
    			            BEGIN CATCH
                                 SELECT ERROR_NUMBER() AS ErrorNumber
                                       ,ERROR_SEVERITY() AS ErrorSeverity
                                       ,ERROR_STATE() AS ErrorState
                                       ,ERROR_PROCEDURE() AS ErrorProcedure
                                       ,ERROR_LINE() AS ErrorLine
                                       ,ERROR_MESSAGE() AS ErrorMessage;

     		                     SELECT @CustomErrorMessage = ''Query Error in TreatmentRejectionDetail''
    		                     RAISERROR (@CustomErrorMessage, 16, 1);
    				             Set @RetMessage = @CustomErrorMessage;

                            END CATCH
    	
    	            -----------------------------------------------------------------

    	            --SimulationOutput --> SimulationYearDetail --> AssetDetail --> TreatmentSchedulingCollisionDetail 


    			            BEGIN TRY

                            ALTER TABLE TreatmentSchedulingCollisionDetail NOCHECK CONSTRAINT all

    			            Delete l4 
    			            FROM SimulationOutput AS l1
    			            JOIN SimulationYearDetail AS l2 ON l2.SimulationOutputId = l1.Id
    			            JOIN AssetDetail AS l3 ON l3.SimulationYearDetailId = l2.Id
    			            JOIN TreatmentSchedulingCollisionDetail AS l4 ON l4.AssetDetailId = l3.Id
    			            WHERE l1.Id IN (SELECT Guid FROM #SimOutputTempGuids);

                            ALTER TABLE TreatmentSchedulingCollisionDetail WITH CHECK CHECK CONSTRAINT all

                            END TRY 
    			            BEGIN CATCH
                                 SELECT ERROR_NUMBER() AS ErrorNumber
                                       ,ERROR_SEVERITY() AS ErrorSeverity
                                       ,ERROR_STATE() AS ErrorState
                                       ,ERROR_PROCEDURE() AS ErrorProcedure
                                       ,ERROR_LINE() AS ErrorLine
                                       ,ERROR_MESSAGE() AS ErrorMessage;

     		                     SELECT @CustomErrorMessage = ''Query Error in TreatmentSchedulingCollisionDetail''
    		                     RAISERROR (@CustomErrorMessage, 16, 1);
    				             Set @RetMessage = @CustomErrorMessage;

                            END CATCH
    	
    	            -----------------------------------------------------------------


    			            --SimulationOutput\SimulationYearDetail\AssetDetail
    			
    			            BEGIN TRY

                            ALTER TABLE AssetDetail NOCHECK CONSTRAINT all

    			            Delete l3 
    			            FROM SimulationOutput AS l1
    			            JOIN SimulationYearDetail AS l2 ON l2.SimulationOutputId = l1.Id
    			            JOIN AssetDetail AS l3 ON l3.SimulationYearDetailId = l2.Id
    			            WHERE l1.Id IN (SELECT Guid FROM #SimOutputTempGuids);

                            ALTER TABLE AssetDetail WITH CHECK CHECK CONSTRAINT all

                            END TRY 
    			            BEGIN CATCH
                                 SELECT ERROR_NUMBER() AS ErrorNumber
                                       ,ERROR_SEVERITY() AS ErrorSeverity
                                       ,ERROR_STATE() AS ErrorState
                                       ,ERROR_PROCEDURE() AS ErrorProcedure
                                       ,ERROR_LINE() AS ErrorLine
                                       ,ERROR_MESSAGE() AS ErrorMessage;

     		                     SELECT @CustomErrorMessage = ''Query Error in AssetDetail''
    		                     RAISERROR (@CustomErrorMessage, 16, 1);
    				             Set @RetMessage = @CustomErrorMessage;

                            END CATCH
    	
    	            -----------------------------------------------------------------


    			            --SimulationOutput\SimulationYearDetail\BudgetDetail
    			
    			            BEGIN TRY

                            ALTER TABLE BudgetDetail NOCHECK CONSTRAINT all

    			            Delete l3 
    			            FROM SimulationOutput AS l1
    			            JOIN SimulationYearDetail AS l2 ON l2.SimulationOutputId = l1.Id
    			            JOIN BudgetDetail AS l3 ON l3.SimulationYearDetailId = l2.Id
    			            WHERE l1.Id IN (SELECT Guid FROM #SimOutputTempGuids);

                            ALTER TABLE BudgetDetail WITH CHECK CHECK CONSTRAINT all

                            END TRY 
    			            BEGIN CATCH
                                 SELECT ERROR_NUMBER() AS ErrorNumber
                                       ,ERROR_SEVERITY() AS ErrorSeverity
                                       ,ERROR_STATE() AS ErrorState
                                       ,ERROR_PROCEDURE() AS ErrorProcedure
                                       ,ERROR_LINE() AS ErrorLine
                                       ,ERROR_MESSAGE() AS ErrorMessage;

     		                     SELECT @CustomErrorMessage = ''Query Error in BudgetDetail''
    		                     RAISERROR (@CustomErrorMessage, 16, 1);
    				             Set @RetMessage = @CustomErrorMessage;

                            END CATCH
    	
    	            ------------------------------------------------------------------


    				            --SimulationOutput\SimulationYearDetail\DeficientConditionGoalDetail
    			
    			            BEGIN TRY

                            ALTER TABLE DeficientConditionGoalDetail NOCHECK CONSTRAINT all

    			            Delete l3 
    			            FROM SimulationOutput AS l1
    			            JOIN SimulationYearDetail AS l2 ON l2.SimulationOutputId = l1.Id
    			            JOIN DeficientConditionGoalDetail AS l3 ON l3.SimulationYearDetailId = l2.Id
    			            WHERE l1.Id IN (SELECT Guid FROM #SimOutputTempGuids);

                            ALTER TABLE DeficientConditionGoalDetail WITH CHECK CHECK CONSTRAINT all

                            END TRY 
    			            BEGIN CATCH
                                 SELECT ERROR_NUMBER() AS ErrorNumber
                                       ,ERROR_SEVERITY() AS ErrorSeverity
                                       ,ERROR_STATE() AS ErrorState
                                       ,ERROR_PROCEDURE() AS ErrorProcedure
                                       ,ERROR_LINE() AS ErrorLine
                                       ,ERROR_MESSAGE() AS ErrorMessage;

     		                     SELECT @CustomErrorMessage = ''Query Error in DeficientConditionGoalDetail''
    		                     RAISERROR (@CustomErrorMessage, 16, 1);
    				             Set @RetMessage = @CustomErrorMessage;

                            END CATCH
    	
    	            ------------------------------------------------------------------

    	
    				            --SimulationOutput\SimulationYearDetail\TargetConditionGoalDetail
    			
    			            BEGIN TRY

                            ALTER TABLE TargetConditionGoalDetail NOCHECK CONSTRAINT all

    			            Delete l3 
    			            FROM SimulationOutput AS l1
    			            JOIN SimulationYearDetail AS l2 ON l2.SimulationOutputId = l1.Id
    			            JOIN TargetConditionGoalDetail AS l3 ON l3.SimulationYearDetailId = l2.Id
    			            WHERE l1.Id IN (SELECT Guid FROM #SimOutputTempGuids);

                            ALTER TABLE TargetConditionGoalDetail WITH CHECK CHECK CONSTRAINT all

                            END TRY 
    			            BEGIN CATCH
                                 SELECT ERROR_NUMBER() AS ErrorNumber
                                       ,ERROR_SEVERITY() AS ErrorSeverity
                                       ,ERROR_STATE() AS ErrorState
                                       ,ERROR_PROCEDURE() AS ErrorProcedure
                                       ,ERROR_LINE() AS ErrorLine
                                       ,ERROR_MESSAGE() AS ErrorMessage;

     		                     SELECT @CustomErrorMessage = ''Query Error in TargetConditionGoalDetail''
    		                     RAISERROR (@CustomErrorMessage, 16, 1);
    				             Set @RetMessage = @CustomErrorMessage;

                            END CATCH
    	
    	            ------------------------------------------------------------------

    			            --SimulationOutput\SimulationYearDetail
    			
    			            BEGIN TRY

                            ALTER TABLE SimulationYearDetail NOCHECK CONSTRAINT all

    			            Delete l2 
    			            FROM SimulationOutput AS l1
    			            JOIN SimulationYearDetail AS l2 ON l2.SimulationOutputId = l1.Id
    			            WHERE l1.Id IN (SELECT Guid FROM #SimOutputTempGuids);

                            ALTER TABLE SimulationYearDetail WITH CHECK CHECK CONSTRAINT all

                            END TRY 
    			            BEGIN CATCH
                                 SELECT ERROR_NUMBER() AS ErrorNumber
                                       ,ERROR_SEVERITY() AS ErrorSeverity
                                       ,ERROR_STATE() AS ErrorState
                                       ,ERROR_PROCEDURE() AS ErrorProcedure
                                       ,ERROR_LINE() AS ErrorLine
                                       ,ERROR_MESSAGE() AS ErrorMessage;

     		                     SELECT @CustomErrorMessage = ''Query Error in SimulationYearDetail''
    		                     RAISERROR (@CustomErrorMessage, 16, 1);
    				             Set @RetMessage = @CustomErrorMessage;

                            END CATCH

    			            ------------------------------------------------------------------

      			            --SimulationOutput

                            BEGIN TRY

                            ALTER TABLE SimulationOutput NOCHECK CONSTRAINT all

    			            Delete l1 
    			            FROM  SimulationOutput AS l1
    			            WHERE l1.Id IN (SELECT Guid FROM #SimOutputTempGuids);

    			            ALTER TABLE SimulationOutput WITH CHECK CHECK CONSTRAINT all
     
                            END TRY 
    			            BEGIN CATCH
                                 SELECT ERROR_NUMBER() AS ErrorNumber
                                       ,ERROR_SEVERITY() AS ErrorSeverity
                                       ,ERROR_STATE() AS ErrorState
                                       ,ERROR_PROCEDURE() AS ErrorProcedure
                                       ,ERROR_LINE() AS ErrorLine
                                       ,ERROR_MESSAGE() AS ErrorMessage;

     		                     SELECT @CustomErrorMessage = ''Query Error in SimulationOutput''
    		                     RAISERROR (@CustomErrorMessage, 16, 1);
    				             Set @RetMessage = @CustomErrorMessage;

                            END CATCH

    		               ---------------------------------------------------------------------------
    			            ------End SimulationOutput Delete-------------------------------------------------------------------

    	            DROP TABLE #SimOutputTempGuids;
                    COMMIT TRANSACTION
                    Print ''Delete Transaction Committed'';
       	            RAISERROR (@RetMessage, 0, 1);
    	            END TRY
    	            BEGIN CATCH
      			            Set @RetMessage = ''Failed '' + @RetMessage;
    			            Set @ErrorMessage =  ERROR_PROCEDURE() + '' (Error At Line: '' + cast( ERROR_LINE() as Varchar(5)) + '' ): '' + char(13) + char(10)  + ERROR_MESSAGE()  -- AS ErrorMessage;
    			            Print ''Rolled Back Entire Delete Transaction in SimulationOutput SP:  '' + @ErrorMessage;
    			            ROLLBACK TRANSACTION;
    			            RAISERROR  (@RetMessage, 16, 1);  
    	            END CATCH;
                END')";

            migrationBuilder.Sql(createProcSql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
           var createProcSql = @"EXEC('CREATE OR ALTER PROCEDURE dbo.usp_delete_aggregations(@NetworkId AS uniqueidentifier=NULL,@RetMessage VARCHAR(250) OUTPUT)
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
	                    DECLARE @BatchSize INT = 100000;
	                    DECLARE @RowsDeleted INT = 1;

	                    BEGIN TRY
                    -----------------------------------------------------------------------

                    --AggregatedResult
                    --AttributeDatum
                    --AttributeDatumLocation


	                    -----Start AggregatedResult Path-----------------------------------------

			                    --MaintainableAsset --> AggregatedResult

	                    BEGIN TRY

                              ALTER TABLE AggregatedResult NOCHECK CONSTRAINT all
		                      SET @RowsDeleted = 1;
		                      --Print ''AggregatedResult '';

		                       WHILE @RowsDeleted > 0
				                    BEGIN
					                    BEGIN TRY
						                    Begin Transaction

						                    --Delete TOP (@BatchSize) l3
						                    SELECT TOP  (@BatchSize) l3.Id  INTO #tempAggregatedResult
						                    FROM Network AS l1
						                    Join MaintainableAsset AS l2 ON l2.NetworkId = l1.Id
						                    JOIN AggregatedResult AS l3 ON l3.MaintainableAssetId = l2.Id
						                    WHERE l1.Id IN (@NetworkId);
						
						                    --DELETE ar
						                    --FROM AggregatedResult As ar
						                    --JOIN #tempAggregatedResult T ON T.Id = ar.Id;

						                    DELETE FROM AggregatedResult WHERE Id in (SELECT Id FROM #tempAggregatedResult);

						                    SET @RowsDeleted = @@ROWCOUNT;

						                    DROP TABLE #tempAggregatedResult;
						                    --WAITFOR DELAY ''00:00:01'';

						                    COMMIT TRANSACTION
						
						                    Print ''Rows Affected Network --> MaintainableAsset-->AggregatedResult: '' +  convert(NVARCHAR(50), @RowsDeleted);
					                    END TRY
					                    BEGIN CATCH
							                    ALTER TABLE AggregatedResult WITH CHECK CHECK CONSTRAINT all
  							                    Set @RetMessage = ''Failed'';
							                    Set @ErrorMessage =  ERROR_PROCEDURE() + '' (Error At Line: '' + cast( ERROR_LINE() as Varchar(5)) + '' ): '' + char(13) + char(10)  + ERROR_MESSAGE()  -- AS ErrorMessage;
							                    Print ''Rolled Back AggregatedResult Delete Transaction in NetworkDelete SP:  '' + @ErrorMessage;
							                    ROLLBACK TRANSACTION;
							                    RAISERROR  (@RetMessage, 16, 1); 
							                    Return -1;
					                    END CATCH;
				                    END

                                ALTER TABLE AggregatedResult WITH CHECK CHECK CONSTRAINT all

                                END TRY 
			                    BEGIN CATCH
                                     SELECT ERROR_NUMBER() AS ErrorNumber
                                           ,ERROR_SEVERITY() AS ErrorSeverity
                                           ,ERROR_STATE() AS ErrorState
                                           ,ERROR_PROCEDURE() AS ErrorProcedure
                                           ,ERROR_LINE() AS ErrorLine
                                           ,ERROR_MESSAGE() AS ErrorMessage;

 		                             SELECT @CustomErrorMessage = ''Query Error in  Network --> MaintainableAsset-->AggregatedResult''
		                             RAISERROR (@CustomErrorMessage, 16, 1);
				                     Set @RetMessage = @CustomErrorMessage;

                                END CATCH

			                    ------End -----------------------------------------------------------------


			                    -------Start AttributeDatum Path-

			                    --MaintainableAsset --> AttributeDatum --> AttributeDatumLocation -->  -->  --> 

                                BEGIN TRY

                              ALTER TABLE AttributeDatumLocation NOCHECK CONSTRAINT all
		                      SET @RowsDeleted = 1;
		  	                    --Print ''AttributeDatumLocation '';

		                      WHILE @RowsDeleted > 0
				                    BEGIN
					                    BEGIN TRY
						                    Begin Transaction

							                    SELECT TOP (@BatchSize) l4.Id  INTO #tempAttributeDatumLocation
							                    FROM Network AS l1
							                    Join MaintainableAsset AS l2 ON l2.NetworkId = l1.Id
							                    JOIN AttributeDatum AS l3 ON l3.MaintainableAssetId = l2.Id
							                    Join AttributeDatumLocation As l4 ON l4.AttributeDatumId = l3.Id
							                    WHERE l1.Id IN (@NetworkId);

						                    --DELETE adl
						                    --FROM AttributeDatumLocation As adl
						                    --JOIN #tempAttributeDatumLocation T ON T.Id = adl.Id;

						                    DELETE FROM AttributeDatumLocation WHERE Id in (SELECT Id FROM #tempAttributeDatumLocation);

						                    SET @RowsDeleted = @@ROWCOUNT;

						                    DROP TABLE #tempAttributeDatumLocation;
						                    --WAITFOR DELAY ''00:00:01'';

						                    COMMIT TRANSACTION
						
						                    Print ''Rows Affected Network --> MaintainableAsset-->AttributeDatumLocation: '' +  convert(NVARCHAR(50), @RowsDeleted);
					                    END TRY
					                    BEGIN CATCH
							                    ALTER TABLE AttributeDatumLocation WITH CHECK CHECK CONSTRAINT all
  							                    Set @RetMessage = ''Failed'';
							                    Set @ErrorMessage =  ERROR_PROCEDURE() + '' (Error At Line: '' + cast( ERROR_LINE() as Varchar(5)) + '' ): '' + char(13) + char(10)  + ERROR_MESSAGE()  -- AS ErrorMessage;
							                    Print ''Rolled Back AttributeDatumLocation Delete Transaction in NetworkDelete SP:  '' + @ErrorMessage;
							                    ROLLBACK TRANSACTION;
							                    RAISERROR  (@RetMessage, 16, 1); 
							                    Return -1;
					                    END CATCH;
				                    END

                                ALTER TABLE AttributeDatumLocation WITH CHECK CHECK CONSTRAINT all

                                END TRY 
			                    BEGIN CATCH
                                     SELECT ERROR_NUMBER() AS ErrorNumber
                                           ,ERROR_SEVERITY() AS ErrorSeverity
                                           ,ERROR_STATE() AS ErrorState
                                           ,ERROR_PROCEDURE() AS ErrorProcedure
                                           ,ERROR_LINE() AS ErrorLine
                                           ,ERROR_MESSAGE() AS ErrorMessage;

 		                             SELECT @CustomErrorMessage = ''Query Error in Network --> MaintainableAsset-->AttributeDatum-->AttributeDatumLocation''
		                             RAISERROR (@CustomErrorMessage, 16, 1);
				                     Set @RetMessage = @CustomErrorMessage;

                                END CATCH

			                    -----------------------------------------------------------------------

    		                    --------MaintainableAsset --> AttributeDatum--

                                BEGIN TRY

			                    ALTER TABLE AttributeDatum NOCHECK CONSTRAINT all
			                    SET @RowsDeleted = 1;
			                    Print ''AttributeDatum '';

			                    WHILE @RowsDeleted > 0
				                    BEGIN
					                    BEGIN TRY
					                    Begin Transaction

						                    Select  TOP (@BatchSize) l3.Id  INTO #tempAttributeDatum
						                    FROM Network AS l1
						                    Join MaintainableAsset AS l2 ON l2.NetworkId = l1.Id
						                    JOIN AttributeDatum AS l3 ON l3.MaintainableAssetId = l2.Id
						                    WHERE l1.Id IN (@NetworkId);

						                    --DELETE ad
						                    --FROM AttributeDatum As ad
						                    --JOIN #tempAttributeDatum T ON T.Id = ad.Id;

						                    DELETE FROM AttributeDatum WHERE Id in (SELECT Id FROM #tempAttributeDatum);

						                    SET @RowsDeleted = @@ROWCOUNT;

						                    DROP TABLE #tempAttributeDatum;
						                    --WAITFOR DELAY ''00:00:01'';

						                    COMMIT TRANSACTION
						
						                    Print ''Rows Affected Network --> MaintainableAsset-->AttributeDatum: '' +  convert(NVARCHAR(50), @RowsDeleted);
					                    END TRY
					                    BEGIN CATCH
						                    ALTER TABLE AttributeDatum WITH CHECK CHECK CONSTRAINT all
						                    Set @RetMessage = ''Failed'';
						                    Set @ErrorMessage =  ERROR_PROCEDURE() + '' (Error At Line: '' + cast( ERROR_LINE() as Varchar(5)) + '' ): '' + char(13) + char(10)  + ERROR_MESSAGE()  -- AS ErrorMessage;
						                    Print ''Rolled Back AttributeDatum Delete Transaction in NetworkDelete SP:  '' + @ErrorMessage;
						                    ROLLBACK TRANSACTION;
						                    RAISERROR  (@RetMessage, 16, 1); 
						                    Return -1;
					                    END CATCH;
				                    END

                                ALTER TABLE AttributeDatum WITH CHECK CHECK CONSTRAINT all

                                END TRY 
			                    BEGIN CATCH
                                     SELECT ERROR_NUMBER() AS ErrorNumber
                                           ,ERROR_SEVERITY() AS ErrorSeverity
                                           ,ERROR_STATE() AS ErrorState
                                           ,ERROR_PROCEDURE() AS ErrorProcedure
                                           ,ERROR_LINE() AS ErrorLine
                                           ,ERROR_MESSAGE() AS ErrorMessage;

 		                             SELECT @CustomErrorMessage = ''Query Error in Network --> MaintainableAsset-->AttributeDatum''
		                             RAISERROR (@CustomErrorMessage, 16, 1);
				                     Set @RetMessage = @CustomErrorMessage;

                                END CATCH


			                    ----------------------------------------------------------------------
			                    -----End AttributeDatum Path--------------------------------------

                       --COMMIT TRANSACTION
                        Print ''Delete Attribute End'';
   	                    RAISERROR (@RetMessage, 0, 1);
	                    END TRY
	                    BEGIN CATCH
  			                    Set @RetMessage = ''Failed '' + @RetMessage;
			                    Set @ErrorMessage =  ERROR_PROCEDURE() + '' (Error At Line: '' + cast( ERROR_LINE() as Varchar(5)) + '' ): '' + char(13) + char(10)  + ERROR_MESSAGE()  -- AS ErrorMessage;
			                    Print ''Error in AttributeDelete SP:  '' + @ErrorMessage;
			                    --ROLLBACK TRANSACTION;
			                    RAISERROR  (@RetMessage, 16, 1);  
	                    END CATCH;

           END')";

            migrationBuilder.Sql(createProcSql);
        }
    }
}
