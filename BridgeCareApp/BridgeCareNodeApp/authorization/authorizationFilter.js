const authorizationConfig = require('./authorizationConfig');
const logger = require('../config/winston');
const axios = require('axios');
const https = require('https');
const auth = require('./../app');

function authorizationFilter(permittedRoles) {
    return async function authenticationHandler(request, response, next) {
        if (auth.securityType == 'pennDOT') {
            accessToken = request.headers.authorization.split(' ')[1];

            requestPath = `${authorizationConfig.issuer}/userinfo?access_token=${accessToken}`;

            userInfoResponse = await axios.get(requestPath, {
                httpsAgent: new https.Agent({
                    rejectUnauthorized: false
                })
            }).then(esecResponse => {
                return esecResponse.data;
            }).catch(error => {
                return {
                    error: response.status(401).json({
                        message: error.response.data.error_description || 'Authentication Failed'
                    })
                };
            });

            if (userInfoResponse.error !== undefined) {
                return userInfoResponse.error;
            }

            roles = userInfoResponse.roles.split('^').map(segment => segment.split(',')[0].split('=')[1]);
            username = userInfoResponse.sub.split(',')[0].split('=')[1];
            if (!Array.isArray(permittedRoles) || permittedRoles.length === 0) {
                request.user = {
                    username,
                    roles
                };
                return next();
            }
            if (permittedRoles.some(permittedRole => roles.some(role => permittedRole === role))) {
                request.user = {
                    username,
                    roles
                };
                return next();
            }

            return response.status(401).json({
                message: 'User is not authorized for this action.'
            });
        }

        if (auth.securityType == 'B2C') {
            // this is to temporary code, Azure AD B2C hasn't been immplemented yet
            username = "pdsystbamsusr02";
            roles = "PD-BAMS-Administrator";
            request.user = {
                username,
                roles
            };
            return next();
        }
    };
}

module.exports = authorizationFilter;