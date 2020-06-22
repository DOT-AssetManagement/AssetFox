import {UserInfo} from '@/shared/models/iAM/authentication';
import {parseLDAP} from './parse-ldap';

export const getUserInfo = () => {
    return JSON.parse(localStorage.getItem('UserInfo') as string) as UserInfo;
};

export const getUserName = () => {
   // it is temporary arrangement to bypass PennDOT security
    return 'pdsystbamsusr02';
    //return parseLDAP(getUserInfo().sub)[0];
};

export const getUserRoles = () => {
    // it is temporary arrangement to bypass PennDOT security
    return 'PD-BAMS-Administrator';
    
    //return parseLDAP(getUserInfo().roles);
};