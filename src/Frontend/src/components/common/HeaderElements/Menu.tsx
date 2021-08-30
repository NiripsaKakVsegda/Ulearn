import React from "react";
import { Link, RouteComponentProps } from "react-router-dom";
import cn from "classnames";

import NotificationsMenu from "./NotificationsMenu";
import ProfileLink from "./ProfileLink";
import LogoutLink from "./LogoutLink";
import Separator from "./Separator";

import { getQueryStringParameter } from "src/utils";

import { AccountState } from "src/redux/account";
import { DeviceType } from "src/consts/deviceType";
import {
	constructLinkWithReturnUrl,
	externalLoginCallback,
	externalLoginConfirmation,
	login,
	register
} from "src/consts/routes";

import styles from '../Header.less';


interface Props extends RouteComponentProps {
	account: AccountState;
	deviceType: DeviceType;
}

function Menu({ account, location, history, deviceType, match, }: Props): React.ReactElement {
	let returnUrl: string | null = (location.pathname + location.search).toLowerCase();

	if(returnUrl.startsWith("/" + login.toLowerCase())
		|| returnUrl.startsWith("/" + register.toLowerCase())
		|| returnUrl.startsWith("/" + externalLoginConfirmation.toLowerCase())
		|| returnUrl.startsWith("/" + externalLoginCallback.toLowerCase())) {
		returnUrl = getQueryStringParameter("returnUrl") || '/';
	}

	if(account.isAuthenticated) {
		return (
			<div className={ styles.accountWrapper }>
				<NotificationsMenu location={ location } match={ match } deviceType={ deviceType } history={ history }/>
				<ProfileLink
					className={ cn(styles.headerElement, styles.button, styles.profileLink) }
					deviceType={ deviceType }
					account={ account }
				/>
				<Separator/>
				<LogoutLink className={ cn(styles.headerElement, styles.button) }/>
			</div>
		);
	} else {
		return (
			<div className={ styles.accountWrapper }>
				<Link
					className={ cn(styles.headerElement, styles.button) }
					to={ constructLinkWithReturnUrl(register, returnUrl) }>
					Зарегистрироваться
				</Link>
				<Separator/>
				<Link
					className={ cn(styles.headerElement, styles.button) }
					to={ constructLinkWithReturnUrl(login, returnUrl) }>
					Войти
				</Link>
			</div>
		);
	}
}

export default Menu;
