<template>
    <v-row column>
        <v-col v-if="false">
            <v-row justify-center>
                <v-col cols = "3">
                    <v-btn
                        @click="onShowCreateCriterionLibraryDialog(false)"
                        class="assetFox-blue-bg text-white"
                    >
                        New Library
                    </v-btn>
                    <v-select
                        v-model="librarySelectItemValue"
                        menu-icon=custom:GhdDownSvg
                        v-if="!hasSelectedCriterionLibrary"
                        :items="criterionLibrarySelectItems"
                        label="Select a Criteria Library"
                        variant="outlined"
                    >
                    </v-select>
                    <v-text-field
                        v-if="hasSelectedCriterionLibrary"
                        v-model="selectedCriterionLibrary.name"
                        @change="canUpdateOrCreate = true"
                    >
                        <template v-slot:append-inner>
                            <v-btn
                                @click="librarySelectItemValue = ''"
                                class="assetFox-orange"
                                icon
                            >
                                <v-icon>fas fa-caret-left</v-icon>
                            </v-btn>
                        </template>
                    </v-text-field>
                    <div v-if="hasSelectedCriterionLibrary">
                        Owner: {{ getOwnerUserName() || '[ No Owner ]' }}
                    </div>
                    <v-checkbox
                        v-if="hasSelectedCriterionLibrary"
                        v-model="selectedCriterionLibrary.isShared"
                        class="sharing"
                        label="Shared"
                        @change="canUpdateOrCreate = true"
                    />
                </v-col>
            </v-row>
        </v-col>
        <v-divider v-show="hasSelectedCriterionLibrary || callFromScenario || callFromLibraryToEditCriterion" />
        <v-col v-show="hasSelectedCriterionLibrary || callFromScenario || callFromLibraryToEditCriterion">
            <v-row justify-center>
                <v-col cols = "10">
                    <CriteriaEditor
                        :criteriaEditorData="criteriaEditorData"
                        @submitCriteriaEditorResult="
                            onSubmitCriteriaEditorResult
                        "
                    />
                </v-col>
            </v-row>
        </v-col>
        <v-divider v-show="hasSelectedCriterionLibrary || callFromScenario || callFromLibraryToEditCriterion" />
        <v-col v-show="hasSelectedCriterionLibrary">
            <v-row justify-center>
                <v-col xs6>
                    <v-textarea
                        v-model="selectedCriterionLibrary.description"
                        label="Description"
                        no-resize
                        outline
                        rows="4"
                    >
                    </v-textarea>
                </v-col>
            </v-row>
        </v-col>
        <v-col>
            <v-row justify-end style="padding-bottom: 40px;" row v-show="hasSelectedCriterionLibrary">
                <v-btn
                    @click="onUpsertCriterionLibrary(selectedCriterionLibrary)"
                    class="assetFox-blue-bg text-white"
                    :disabled="!canUpdateOrCreate || !hasLibraryEditPermission"
                >
                    Update Library
                </v-btn>
                <v-btn
                    @click="onShowCreateCriterionLibraryDialog(true)"
                    class="assetFox-blue-bg text-white"
                    :disabled="!canUpdateOrCreate"
                >
                    Create as New Library
                </v-btn>
                <v-btn variant = "outlined"
                    @click="onShowConfirmDeleteAlert"
                    class="assetFox-orange-bg text-white"
                    :disabled="!hasLibraryEditPermission"
                >
                    Delete Library
                </v-btn>
            </v-row>
        </v-col>

        <CreateCriterionLibraryDialog
            :dialogData="createCriterionLibraryDialogData"
            @submit="onUpsertCriterionLibrary"
        />

        <ConfirmDeleteAlert
            :dialogData="confirmDeleteAlertData"
            @submit="onSubmitConfirmDeleteAlertResult"
        />
    </v-row>
</template>

<script lang="ts" setup>
import Vue, { shallowRef } from 'vue';
import {inject, reactive, ref, onMounted, onBeforeUnmount, watch, Ref} from 'vue';
import CriteriaEditor from '@/shared/components/CriteriaEditor.vue';
import { SelectItem } from '@/shared/models/vue/select-item';
import {
    CriteriaEditorData,
    CriteriaEditorResult,
    CriterionLibrary,
    emptyCriteriaEditorData,
    emptyCriterionLibrary,
} from '@/shared/models/iAM/criteria';

