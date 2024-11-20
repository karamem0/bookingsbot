//
// Copyright (c) 2021-2024 karamem0
//
// This software is released under the MIT License.
//
// https://github.com/karamem0/bookings-bot/blob/main/LICENSE
//

import React from 'react';

import { FormattedMessage, useIntl } from 'react-intl';
import {
  Link,
  Spinner,
  Text
} from '@fluentui/react-components';
import ReactWebChat, { createDirectLine } from 'botframework-webchat';
import { EventHandler } from '../types/Event';
import { FluentThemeProvider } from 'botframework-webchat-fluent-theme';
import { css } from '@emotion/react';
import messages from '../messages';
import { useTheme } from '../providers/ThemeProvider';

interface MainPageProps {
  loading?: boolean,
  token?: string,
  onSingOut?: EventHandler
}

function MainPage(props: Readonly<MainPageProps>) {

  const {
    loading,
    token,
    onSingOut
  } = props;

  const intl = useIntl();
  const { theme } = useTheme();

  const directLine = React.useMemo(() => createDirectLine({
    token: token
  }), [
    token
  ]);

  return (
    <FluentThemeProvider>
      {
        loading ? (
          <div
            css={css`
            display: grid;
            min-width: 100%;
            min-height: 100svh;
          `}>
            <Spinner />
          </div>
        ) : (
          <div
            css={css`
              display: flex;
              flex-flow: column;
              min-width: 100%;
              min-height: 100svh;
            `}>
            <header
              css={css`
                display: flex;
                flex-flow: row;
                justify-content: space-between;
                align-items: center;
                min-height: 2.5rem;
                padding: 0 1rem;
                background-color: ${theme.colorNeutralBackgroundInverted};
              `}>
              <Text
                as="h1"
                css={css`
                  font-size: ${theme.fontSizeBase400};
                  font-weight: bold;
                  line-height: calc(${theme.fontSizeBase400} * 1.25);
                  color: ${theme.colorNeutralForegroundInverted};
                `}>
                <FormattedMessage {...messages.AppName} />
              </Text>
              <Link
                as="button"
                css={css`
                  color: ${theme.colorNeutralForegroundInverted};
                `}
                onClick={onSingOut}>
                <FormattedMessage {...messages.SignOut} />
              </Link>
            </header>
            <main
              css={css`
                height: calc(100svh - 2.5rem);
                padding: 1rem;
                background: linear-gradient(${theme.colorBrandBackground2Hover}, ${theme.colorBrandBackground2});
              `}>
              <ReactWebChat
                directLine={directLine}
                locale={intl.locale}
                styleOptions={{
                  hideUploadButton: true,
                  sendBoxButtonShadeBorderRadius: 8
                }} />
            </main>
          </div>
        )
      }
    </FluentThemeProvider>
  );

}

export default React.memo(MainPage);
