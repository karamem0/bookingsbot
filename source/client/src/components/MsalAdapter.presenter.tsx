//
// Copyright (c) 2021-2024 karamem0
//
// This software is released under the MIT License.
//
// https://github.com/karamem0/bookings-bot/blob/main/LICENSE
//

import React from 'react';

interface MsalAdapterProps {
  loading?: boolean
}

function MsalAdapter(props: Readonly<React.PropsWithChildren<MsalAdapterProps>>) {

  const {
    children,
    loading
  } = props;

  return loading ? null : children;

}

export default React.memo(MsalAdapter);
