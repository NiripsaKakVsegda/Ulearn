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
						/>
					)
				}
			</div>
			{ groups.length === 0 &&
				<div className={ styles.noGroups }>
					{ noGroupsMessage }
				</div>
			}
		</section>
	);

	function renderGroups(groups: GroupInfoType[]) {
		const toRender: GroupInfoWithSubGroups[] = [];
		const subGroups = groups.filter(g => g.superGroupId);
		const subGroupsBySuperGroupId: { [id: number]: GroupInfoType[] } = {};

		for (const subGroup of subGroups) {
			const id = subGroup.superGroupId!;
			if(!subGroupsBySuperGroupId[id]) {
				subGroupsBySuperGroupId[id] = [];
			}
			subGroupsBySuperGroupId[id].push(subGroup);
		}


		for (const group of groups.filter(g => !g.superGroupId)) {
			if(group.groupType === GroupType.SuperGroup) {
				(group as GroupInfoWithSubGroups).subGroups = subGroupsBySuperGroupId[group.id];
			}
			toRender.push(group);
		}

		return toRender.map(renderGroup);
	}

	function renderGroup(group: GroupInfoWithSubGroups) {
		if(group.groupType === GroupType.SuperGroup) {
			return (
				<>
					<GroupInfo
						key={ group.id }
						courseId={ courseId }
						group={ group }
						deleteGroup={ deleteGroup }
						toggleArchived={ toggleArchived }
						page={ page }
					/>
					{ group.subGroups && <ul className={ styles.subGroupsWrapper }>
						{
							group.subGroups.map(g => {
								return (<li>{ renderGroup(g) }</li>);
							})
						}
					</ul>
					}
				</>
			);
		}
		return (
			<GroupInfo
				key={ group.id }
				courseId={ courseId }
				group={ group }
				deleteGroup={ deleteGroup }
				toggleArchived={ toggleArchived }
				page={ page }
			/>
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
