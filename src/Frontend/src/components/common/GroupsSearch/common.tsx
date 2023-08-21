import { ShortGroupInfo } from "../../../models/comments";
import { MenuItemState } from "ui";
import cn from "classnames";
import styles from "./groupsSearch.less";
import texts from "./GroupsSearch.texts";
import React from "react";

export function renderGroupItem(group: ShortGroupInfo, state: MenuItemState, inputWidth?: number): React.ReactNode {
	const width = inputWidth && inputWidth > 24
		? inputWidth - 24
		: 'initial';

	let additionalInfo;
	if(state === "hover") {
		additionalInfo = <span
			className={ cn(
				styles.additionalInfo,
				styles.additionalInfoHover
			) }
		>
			{ texts.getStudentsCount(group.membersCount) }
		</span>;
	} else if(group.isArchived) {
		additionalInfo = <span className={ styles.additionalInfo }>
				{ texts.archivedGroup }
			</span>;
	}

	return <span className={ styles.groupItem } style={ { width } }>
		<span className={ styles.name }>{ group.name }</span>
		{ additionalInfo }
	</span>;
}

export function renderGroupValue(group: ShortGroupInfo, selected = false) {
	return <span className={ styles.groupItem }>
		<span className={ styles.name }>{ group.name }</span>
		<span
			className={ cn(
				styles.additionalInfo,
				{ [styles.additionalInfoHover]: selected }
			) }
		>
			{ texts.getStudentsCount(group.membersCount) }
		</span>
	</span>;
}

export function getGroupId(group: ShortGroupInfo) {
	return group.id;
}

export function getGroupName(group: ShortGroupInfo) {
	return group.name;
}
