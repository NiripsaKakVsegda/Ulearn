import { getSlideBlocks } from "src/api/slides";
import {
	SlideAction,

	SLIDES_SLIDE_READY,

	SLIDE_LOAD_START,
	SLIDE_LOAD_SUCCESS,
	SLIDE_LOAD_FAIL,
} from "src/actions/slides.types";
import { Block, } from "src/models/slide";
import { Dispatch } from "redux";

export const loadSlideStartAction = (
	courseId: string,
	slideId: string,
): SlideAction => ({
	type: SLIDE_LOAD_START,
	courseId,
	slideId,
});

export const loadSlideSuccessAction = (
	courseId: string,
	slideId: string,
	slideBlocks: Block[]
): SlideAction => ({
	type: SLIDE_LOAD_SUCCESS,
	courseId,
	slideId,
	slideBlocks,
});

export const loadSlideFailAction = (courseId: string, slideId: string, error: string): SlideAction => ({
	type: SLIDE_LOAD_FAIL,
	courseId,
	slideId,
	error,
});

export const slideReadyAction = (isSlideReady: boolean): SlideAction => ({
	type: SLIDES_SLIDE_READY,
	isSlideReady,
});

export const loadSlide = (courseId: string, slideId: string): (dispatch: Dispatch) => void => {
	courseId = courseId.toLowerCase();

	return (dispatch: Dispatch) => {
		dispatch(loadSlideStartAction(courseId, slideId,));

		getSlideBlocks(courseId, slideId)
			.then(slideBlocks => {
				dispatch(loadSlideSuccessAction(courseId, slideId, slideBlocks));
			})
			.catch(err => {
				dispatch(loadSlideFailAction(courseId, slideId, err));
			});
	};
};

export const setSlideReady = (isSlideReady: boolean) => {
	return (dispatch: Dispatch): void => {
		dispatch(slideReadyAction(isSlideReady));
	};
};
