import React, { FC } from "react";
import GroupsListItem from "../GroupsListItem/GroupsListItem";

import { GroupInfo } from "src/models/groups";

import styles from "./groupList.less";
import { useSearchParams } from "react-router-dom";

interface Props {
	userId?: string | null;
	courseId: string;
	groups: GroupInfo[];
	noGroupsMessage: string;

	onDeleteGroup: (group: GroupInfo) => void;
	onToggleArchivedGroup: (group: GroupInfo) => void;
}

const GroupList: FC<Props> = ({
	userId,
	courseId,
	groups,
	noGroupsMessage,
	onDeleteGroup,
	onToggleArchivedGroup,
}) => {
	const page = useSearchParams()[0].get('groupsSettings');

	return (
		<section className={ styles.wrapper }>
			<div className={ styles.content }>
				{ [...groups]
					.sort(groupSorter)
					.map(group =>
						<GroupsListItem
							key={ group.id }
							courseId={ courseId }
							group={ group }
							page={ page }
							deleteGroup={ onDeleteGroup }
							toggleArchived={ onToggleArchivedGroup }
						/>)
				}
			</div>
			{ groups.length === 0 &&
				<div className={ styles.noGroups }>
					{ noGroupsMessage }
				</div>
			}
		</section>
	);

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
