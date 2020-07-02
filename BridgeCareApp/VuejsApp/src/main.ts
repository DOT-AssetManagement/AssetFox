import '@babel/polyfill';
import '@fortawesome/fontawesome-free/css/all.css';
import Vue from 'vue';
import 'vuetify/dist/vuetify.min.css';
import 'bootstrap';
import 'bootstrap/dist/css/bootstrap.min.css';
import Vuetify from 'vuetify';
import App from './App.vue';
import router from './router';
import store from './store/root-store';
import './assets/css/main.css';
import 'izitoast/dist/css/iziToast.min.css';
import 'izitoast/dist/js/iziToast.min';
// @ts-ignore
import VueWorker from 'vue-worker';
import '@progress/kendo-ui';
import '@progress/kendo-theme-default/dist/all.css';
import { KendoChartInstaller } from '@progress/kendo-charts-vue-wrapper';
import VueCurrencyInput from 'vue-currency-input';
import msal from 'vue-msal';
// import config from '@/config/azure-b2c-config';
// import AuthService from '@/msal/index'

Vue.use(Vuetify, {
    iconfont: 'fa',
});
// Vue.use(msal, {
//     auth: {
//         clientId: config.clientId,
//         authority: config.authority,
//         redirectUri: config.redirectUri,
//         postLogoutRedirectUri: config.postLogoutRedirectUri,
//         cacheLocation: 'localStorage',
//         validateAuthority: config.validateAuthority,
//         scopes: [config.clientId],
//     },
//     request: {
//         scopes: ['https://graph.microsoft.com/.default']
//     },
//     router: router,
//     requireAuthOnInitialize: true,
//     graph: {
//         callAfterInit: false,
//     },
// });

Vue.use(VueWorker);

Vue.use(KendoChartInstaller);

Vue.use(VueCurrencyInput);

//Vue.prototype.$AuthService = new AuthService()

Vue.config.productionTip = false;

new Vue({
    store,
    router,
    render: h => h(App),
}).$mount('#app');
