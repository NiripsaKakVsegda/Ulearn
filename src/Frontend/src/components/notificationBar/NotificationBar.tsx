import { Delete } from "@skbkontur/react-icons";
import React from "react";
import { ReactCookieProps, withCookies } from "react-cookie";

import api from "src/api";

import { NotificationBarResponse } from "src/models/notifications";

import styles from './NotificationBar.less';

const notificationBarCookieName = 'ulearn.notificationBar';

function NotificationBar({ cookies }: ReactCookieProps): React.ReactElement<ReactCookieProps> | null {
	if(!cookies) {
		return null;
	}

	const notificationBarState: NotificationBarResponse = { message: undefined, force: false };
	const [state, setState] = React.useState(notificationBarState);

	if(state.message === undefined) {
		api.notifications.getGlobalNotification()
			.then(r => setState(r))
			.catch(() => setState({ message: null, force: false }));
	}

	if(!state.message || (cookies.get(notificationBarCookieName) && !state.force)) {
		return null;
	}

	return (
		<div className={ styles.wrapper }>
			{ state.message }
			{ !state.force && <Delete className={ styles.closeButton } onClick={ closeForThisDay }/> }
		</div>
	);

	function closeForThisDay() {
		if(!cookies) {
			return;
		}

		const date = new Date();
		date.setDate(date.getDate() + 1);
		cookies.set(notificationBarCookieName, true, { expires: date, domain: location.origin });
	}
}

export default withCookies(NotificationBar);
