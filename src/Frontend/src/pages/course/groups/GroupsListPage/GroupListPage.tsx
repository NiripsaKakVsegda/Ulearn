import React, { FC, useState } from 'react';
import { WithRouter } from "../../../../models/router";
import { CourseState } from "../../../../redux/course";
import { withCourseRouting, withOldRouter } from "../../../../utils/router";
import { connect } from "react-redux";
import { RootState } from "../../../../redux/reducers";
import GroupsListHeader from "../../../../components/groups/GroupMainPage/GroupsListHeader/GroupsListHeader";
import GroupList from "../../../../components/groups/GroupMainPage/GroupList/GroupList";
import Page from "../../../index";
import {
	GroupInfo,
	GroupsInfoResponse,
	GroupsListParameters,
	GroupType,
	SuperGroupsListResponse
} from "../../../../models/groups";
import { Loader, Toast } from "ui";
import CourseLoader from "../../../../components/course/Course/CourseLoader";
import { groupsApi, superGroupsApi } from "../../../../redux/toolkit/api/groups/groupsApi";
import { GroupsListTab } from "../../../../consts/groupsPages";
import Error404 from "../../../../components/common/Error/Error404";
import { AppDispatch } from "../../../../setupStore";
import { groupSettingsApi } from "../../../../redux/toolkit/api/groups/groupSettingsApi";
import texts from './GroupsListPage.texts';
import { buildUserInfo, isInstructor } from "../../../../utils/courseRoles";
import { AccountState } from "../../../../redux/account";

interface Props extends WithRouter {
	account: AccountState;
	isInstructor?: boolean;
	courses: CourseState;

	updateGroupsState: (params: Partial<GroupsListParameters>, recipe: (draft: GroupsInfoResponse) => void) => void;
	updateSuperGroupsState: (params: Partial<GroupsListParameters>,
		recipe: (draft: SuperGroupsListResponse) => void
	) => void;
}

