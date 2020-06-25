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
import {KendoChartInstaller} from '@progress/kendo-charts-vue-wrapper';
import VueCurrencyInput from 'vue-currency-input';
import msal from 'vue-msal';
import config from '@/config/azure-b2c-config';

Vue.use(Vuetify, {
    iconfont: 'fa'
});
Vue.use(msal, {
    auth: {
        clientId: config.clientId,
        tenantId: config.tenantId,
        redirectUri: config.redirectUri
    },
    router: router,
    requireAuthOnInitialize: true,
    graph:{
        callAfterInit: true
    }
});

Vue.use(VueWorker);

Vue.use(KendoChartInstaller);

Vue.use(VueCurrencyInput);

Vue.config.productionTip = false;

new Vue({
    store,
    router,
    render: h => h(App)
}).$mount('#app');
