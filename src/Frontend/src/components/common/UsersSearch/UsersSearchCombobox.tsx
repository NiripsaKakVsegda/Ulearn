import React, { FC, useMemo, useRef } from 'react';
import { ComboBox, MenuItemState, ThemeContext } from "ui";
import texts from "./UsersSearch.texts";
import { roundInputs } from "../../../uiTheme";
import { getStudentId, getUserName, renderUserItem, renderUserValue } from "./common";
import { ShortUserInfo } from "../../../models/users";
import cn from "classnames";
import styles from "./usersSearch.less";

interface Props {
	searchUsers: (query: string) => Promise<ShortUserInfo[]>;
	user?: ShortUserInfo;
	onSelectUser: (user?: ShortUserInfo) => void;

	className?: string;

	clearInputAfterSelect?: boolean;

	width?: number | string;
	size?: 'small' | 'medium' | 'large';
	error?: boolean;
	disabled?: boolean;

	placeholder?: string;
	notFoundMessage?: string;
}

const UsersSearchCombobox: FC<Props> = (props) => {
	const comboboxRef = useRef<ComboBox<ShortUserInfo>>(null);

	const inputWidth = useMemo(() => {
		if(window.innerWidth < 577) {
			return undefined; // Combobox превращается в модалку.
		}
		const rootNode = (comboboxRef.current as unknown as Node)?.getRootNode() as HTMLElement;
		return rootNode?.offsetWidth || undefined;
	}, [comboboxRef.current, window.innerWidth]);

	const renderItem = (user: ShortUserInfo, state: MenuItemState) =>
		renderUserItem(user, state, inputWidth);

	const renderNotFound = () => props.notFoundMessage ?? texts.noUsersFound;

	return <ThemeContext.Provider value={ roundInputs }>
		<ComboBox<ShortUserInfo>
			className={ cn(styles.input, props.className) }
			getItems={ props.searchUsers }
			value={ props.user }
			onValueChange={ selectUser }
			onInputValueChange={ resetUser }
			itemToValue={ getStudentId }
			valueToString={ getUserName }
			renderNotFound={ renderNotFound }
			renderItem={ renderItem }
			renderValue={ renderUserValue }
			placeholder={ props.placeholder ?? texts.placeholder }
			width={ props.width }
			size={ props.size }
			error={ props.error }
			disabled={ props.disabled }
			ref={ comboboxRef }
		/>
	</ThemeContext.Provider>;

	function selectUser(user: ShortUserInfo) {
		props.onSelectUser(user);
		if(props.clearInputAfterSelect) {
			comboboxRef.current?.reset();
		}
		comboboxRef.current?.blur();
	}

	function resetUser() {
		if(props.user) {
			props.onSelectUser(undefined);
		}
	}
};

export default UsersSearchCombobox;
