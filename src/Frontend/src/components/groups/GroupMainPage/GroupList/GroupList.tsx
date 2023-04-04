import React from "react";
import { Loader, Token } from "ui";
import GroupInfo from "../GroupInfo/GroupInfo";

import { GroupInfo as GroupInfoType, GroupInfoWithSubGroups, GroupType } from "src/models/groups";

import styles from "./groupList.less";
import { getQueryStringParameter } from "../../../../utils";

interface Props {
	courseId: string;
	groups: GroupInfoType[];
	loading: boolean;

	userId?: string | null;

	children?: React.ReactNode;

	deleteGroup: (group: GroupInfoType, groupType: 'archiveGroups' | 'groups') => void;
	toggleArchived: (group: GroupInfoType, isNotArchived: boolean) => void;
}

function GroupList({
	courseId,
	groups,
	loading,
	deleteGroup,
	toggleArchived,
	children,
	userId,
}: Props): React.ReactElement {
	const page = getQueryStringParameter('groupsSettings');
	return (
		<section className={ styles.wrapper }>
			{ loading &&
				<div className={ styles.loaderWrapper }>
					<Loader type="big" active={ true }/>
				</div>
			}
			{ !loading &&
				<div className={ styles.content }>
					{ groups && renderGroups((JSON.parse(JSON.stringify(groups)) as GroupInfoType[])
						.sort((a, b) => {
							if(userId) {
								const teachersInA = new Set([a.owner.id, ...a.accesses.map(item => item.user.id)]);
								const isUserInA = teachersInA.has(userId);
								const teachersInB = new Set([b.owner.id, ...b.accesses.map(item => item.user.id)]);
								const isUserInB = teachersInB.has(userId);

								if(teachersInA.size === 1 && isUserInA && teachersInB.size === 1 && isUserInB) {
									return 0;
								}

								if(teachersInA.size === 1 && isUserInA) {
									return -1;
								}

								if(teachersInB.size === 1 && isUserInB) {
									return 1;
								}

								if(isUserInA && isUserInB) {
									return 0;
								}
								if(isUserInA) {
									return -1;
								}
								if(isUserInB) {
									return 1;
								}
							}

							return a.name.localeCompare(b.name);
						}))
					}
				</div>
			}
			{ !loading && groups && groups.length === 0 &&
				<div className={ styles.noGroups }>
					{ children }
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
					<p className={ styles.superGroupGroupsText }>
						Группы принадлежащие { group.name }:
					</p>
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
}

export default GroupList;
