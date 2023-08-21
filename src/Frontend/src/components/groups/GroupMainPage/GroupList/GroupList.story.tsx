import React from "react";
import { GroupInfo } from '../../../../models/groups';
import { getMockedGroup, getMockedUser } from "../../../../storiesUtils";
import GroupList from "./GroupList";

import "./groupList.less";

export default {
	title: "Group/GroupList"
};

const mock = () => ({});
export const Default = (): React.ReactNode =>
	<GroupList
		groups={ getGroups() }
		superGroups={ [] }
		courseId={ '1' }
		userId={ '1' }
		noGroupsMessage={ 'No GROUPS' }
		onDeleteGroup={ mock }
		onToggleArchivedGroup={ mock }
	/>;

Default.storyName = "default";

function getGroups(): GroupInfo[] {
	return [
		getMockedGroup({
			id: 17,
			createTime: "2018-10-19T13:46:28.153",
			name: "asdfasdfasdfasdf",
			isArchived: false,
			owner: getMockedUser({
				id: "4052ea63-34dd-4398-b8bb-ac4e6a85d1d0",
				firstName: "",
				lastName: "",
				visibleName: "paradeeva"
			}),
			inviteHash: "b7638c37-62c6-49a9-898c-38788169987c",
			isInviteLinkEnabled: true,
			isManualCheckingEnabled: false,
			isManualCheckingEnabledForOldSolutions: false,
			defaultProhibitFurtherReview: true,
			canStudentsSeeGroupProgress: true,
			studentsCount: 0,
			accesses: [],
			apiUrl: "/groups/17",
			areYouStudent: false
		}),
		getMockedGroup({
			id: 14,
			createTime: null,
			name: "asdfghj",
			isArchived: false,
			owner: getMockedUser({
				id: "4052ea63-34dd-4398-b8bb-ac4e6a85d1d0",
				firstName: "",
				lastName: "",
				visibleName: "paradeeva"
			}),
			inviteHash: "e22214b7-774c-4f14-b80c-910c6e715301",
			isInviteLinkEnabled: true,
			isManualCheckingEnabled: true,
			isManualCheckingEnabledForOldSolutions: true,
			defaultProhibitFurtherReview: true,
			canStudentsSeeGroupProgress: true,
			studentsCount: 0,
			accesses: [],
			apiUrl: "/groups/14",
			areYouStudent: false
		})
	];
}
