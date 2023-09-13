import React, { FC, useMemo, useRef } from 'react';
import { ShortGroupInfo } from "../../../models/comments";
import { MenuItemState, ThemeContext, Token, TokenInput, TokenProps } from "ui";
import { getGroupId, getGroupName, renderGroupItem, renderGroupValue } from "./common";
import texts from "./GroupsSearch.texts";
import styles from "./groupsSearch.less";
import { roundInputs } from "../../../uiTheme";
import cn from "classnames";

interface Props {
	searchGroups: (query: string) => Promise<ShortGroupInfo[]>;
	groups: ShortGroupInfo[];
	onChangeGroups: (groups: ShortGroupInfo[]) => void;

	className?: string;

	width?: number | string;
	error?: boolean;
	disabled?: boolean;

	placeholder?: string;
	notFoundMessage?: string;
}

const GroupsSearchTokenInput: FC<Props> = (props) => {
	const tokenInputRef = useRef<TokenInput<ShortGroupInfo>>(null);

	const inputWidth = useMemo(() => {
		const rootNode = (tokenInputRef.current as unknown as Node)?.getRootNode() as HTMLElement;
		return rootNode?.getElementsByTagName('label').item(0)?.offsetWidth || undefined;
	}, [tokenInputRef.current, window.innerWidth]);

	const renderItem = (group: ShortGroupInfo, state: MenuItemState) =>
		renderGroupItem(group, state, inputWidth);

	const renderGroupToken = (group: ShortGroupInfo, tokenProps: Partial<TokenProps>) => {
		return <Token
			{ ...tokenProps }
			key={ group.id }
			data-id={ group.id }
			onRemove={ handleRemoveGroupClick }
			onDoubleClick={ preventDoubleClickDefault }
		>
			{ tokenProps.isActive
				? renderGroupValue(group, true)
				: group.name
			}
		</Token>;
	};

	const renderNotFound = () => props.notFoundMessage ?? texts.noGroupsFound;

	return <ThemeContext.Provider value={ roundInputs }>
		<TokenInput<ShortGroupInfo>
			className={ cn(
				styles.input,
				{[styles.notEmpty]: !!props.groups?.length},
				props.className
			) }
			getItems={ getFilteredGroups }
			selectedItems={ props.groups }
			onValueChange={ props.onChangeGroups }
			toKey={ getGroupId }
			valueToString={ getGroupName }
			renderNotFound={ renderNotFound }
			renderItem={ renderItem }
			renderToken={ renderGroupToken }
			renderValue={ renderGroupValue }
			placeholder={ props.placeholder ?? texts.placeholder }
			width={ props.width }
			error={ props.error }
			disabled={ props.disabled }
			ref={ tokenInputRef }
		/>
	</ThemeContext.Provider>;

	async function getFilteredGroups(query: string): Promise<ShortGroupInfo[]> {
		const foundGroups = await props.searchGroups(query);
		if(!props.groups.length) {
			return foundGroups;
		}

		return foundGroups.filter(foundGroup =>
			props.groups.every(addedGroup => addedGroup.id !== foundGroup.id));
	}

	function handleRemoveGroupClick(event: React.MouseEvent) {
		const { id } = (event.currentTarget as HTMLElement).parentElement?.dataset ?? {};
		const numberId = parseInt(id ?? '');
		if(!numberId || isNaN(numberId)) {
			return;
		}
		props.onChangeGroups(props.groups.filter(g => g.id !== numberId));
	}

	function preventDoubleClickDefault(event: React.MouseEvent) {
		event.preventDefault();
	}
};

export default GroupsSearchTokenInput;
