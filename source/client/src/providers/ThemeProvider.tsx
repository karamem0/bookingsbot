//
// Copyright (c) 2021-2024 karamem0
//
// This software is released under the MIT License.
//
// https://github.com/karamem0/bookings-bot/blob/main/LICENSE
//

import React from 'react';

import {
  FluentProvider,
  Theme,
  webLightTheme
} from '@fluentui/react-components';

interface ThemeContextState {
  theme: Theme
}

const ThemeContext = React.createContext<ThemeContextState | undefined>(undefined);

export const useTheme = (): ThemeContextState => {
  const value = React.useContext(ThemeContext);
  if (value == null) {
    throw new Error();
  }
  return value;
};

function ThemeProvider(props: Readonly<React.PropsWithChildren<unknown>>) {

  const { children } = props;

  const value = React.useMemo<ThemeContextState>(() => ({
    theme: webLightTheme
  }), []);

  return (
    <ThemeContext.Provider value={value}>
      <FluentProvider theme={value.theme}>
        {children}
      </FluentProvider>
    </ThemeContext.Provider>
  );

}

export default ThemeProvider;

