import { Treatment } from '@/shared/models/iAM/treatment';
import { TreatmentRuleOverride } from '@/shared/models/iAM/treatment-rule-override';

export interface CreateTreatmentLibraryDialogData {
    showDialog: boolean;
    selectedTreatmentLibraryTreatments: Treatment[];
    selectedTreatmentLibraryRuleOverrides: TreatmentRuleOverride[];
}

export const emptyCreateTreatmentLibraryDialogData: CreateTreatmentLibraryDialogData = {
    showDialog: false,
    selectedTreatmentLibraryTreatments: [],
    selectedTreatmentLibraryRuleOverrides: [],
};
