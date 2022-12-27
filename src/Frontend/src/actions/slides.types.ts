import { FAIL, START, SUCCESS, } from "src/consts/actions";
import { Block, } from "src/models/slide";

const SLIDES = "SLIDES__SLIDE";
const SLIDE_LOAD = SLIDES + "_LOAD";
const CHECKUP_UPDATE = SLIDES + "_CHECKUP_UPDATE";
export const CHECKUP_UPDATE_START = CHECKUP_UPDATE + START;
export const CHECKUP_UPDATE_SUCCESS = CHECKUP_UPDATE + SUCCESS;
export const CHECKUP_UPDATE_FAIL = CHECKUP_UPDATE + FAIL;

export const SLIDE_LOAD_START = SLIDE_LOAD + START;
export const SLIDE_LOAD_SUCCESS = SLIDE_LOAD + SUCCESS;
export const SLIDE_LOAD_FAIL = SLIDE_LOAD + FAIL;

export const SLIDES_SLIDE_READY = SLIDES + "_READY";

export interface SlideReadyAction {
	type: typeof SLIDES_SLIDE_READY;
	isSlideReady: boolean;
}

interface SlideBaseAction {
	courseId: string;
	slideId: string;
}

export interface CheckupUpdateBase extends SlideBaseAction {
	isChecked: boolean;
	checkupId: string,
}

export interface SlideLoadStartAction extends SlideBaseAction {
	type: typeof SLIDE_LOAD_START;
}

export interface SlideLoadSuccessAction extends SlideBaseAction {
	type: typeof SLIDE_LOAD_SUCCESS;
	slideBlocks: Block[];
}

export interface SlideLoadFailAction extends SlideBaseAction {
	type: typeof SLIDE_LOAD_FAIL;
	error: string;
}

export interface CheckupUpdateStartAction extends CheckupUpdateBase {
	type: typeof CHECKUP_UPDATE_START;
}

export interface CheckupUpdateSuccessAction {
	type: typeof CHECKUP_UPDATE_SUCCESS;
}

export interface CheckupUpdateFailAction extends CheckupUpdateBase {
	type: typeof CHECKUP_UPDATE_FAIL;
}

export type SlideAction =
	SlideReadyAction
	| SlideLoadSuccessAction
	| SlideLoadFailAction
	| SlideLoadStartAction
	| CheckupUpdateStartAction
	| CheckupUpdateSuccessAction
	| CheckupUpdateFailAction
	;

