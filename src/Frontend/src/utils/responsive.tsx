import React from 'react';
import Responsive from 'react-responsive';

/* See also variables.less for values */

type propType = unknown & { children?: React.ReactNode };

export const Desktop = (props: propType): React.ReactElement => <Responsive { ...props } minWidth={ 1281 }/>;
export const Tablet = (props: propType): React.ReactElement => <Responsive { ...props } minWidth={ 801 }
																		   maxWidth={ 1280 }/>;
export const Mobile = (props: propType): React.ReactElement => <Responsive { ...props } maxWidth={ 800 }/>;
export const NotMobile = (props: propType): React.ReactElement => <Responsive { ...props } minWidth={ 801 }/>;

