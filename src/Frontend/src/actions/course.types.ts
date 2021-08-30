import { FAIL, START, SUCCESS } from "src/consts/actions";
import { RateTypes } from "src/consts/rateTypes";
import { UnitFlashcards } from "src/models/flashcards";
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

//flashcards
const FLASHCARDS = "FLASHCARDS";
const FLASHCARDS_RATE = FLASHCARDS + "__RATE";
export const FLASHCARDS_RATE_START = FLASHCARDS_RATE + START;
export const FLASHCARDS_RATE_FAIL = FLASHCARDS_RATE + FAIL;
export const FLASHCARDS_RATE_SUCCESS = FLASHCARDS_RATE + SUCCESS;

const FLASHCARDS_LOAD = FLASHCARDS + "__LOAD";
export const FLASHCARDS_LOAD_START = FLASHCARDS_LOAD + START;
export const FLASHCARDS_LOAD_SUCCESS = FLASHCARDS_LOAD + SUCCESS;
export const FLASHCARDS_LOAD_FAIL = FLASHCARDS_LOAD + FAIL;

export interface FlashcardsLoadStartAction {
	type: typeof FLASHCARDS_LOAD_START,
}

export interface FlashcardsLoadSuccessAction {
	type: typeof FLASHCARDS_LOAD_SUCCESS,
	courseId: string,
	result: UnitFlashcards[],
}

export interface FlashcardsLoadFailAction {
	type: typeof FLASHCARDS_LOAD_FAIL,
}


export interface FlashcardsRateStartAction {
	type: typeof FLASHCARDS_RATE_START,
	courseId: string,
	unitId: string,
	flashcardId: string,
	rate: RateTypes,
	newTLast: number,
}

export interface FlashcardsRateSuccessAction {
	type: typeof FLASHCARDS_RATE_FAIL,
}

export interface FlashcardsRateFailAction {
	type: typeof FLASHCARDS_RATE_SUCCESS,
}


export type FlashcardsAction =
	FlashcardsLoadStartAction
	| FlashcardsLoadSuccessAction
	| FlashcardsLoadFailAction
	| FlashcardsRateStartAction
	| FlashcardsRateSuccessAction
	| FlashcardsRateFailAction;

export type CourseLoadAction = CourseEnteredAction
	| CourseLoadErrorsAction
	| CourseUpdatedAction
	| CourseLoadFailAction
	| CourseLoadSuccessAction
	| CourseLoadStartAction

export type CourseAction = CourseLoadAction | FlashcardsAction;
