//
// Copyright (c) 2021-2024 karamem0
//
// This software is released under the MIT License.
//
// https://github.com/karamem0/bookings-bot/blob/main/LICENSE
//

import { BrowserCacheLocation } from '@azure/msal-browser';
import env from '../env';

export const msalConfig = {
  auth: {
    authority: env.VITE_MSAL_AUTHORITY,
    clientId: env.VITE_MSAL_CLIENT_ID,
    redirectUri: `${window.location.origin}`
  },
  cache: {
    cacheLocation: BrowserCacheLocation.SessionStorage,
    storeAuthStateInCookie: false
  }
};

export const loginParams = {
  scopes: [
    `${env.VITE_MSAL_SERVER_ID}/user_impersonation`
  ]
};
