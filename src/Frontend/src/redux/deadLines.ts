import {
	DEAD_LINES_LOAD_FAIL,
	DEAD_LINES_LOAD_START,
	DEAD_LINES_LOAD_SUCCESS,
	DeadLinesAction,
	DeadLinesLoadFailAction,
	DeadLinesLoadSuccess,
} from "src/actions/deadLines.types";
import { DeadLineInfo } from "src/models/deadLines";
import { ReduxData } from "./index";

interface DeadLinesState {
	deadLines: {
		[courseId: string]: DeadLineInfo[] | ReduxData;
	};
}

const initialDeadLinesState: DeadLinesState = {
	deadLines: {},
};

export default function deadLinesReducer(state: DeadLinesState = initialDeadLinesState,
	action: DeadLinesAction
): DeadLinesState {
	switch (action.type) {
		case DEAD_LINES_LOAD_START: {
			const { courseId } = action;
			return {
				...state,
				deadLines: {
					...state.deadLines,
					[courseId]: { isLoading: true },
				}
			};
		}
		case DEAD_LINES_LOAD_SUCCESS: {
			const { courseId, deadLines, } = action as DeadLinesLoadSuccess;

			return {
				...state,
				deadLines: {
					...state.deadLines,
					[courseId]: deadLines,
				}
			};
		}
		case DEAD_LINES_LOAD_FAIL: {
			const { courseId, error } = action as DeadLinesLoadFailAction;
			return {
				...state,
				deadLines: {
					...state.deadLines,
					[courseId]: { error },
				}
			};
		}
		default:
			return state;
	}
}
