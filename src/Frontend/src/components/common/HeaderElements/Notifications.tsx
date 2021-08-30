import React from "react";

import { Loader } from "@skbkontur/react-ui/index";

import styles from '../Header.less';

interface Props {
	isLoading: boolean;
	notifications: string;
}

function Notifications({ isLoading, notifications, }: Props): React.ReactElement {
	if(isLoading) {
		return (
			<div className={ styles.notificationsDropdown }>
				<Loader type="normal" active={ true }/>
			</div>
		);
	}

	return <div className={ styles.notificationsDropdown }
				dangerouslySetInnerHTML={ { __html: notifications } }/>;
}

export default Notifications;
