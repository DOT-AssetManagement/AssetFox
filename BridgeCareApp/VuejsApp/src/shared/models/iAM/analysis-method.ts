import {CriterionLibrary, emptyCriterionLibrary} from '@/shared/models/iAM/criteria';
import {clone} from 'ramda';
import {getBlankGuid} from '@/shared/utils/uuid-utils';

export enum OptimizationStrategy {
    Benefit,
    BenefitToCostRatio,
    RemainingLife,
    RemainingLifeToCostRatio
}

export enum SpendingStrategy {
    NoSpending,
    UnlimitedSpending,
    UntilTargetAndDeficientConditionGoalsMet,
    UntilTargetConditionGoalsMet,
    UntilDeficientConditionGoalsMet,
    AsBudgetPermits
}

export interface Benefit {
    id: string;
    limit: number;
    attribute: string;
}

export interface AnalysisMethod {
    id: string;
    description: string;
    optimizationStrategy: OptimizationStrategy;
    spendingStrategy: SpendingStrategy;
    shouldApplyMultipleFeasibleCosts: boolean;
    shouldDeteriorateDuringCashFlow: boolean;
    shouldUseExtraFundsAcrossBudgets: boolean;
    shouldAllowMultipleTreatments: boolean;
    shouldUseSuperAssets: boolean;
    attribute: string;
    benefit: Benefit;
    criterionLibrary: CriterionLibrary;
    superAssetAttribute: string;
    lastKnownAssetCount: number;
}

export const emptyBenefit: Benefit = {
    id: getBlankGuid(),
    limit: 0,
    attribute: ''
};

export const emptyAnalysisMethod: AnalysisMethod = {
    id: getBlankGuid(),
    description: '',
    optimizationStrategy: OptimizationStrategy.Benefit,
    spendingStrategy: SpendingStrategy.NoSpending,
    shouldApplyMultipleFeasibleCosts: false,
    shouldDeteriorateDuringCashFlow: false,
    shouldUseExtraFundsAcrossBudgets: false,
    shouldAllowMultipleTreatments: false,
    shouldUseSuperAssets: false,
    attribute: '',
    superAssetAttribute: '',
    benefit: clone(emptyBenefit),
    criterionLibrary: clone(emptyCriterionLibrary),
    lastKnownAssetCount: -1,
};