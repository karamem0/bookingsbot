//
// Copyright (c) 2021-2024 karamem0
//
// This software is released under the MIT License.
//
// https://github.com/karamem0/bookings-bot/blob/main/LICENSE
//

/// <reference types="vite/client" />

declare module 'ress';

interface ImportMetaEnv {
  readonly VITE_MSAL_AUTHORITY: string,
  readonly VITE_MSAL_CLIENT_ID: string,
  readonly VITE_MSAL_SERVER_ID: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}
