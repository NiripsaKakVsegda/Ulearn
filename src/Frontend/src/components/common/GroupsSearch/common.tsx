import { ShortGroupInfo } from "../../../models/comments";
import { MenuItemState } from "ui";
import cn from "classnames";
import styles from "./groupsSearch.less";
import texts from "./GroupsSearch.texts";
import React from "react";
import { GroupType } from "../../../models/groups";

export function renderGroupItem(group: ShortGroupInfo, state: MenuItemState, inputWidth?: number): React.ReactNode {
	const width = inputWidth && inputWidth > 24
		? inputWidth - 24
		: 'initial';
	const isHover = state === "hover";

	let additionalInfo;
	if(isHover) {
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
		{ renderGroupName(group, isHover) }
		{ additionalInfo }
	</span>;
}

export function renderGroupValue(group: ShortGroupInfo, hover = false) {
	return <span className={ styles.groupItem }>
		{ renderGroupName(group, hover) }
		<span
			className={ cn(
				styles.additionalInfo,
				{ [styles.additionalInfoHover]: hover }
			) }
		>
			{ texts.getStudentsCount(group.membersCount) }
		</span>
	</span>;
}

function renderGroupName(group: ShortGroupInfo, hover = false) {
	return <span className={ styles.name }>
		<span>
			{ group.name }
		</span>
		{ group.groupType === GroupType.SuperGroup &&
			<span
				className={ cn(
					styles.additionalInfo,
					{ [styles.additionalInfoHover]: hover }
				) }
			>
				&nbsp;{ texts.defaultGroupPostfix }
			</span>
		}
	</span>;
}

export function getGroupId(group: ShortGroupInfo) {
	return group.id;
}

export function getGroupName(group: ShortGroupInfo) {
	return group.name;
}
