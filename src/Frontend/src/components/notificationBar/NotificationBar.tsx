import { Delete } from "@skbkontur/react-icons";
import React from "react";
import { ReactCookieProps, withCookies } from "react-cookie";

import api from "src/api";

import { NotificationBarResponse } from "src/models/notifications";

import styles from './NotificationBar.less';
import { AccountState } from "../../redux/account";

const notificationBarCookieName = 'ulearn.notificationBar';

function NotificationBar({
	isSystemAdministrator,
	cookies
}: ReactCookieProps & { isSystemAdministrator: boolean; }): React.ReactElement<ReactCookieProps> | null {
	if(!cookies) {
		return null;
	}

	const notificationBarState: NotificationBarResponse = {
		message: undefined,
		force: false
	};
	const [state, setState] = React.useState(notificationBarState);
	const [lastUpdateTime, setTime] = React.useState(new Date(0));

	if(state.message === undefined && lastUpdateTime <= new Date()) {
		setTime(new Date(new Date().getTime() + 5 * 60_000));
		api.notifications.getGlobalNotification()
			.then(r => setState(r))
			.catch(() => setState({ message: null, force: false }));
	}
	if(isSystemAdministrator) {
		if(!state.message) {
			return null;
		}

		return (
			<div className={ styles.wrapper }>
				Пользователи видят сообщение: "{ state.message }" { state.overlap && 'на весь экран' }
				{ !state.overlap && `и ${ state.force ? 'не' : '' } могут его закрыть` }
			</div>
		);
	}

	if(!state.message || (cookies.get(notificationBarCookieName) && !state.force)) {
		return null;
	}

	return (
		<div className={ state.overlap ? styles.overlap : styles.wrapper }>
			{ state.message }
			{ (!state.force && !state.overlap) &&
				<Delete className={ styles.closeButton } onClick={ closeForThisDay }/> }
		</div>
	);

	function closeForThisDay() {
		if(!cookies) {
			return;
		}
		const date = new Date();
		date.setDate(date.getDate() + 1);
		cookies.set(notificationBarCookieName, true, { expires: date, domain: location.hostname });
	}
}

export default withCookies(NotificationBar);
