import React, { FC, useMemo, useRef } from 'react';
import { MenuItemState, ThemeContext, Token, TokenInput, TokenProps } from "ui";
import { getStudentId, getUserName, renderUserItem, renderUserValue } from "./common";
import texts from "./UsersSearch.texts";
import styles from "./usersSearch.less";
import { roundInputs } from "../../../uiTheme";
import { ShortUserInfo } from "../../../models/users";
import { getNameWithLastNameFirst } from "../Profile/Profile";

interface Props {
	searchUsers: (query: string) => Promise<ShortUserInfo[]>;
	users: ShortUserInfo[];
	onChangeUsers: (users: ShortUserInfo[]) => void;

	width?: number | string;
	error?: boolean;
	disabled?: boolean;

	placeholder?: string;
	notFoundMessage?: string;
}

const UsersSearchTokenInput: FC<Props> = (props) => {
	const tokenInputRef = useRef<TokenInput<ShortUserInfo>>(null);

	const inputWidth = useMemo(() => {
		const rootNode = (tokenInputRef.current as unknown as Node)?.getRootNode() as HTMLElement;
		return rootNode?.getElementsByTagName('label').item(0)?.offsetWidth || undefined;
	}, [tokenInputRef.current, window.innerWidth]);

	const renderItem = (user: ShortUserInfo, state: MenuItemState) =>
		renderUserItem(user, state, inputWidth);

	const renderUserToken = (user: ShortUserInfo, tokenProps: Partial<TokenProps>) => {
		return <Token
			{ ...tokenProps }
			key={ user.id }
			data-id={ user.id }
			onRemove={ handleRemoveUserClick }
			onDoubleClick={ preventDoubleClickDefault }
		>
			{ tokenProps.isActive
				? renderUserValue(user, true)
				: getNameWithLastNameFirst(user)
			}
		</Token>;
	};

	const renderNotFound = () => props.notFoundMessage ?? texts.noUsersFound;

	return <ThemeContext.Provider value={ roundInputs }>
		<TokenInput<ShortUserInfo>
			className={ styles.tokenInput }
			getItems={ getFilteredUsers }
			selectedItems={ props.users }
			onValueChange={ props.onChangeUsers }
			toKey={ getStudentId }
			valueToString={ getUserName }
			renderNotFound={ renderNotFound }
			renderItem={ renderItem }
			renderToken={ renderUserToken }
			renderValue={ renderUserValue }
			placeholder={ props.placeholder ?? texts.placeholder }
			width={ props.width }
			error={ props.error }
			disabled={ props.disabled }
			ref={ tokenInputRef }
		/>
	</ThemeContext.Provider>;

	async function getFilteredUsers(query: string): Promise<ShortUserInfo[]> {
		const foundUsers = await props.searchUsers(query);
		if(!props.users.length) {
			return foundUsers;
		}
		return foundUsers.filter(foundUser =>
			props.users.every(addedUser => addedUser.id !== foundUser.id));
	}

	function handleRemoveUserClick(event: React.MouseEvent) {
		const { id } = (event.currentTarget as HTMLElement).parentElement?.dataset ?? {};
		if(!id) {
			return;
		}
		props.onChangeUsers(props.users.filter(g => g.id !== id));
	}

	function preventDoubleClickDefault(event: React.MouseEvent) {
		event.preventDefault();
	}
};

export default UsersSearchTokenInput;
