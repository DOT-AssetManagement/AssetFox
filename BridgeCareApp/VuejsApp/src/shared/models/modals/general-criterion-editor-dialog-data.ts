import { CriterionLibrary, emptyCriteria, emptyCriterionLibrary } from "../iAM/criteria";

export interface GeneralCriterionEditorDialogData{
    CriteriaExpression: string | null;
    resultsCount: number | null;
    showDialog: boolean;
}

export const emptyGeneralCriterionEditorDialogData: GeneralCriterionEditorDialogData = {
    CriteriaExpression: "",
    resultsCount: null,
    showDialog: false
}