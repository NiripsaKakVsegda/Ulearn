import api from "./index";
import { buildQuery } from "src/utils";
import { DeadLineInfo, DeadLinesResponse, } from "src/models/deadLines";
import { Dispatch } from "redux";
import { deadLinesLoadFailAction, deadLinesLoadStartAction, deadLinesLoadSuccessAction, } from "src/actions/deadLines";
import { deadLines } from "src/consts/routes";

const forUser = 'for-user';

export function getDeadLines(courseId: string, groupId: number): Promise<DeadLinesResponse> {
	return api.get(deadLines + buildQuery({ courseId, groupId }));
}

export function getDeadLinesByStudentId(courseId: string, studentId: string): Promise<DeadLinesResponse> {
	return api.get(`${ deadLines }/${ forUser }/${ studentId }` + buildQuery({ courseId }));
}

export function getDeadLinesForCurrentUser(courseId: string): Promise<DeadLinesResponse> {
	return api.get(`${ deadLines }/${ forUser }` + buildQuery({ courseId }));
}

export function changeDeadLine(deadLine: DeadLineInfo): Promise<Response> {
	return api.patch(`${ deadLines }/${ deadLine.id }` + buildQuery({
		...deadLine,
		userId: deadLine.userId === null ? undefined : deadLine.userId,
		slideId: deadLine.slideId === null ? undefined : deadLine.slideId,
		error: undefined,
		id: undefined,
	}));
}

export function createDeadLine(courseId: string, deadLine: DeadLineInfo): Promise<DeadLineInfo> {
	return api.post(deadLines + buildQuery({
		...deadLine,
		courseId,
		userId: deadLine.userId === null ? undefined : deadLine.userId,
		slideId: deadLine.slideId === null ? undefined : deadLine.slideId,
		error: undefined,
		id: undefined,
	}));
}

export function deleteDeadLine(deadLineId: string): Promise<Response> {
	return api.delete(`${ deadLines }/${ deadLineId }`);
}

//REDUX

const getDeadLinesForCurrentUserRedux = (courseId: string,) => {
	return (dispatch: Dispatch): Promise<DeadLinesResponse | string> => {
		dispatch(deadLinesLoadStartAction(courseId));
		return getDeadLinesForCurrentUser(courseId)
			.then(r => {
				dispatch(deadLinesLoadSuccessAction(courseId, r.deadLines));
				return r;
			})
			.catch(error => {
				dispatch(deadLinesLoadFailAction(courseId, error,));
				return error;
			});
	};
};

export const redux = {
	getDeadLinesForCurrentUserRedux,
};
