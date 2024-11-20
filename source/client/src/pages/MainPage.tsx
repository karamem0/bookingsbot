//
// Copyright (c) 2021-2024 karamem0
//
// This software is released under the MIT License.
//
// https://github.com/karamem0/bookings-bot/blob/main/LICENSE
//

import React from 'react';

import Presenter from './MainPage.presenter';
import axios from 'axios';
import { loginParams } from '../config/MsalConfig';
import { useAsyncFn } from 'react-use';
import { useMsal } from '@azure/msal-react';

function MainPage() {

  const msal = useMsal();

  const [ token, setToken ] = React.useState<string | undefined>(undefined);

  const [ , fetch ] = useAsyncFn(async (token: string) => {
    return await Promise.resolve()
      .then(() => axios.post(
        '/api/token',
        undefined,
        {
          headers: {
            'Authorization': `Bearer ${token}`
          }
        }
      ))
      .then((value) => value.data.token as string | undefined);
  });

  const handleSignOut = React.useCallback(async () => {
    await msal.instance.logoutRedirect({
      account: msal.instance.getActiveAccount()
    });
  }, [
    msal
  ]);

  React.useEffect(() => {
    (async () => {
      if (msal.accounts.length > 0) {
        msal.instance.setActiveAccount(msal.accounts[0]);
        const account = msal.instance.getActiveAccount();
        if (account != null) {
          const auth = await msal.instance.acquireTokenSilent(loginParams);
          const token = await fetch(auth.accessToken);
          setToken(token);
        }
      }
    })();
  }, [
    msal,
    fetch
  ]);

  return (
    <Presenter
      loading={token == null}
      token={token}
      onSingOut={handleSignOut} />
  );

}

export default MainPage;
