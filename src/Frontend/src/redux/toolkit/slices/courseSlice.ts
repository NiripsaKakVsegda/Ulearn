import { CourseInfo } from "../../../models/course";
import { createSlice, PayloadAction } from "@reduxjs/toolkit";

export interface CourseState {
	currentCourseId?: string;
	courseLoading: boolean;
	courseLoadingErrorStatus: null | string;
	courseById: { [courseId: string]: CourseInfo };
	fullCoursesInfo: { [courseId: string]: CourseInfo };
}

const initialState: CourseState = {
	courseById: {},
	currentCourseId: undefined,
	fullCoursesInfo: {},
	courseLoading: false,
	courseLoadingErrorStatus: null,
};

export const courseSlice = createSlice({
	name: 'course',
	initialState,
	reducers: {
		updateCourse: (state, action: PayloadAction<{ [courseId: string]: CourseInfo }>) => {
			state.courseById = action.payload;
		},
		enterCourse: (state, action: PayloadAction<string>) => {
			state.currentCourseId = action.payload;
		},
		startLoad: (state) => {
			state.courseLoading = true;
			state.courseLoadingErrorStatus = null;
		},
		loadSuccess: (state, action: PayloadAction<{ courseId: string, result: CourseInfo }>) => {
			state.courseLoading = false;
			state.fullCoursesInfo[action.payload.courseId] = action.payload.result;
		},
		loadFail: (state, action: PayloadAction<string>) => {
			state.courseLoading = false;
			state.courseLoadingErrorStatus = action.payload;
		},
		loadErrors: (state, action: PayloadAction<{ courseId: string, result: string | null }>) => {
			state.fullCoursesInfo[action.payload.courseId].tempCourseError = action.payload.result;
		}
	}
});

export const { updateCourse, enterCourse, startLoad, loadSuccess, loadFail, loadErrors } = courseSlice.actions;

export default courseSlice.reducer;
