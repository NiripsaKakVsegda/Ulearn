import React from "react";
import UnloadingHeader from "./UnloadingHeader";

import api from "src/api";
import { mockFunc } from "src/utils/storyMock";

export default {
	title: "GoogleSheet/UnloadingList/Header",
};

export const Default = (): React.ReactNode => (
	<UnloadingHeader
		courseId={ 'basicprogramming' }
		api={ {
			getAllCourseGroups: api.groups.getCourseGroups,
			getAllCourseTasks: api.googleSheet.getAllCourseTasks,
			createTask: api.googleSheet.addNewTask,
			updateTask: api.googleSheet.updateCourseTask,
			deleteTask: api.googleSheet.deleteTask,
			exportTaskNow: api.googleSheet.exportTaskNow,
			getTaskById: api.googleSheet.getTaskById,
		} }/>
);
