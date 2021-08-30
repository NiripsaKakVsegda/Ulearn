import { Dispatch } from "redux";

import { getCourse, getCourseErrors } from 'src/api/courses';
import { CourseInfo } from "src/models/course";
import {
	COURSES_UPDATED,
	COURSE_LOAD_ERRORS,
	COURSE_LOAD_FAIL,
	COURSE_LOAD_START,
	COURSE_LOAD_SUCCESS,
	COURSES_COURSE_ENTERED,
	CourseAction,
} from "src/actions/course.types";

export const courseUpdatedAction = (courseById: { [courseId: string]: CourseInfo },): CourseAction => ({
	type: COURSES_UPDATED,
	courseById,
});

export const changeCurrentCourseAction = (courseId: string): CourseAction => ({
	type: COURSES_COURSE_ENTERED,
	courseId,
});

export const loadCourseStartAction = (): CourseAction => ({
	type: COURSE_LOAD_START,
});

export const loadCourseSuccessAction = (courseId: string, result: CourseInfo): CourseAction => ({
	type: COURSE_LOAD_SUCCESS,
	courseId,
	result,
});

const loadCourseErrorsSuccessAction = (courseId: string, result: string | null): CourseAction => ({
	type: COURSE_LOAD_ERRORS,
	courseId,
	result,
});

const loadCourseFailAction = (error: string): CourseAction => ({
	type: COURSE_LOAD_FAIL,
	error,
});


export const loadCourse = (courseId: string): (dispatch: Dispatch) => void => {
	courseId = courseId.toLowerCase();

	return (dispatch: Dispatch) => {
		dispatch(loadCourseStartAction());

		getCourse(courseId)
			.then(result => {
				dispatch(loadCourseSuccessAction(courseId, result));
			})
			.catch(err => {
				dispatch(loadCourseFailAction(err.status));
			});
	};
};

export const loadCourseErrors = (courseId: string): (dispatch: Dispatch) => void => {
	courseId = courseId.toLowerCase();

	return (dispatch: Dispatch) => {
		getCourseErrors(courseId)
			.then(result => {
				if(result.status === 204) {
					dispatch(loadCourseErrorsSuccessAction(courseId, null));
				} else {
					dispatch(loadCourseErrorsSuccessAction(courseId, result.tempCourseError));
				}
			})
			.catch(() => {
				dispatch(loadCourseErrorsSuccessAction(courseId, null));
			});
	};
};
