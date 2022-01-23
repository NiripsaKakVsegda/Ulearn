import { Dispatch } from "redux";
import api from "./index";
import {
	antiplagiarimsStatusLoadFailAction,
	antiplagiarimsStatusLoadStartAction,
	antiplagiarimsStatusLoadSuccessAction,
	deadLinesLoadFailAction,
	deadLinesLoadStartAction,
	deadLinesLoadSuccessAction,
	studentLoadFailAction,
	studentLoadStartAction,
	studentLoadSuccessAction,
	studentProhibitFurtherManualCheckingFailAction,
	studentProhibitFurtherManualCheckingStartAction,
} from "src/actions/instructor";
import { ShortUserInfo } from "src/models/users";
import { antiplagiarism, } from "src/consts/routes";
import { buildQuery } from "src/utils";
import { AntiPlagiarismStatusResponse, QueueItemType, ReviewQueueResponse, } from "src/models/instructor";
import { DeadLinesResponse } from "../components/groups/GroupSettingsPage/GroupDeadLines/GroupDeadLines";
import { getDeadLinesByStudentId } from "./deadLines";

export function getAntiPlagiarismStatus(courseId: string,
	submissionId: number,
): Promise<AntiPlagiarismStatusResponse> {
	const url = `${ antiplagiarism }/${ submissionId }` + buildQuery({ courseId });
	return api.get<AntiPlagiarismStatusResponse>(url);
}

export function prohibitFurtherManualChecking(
	courseId: string,
	slideId: string,
	userId: string,
	prohibit: boolean,
): Promise<Response> {
	const url = `user-progress/${ courseId }/${ slideId }/prohibit-further-manual-checking`
		+ buildQuery({ userId, prohibit });
	return api.put(url);
}

export function getReviewQueue(
	courseId: string,
	groupsIds: string | 'all' | 'not-in-group' | undefined,
	slideId: string | undefined,
	userIds: string[] | undefined,
	done: boolean,
): Promise<ReviewQueueResponse> {
	const url = `review-queue/${ courseId }`
		+ buildQuery({ groupsIds, slideId, userIds, done, });
	return api.get(url);
}

export function lockSubmissionCheck(
	courseId: string,
	submissionId: number,
	type: QueueItemType,
): Promise<Response> {
	const url = `review-queue/${ courseId }/${ submissionId }` + buildQuery({ type });
	return api.post(url);
}

//REDUX

const getStudentInfoRedux = (studentId: string,) => {
	return (dispatch: Dispatch): Promise<string | ShortUserInfo> => {
		dispatch(studentLoadStartAction(studentId,));
		return api.users.getOtherUserInfo(studentId,)
			.then(user => {
				if(user) {
					dispatch(studentLoadSuccessAction(user));
					return user;
				} else {
					throw new Error('User not found, or you don\'t have permission');
				}
			})
			.catch(error => {
				dispatch(studentLoadFailAction(studentId, error));
				return error;
			});
	};
};

const getAntiPlagiarismStatusRedux = (courseId: string, submissionId: number,) => {
	return (dispatch: Dispatch): Promise<AntiPlagiarismStatusResponse | string> => {
		dispatch(antiplagiarimsStatusLoadStartAction(submissionId,));
		return getAntiPlagiarismStatus(courseId, submissionId)
			.then(json => {
				dispatch(antiplagiarimsStatusLoadSuccessAction(submissionId, json,));
				return json;
			})
			.catch(error => {
				dispatch(antiplagiarimsStatusLoadFailAction(submissionId, error,));
				return error;
			});
	};
};

const prohibitFurtherManualCheckingRedux = (
	courseId: string,
	slideId: string,
	userId: string,
	prohibit: boolean,
) => {
	return (dispatch: Dispatch): Promise<Response> => {
		dispatch(studentProhibitFurtherManualCheckingStartAction(courseId, slideId, userId, prohibit,));
		return prohibitFurtherManualChecking(courseId, slideId, userId, prohibit)
			.catch(error => {
				dispatch(studentProhibitFurtherManualCheckingFailAction(courseId, slideId, userId, prohibit, error));
				return error;
			});
	};
};

const getDeadLinesRedux = (courseId: string, studentId: string) => {
	return (dispatch: Dispatch): Promise<DeadLinesResponse | string> => {
		dispatch(deadLinesLoadStartAction(courseId, studentId));
		return api.deadLines.getDeadLinesByStudentId(courseId, studentId)
			.then(r => {
				dispatch(deadLinesLoadSuccessAction(courseId, studentId, r.deadLines));
				return r;
			})
			.catch(error => {
				dispatch(deadLinesLoadFailAction(courseId, studentId, error,));
				return error;
			});
	};
};

export const redux = {
	getAntiPlagiarismStatus: getAntiPlagiarismStatusRedux,
	getStudentInfo: getStudentInfoRedux,
	prohibitFurtherManualChecking: prohibitFurtherManualCheckingRedux,
	getDeadLines: getDeadLinesRedux,
};
