<template>
    <v-dialog max-width="450px" persistent v-model="dialogData.showDialog">
        <v-card elevation="5" 
         class="modal-pop-up-padding">
            <v-card-title>
                <v-row justify="space-between">
                <h3 class="dialog-header">
                    Clone scenario
                </h3>
                <v-btn @click="onSubmit(false)" flat>
                    <i class="fas fa-times fa-2x"></i>
                </v-btn>
            </v-row>
            </v-card-title>
            <v-card-text>
              <v-select
                    :items="networkStore.networksGetter"
                    label="Select a network"
                    item-title="name"
                    v-model="networkMetaData"
                    menu-icon=custom:GhdDownSvg
                    return-object
                    @update:modelValue="selectedNetwork(`${networkMetaData.name}`, `${networkMetaData.id}`)"
                    density="compact"
                    variant="outlined"
                ></v-select>
                <v-text-field
                    id="CloneScenarioDialog-scenarioName-textField"
                    label="Scenario name"
                    variant="outlined"
                    density="compact"
                    v-model="dialogData.scenario.name"
                    :error-messages="scenarioNameErrors"
                ></v-text-field>
                <v-checkbox v-model="shared" label="Share with all?" />
            </v-card-text>
            <v-card-actions>
                <v-row justify="center">
                    <v-btn
                        id="CloneScenarioDialog-cancel-btn"
                        @click="onSubmit(false)"
                        class="ghd-button ghd-blue"
                        >Cancel</v-btn
                    >
                    <v-btn
                        id="CloneScenarioDialog-save-btn"
                        :disabled="isSaveButtonDisabled"
                        variant="outlined"
                        @click="onSubmit(true)"
                        class="ghd-button ghd-blue"
                    >
                        Save
                    </v-btn>
                </v-row>
            </v-card-actions>
        </v-card>
    </v-dialog>
</template>

