import api from "./index";
import { buildQuery } from "src/utils";
import { DeadLinesResponse, } from "src/models/deadLines";
import { Dispatch } from "redux";
import { deadLinesLoadFailAction, deadLinesLoadStartAction, deadLinesLoadSuccessAction, } from "src/actions/deadLines";
import { deadLines } from "src/consts/routes";

const forUser = "for-user";

export function getDeadLinesByStudentId(courseId: string, studentId: string): Promise<DeadLinesResponse> {
	return api.get(`${ deadLines }/${ forUser }/${ studentId }` + buildQuery({ courseId }));
}

export function getDeadLinesForCurrentUser(courseId: string): Promise<DeadLinesResponse> {
	return api.get(`${ deadLines }/${ forUser }` + buildQuery({ courseId }));
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
