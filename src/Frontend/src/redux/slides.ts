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

interface SlidesState {
	isSlideReady: boolean;
	slideLoading: { courseId: string; slideId: string; } | false;
	slidesByCourses: { [courseId: string]: { [slideId: string]: Block[] } };
	slideError: string | null;
}

const initialCoursesSlidesState: SlidesState = {
	isSlideReady: false,
	slideLoading: false,
	slideError: null,
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

			return {
				...state,
				slideLoading: { courseId, slideId, },
				slideError: null,
			};
		}
		case SLIDE_LOAD_SUCCESS: {
			const { courseId, slideId, slideBlocks } = action as SlideLoadSuccessAction;
			const { slidesByCourses } = state;

			return {
				...state,
				slideLoading: false,
				slideError: null,
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
			const { error } = action as SlideLoadFailAction;

			return {
				...state,
				slideLoading: false,
				slideError: error,
			};
		}
		default:
			return state;
	}
}
