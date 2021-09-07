import React from "react";
import UnloadingList from "./UnloadingList";
import api from "src/api";

export default {
	title: "GoogleSheet/UnloadingList",
};

export const Default = (): React.ReactNode =>
	<UnloadingList
		userId={ '1' }
		api={ {
			getAllCourseTasks: api.googleSheet.getAllCourseTasks,
			deleteTask: api.googleSheet.deleteTask,
			exportTaskNow: api.googleSheet.exportTaskNow,
			updateTask: api.googleSheet.updateCourseTask,
			createTask: api.googleSheet.addNewTask,
			getAllCourseGroups: api.groups.getCourseGroups,
			getTaskById: api.googleSheet.getTaskById,
		} }
	/>;

