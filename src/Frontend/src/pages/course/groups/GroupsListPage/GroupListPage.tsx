import React, { FC, useState } from 'react';
import { WithRouter } from "../../../../models/router";
import { CourseState } from "../../../../redux/course";
import { withCourseRouting, withOldRouter } from "../../../../utils/router";
import { connect } from "react-redux";
import { RootState } from "../../../../redux/reducers";
import GroupsListHeader from "../../../../components/groups/GroupMainPage/GroupsListHeader/GroupsListHeader";
import GroupList from "../../../../components/groups/GroupMainPage/GroupList/GroupList";
import Page from "../../../index";
import { GroupInfo, GroupsInfoResponse, GroupsListParameters } from "../../../../models/groups";
import { Loader, Toast } from "ui";
import CourseLoader from "../../../../components/course/Course/CourseLoader";
import { groupsApi } from "../../../../redux/toolkit/api/groups/groupsApi";
import { GroupsListTab } from "../../../../consts/groupsPages";
import Error404 from "../../../../components/common/Error/Error404";
import { AppDispatch } from "../../../../setupStore";
import { groupSettingsApi } from "../../../../redux/toolkit/api/groups/groupSettingsApi";
import texts from './GroupsListPage.texts';

interface Props extends WithRouter {
	userId?: string | null;
	courses: CourseState;

	updateGroupsState: (params: Partial<GroupsListParameters>, recipe: (draft: GroupsInfoResponse) => void) => void;
}

const GroupListPage: FC<Props> = ({ userId, courses, navigate, params, updateGroupsState }) => {
	const [deleteGroup] = groupsApi.useDeleteGroupMutation();
	const [updateGroupSettings] = groupSettingsApi.useSaveGroupSettingsMutation();

	const courseId = params.courseId.toLowerCase();

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

	const groups = tab === GroupsListTab.Active ? activeGroups : archivedGroups;
	const isLoading = tab === GroupsListTab.Active ? isActiveGroupsLoading : isArchivedGroupsLoading;

	const course = courses.courseById[courseId];
	if(!course) {
		return <CourseLoader/>;
	}

	if(isActiveGroupsError || isArchivedGroupsError) {
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
		deleteGroup({ group }).unwrap()
			.then(() => {
				const params: Partial<GroupsListParameters> = group.isArchived
					? { courseId, archived: true }
					: { courseId };
				updateGroupsState(params, draft => {
					draft.groups = draft.groups.filter(source => source.id !== group.id);
				});

				Toast.push(texts.buildDeleteGroupToast(group.name));
			});
	}

	function onToggleArchived(group: GroupInfo) {
		const isArchived = !group.isArchived;
		updateGroupSettings({ groupId: group.id, groupSettings: { isArchived } }).unwrap()
			.then(() => {
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

				Toast.push(texts.buildArchiveToggleToast(group.name, isArchived));
			});
	}
};

const mapStateToProps = (state: RootState) => ({
	courses: state.courses,
	userId: state.account.id,
});

const mapDispatchToProps = (dispatch: AppDispatch) => ({
	updateGroupsState: (params: Partial<GroupsListParameters>, recipe: (draft: GroupsInfoResponse) => void) =>
		dispatch(groupsApi.util.updateQueryData('getGroups', params, recipe)),
});

const connected = connect(mapStateToProps, mapDispatchToProps)(GroupListPage);

export default withOldRouter(withCourseRouting(connected));

