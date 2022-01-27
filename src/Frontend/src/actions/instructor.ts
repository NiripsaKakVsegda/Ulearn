import {
	ANTIPLAGIARISM_STATUS_LOAD_FAIL,
	ANTIPLAGIARISM_STATUS_LOAD_START,
	ANTIPLAGIARISM_STATUS_LOAD_SUCCESS,
	AntiPlagiarismStatusLoadFailAction,
	AntiPlagiarismStatusLoadStartAction,
	AntiPlagiarismStatusLoadSuccessAction,
	DeadLinesLoadFailAction,
	DeadLinesLoadStartAction,
	DeadLinesLoadSuccessAction,
	INSTRUCTOR_DEAD_LINES_LOAD_FAIL,
	INSTRUCTOR_DEAD_LINES_LOAD_START,
	INSTRUCTOR_DEAD_LINES_LOAD_SUCCESS,
	INSTRUCTOR_STUDENT_INFO_LOAD_FAIL,
	INSTRUCTOR_STUDENT_INFO_LOAD_START,
	INSTRUCTOR_STUDENT_INFO_LOAD_SUCCESS,
	INSTRUCTOR_STUDENT_MODE_TOGGLE,
	INSTRUCTOR_STUDENT_PROHIBIT_FURTHER_MANUAL_CHECKING_FAIL,
	INSTRUCTOR_STUDENT_PROHIBIT_FURTHER_MANUAL_CHECKING_LOAD,
	INSTRUCTOR_STUDENT_PROHIBIT_FURTHER_MANUAL_CHECKING_START,
	StudentInfoLoadFailAction,
	StudentInfoLoadStartAction,
	StudentInfoLoadSuccessAction,
	StudentModeAction,
	StudentProhibitFurtherManualCheckingFailAction,
	StudentProhibitFurtherManualCheckingLoadAction,
	StudentProhibitFurtherManualCheckingStartAction,
} from 'src/actions/instructor.types';
import { ShortUserInfo } from "src/models/users";
import { AntiPlagiarismStatusResponse, } from "../models/instructor";
import { DeadLineInfo } from "src/models/deadLines";

export const studentModeToggleAction = (isStudentMode: boolean): StudentModeAction => ({
	type: INSTRUCTOR_STUDENT_MODE_TOGGLE,
	isStudentMode,
});

export const studentProhibitFurtherManualCheckingStartAction = (
	courseId: string,
	slideId: string,
	userId: string,
	prohibit: boolean,
): StudentProhibitFurtherManualCheckingStartAction => ({
	type: INSTRUCTOR_STUDENT_PROHIBIT_FURTHER_MANUAL_CHECKING_START,
	courseId,
	slideId,
	userId,
	prohibit,
});

export const studentProhibitFurtherManualCheckingLoadAction = (
	courseId: string,
	slideId: string,
	userId: string,
	prohibit: boolean,
): StudentProhibitFurtherManualCheckingLoadAction => ({
	type: INSTRUCTOR_STUDENT_PROHIBIT_FURTHER_MANUAL_CHECKING_LOAD,
	courseId,
	slideId,
	userId,
	prohibit,
});

export const studentProhibitFurtherManualCheckingFailAction = (
	courseId: string,
	slideId: string,
	userId: string,
	prohibit: boolean,
	error: string,
): StudentProhibitFurtherManualCheckingFailAction => ({
	type: INSTRUCTOR_STUDENT_PROHIBIT_FURTHER_MANUAL_CHECKING_FAIL,
	courseId,
	slideId,
	userId,
	prohibit,
	error,
});

export const studentLoadStartAction = (studentId: string,): StudentInfoLoadStartAction => ({
	type: INSTRUCTOR_STUDENT_INFO_LOAD_START,
	studentId,
});

export const studentLoadSuccessAction = (
	userInfo: ShortUserInfo,
): StudentInfoLoadSuccessAction => ({
	type: INSTRUCTOR_STUDENT_INFO_LOAD_SUCCESS,
	studentId: userInfo.id,
	userInfo,
});

export const studentLoadFailAction = (
	studentId: string,
	error: string,
): StudentInfoLoadFailAction => ({
	type: INSTRUCTOR_STUDENT_INFO_LOAD_FAIL,
	studentId,
	error,
});

export const antiplagiarimsStatusLoadStartAction = (
	submissionId: number,
): AntiPlagiarismStatusLoadStartAction => ({
	type: ANTIPLAGIARISM_STATUS_LOAD_START,
	submissionId,
});

export const antiplagiarimsStatusLoadSuccessAction = (
	submissionId: number,
	response: AntiPlagiarismStatusResponse,
): AntiPlagiarismStatusLoadSuccessAction => ({
	type: ANTIPLAGIARISM_STATUS_LOAD_SUCCESS,
	submissionId,
	response,
});

export const antiplagiarimsStatusLoadFailAction = (
	submissionId: number,
	error: string,
): AntiPlagiarismStatusLoadFailAction => ({
	type: ANTIPLAGIARISM_STATUS_LOAD_FAIL,
	submissionId,
	error,
});

export const deadLinesLoadStartAction = (
	courseId: string,
	studentId: string,
): DeadLinesLoadStartAction => ({
	type: INSTRUCTOR_DEAD_LINES_LOAD_START,
	courseId,
	studentId,
});

export const deadLinesLoadSuccessAction = (
	courseId: string,
	studentId: string,
	deadLines: DeadLineInfo[],
): DeadLinesLoadSuccessAction => ({
	type: INSTRUCTOR_DEAD_LINES_LOAD_SUCCESS,
	courseId,
	studentId,
	deadLines,
});

export const deadLinesLoadFailAction = (
	courseId: string,
	studentId: string,
	error: string,
): DeadLinesLoadFailAction => ({
	type: INSTRUCTOR_DEAD_LINES_LOAD_FAIL,
	courseId,
	studentId,
	error,
});
