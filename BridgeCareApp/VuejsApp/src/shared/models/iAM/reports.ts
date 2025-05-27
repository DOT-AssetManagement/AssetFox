import { emptyEquation, Equation } from '@/shared/models/iAM/equation';
import {
    CriterionLibrary,
    emptyCriterionLibrary,
} from '@/shared/models/iAM/criteria';
import { clone } from 'ramda';
import { getBlankGuid } from '@/shared/utils/uuid-utils';

export interface Report {
    id: string;
    name: string;
    mergedExpression: string;
    isGenerated: boolean;
    reportStatus?: string;
}
export const emptyReport: Report = {
    id: getBlankGuid(),
    name: '',
    mergedExpression: '',
    isGenerated: false,
};

export interface ReportDetails {
    simulationId: string;
    reportName: string;
    isGenerated: boolean;
    reportStatus?: string;
}

export const reportDetails: ReportDetails = {
    simulationId: getBlankGuid(),
    reportName: '',
    isGenerated: false
};

export interface DownloadReportParams {
    scenarioId: string;
    reportName: string;
    // Optional: for constructing a more user-friendly filename on client if server doesn't provide good one
    simulationName?: string; 
};

export interface UserDefinedReportRequestModel {
    attributes:string[];
    years: number[];
    displayConditionOfNetwork: boolean;
    displayInitialAssets: boolean; // assets attributes
    displayYearAssets: boolean; // combine yearly assets + metadata + attributes
    displayBudgets: boolean; // asset, years, budget info - vertical data in sheet
    displayDeficientConditionGoals: boolean;
    displayTargetConditionGoals: boolean;    
    displayTreatmentOptions: boolean;
    displayTreatmentSchedulingCollisions: boolean;
    displayTreatmentRejections: boolean;
    displayTreatmentCashflowConsiderations: boolean; // treatmentConsiderations
    displyTreatmentCurrentBudgetsToSpend: boolean; // treatmentConsiderations
    displayTreatmentAllocations: boolean; // treatmentConsiderations
}

export const emptyUserDefinedReportRequestModel : UserDefinedReportRequestModel ={
    attributes: [],
    years: [],
    displayConditionOfNetwork: false,
    displayInitialAssets: false,
    displayYearAssets: false,
    displayBudgets: false,
    displayDeficientConditionGoals: false,
    displayTargetConditionGoals: false,
    displayTreatmentOptions: false,
    displayTreatmentSchedulingCollisions: false,
    displayTreatmentRejections: false,
    displayTreatmentCashflowConsiderations: false,
    displyTreatmentCurrentBudgetsToSpend: false,
    displayTreatmentAllocations: false
};