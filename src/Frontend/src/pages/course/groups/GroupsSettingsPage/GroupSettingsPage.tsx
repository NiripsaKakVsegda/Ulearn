import React, { useEffect } from 'react';
import { connect } from "react-redux";
import { Navigate } from 'react-router-dom';

import CourseLoader from "../../../../components/course/Course/CourseLoader";
import GroupMembers from "../../../../components/groups/GroupSettingsPage/GroupMembers/GroupMembers";
import GroupSettings from "../../../../components/groups/GroupSettingsPage/GroupSettings/GroupSettings";
import Error404 from "../../../../components/common/Error/Error404";
import Page from "../../../index";
import { changeCurrentCourseAction } from "../../../../actions/course";
import GroupAdditionalContent
	from "../../../../components/groups/GroupSettingsPage/GroupAdditionalContent/GroupAdditionalContent";
import GroupDeadLines from "../../../../components/groups/GroupSettingsPage/GroupDeadLines/GroupDeadLines";
import { withOldRouter } from "../../../../utils/router";
import { RootState } from "../../../../redux/reducers";

import { DispatchFromRedux, Props, PropsFromRedux } from './GroupSettingsPage.types';
import styles from "./groupSettingsPage.less";
import GroupSettingsHeader
	from "../../../../components/groups/GroupSettingsPage/GroupSettingsHeader/GroupSettingsHeader";
import { GroupSettingsTab } from "../../../../consts/groupsPages";
import { AppDispatch } from "../../../../setupStore";
import { groupSettingsApi } from "../../../../redux/toolkit/api/groups/groupSettingsApi";
import texts from './GroupsSettingsPage.texts';
import { GroupType } from "../../../../models/groups";
import SuperGroupPage from "../../../../components/groups/GroupSettingsPage/SuperGroup/SuperGroupPage";

function GroupSettingsPage(props: Props) {
	const {
		params,
		account,
		enterToCourse,
		navigate,
	} = props;

	const courseId = params.courseId.toLowerCase();
	const groupId = parseInt(params.groupId || '0');
	const pageTab = params.groupPage as GroupSettingsTab;

	const { data: group, isLoading, isError, refetch } = groupSettingsApi.useGetGroupQuery({ groupId });

	useEffect(() => {
		enterToCourse(courseId);
	}, []);

	if(isError) {
		return <Error404/>;
	}

	if(isLoading || !group) {
		return <CourseLoader/>;
	}
	const loadedGroup = group;

	if(loadedGroup.groupType === GroupType.SuperGroup) {
		return <SuperGroupPage
			groupInfo={ loadedGroup }
			refetchGroup={ refetch }
		/>;
	}

	return (
		<Page metaTitle={ texts.buildPageTitle(group.name) }>
			<GroupSettingsHeader
				navigatePrevPage={ navigatePrevPage }
				groupName={ loadedGroup.name ? loadedGroup.name : " " }
				groupPageTab={ pageTab }
				onChangeTab={ onChangeTab }
			/>
			<div className={ styles.content }>
				{ renderCurrentTab() }
			</div>
		</Page>
	);

	function renderCurrentTab(): React.ReactNode {
		switch (pageTab) {
			case GroupSettingsTab.Settings:
				return <GroupSettings
					group={ loadedGroup }
				/>;
			case GroupSettingsTab.Members:
				return <GroupMembers
					courseId={ courseId }
					account={ account }
					group={ loadedGroup }
				/>;
			case GroupSettingsTab.AdditionalContent:
				return <GroupAdditionalContent
					courseId={ courseId }
					groupId={ groupId }
				/>;
			case GroupSettingsTab.DeadLines:
				return <>
					<GroupDeadLines
						courseId={ courseId.toLowerCase() }
						groupId={ groupId }
						user={ account }
					/>
				</>;
			default:
				return <Navigate replace to={ `/${ courseId }/groups/${ groupId }/${ GroupSettingsTab.Settings }` }/>;
		}
	}

	function navigatePrevPage() {
		navigate(`/${ courseId }/groups/`);
	}

	function onChangeTab(value: GroupSettingsTab) {
		navigate(`/${ courseId }/groups/${ groupId }/${ value }`);
	}
}

const mapStateToProps = (state: RootState): PropsFromRedux => ({
	account: state.account,
});

const mapDispatchToProps = (dispatch: AppDispatch): DispatchFromRedux => ({
	enterToCourse: (courseId: string) => dispatch(changeCurrentCourseAction(courseId)),
});

const connected = connect(mapStateToProps, mapDispatchToProps)(GroupSettingsPage);

export default withOldRouter(connected);
