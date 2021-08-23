import { FAIL, START, SUCCESS, } from "src/consts/actions";
import { Block, } from "src/models/slide";

const SLIDES = "SLIDES__SLIDE";
const SLIDE_LOAD = SLIDES + "_LOAD";
export const SLIDE_LOAD_START = SLIDE_LOAD + START;
export const SLIDE_LOAD_SUCCESS = SLIDE_LOAD + SUCCESS;
export const SLIDE_LOAD_FAIL = SLIDE_LOAD + FAIL;

export const SLIDES_SLIDE_READY = SLIDES + "_READY";

export interface SlideReadyAction {
	type: typeof SLIDES_SLIDE_READY;
	isSlideReady: boolean;
}

export interface SlideLoadStartAction {
	type: typeof SLIDE_LOAD_START;
	courseId: string;
	slideId: string;
}

export interface SlideLoadSuccessAction {
	type: typeof SLIDE_LOAD_SUCCESS;
	courseId: string;
	slideId: string;
	slideBlocks: Block[];
}

export interface SlideLoadFailAction {
	type: typeof SLIDE_LOAD_FAIL;
	error: string;
}

export type SlideAction =
	SlideReadyAction
	| SlideLoadSuccessAction
	| SlideLoadFailAction
	| SlideLoadStartAction
	;

