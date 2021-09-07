import React from "react";
import api from "src/api";
import CreateUnloadingModal from "./CreateUnloadingModal";

import { ViewportWrapper } from "../../../../course/Navigation/stroies.data";
import { mockFunc } from "src/utils/storyMock";

export default {
	title: "GoogleSheet/UnloadingList/Header/CreateNewUnloadingModal",
};

export const Default = (): React.ReactNode => (
	<ViewportWrapper>
		<CreateUnloadingModal
			api={ {
				getAllCourseGroups: api.groups.getCourseGroups,
				getAllCourseTasks: api.googleSheet.getAllCourseTasks,
				createTask: api.googleSheet.addNewTask,
				updateTask: api.googleSheet.updateCourseTask,
				deleteTask: api.googleSheet.deleteTask,
				exportTaskNow: api.googleSheet.exportTaskNow,
				getTaskById: api.googleSheet.getTaskById,
			} }
			onCloseModal={ mockFunc }
			courseId={ 'basicprogramming2' }
		/>
	</ViewportWrapper>
);
