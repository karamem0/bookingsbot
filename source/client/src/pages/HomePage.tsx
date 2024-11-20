//
// Copyright (c) 2021-2024 karamem0
//
// This software is released under the MIT License.
//
// https://github.com/karamem0/bookings-bot/blob/main/LICENSE
//

import React from 'react';

import { Event } from '../types/Event';
import Presenter from './HomePage.presenter';
import { loginParams } from '../config/MsalConfig';
import messages from '../messages';
import { useIntl } from 'react-intl';
import { useMsal } from '@azure/msal-react';

function HomePage() {

  const intl = useIntl();
  const msal = useMsal();

  const handleLinkClick = React.useCallback((_: Event, data?: string) => {
    switch (data) {
      case 'GitHub':
        window.open(intl.formatMessage(messages.GitHubLink), '_blank');
        break;
      case 'PrivacyPolicy':
        window.open(intl.formatMessage(messages.PrivacyPolicyLink), '_blank');
        break;
      case 'TermsOfUse':
        window.open(intl.formatMessage(messages.TermsOfUseLink), '_blank');
        break;
      default:
        break;
    }
  }, []);

  const handleSignIn = React.useCallback(async () => {
    await msal.instance.loginRedirect(loginParams);
  }, [
    msal
  ]);

  return (
    <Presenter
      onLinkClick={handleLinkClick}
      onSignIn={handleSignIn} />
  );

}

export default HomePage;
