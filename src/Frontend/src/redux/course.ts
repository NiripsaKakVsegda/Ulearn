import { CourseInfo } from "src/models/course";
import {
	COURSE_LOAD_ERRORS,
	COURSE_LOAD_FAIL,
	COURSE_LOAD_START,
	COURSE_LOAD_SUCCESS,
	CourseAction,
	CourseEnteredAction,
	CourseLoadErrorsAction,
	CourseLoadFailAction,
	CourseLoadSuccessAction,
	COURSES_COURSE_ENTERED,
	COURSES_UPDATED,
	CourseUpdatedAction,
} from "src/actions/course.types";

export interface CourseState {
	currentCourseId?: string;
	courseLoading: boolean;
	courseLoadingErrorStatus: null | string;
	courseById: { [courseId: string]: CourseInfo };
	fullCoursesInfo: { [courseId: string]: CourseInfo };
}

const initialCoursesState: CourseState = {
	courseById: {},
	currentCourseId: undefined,
	fullCoursesInfo: {},
	courseLoading: false,
	courseLoadingErrorStatus: null,
};

export default function courseReducer(state: CourseState = initialCoursesState, action: CourseAction): CourseState {
	switch (action.type) {
		case COURSES_UPDATED: {
			const { courseById } = action as CourseUpdatedAction;
			return {
				...state,
				courseById,
			};
		}
		case COURSES_COURSE_ENTERED: {
			const { courseId } = action as CourseEnteredAction;
			return {
				...state,
				currentCourseId: courseId,
			};
		}
		case COURSE_LOAD_START: {
			return {
				...state,
				courseLoading: true,
				courseLoadingErrorStatus: null,
			};
		}
		case COURSE_LOAD_SUCCESS: {
			const { courseId, result, } = action as CourseLoadSuccessAction;
			return {
				...state,
				courseLoading: false,
				fullCoursesInfo: {
					...state.fullCoursesInfo,
					[courseId]: result,
				}
			};
		}
		case COURSE_LOAD_ERRORS: {
			const { courseId, result, } = action as CourseLoadErrorsAction;
			return {
				...state,
				fullCoursesInfo: {
					...state.fullCoursesInfo,
					[courseId]: {
						...state.fullCoursesInfo[courseId],
						tempCourseError: result,
					},
				}
			};
		}
		case COURSE_LOAD_FAIL: {
			const { error, } = action as CourseLoadFailAction;
			return {
				...state,
				courseLoading: false,
				courseLoadingErrorStatus: error,
			};
		}
		default:
			return state;
	}
}
