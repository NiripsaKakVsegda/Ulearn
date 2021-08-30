import React from "react";

import { NotificationBell } from "icons";

import styles from '../Header.less';

interface Props {
	counter: number,
}

function NotificationsIcon({ counter }: Props): React.ReactElement {
	return (
		<>
			<NotificationBell size={ 20 }/>
			{
				counter > 0 &&
				<span className={ styles.notificationsCounter }>
                        { counter > 99 ? "99+" : counter }
				</span>
			}
		</>
	);
}

export default NotificationsIcon;
