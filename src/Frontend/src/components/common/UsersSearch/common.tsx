import { MenuItemState } from "ui";
import cn from "classnames";
import styles from "./usersSearch.less";
import React from "react";
import { ShortUserInfo } from "../../../models/users";
import { getNameWithLastNameFirst } from "../Profile/Profile";
import Avatar from "../Avatar/Avatar";

export function renderUserItem(user: ShortUserInfo, state: MenuItemState, inputWidth?: number): React.ReactNode {
	const width = inputWidth && inputWidth > 24
		? inputWidth - 24
		: 'initial';

	return <span className={ styles.userItem } style={ { width } }>
			<span className={ styles.user }>
				<Avatar user={ user } size={ 'small' }/>
				<span className={ styles.name }>{ getNameWithLastNameFirst(user) }</span>
			</span>
			<span className={ cn(
				styles.additionalInfo,
				{ [styles.additionalInfoHover]: state === "hover" }
			) }>
				{ user.login }
			</span>
		</span>;
}

export function renderUserValue(user: ShortUserInfo, selected = false) {
	return <span className={ styles.userItem }>
		<span className={ styles.name }>{ getNameWithLastNameFirst(user) }</span>
		<span
			className={ cn(
				styles.additionalInfo,
				{ [styles.additionalInfoHover]: selected }
			) }
		>
			{ user.login }
		</span>
	</span>;
}

export function getStudentId(user: ShortUserInfo) {
	return user.id;
}

export function getUserName(user: ShortUserInfo) {
	return getNameWithLastNameFirst(user);
}
