import {
	SlideAction,

	SLIDES_SLIDE_READY,
	SlideReadyAction,

	SLIDE_LOAD_START,
	SLIDE_LOAD_SUCCESS,
	SLIDE_LOAD_FAIL,
	SlideLoadStartAction,
	SlideLoadSuccessAction,
	SlideLoadFailAction,
} from 'src/actions/slides.types';
import { Block, } from "src/models/slide";
import { ReduxData } from "./index";

interface SlidesState {
	isSlideReady: boolean;
	slidesByCourses: { [courseId: string]: { [slideId: string]: Block[] | ReduxData } };
}

const initialCoursesSlidesState: SlidesState = {
	isSlideReady: false,
	slidesByCourses: {},
};

export default function slides(state = initialCoursesSlidesState, action: SlideAction): SlidesState {
	switch (action.type) {
		case SLIDES_SLIDE_READY: {
			const { isSlideReady } = action as SlideReadyAction;
			return {
				...state,
				isSlideReady,
			};
		}

		case SLIDE_LOAD_START: {
			const { courseId, slideId, } = action as SlideLoadStartAction;
			const { slidesByCourses } = state;

			return {
				...state,
				slidesByCourses: {
					...slidesByCourses,
					[courseId]: {
						...slidesByCourses[courseId],
						[slideId]: { isLoading: true },
					}
				}
			};
		}
		case SLIDE_LOAD_SUCCESS: {
			const { courseId, slideId, slideBlocks } = action as SlideLoadSuccessAction;
			const { slidesByCourses } = state;

			return {
				...state,
				slidesByCourses: {
					...slidesByCourses,
					[courseId]: {
						...slidesByCourses[courseId],
						[slideId]: slideBlocks,
					}
				}
			};
		}
		case SLIDE_LOAD_FAIL: {
			const { slideId, courseId, error, } = action as SlideLoadFailAction;

			const { slidesByCourses } = state;

			return {
				...state,
				slidesByCourses: {
					...slidesByCourses,
					[courseId]: {
						...slidesByCourses[courseId],
						[slideId]: { error },
					}
				}
			};
		}
		default:
			return state;
	}
}
