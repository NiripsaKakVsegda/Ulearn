import api from "./index";
import { buildQuery } from "../utils";
import { DeadLineInfo, DeadLinesResponse, } from "../components/groups/GroupSettingsPage/GroupDeadLines/GroupDeadLines";
import { Dispatch } from "redux";
import { deadLinesLoadFailAction, deadLinesLoadStartAction, deadLinesLoadSuccessAction, } from "../actions/deadLines";

export function getDeadLines(courseId: string, groupId: number): Promise<DeadLinesResponse> {
	return api.get(`dead-lines` + buildQuery({ courseId, groupId }));
}

export function getDeadLinesByStudentId(courseId: string, studentId: string): Promise<DeadLinesResponse> {
	return api.get(`dead-lines/for-user/${ studentId }` + buildQuery({ courseId }));
}

export function getDeadLinesForCurrentUser(courseId: string): Promise<DeadLinesResponse> {
	return api.get(`dead-lines/for-user` + buildQuery({ courseId }));
}

export function changeDeadLine(deadLine: DeadLineInfo): Promise<Response> {
	return api.patch(`dead-lines/${ deadLine.id }` + buildQuery({
		...deadLine,
		userId: deadLine.userId === null ? undefined : deadLine.userId,
		slideId: deadLine.slideId === null ? undefined : deadLine.slideId,
		error: undefined,
		id: undefined,
	}));
}

export function createDeadLine(courseId: string, deadLine: DeadLineInfo): Promise<DeadLineInfo> {
	return api.post(`dead-lines` + buildQuery({
		...deadLine,
		courseId,
		userId: deadLine.userId === null ? undefined : deadLine.userId,
		slideId: deadLine.slideId === null ? undefined : deadLine.slideId,
		error: undefined,
		id: undefined,
	}));
}

export function deleteDeadLine(deadLineId: string): Promise<Response> {
	return api.delete(`dead-lines/${ deadLineId }`);
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
