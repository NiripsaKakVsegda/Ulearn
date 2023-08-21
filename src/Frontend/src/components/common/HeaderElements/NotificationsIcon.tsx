import { NotificationBellIcon20Solid } from '@skbkontur/icons/NotificationBellIcon20Solid';
import React from "react";

import styles from '../Header.less';

interface Props {
	counter: number,
}

function NotificationsIcon({ counter }: Props): React.ReactElement {
	return (
		<>
			<NotificationBellIcon20Solid size={ 20 }/>
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
