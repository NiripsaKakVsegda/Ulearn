import {
	USER__PROGRESS_LOAD_FAIL,
	USER__PROGRESS_LOAD_START,
	USER__PROGRESS_LOAD_SUCCESS,
	USER__PROGRESS_UPDATE,
	UserProgressActionTypes,
} from "src/actions/userProgress.types";
import { SlideUserProgress } from "src/models/userProgress";

interface UserProgressState {
	loading: boolean;
	progress: { [courseId: string]: { [slideId: string]: SlideUserProgress } };
}

const initialState: UserProgressState = {
	progress: {},
	loading: false,
};

export default function userProgressReducer(state = initialState, action: UserProgressActionTypes): UserProgressState {
	switch (action.type) {
		case USER__PROGRESS_LOAD_START:
			return {
				...state,
				loading: true,
			};
		case USER__PROGRESS_LOAD_SUCCESS:
			return {
				...state,
				loading: false,
				progress: {
					...state.progress,
					[action.courseId]: action.result,
				}
			};
		case USER__PROGRESS_LOAD_FAIL:
			return {
				...state,
				loading: false,
			};
		case USER__PROGRESS_UPDATE:
			return {
				...state,
				progress: {
					...state.progress,
					[action.courseId]: {
						...state.progress[action.courseId],
						[action.slideId]: {
							...state.progress[action.courseId][action.slideId],
							...action.fieldsToUpdate
						},
					},
				}
			};
		default:
			return state;
	}
}
