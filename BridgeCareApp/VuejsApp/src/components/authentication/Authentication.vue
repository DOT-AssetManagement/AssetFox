<template>
    <v-container fluid grid-list-xl>
        <v-layout>
            <v-flex xs12>
                <v-layout justify-center>
                    <v-card>
                        <v-card-title>
                            <h3>Authenticating...</h3>
                        </v-card-title>
                    </v-card>
                </v-layout>
            </v-flex>
        </v-layout>
    </v-container>
</template>

<script lang="ts">
import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import { Action, State } from 'vuex-class';

@Component
export default class Authentication extends Vue {
    @State(state => state.authentication.authenticated) authenticated: boolean;
    @State(state => state.authentication.hasRole) hasRole: boolean;
    @State(state => state.authentication.securityType) securityType: any;

    @Action('setSuccessMessage') setSuccessMessageAction: any;
    @Action('setErrorMessage') setErrorMessageAction: any;
    @Action('getUserTokens') getUserTokensAction: any;
    @Action('getUserInfo') getUserInfoAction: any;
    @Action('getNetworks') getNetworksAction: any;
    @Action('getAttributes') getAttributesAction: any;
    @Action('getAzureAccountDetails') getAzureAccountDetailsAction: any;

    mounted() {
        const code: string = this.$route.query.code as string;
        const state: string = this.$route.query.state as string;

        // The ESEC login will always redirect the browser to the iam-deploy site.
        // If the state is set, we know the authentication was started by a local client,
        // and so we should send the browser back to that client.

        if (this.securityType == 'pennDOT') {
            if (state === 'localhost8080') {
                window.location.href = `http://localhost:8080/Authentication/?code=${code}`;
                return;
            }
            this.getUserTokensAction(code).then(() => {
                if (!this.authenticated) {
                    this.onAuthenticationFailure();
                } else {
                    this.getUserInfoAction().then(() => {
                        if (!this.hasRole) {
                            this.onRoleFailure();
                        } else {
                            this.onAuthenticationSuccess();
                        }
                    });
                }
            });
        }
        if (this.securityType == 'B2C') {
            //var status: any = this.msal.isAuthenticated;
            this.getAzureAccountDetailsAction();
            if(!this.authenticated){
                this.onAuthenticationFailure();
            }
            else{
                this.onAuthenticationSuccess();
                this.$router.push('/Home');
            }
            var status: any = true;
            // this.getAuthenticationAzureAction({ status: status }).then(
            //     () => {
            //         if (status == true) {
            //             var userData = this.$AuthService.getUser();
            //             this.setAzureUserNameAction({
            //                 userName: userData.name,
            //             });
            //             this.onAuthenticationSuccess();
            //         }
            //     },
            // );

            // if (this.msal.isAuthenticated) {
            //     var userData: any = this.msal.user; // user data contains idToken Object idTokenClaims object
            //     this.setAzureUserNameAction({ userName: userData.name });
            //     this.onAuthenticationSuccess();
            // } else {
            //     this.onAuthenticationFailure();
            // }
            // if (this.msal.graph && this.msal.graph.profile) {
            //     var profile = this.msal.graph.profile;
            // }
        }
    }

    onAuthenticationSuccess() {
        this.setSuccessMessageAction({ message: 'Authentication successful.' });
        this.$router.push('/Home/');
    }

    onAuthenticationFailure() {
        this.setErrorMessageAction({ message: 'Authentication failed.' });
        this.$router.push('/AuthenticationFailure/');
    }

    onRoleFailure() {
        this.$router.push('/NoRole/');
    }
}
</script>
