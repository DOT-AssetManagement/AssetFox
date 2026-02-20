export interface TreatmentRuleOverride {
    id: string;
    name: string;
    description: string;
    isEnabledByDefault: boolean;
    criteria: TreatmentRuleOverrideCriterion[];
}

export interface TreatmentRuleOverrideCriterion {
    id: string;
    treatmentName: string;
    criterionExpression: string;
}

export interface ScenarioOverrideSetting {
    overrideName: string;
    isEnabled: boolean;
}

export const emptyTreatmentRuleOverride: TreatmentRuleOverride = {
    id: '',
    name: '',
    description: '',
    isEnabledByDefault: true,
    criteria: [],
};

export const emptyTreatmentRuleOverrideCriterion: TreatmentRuleOverrideCriterion = {
    id: '',
    treatmentName: '',
    criterionExpression: '',
};
