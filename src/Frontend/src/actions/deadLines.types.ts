import { FAIL, FailAction, START, SUCCESS } from "../consts/actions";
import { DeadLinesResponse } from "../components/groups/GroupSettingsPage/GroupDeadLines/GroupDeadLines";

const DEAD_LINES_LOAD = "DEAD_LINES_LOAD";
export const DEAD_LINES_LOAD_START = DEAD_LINES_LOAD + START;
export const DEAD_LINES_LOAD_SUCCESS = DEAD_LINES_LOAD + SUCCESS;
export const DEAD_LINES_LOAD_FAIL = DEAD_LINES_LOAD + FAIL;

export interface DeadLinesLoadSuccess extends DeadLinesResponse {
	type: typeof DEAD_LINES_LOAD_SUCCESS;
	courseId: string;
}

export interface DeadLinesLoadStartAction {
	type: typeof DEAD_LINES_LOAD_START;
	courseId: string;
}

export interface DeadLinesLoadFailAction extends FailAction {
	type: typeof DEAD_LINES_LOAD_START;
	courseId: string;
}


export type DeadLinesAction = DeadLinesLoadSuccess | DeadLinesLoadStartAction | DeadLinesLoadFailAction;
