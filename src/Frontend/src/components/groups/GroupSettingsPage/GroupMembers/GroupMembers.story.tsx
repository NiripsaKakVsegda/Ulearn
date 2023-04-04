import React from "react";
import GroupMembers from "./GroupMembers";

import "./groupMembers.less";
import { ViewportWrapper } from "../../../course/Navigation/stroies.data";
import { GroupInfo } from "../../../../models/groups";
import { AccountState } from "../../../../redux/account";
import { SystemAccessType } from "../../../../consts/accessType";

export default {
	title: "Settings/GroupMembers",
};

export const Default = (): React.ReactNode =>
	<ViewportWrapper>
		<GroupMembers
			account={ getAccount() }
			courseId={ '1' }
			group={ getGroup() }
		/>
	</ViewportWrapper>;

Default.storyName = "default";

function getAccount(): AccountState {
	return {
		id: '1',
		firstName: 'firstName',
		lastName: 'lastName',
		avatarUrl: null,
		visibleName: 'visibleName',
		isAuthenticated: true,
		accountLoaded: true,
		accountProblems: [],
		accessesByCourse: {},
		groupsAsStudent: [],
		isHijacked: false,
		isSystemAdministrator: true,
		systemAccesses: [SystemAccessType.viewAllProfiles],
		roleByCourse: {}
	};
}

function getGroup(): GroupInfo {
	return {
		id: 17,
		name: "asdfasdfasdfasdf",
		isArchived: false,
		owner: {
			id: "4052ea63-34dd-4398-b8bb-ac4e6a85d1d0",
			visibleName: "paradeeva",
			avatarUrl: null,
			lastName: "paradeeva",
			firstName: "paradeeva",
		},
		inviteHash: "b7638c37-62c6-49a9-898c-38788169987c",
		isInviteLinkEnabled: true,
		isManualCheckingEnabled: false,
		isManualCheckingEnabledForOldSolutions: false,
		defaultProhibitFurtherReview: true,
		canStudentsSeeGroupProgress: true,
		studentsCount: 0,
		accesses: [],
		apiUrl: "/groups/17",
		areYouStudent: false,
	};
}