const GroupListPage: FC<Props> = ({
	account,
	courses,
	navigate,
	params,
	updateGroupsState,
	updateSuperGroupsState,
}) => {
	const [deleteGroup] = groupsApi.useDeleteGroupMutation();
	const [updateGroupSettings] = groupSettingsApi.useSaveGroupSettingsMutation();
	const userId = account.id;
	const courseId = params.courseId.toLowerCase();
	const _isInstructor = isInstructor(buildUserInfo(account, courseId,));

	const {
		superGroups,
		isSuperGroupsLoading,
		isSuperGroupsError,
	} = superGroupsApi.useGetGroupsQuery({ courseId }, {
		selectFromResult: ({ data, isLoading, isError }) => ({
			superGroups: data?.superGroups.map(
				g => ({ ...g, subGroups: data?.subGroupsBySuperGroupId[g.id] })) || [],
			isSuperGroupsLoading: isLoading,
			isSuperGroupsError: isError
		}),
		skip: !_isInstructor,
	});

	const { activeGroups, isActiveGroupsLoading, isActiveGroupsError } = groupsApi.useGetGroupsQuery({ courseId }, {
		selectFromResult: ({ data, isLoading, isError }) => ({
			activeGroups: data?.groups || [],
			isActiveGroupsLoading: isLoading,
			isActiveGroupsError: isError
		})
	});

	const [loadArchived, setLoadArchived] = useState(false);
	const { archivedGroups, isArchivedGroupsLoading, isArchivedGroupsError } = groupsApi.useGetGroupsQuery(
		{ courseId, archived: true },
		{
			selectFromResult: ({ data, isLoading, isError }) => ({
				archivedGroups: data?.groups
					.filter(group => group.isArchived) || [],
				isArchivedGroupsLoading: isLoading,
				isArchivedGroupsError: isError
			}),
			skip: !loadArchived
		}
	);

	const [tab, setTab] = useState<GroupsListTab>(GroupsListTab.Active);
	const isActive = tab === GroupsListTab.Active;
	const filteredSuperGroups = superGroups.filter(g => g.isArchived === !isActive);

	const subGroupsIds = superGroups.reduce((pv, cv) => {
		return [...pv, ...(cv.subGroups?.map(g => g.id) || [])];
	}, [] as number[]);
	const groups = (tab === GroupsListTab.Active
		? activeGroups
		: archivedGroups)
		.filter(g => !subGroupsIds.includes(g.id));
	const isLoading = isSuperGroupsLoading || tab === GroupsListTab.Active
		? isActiveGroupsLoading
		: isArchivedGroupsLoading;

	const course = courses.courseById[courseId];
	if(!course) {
		return <CourseLoader/>;
	}

	if(isActiveGroupsError || isArchivedGroupsError || isSuperGroupsError) {
		return <Error404/>;
	}

	return (
		<Page metaTitle={ texts.buildPageTitle(course.title) }>
			<GroupsListHeader
				onTabChange={ onTabChange }
				tab={ tab }
				course={ course }
				navigateNewGroup={ navigateNewGroup }
			/>
			<Loader type="big" active={ isLoading }>
				<GroupList
					userId={ userId }
					courseId={ courseId }
					groups={ groups }
					superGroups={ filteredSuperGroups }
					noGroupsMessage={ tab === GroupsListTab.Active ? texts.noActiveGroupsMessage : texts.noArchiveGroupsMessage }
					onDeleteGroup={ onDeleteGroup }
					onToggleArchivedGroup={ onToggleArchived }
				/>
			</Loader>
		</Page>
	);

	function onTabChange(tab: GroupsListTab) {
		setTab(tab);

		if(tab === GroupsListTab.Archive && !loadArchived) {
			setLoadArchived(true);
		}
	}

	function navigateNewGroup(groupId: number) {
		navigate(`/${ courseId }/groups/${ groupId }`);
	}

	function onDeleteGroup(group: GroupInfo) {
		const isSuperGroup = group.groupType === GroupType.SuperGroup;

		deleteGroup({ group })
			.unwrap()
			.then(() => {
				if(!isSuperGroup) {
					const params: Partial<GroupsListParameters> = group.isArchived
						? { courseId, archived: true }
						: { courseId };

					updateGroupsState(params, draft => {
						draft.groups = draft.groups.filter(source => source.id !== group.id);
					});
				} else {
					updateSuperGroupsState({ courseId }, draft => {
						draft.superGroups = draft.superGroups.filter(source => source.id !== group.id);
					});
				}

				const superGroupId = group.superGroupId;
				if(superGroupId !== null) {
					updateSuperGroupsState({ courseId }, draft => {
						draft.subGroupsBySuperGroupId[superGroupId]
							= draft.subGroupsBySuperGroupId[superGroupId]
							.filter(source => source.id !== group.id);
					});
				}

				Toast.push(texts.buildDeleteGroupToast(group.name, isSuperGroup));
			});
	}

	function onToggleArchived(group: GroupInfo) {
		const isArchived = !group.isArchived;
		const isSuperGroup = group.groupType === GroupType.SuperGroup;
		const superGroupId = group.superGroupId;
		updateGroupSettings({ groupId: group.id, groupSettings: { isArchived } })
			.unwrap()
			.then(() => {
				if(!isSuperGroup) {
					updateGroupsState({ courseId }, draft => {
						if(isArchived) {
							draft.groups = draft.groups.filter(source => source.id !== group.id);
						} else {
							draft.groups.push({ ...group, isArchived: false });
						}
					});
					updateGroupsState({ courseId, archived: true }, draft => {
						if(isArchived) {
							draft.groups.push({ ...group, isArchived: true });
						} else {
							draft.groups = draft.groups.filter(source => source.id !== group.id);
						}
					});

					if(superGroupId !== null) {
						updateSuperGroupsState({ courseId }, draft => {
							const groupsToUpdate = draft.subGroupsBySuperGroupId[superGroupId];
							const subId = groupsToUpdate.findIndex(source => source.id === group.id);
							draft.subGroupsBySuperGroupId[superGroupId][subId] = {
								...draft.subGroupsBySuperGroupId[superGroupId][subId],
								isArchived
							};
						});
					}
				} else {
					updateSuperGroupsState({ courseId }, draft => {
						const subId = draft.superGroups.findIndex(source => source.id === group.id);
						draft.superGroups[subId] = { ...draft.superGroups[subId], isArchived };
					});
				}


				Toast.push(texts.buildArchiveToggleToast(group.name, isSuperGroup, isArchived));
			});
	}
};

const mapStateToProps = (state: RootState) => ({
	courses: state.courses,
	account: state.account,
});

const mapDispatchToProps = (dispatch: AppDispatch) => ({
	updateGroupsState: (params: Partial<GroupsListParameters>, recipe: (draft: GroupsInfoResponse) => void) =>
		dispatch(groupsApi.util.updateQueryData('getGroups', params, recipe)),
	updateSuperGroupsState: (params: Partial<GroupsListParameters>, recipe: (draft: SuperGroupsListResponse) => void) =>
		dispatch(superGroupsApi.util.updateQueryData('getGroups', params, recipe)),
});

const connected = connect(mapStateToProps, mapDispatchToProps)(GroupListPage);

export default withOldRouter(withCourseRouting(connected));

