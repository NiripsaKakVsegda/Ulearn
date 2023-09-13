import React, { FC, useMemo, useRef } from 'react';
import { ShortGroupInfo } from "../../../models/comments";
import { ComboBox, MenuItemState, ThemeContext } from "ui";
import texts from "./GroupsSearch.texts";
import { roundInputs } from "../../../uiTheme";
import { getGroupId, getGroupName, renderGroupItem, renderGroupValue } from "./common";
import cn from "classnames";
import styles from "./groupsSearch.less";

interface Props {
	searchGroups: (query: string) => Promise<ShortGroupInfo[]>;
	group?: ShortGroupInfo;
	onSelectGroup: (group?: ShortGroupInfo) => void;

	className?: string;

	clearInputAfterSelect?: boolean;

	width?: number | string;
	size?: 'small' | 'medium' | 'large';
	error?: boolean;
	disabled?: boolean;

	placeholder?: string;
	notFoundMessage?: string;
}

const GroupsSearchCombobox: FC<Props> = (props) => {
	const comboboxRef = useRef<ComboBox<ShortGroupInfo>>(null);

	const inputWidth = useMemo(() => {
		if(window.innerWidth < 577) {
			return undefined; // Combobox превращается в модалку.
		}
		const rootNode = (comboboxRef.current as unknown as Node)?.getRootNode() as HTMLElement;
		return rootNode?.offsetWidth || undefined;
	}, [comboboxRef.current, window.innerWidth]);

	const renderItem = (group: ShortGroupInfo, state: MenuItemState) =>
		renderGroupItem(group, state, inputWidth);

	const renderNotFound = () => props.notFoundMessage ?? texts.noGroupsFound;

	return <ThemeContext.Provider value={ roundInputs }>
		<ComboBox<ShortGroupInfo>
			className={ cn(styles.input, props.className) }
			getItems={ props.searchGroups }
			value={ props.group }
			onValueChange={ selectGroup }
			onInputValueChange={ resetGroup }
			itemToValue={ getGroupId }
			valueToString={ getGroupName }
			renderNotFound={ renderNotFound }
			renderItem={ renderItem }
			renderValue={ renderGroupValue }
			placeholder={ props.placeholder ?? texts.placeholder }
			width={ props.width }
			size={ props.size }
			error={ props.error }
			disabled={ props.disabled }
			ref={ comboboxRef }
		/>
	</ThemeContext.Provider>;

	function selectGroup(group: ShortGroupInfo) {
		props.onSelectGroup(group);
		if(props.clearInputAfterSelect) {
			comboboxRef.current?.reset();
		}
		comboboxRef.current?.blur();
	}

	function resetGroup() {
		if(props.group) {
			props.onSelectGroup(undefined);
		}
	}
};

export default GroupsSearchCombobox;
