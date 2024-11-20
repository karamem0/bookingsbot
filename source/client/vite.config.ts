//
// Copyright (c) 2021-2024 karamem0
//
// This software is released under the MIT License.
//
// https://github.com/karamem0/bookings-bot/blob/main/LICENSE
//

import fs from 'fs';

import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
  'build': {
    'outDir': 'dist',
    'sourcemap': true
  },
  'plugins': [
    react({
      'babel': {
        'plugins': [
          '@emotion',
          [
            'formatjs',
            {
              ast: true,
              idInterpolationPattern: '[sha512:contenthash:base64:6]'
            }
          ]
        ]
      },
      'jsxImportSource': '@emotion/react'
    })
  ],
  'server': {
    'https': {
      'cert': fs.readFileSync('./cert/localhost.crt'),
      'key': fs.readFileSync('./cert/localhost.key')
    },
    'proxy': {
      '/api': {
        'changeOrigin': true,
        'secure': false,
        'target': 'http://localhost:3978'
      }
    }
  }
});
