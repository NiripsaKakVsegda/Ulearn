import { FAIL, START, SUCCESS } from "src/consts/actions";
import { CourseInfo } from "src/models/course";

export const COURSES_COURSE_ENTERED = "COURSES__COURSE_ENTERED";
export const COURSES_UPDATED = "COURSES__UPDATED";

const COURSES_COURSE_LOAD = "COURSES__COURSE_LOAD";
export const COURSE_LOAD_SUCCESS = COURSES_COURSE_LOAD + SUCCESS;
export const COURSE_LOAD_START = COURSES_COURSE_LOAD + START;
export const COURSE_LOAD_FAIL = COURSES_COURSE_LOAD + FAIL;
export const COURSE_LOAD_ERRORS = COURSES_COURSE_LOAD + "_ERRORS";


export interface CourseEnteredAction {
	type: typeof COURSES_COURSE_ENTERED,
	courseId: string,
}

export interface CourseLoadErrorsAction {
	type: typeof COURSE_LOAD_ERRORS,
	courseId: string,
	result: string | null,
}

export interface CourseUpdatedAction {
	type: typeof COURSES_UPDATED,
	courseById: { [courseId: string]: CourseInfo },
}

export interface CourseLoadSuccessAction {
	type: typeof COURSE_LOAD_SUCCESS,
	courseId: string,
	result: CourseInfo,
}

export interface CourseLoadStartAction {
	type: typeof COURSE_LOAD_START,
}

export interface CourseLoadFailAction {
	type: typeof COURSE_LOAD_FAIL,
	error: string,
}

export type CourseLoadAction = CourseEnteredAction
	| CourseLoadErrorsAction
	| CourseUpdatedAction
	| CourseLoadFailAction
	| CourseLoadSuccessAction
	| CourseLoadStartAction

export type CourseAction = CourseLoadAction;
