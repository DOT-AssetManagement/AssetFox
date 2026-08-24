<template>
    <v-container fluid grid-list-xl>
        <v-row>
            <v-col cols="12">
                <v-row justify="center">
                    <v-card>
                        <v-card-title>
                            <h3>Beginning Authentication</h3>
                        </v-card-title>
                        <v-card-text v-if="securityType === esecSecurityType">
                            You should be redirected to the PennDOT login page shortly. If you are not redirected within
                            5 seconds, press the button below.
                        </v-card-text>
                        <v-btn id="AuthenticationStart-goToLoginPage-btn"
                               v-if="securityType === esecSecurityType" 
                               @click="onRedirect" class="v-btn theme--light assetFox-blue-bg text-white">
                            Go to login page
                        </v-btn>
                        <v-card-text v-if="securityType === b2cSecurityType || securityType === entraSecurityType">
                            Please click 'Login' if the login pop-up does not show within ~5 seconds.
                        </v-card-text>
                    </v-card>
                </v-row>
            </v-col>
        </v-row>
    </v-container>
</template>

<script setup lang="ts">
    import Vue, { inject, onMounted } from 'vue';
    import { SecurityTypes, isAzureSecurityType } from '@/shared/utils/security-types';
    import { hasValue } from '@/shared/utils/has-value-util';
    import { isAuthenticatedUser,
    } from '@/shared/utils/authentication-utils';
import { useRouter } from 'vue-router';
    import { useAuthenticationStore, useAzureB2CStore, useConfigStore } from '@/stores';

    const authenticationStore = useAuthenticationStore();
    const azureB2CStore = useAzureB2CStore();
    const configStore = useConfigStore();
    let authenticated: boolean = authenticationStore.authenticated;
    let hasRole: boolean = authenticationStore.hasRole ;
    let checkedForRole: boolean = authenticationStore.checkedForRole ;
    let securityType: string = authenticationStore.securityType ;
    const config = configStore.getConfig;

    let esecSecurityType: string = SecurityTypes.esec;
    let b2cSecurityType: string = SecurityTypes.b2c;
    let entraSecurityType: string = SecurityTypes.entra;

    const $router = useRouter();

    onMounted(() => mounted())
    function mounted() {
        const hasAuthInfo: boolean =
            securityType === SecurityTypes.esec
                ? hasValue(localStorage.getItem('UserTokens'))
                : hasValue(localStorage.getItem('LoggedInUser'));
        if (!authenticated && hasAuthInfo) {
            isAuthenticatedUser()
                .then((isAuthenticated: boolean | void) => {
                    if (isAuthenticated) {
                        $router.push('/Scenarios/')
                    }
                });
        } else if (authenticated && hasAuthInfo) {
            $router.push('/Scenarios/');
        } else {
            if (securityType === SecurityTypes.esec) {
                onRedirect();
            } else if (isAzureSecurityType(securityType)) {
                azureB2CStore.azureB2CLogin();
            }
        }
    }

    function onRedirect() { 
        if (!config) {
            return;
        }

        let href: string = `${config.authorizationEndpoint}?response_type=code&scope=openid&scope=${config.esecScope}`;
        href += `&client_id=${config.clientId}`;
        href += `&redirect_uri=${config.redirectUri}`;

        // The 'state' query parameter that is sent to ESEC will be sent back to
        // the /Authentication page of the iam-deploy app.
        if (import.meta.env.VITE_APP_IS_PRODUCTION !== 'true') {
            href += '&state=localhost' + "8080";
        }

        window.location.href = href;
    }
</script>
