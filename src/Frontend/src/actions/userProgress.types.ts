import { SlideUserProgress } from "src/models/userProgress";

export const USER__PROGRESS_LOAD_START = "USER__PROGRESS_LOAD_START";
export const USER__PROGRESS_LOAD_SUCCESS = "USER__PROGRESS_LOAD_SUCCESS";
export const USER__PROGRESS_LOAD_FAIL = "USER__PROGRESS_LOAD_FAIL";
export const USER__PROGRESS_UPDATE = "USER__PROGRESS_UPDATE";

export interface LoadUserProgressStartAction {
	type: typeof USER__PROGRESS_LOAD_START
}

export interface LoadUserProgressSuccessAction {
	type: typeof USER__PROGRESS_LOAD_SUCCESS,
	courseId: string,
	result: { [slideId: string]: SlideUserProgress }
}

export interface LoadUserProgressFailAction {
	type: typeof USER__PROGRESS_LOAD_FAIL,
}

export interface UserProgressUpdateAction {
	type: typeof USER__PROGRESS_UPDATE,
	courseId: string,
	slideId: string,
	fieldsToUpdate: Partial<SlideUserProgress>,
}

export type UserProgressActionTypes =
	LoadUserProgressSuccessAction
	| LoadUserProgressStartAction
	| LoadUserProgressFailAction
	| UserProgressUpdateAction;