<script lang="ts" setup>
import { toRefs, computed, ref, watch } from 'vue'; 
import { getUserName } from '@/shared/utils/get-user-info';
import { User } from '@/shared/models/iAM/user';
import {
    ScenarioUser,
    CloneScenario,
    emptyCloneScenario,
} from '@/shared/models/iAM/scenario';
import { getBlankGuid, getNewGuid } from '@/shared/utils/uuid-utils';
import { find, isNil, propEq } from 'ramda';
import { emptyNetwork, Network } from '@/shared/models/iAM/network';
import { CloneSimulationDialogData } from '@/shared/models/modals/clone-scenario-dialog-data';
import {
    InputValidationRules,
    rules as validationRules,
} from '@/shared/utils/input-validation-rules';
import { useNetworkStore, useScenarioStore, useUserStore } from '@/stores';

    const networkStore = useNetworkStore();
    const scenarioStore = useScenarioStore();
    const userStore = useUserStore();
    const props = defineProps<{dialogData: CloneSimulationDialogData}>();
    const emit = defineEmits(['submit']);
    const { dialogData } = toRefs(props);

    let shared =  ref<boolean>(false);
    let rules: InputValidationRules = validationRules;

    let newScenario: CloneScenario = { ...emptyCloneScenario, id: getNewGuid() };
    let selectedNetworkId: string = getBlankGuid();
    let isNetworkSelected = ref(false);
    let networkMetaData = ref<Network>({...emptyNetwork})
    let selectedNetworkName: string;
    let hasCompatibleNetworks: boolean = false;
    const scenarioNameErrors = ref<string[]>([]);

    function initializeNetworkSelection(force: boolean = false) {
        const sourceNetworkId: string = props.dialogData.scenario.networkId;
        const sourceNetworkName: string = props.dialogData.scenario.networkName;
        const sourceNetwork = networkStore.networksGetter.find((network: Network) => network.id === sourceNetworkId);

        const hasCurrentSelection =
            !isNil(selectedNetworkId) &&
            selectedNetworkId !== '' &&
            selectedNetworkId !== getBlankGuid();

        if (!force && hasCurrentSelection) {
            if (!isNil(sourceNetwork) && !isNil(networkMetaData.value) && networkMetaData.value.id === selectedNetworkId) {
                networkMetaData.value = sourceNetwork;
            }
            return;
        }

        selectedNetworkId = sourceNetworkId;
        selectedNetworkName = sourceNetworkName;

        if (!isNil(sourceNetwork)) {
            networkMetaData.value = sourceNetwork;
        }

        isNetworkSelected.value =
            !isNil(sourceNetworkId) &&
            sourceNetworkId !== '' &&
            sourceNetworkId !== getBlankGuid();
    }

    watch(dialogData,()=> {
        onModifyScenarioUserAccess();
    });

    watch(
        () => [props.dialogData.showDialog, props.dialogData.scenario.id],
        ([showDialog]) => {
            if (showDialog) {
                initializeNetworkSelection(true);
            }
        },
    );

    watch(() => networkStore.networksGetter, ()=>  {
        initializeNetworkSelection();
        hasCompatibleNetworks = true;
    });

    watch(shared, ()=> {
        onModifyScenarioUserAccess();
    });    

    function selectedNetwork(networkName: string, networkId: string){
      selectedNetworkId = networkId;
      selectedNetworkName = networkName;
      if(networkId != '' && !isNil(networkId) && networkId != getBlankGuid()){
         isNetworkSelected.value = true;
      }
      else{
         isNetworkSelected.value = false;
      }
    }

    function onModifyScenarioUserAccess() {
        if (props.dialogData.showDialog) {
            const currentUser: User = find(
                propEq('username', getUserName()),
                userStore.getUsers,
            ) as User;
            const owner: ScenarioUser = {
                userId: currentUser.id,
                username: currentUser.username,
                canModify: true,
                isOwner: true,
            };

            newScenario = {
                ...props.dialogData.scenario,
                destinationNetworkId: selectedNetworkId,
                networkId: props.dialogData.scenario.networkId,
                users: shared.value
                    ? [
                          owner,
                          ...userStore.getUsers
                              .filter(
                                  (user: User) =>
                                      user.username !== currentUser.username,
                              )
                              .map((user: User) => ({
                                  userId: user.id,
                                  username: user.username,
                                  canModify: false,
                                  isOwner: false,
                              })),
                      ]
                    : [owner]
            };

            networkStore.getCompatibleNetworks(props.dialogData.scenario.networkId);
        }
    }

    function validateScenarioName() {
        scenarioNameErrors.value = [];

        if (!props.dialogData.scenario.name) {
            scenarioNameErrors.value.push('This field is required');
        }

        const specialCharError = rules.generalRules.valueContainsNoCertainSpecialCharacters(props.dialogData.scenario.name);
        if (specialCharError !== true) {
            scenarioNameErrors.value.push(specialCharError as string);
        }

        if (scenarioStore.getScenarioNames.find((scenarioName: string) => scenarioName == props.dialogData.scenario.name) != undefined)
        {
            scenarioNameErrors.value.push("There is already a scenario with that name")
        }

    }

    const hasValidationErrors = computed(() => scenarioNameErrors.value.length > 0);
    const isSaveButtonDisabled = computed(() => 
        newScenario.name === '' || !isNetworkSelected.value || hasValidationErrors.value
    );
    watch(() => dialogData.value.scenario.name, validateScenarioName);

    function onSubmit(submit: boolean) {
        if (submit) {
            newScenario.destinationNetworkId = selectedNetworkId;
            newScenario.networkId = props.dialogData.scenario.networkId
            newScenario.networkName = selectedNetworkName;      
            newScenario.name = props.dialogData.scenario.name;
            emit('submit', newScenario);
        } else {
            emit('submit', null);
        }

        newScenario = { ...emptyCloneScenario, id: getNewGuid() };
        shared = ref(false);
        hasCompatibleNetworks = false;
    }

</script>
