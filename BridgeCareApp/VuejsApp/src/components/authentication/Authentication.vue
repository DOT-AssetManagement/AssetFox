<template>
    <v-container fluid grid-list-xl>
        <v-row>
            <v-col cols="12">
                <v-row justify="center">
                    <v-card>
                        <v-card-title>
                            <h3>Authenticating...</h3>
                        </v-card-title>
                    </v-card>
                </v-row>
            </v-col>
        </v-row>
    </v-container>
</template>

<script setup lang="ts">
    import Vue, { computed, onMounted, ref } from 'vue';
    import { SecurityTypes } from '@/shared/utils/security-types';
    import { useStore } from 'vuex';
import { useRouter } from 'vue-router';
import AuthenticationService from '@/services/authentication.service';
    import { useAuthenticationStore, useNotificationStore, useUserStore } from '@/stores';

    let store = useStore();
    const authenticationStore = useAuthenticationStore();
    const notificationStore = useNotificationStore();
    const userStore = useUserStore();
    let authenticated = computed<boolean>(() => authenticationStore.authenticated);  
    let hasRole= computed<boolean>(() => authenticationStore.hasRole);
    let hasAdminAccess= computed<boolean>(() => authenticationStore.hasAdminAccess);
    let securityType: string = authenticationStore.securityType;

    function setSuccessMessageAction(payload?: any) { store.dispatch('setSuccessMessage');}
    function setErrorMessageAction(payload?: any){ store.dispatch('setErrorMessage');}
    async function getUserTokensAction(payload?: any): Promise<any> {await store.dispatch('getUserTokens', payload);}
    async function getUserInfoAction(payload?: any): Promise<any> {await store.dispatch('getUserInfo');}
    async function getAzureAccountDetailsAction(payload?: any): Promise<any> {await store.dispatch('getAzureAccountDetails');}
    const $router = useRouter();

    onMounted(() => mounted());
    async function mounted() {
        const code: string = $router.currentRoute.value.query.code as string;
        const state: string = $router.currentRoute.value.query.state as string;

        if (securityType === SecurityTypes.esec) {
            // The ESEC login will always redirect the browser to the iam-deploy site.
            // If the state is set, we know the authentication was started by a local client,
            // and so we should send the browser back to that client.
            if (state === 'localhost8080') {
                window.location.href = `http://localhost:8080/Authentication/?code=${code}`;
                return;
            }

            getUserTokensAction({ code: code }).then(async () => {
                const activeStatus = await AuthenticationService.getActiveStatus();
                if(activeStatus.data == true)
                {
                        if (!authenticated.value) {
                        onAuthenticationFailure();
                    } else {
                        getUserInfoAction().then(() => {
                            userStore.getUserCriteriaFilter().then(() => {
                                if (!hasRole.value || (!userStore.getCurrentUserCriteriaFilter.hasAccess && !hasAdminAccess.value)) {
                                    onRoleFailure();
                                } else {
                                    onAuthenticationSuccess();
                                }
                            });
                        });
                    }
                }
                else
                {
                    $router.push('/AccessDenied/');
                }
            });
        }

        //Is this deprecated???
        if (securityType === SecurityTypes.b2c) {
            getAzureAccountDetailsAction();
            if (!authenticated.value) {
                onAuthenticationFailure();
            } else {
                onAuthenticationSuccess();
            }
        }
    }

    function onAuthenticationSuccess() {
        $router.push('/Scenarios/');
    }

    function onAuthenticationFailure() {
        notificationStore.addErrorNotification({ message: 'Authentication failed.' });
        $router.push('/AuthenticationFailure/');
    }

    function onRoleFailure() {
        $router.push('/NoRole/');
    }
</script>
