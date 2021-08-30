import {
	INSTRUCTOR_STUDENT_MODE_TOGGLE,
	StudentModeAction,

	INSTRUCTOR_STUDENT_PROHIBIT_FURTHER_MANUAL_CHECKING_START,
	INSTRUCTOR_STUDENT_PROHIBIT_FURTHER_MANUAL_CHECKING_FAIL,
	StudentProhibitFurtherManualCheckingStartAction,
	StudentProhibitFurtherManualCheckingFailAction,

	INSTRUCTOR_STUDENT_INFO_LOAD_START,
	INSTRUCTOR_STUDENT_INFO_LOAD_FAIL,
	INSTRUCTOR_STUDENT_INFO_LOAD_SUCCESS,
	StudentInfoLoadFailAction,
	StudentInfoLoadStartAction,
	StudentInfoLoadSuccessAction,

	ANTIPLAGIARISM_STATUS_LOAD_START,
	ANTIPLAGIARISM_STATUS_LOAD_SUCCESS,
	ANTIPLAGIARISM_STATUS_LOAD_FAIL,
	AntiPlagiarismStatusLoadStartAction,
	AntiPlagiarismStatusLoadSuccessAction,
	AntiPlagiarismStatusLoadFailAction,
	INSTRUCTOR_STUDENT_PROHIBIT_FURTHER_MANUAL_CHECKING_LOAD,
	StudentProhibitFurtherManualCheckingLoadAction,
} from 'src/actions/instructor.types';
import { ShortUserInfo } from "src/models/users";
import {
	AntiPlagiarismStatusResponse,
} from "../models/instructor";

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
