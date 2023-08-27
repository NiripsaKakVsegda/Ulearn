import React, { FC } from "react";
import { useSearchParams } from "react-router-dom";

import { GroupInfo, GroupInfoWithSubGroups } from "src/models/groups";
import GroupsListItem from "../GroupsListItem/GroupsListItem";

import styles from "./groupList.less";
import cn from "classnames";

interface Props {
	userId?: string | null;
	courseId: string;
	groups: GroupInfo[];
	superGroups: GroupInfoWithSubGroups[];
	noGroupsMessage: string;

	onDeleteGroup: (group: GroupInfo) => void;
	onToggleArchivedGroup: (group: GroupInfo) => void;
}

const GroupList: FC<Props> = ({
	userId,
	courseId,
	groups,
	superGroups,
	noGroupsMessage,
	onDeleteGroup,
	onToggleArchivedGroup,
}) => {
	const page = useSearchParams()[0].get('groupsSettings');

	return (
		<section className={ styles.wrapper }>
			<ul className={ styles.content }>
				{ [...groups, ...superGroups]
					.sort(groupSorter)
					.map((g) => renderGroup(g))
				}
			</ul>
			{ groups.length === 0 && superGroups.length === 0 &&
				<div className={ styles.noGroups }>
					{ noGroupsMessage }
				</div>
			}
		</section>
	);

	function renderGroup(group: GroupInfoWithSubGroups, isSubGroup = false) {
		return (
			<li
				key={ group.id }
				className={ cn({
					[styles.mainGroupItem]: !isSubGroup,
					[styles.subGroupsItem]: isSubGroup
				}) }
			>
				<GroupsListItem
					courseId={ courseId }
					group={ group }
					deleteGroup={ onDeleteGroup }
					toggleArchived={ onToggleArchivedGroup }
					page={ page }
					isSubGroup={ isSubGroup }
				/>
				{ group.subGroups && group.subGroups.length > 0 &&
					<div className={ styles.subGroupsContainer }>
						<ul className={ styles.subGroupsWrapper }>
							{ group.subGroups
								.map(g => renderGroup(g, true))
							}
						</ul>
					</div>
				}
			</li>
		);
	}

	function groupSorter(a: GroupInfo, b: GroupInfo) {
		const nameCompare = a.name.localeCompare(b.name);

		if(!userId) {
			return nameCompare;
		}

		const aTeachers = [a.owner.id, ...a.accesses.map(item => item.user.id)];
		const bTeachers = [b.owner.id, ...b.accesses.map(item => item.user.id)];

		const isUserInA = aTeachers.includes(userId);
		const isUserInB = bTeachers.includes(userId);

		if(aTeachers.length === 1 && isUserInA && bTeachers.length === 1 && isUserInB) {
			return nameCompare;
		}

		if(aTeachers.length === 1 && isUserInA) {
			return -1;
		}

		if(bTeachers.length === 1 && isUserInB) {
			return 1;
		}

		if(isUserInA && isUserInB) {
			return nameCompare;
		}
		if(isUserInA) {
			return -1;
		}
		if(isUserInB) {
			return 1;
		}

		return nameCompare;

	}
};

export default GroupList;