import { clone, isNil } from 'ramda';
import {
    hasUnsavedChanges,
    hasUnsavedChangesCore,
} from '@/shared/utils/has-unsaved-changes-helper';
import {
    CreateCriterionLibraryDialogData,
    emptyCreateCriterionLibraryDialogData,
} from '@/shared/models/modals/create-criterion-library-dialog-data';
import CreateCriterionLibraryDialog from '@/components/criteria-editor/criteria-editor-dialogs/CreateCriterionLibraryDialog.vue';
import { AlertData, emptyAlertData } from '@/shared/models/modals/alert-data';
import ConfirmDeleteAlert from '@/shared/modals/Alert.vue';
import { getBlankGuid } from '@/shared/utils/uuid-utils';
import { hasValue } from '@/shared/utils/has-value-util';
import { getUserName } from '@/shared/utils/get-user-info';
import { useRouter } from 'vue-router';
import { useAuthenticationStore, useCriterionLibraryStore, useUnsavedChangesStore, useUserStore } from '@/stores';

    let dialogLibraryId = shallowRef<string>('');
    let dialogIsFromScenario = shallowRef<boolean>();
    let dialogIsFromLibrary: boolean;
    const authenticationStore = useAuthenticationStore();
    const criterionLibraryStore = useCriterionLibraryStore();
    const unsavedChangesStore = useUnsavedChangesStore();
    const userStore = useUserStore();

    let hasSelectedCriterionLibrary: boolean = false;
    let criterionLibrarySelectItems: SelectItem[] = [];
    let librarySelectItemValue = shallowRef<string>('');
    let selectedCriterionLibrary: CriterionLibrary = clone(emptyCriterionLibrary);
    let criteriaEditorData: CriteriaEditorData = {
        ...emptyCriteriaEditorData,
        isLibraryContext: true,
    };
    let isLibraryContext: boolean = true;
    const props = defineProps<{createCriterionLibraryDialogData: CreateCriterionLibraryDialogData}>()
    let confirmDeleteAlertData: AlertData = clone(emptyAlertData);
    let canUpdateOrCreate = shallowRef<boolean>(false);
    let uuidNIL: string = getBlankGuid();
    let callFromScenario = shallowRef<boolean>();
    let callFromLibraryToEditCriterion: boolean = false;
    let criteriaForScenario = shallowRef<string>('');
    let selectedScenarioRelatedCriteria: CriterionLibrary = clone(
        emptyCriterionLibrary,
    );
    let hasCreatedLibrary = shallowRef<boolean>(false);
    let hasLibraryEditPermission = shallowRef<boolean>(false);
    const emit = defineEmits(['submit'])    
    const $router = useRouter();

   created();
   function created() {
         ((vm: any) => {
            criterionLibraryStore.getHasPermittedAccess();
            if ($router.currentRoute.value.path.indexOf('CriterionLibraryEditor/Library') !== -1) {
                vm.librarySelectItemValue = null;
                criterionLibraryStore.getCriterionLibraries();
            } else {
                vm.isLibraryContext = false;
            }
        });
    }

    onBeforeUnmount(() => beforeDestroy());
    function beforeDestroy() {
        if (isLibraryContext) {
            unsavedChangesStore.setHasUnsavedChanges(false);
        }
    }


    watch(() => criterionLibraryStore.criterionLibrariesGetter, () => onStateCriterionLibrariesChanged())
    function onStateCriterionLibrariesChanged() {
        criterionLibrarySelectItems = criterionLibraryStore.criterionLibrariesGetter
            .filter((lib: CriterionLibrary) => lib.isSingleUse == false)
            .map((library: CriterionLibrary) => ({
                text: library.name,
                value: library.id,
            }));
        if (!isLibraryContext && hasValue(librarySelectItemValue)) {
            criterionLibraryStore.getSelectedCriterionLibrary(librarySelectItemValue as any);
        }
    }

    watch(dialogLibraryId, () => onDialogLibraryIdChanged) 
    function onDialogLibraryIdChanged(){
        if (
            (!dialogIsFromScenario && !dialogIsFromLibrary) ||
            dialogLibraryId.value == uuidNIL
        ) {
            librarySelectItemValue = dialogLibraryId;
        }
    }

    watch(librarySelectItemValue, () => onLibrarySelectItemValueChanged) 
    function onLibrarySelectItemValueChanged(){
        criterionLibraryStore.getSelectedCriterionLibrary(librarySelectItemValue.value);
    }

    watch(() => criterionLibraryStore.selectedCriterionLibraryGetter, () => onStateSelectedCriterionLibraryChanged())
    function onStateSelectedCriterionLibraryChanged(){
        canUpdateOrCreate.value = false;
        selectedCriterionLibrary = clone(
            criterionLibraryStore.selectedCriterionLibraryGetter
        );
    }

    watch(() => criterionLibraryStore.getScenarioRelatedCriteria, onScenarioRelatedCriteria)
    function onScenarioRelatedCriteria() {
        criteriaForScenario = dialogLibraryId;
        callFromScenario = dialogIsFromScenario;
        callFromLibraryToEditCriterion = dialogIsFromLibrary;
        selectedScenarioRelatedCriteria = clone(
            criterionLibraryStore.getScenarioRelatedCriteria
        );
        criteriaEditorData = {
            ...criteriaEditorData,
            mergedCriteriaExpression: selectedScenarioRelatedCriteria
                .mergedCriteriaExpression,
        };
    }

    watch(canUpdateOrCreate, () => onCanUpdateOrCreateChanged) 
    function onCanUpdateOrCreateChanged(){


    }

    watch(selectedCriterionLibrary, () => onSelectedCriterionLibraryChanged)
    function onSelectedCriterionLibraryChanged() {
        hasSelectedCriterionLibrary =
            selectedCriterionLibrary.id !== uuidNIL;

        if (hasSelectedCriterionLibrary) {
            checkLibraryEditPermission();
            hasCreatedLibrary.value = false;
        }

        if (
            (callFromScenario || dialogIsFromLibrary) &&
            hasSelectedCriterionLibrary == false
        ) {
            criteriaEditorData = {
                ...criteriaEditorData,
                mergedCriteriaExpression: selectedScenarioRelatedCriteria
                    .mergedCriteriaExpression,
            };
        } else {
            criteriaEditorData = {
                ...criteriaEditorData,
                mergedCriteriaExpression: selectedCriterionLibrary
                    .mergedCriteriaExpression,
            };
        }

        if (isLibraryContext) {
            unsavedChangesStore.setHasUnsavedChanges(
                hasUnsavedChangesCore(
                    'criterion-library',
                    selectedCriterionLibrary,
                    criterionLibraryStore.selectedCriterionLibraryGetter,
                ),
            );
        } else {
           emit('submit', selectedCriterionLibrary);
        }
    }

   onMounted(() => mounted());
   function mounted() {
        if (hasValue(criterionLibraryStore.criterionLibrariesGetter)) {
            criterionLibrarySelectItems = criterionLibraryStore.criterionLibrariesGetter
            .filter((lib: CriterionLibrary) => lib.isSingleUse == false)
            .map((library: CriterionLibrary) => ({
                text: library.name,
                value: library.id,
            }));

            if (
                (!isLibraryContext || !dialogIsFromLibrary) &&
                hasValue(librarySelectItemValue)
            ) {
                criterionLibraryStore.getSelectedCriterionLibrary(librarySelectItemValue as any);
            }
        }
    }

    function checkLibraryEditPermission() {
        hasLibraryEditPermission.value = authenticationStore.getHasAdminAccess || ((criterionLibraryStore.hasPermittedAccessGetter && checkUserIsLibraryOwner()) as any);
    }

    async function checkUserIsLibraryOwner() {
        return userStore.getUserNameById(selectedCriterionLibrary.owner ?? "") == getUserName();
    }

    async function getOwnerUserName(): Promise<string> {

        if (!hasCreatedLibrary) {
        return userStore.getUserNameById(selectedCriterionLibrary.owner ?? "");
        }
        
        return getUserName();
    }

    function onShowCreateCriterionLibraryDialog(createAsNew: boolean) {

        props.createCriterionLibraryDialogData.showDialog = true;
        props.createCriterionLibraryDialogData.mergedCriteriaExpression = createAsNew ? selectedCriterionLibrary.mergedCriteriaExpression != null
                    ? selectedCriterionLibrary.mergedCriteriaExpression
                    : ''
                : '';
    }

    function onSubmitCriteriaEditorResult(result: CriteriaEditorResult) {
        canUpdateOrCreate.value = result.validated;

        if (result.validated) {
            if (!dialogIsFromScenario && !dialogIsFromLibrary) {
                selectedCriterionLibrary = {
                    ...selectedCriterionLibrary,
                    mergedCriteriaExpression: result.criteria!,
                };
            } else {
                selectedScenarioRelatedCriteria = {
                    ...selectedScenarioRelatedCriteria,
                    mergedCriteriaExpression: result.criteria!,
                    isSingleUse: true,
                };
                criterionLibraryStore.upsertSelectedScenarioRelatedCriterion(
                    selectedScenarioRelatedCriteria,
                );
            }

            criterionLibraryStore.setSelectedCriterionIsValid(true);
        } else {
            criterionLibraryStore.setSelectedCriterionIsValid(false);
        }
    }

    function onUpsertCriterionLibrary(criterionLibrary: CriterionLibrary) {
        props.createCriterionLibraryDialogData.showDialog = clone(emptyCreateCriterionLibraryDialogData.showDialog);

        if (!isNil(criterionLibrary)) {
            // undefined dialogIsFromLibrary means, the call has come from none of the scenario related component
            if(isNil(dialogIsFromLibrary) || dialogIsFromLibrary){
                criterionLibrary.isSingleUse = false;
            }
            criterionLibraryStore.upsertCriterionLibrary(criterionLibrary)
                .then(() => (librarySelectItemValue.value = criterionLibrary.id));

            hasCreatedLibrary.value = true;
        }
    }

    function onShowConfirmDeleteAlert() {
        confirmDeleteAlertData = {
            showDialog: true,
            heading: 'Warning',
            choice: true,
            message: 'Are you sure you want to delete?',
        };
    }

    function onSubmitConfirmDeleteAlertResult(submit: boolean) {
        confirmDeleteAlertData = clone(emptyAlertData);

        if (submit) {
            criterionLibraryStore.deleteCriterionLibrary(selectedCriterionLibrary.id)
                .then(() => (librarySelectItemValue.value = ''));
        }
    }

</script>
