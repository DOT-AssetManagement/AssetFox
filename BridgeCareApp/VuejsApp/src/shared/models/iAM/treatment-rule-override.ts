export interface TreatmentRuleOverride {
    id: string;
    name: string;
    description: string;
    sourceTreatmentRuleOverrideId?: string;
    isEnabledByDefault: boolean;
    isEnabled: boolean;
    criteria: TreatmentRuleOverrideCriterion[];
}

export interface TreatmentRuleOverrideCriterion {
    id: string;
    logicalOperator: 'AND' | 'OR';
    sortOrder: number;
    treatmentIds: string[];
    treatments: { id: string; name: string }[];
    treatmentName?: string;
    criterionExpression: string;
}

export interface ScenarioOverrideSetting {
    overrideId: string;
    overrideName: string;
    isEnabled: boolean;
}

export const emptyTreatmentRuleOverride: TreatmentRuleOverride = {
    id: '',
    name: '',
    description: '',
    isEnabledByDefault: true,
    isEnabled: true,
    criteria: [],
};

export const emptyTreatmentRuleOverrideCriterion: TreatmentRuleOverrideCriterion = {
    id: '',
    logicalOperator: 'AND',
    sortOrder: 0,
    treatmentIds: [],
    treatments: [],
    criterionExpression: '',
};
