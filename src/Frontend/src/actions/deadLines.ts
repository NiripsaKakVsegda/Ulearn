import { DeadLineInfo, } from "../components/groups/GroupSettingsPage/GroupDeadLines/GroupDeadLines";
import {
	DEAD_LINES_LOAD_FAIL,
	DEAD_LINES_LOAD_START,
	DEAD_LINES_LOAD_SUCCESS,
	DeadLinesLoadFailAction,
	DeadLinesLoadStartAction,
	DeadLinesLoadSuccess,
} from "./deadLines.types";

export const deadLinesLoadSuccessAction = (courseId: string, deadLines: DeadLineInfo[]): DeadLinesLoadSuccess => ({
	type: DEAD_LINES_LOAD_SUCCESS,
	courseId,
	deadLines,
});

export const deadLinesLoadStartAction = (courseId: string): DeadLinesLoadStartAction => ({
	type: DEAD_LINES_LOAD_START,
	courseId,
});

export const deadLinesLoadFailAction = (courseId: string, error: string,): DeadLinesLoadFailAction => ({
	type: DEAD_LINES_LOAD_FAIL,
	courseId,
	error,
});
