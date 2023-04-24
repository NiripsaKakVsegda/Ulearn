import React, { FC } from "react";
import GroupsListItem from "../GroupsListItem/GroupsListItem";

import { GroupInfo, GroupInfoWithSubGroups, GroupType } from "src/models/groups";

import styles from "./groupList.less";
import { useSearchParams } from "react-router-dom";

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
			<div className={ styles.content }>
				{ [...groups, ...superGroups]
					.sort(groupSorter)
					.map((g) => renderGroup(g))
				}
			</div>
			{ groups.length === 0 && superGroups.length === 0 &&
				<div className={ styles.noGroups }>
					{ noGroupsMessage }
				</div>
			}
		</section>
	);

	function renderGroup(group: GroupInfoWithSubGroups, isSubGroup = false) {
		return (
			<>
				<GroupsListItem
					key={ group.id }
					courseId={ courseId }
					group={ group }
					deleteGroup={ onDeleteGroup }
					toggleArchived={ onToggleArchivedGroup }
					page={ page }
					isSubGroup={ isSubGroup }
				/>
				{ group.subGroups &&
					<div className={ styles.subGroupsContainer }>
						<ul className={ styles.subGroupsWrapper }>
							{
								group.subGroups.map(g => {
									return (<li className={ styles.subGroupsItem }>{ renderGroup(g, true) }</li>);
								})
							}
						</ul>
					</div>
				}
			</>
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
