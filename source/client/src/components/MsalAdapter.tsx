//
// Copyright (c) 2021-2024 karamem0
//
// This software is released under the MIT License.
//
// https://github.com/karamem0/bookings-bot/blob/main/LICENSE
//

import React from 'react';

import { InteractionStatus } from '@azure/msal-browser';
import Presenter from './MsalAdapter.presenter';
import { useMsal } from '@azure/msal-react';

function MsalAdapter(props: Readonly<React.PropsWithChildren<unknown>>) {

  const { children } = props;

  const msal = useMsal();

  return (
    <Presenter loading={msal.inProgress !== InteractionStatus.None}>
      {children}
    </Presenter>
  );

}

export default MsalAdapter;
