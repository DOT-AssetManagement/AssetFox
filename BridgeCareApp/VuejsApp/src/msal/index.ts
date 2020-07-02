import * as Msal from 'msal';
import {msalConfig, azureB2CConfig} from '../config/azure-b2c-config';

export default class AuthService {
  
  app: any;
  config: any;
  constructor() {
    // this.applicationConfig = {
    //   clientID: config.clientid,
    //   authority: config.authority
    // }
        this.config = {
          auth:{
            clientId: azureB2CConfig.clientId,
            authority: azureB2CConfig.authority,
            redirectUri: azureB2CConfig.redirectUri,
            postLogoutRedirectUri: azureB2CConfig.postLogoutRedirectUri,
            // cacheLocation: 'localStorage',
            validateAuthority: azureB2CConfig.validateAuthority,
            // scopes: [config.clientId],
          }
    };
    const loginRequest = {
      scopes: ['User.ReadWrite']
  };
    this.app = new Msal.UserAgentApplication(
      this.config);
  }

  login() {
    this.app.loginPopup().then(
      (token: any) => {
        console.log('JWT token ' + token);
      },
      (error: any) => {
        console.log('Login error ' + error);
        
        //error handling
        if(error.errorMessage){
          // Check for forgot password error
          if (error.errorMessage.indexOf('AADB2C90118') > -1){
            this.app.loginPopup(azureB2CConfig.forgetPasswordAuthprity)
            .then((loginResponse: any) => {
              console.log(loginResponse);
              window.alert('Password has been reset successfully. \nPlease sign-in with your new password.');
            });
          }
        }
      }
    );
  }

  logout() {
    this.app._user = null;
    this.app.logout();
  }

  getUser() {
    return this.app.getUser();
  }
}