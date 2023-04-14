import React from "react";
import GroupHeader from "./GroupsListHeader";

import "./groupsListHeader.less";
import { GroupsListTab } from "../../../../consts/groupsPages";
import { CourseInfo } from "../../../../models/course";

export default {
	title: "Group/GroupHeader",
};

export const Default = (): React.ReactNode => (
	<GroupHeader
		tab={ GroupsListTab.Active }
		course={ getCourse() }
		onTabChange={ () => ({}) }
		navigateNewGroup={ () => ({}) }
	/>
);

function getCourse(): CourseInfo {
	return {
		id: '1',
		title: 'course',
		units: [],
		scoring: { groups: [] },
		isTempCourse: false,
		containsFlashcards: false,
		nextUnitPublishTime: null,
		tempCourseError: null
	};
}

Default.storyName = "default";
