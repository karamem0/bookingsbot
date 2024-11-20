//
// Copyright (c) 2021-2024 karamem0
//
// This software is released under the MIT License.
//
// https://github.com/karamem0/bookings-bot/blob/main/LICENSE
//

import React from 'react';
import ReactDOM from 'react-dom/client';

import * as ress from 'ress';
import { AuthenticatedTemplate, UnauthenticatedTemplate } from '@azure/msal-react';
import { Global } from '@emotion/react';
import HomePage from './pages/HomePage';
import IntlProvider from './providers/IntlProvider';
import MainPage from './pages/MainPage';
import MsalAdapter from './components/MsalAdapter';
import MsalProvider from './providers/MsalProvider';
import ThemeProvider from './providers/ThemeProvider';

ReactDOM
  .createRoot(document.getElementById('root') as Element)
  .render(
    <React.Fragment>
      <Global styles={ress} />
      <ThemeProvider>
        <IntlProvider>
          <MsalProvider>
            <MsalAdapter>
              <AuthenticatedTemplate>
                <MainPage />
              </AuthenticatedTemplate>
              <UnauthenticatedTemplate>
                <HomePage />
              </UnauthenticatedTemplate>
            </MsalAdapter>
          </MsalProvider>
        </IntlProvider>
      </ThemeProvider>
    </React.Fragment>
  );
