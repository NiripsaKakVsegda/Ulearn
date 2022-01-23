import { FAIL, FailAction, LOAD, loadFail, loadStart, loadSuccess, START, } from "src/consts/actions";
import { ShortUserInfo } from "src/models/users";
import { AntiPlagiarismStatusResponse, } from "src/models/instructor";
import { DeadLineInfo } from "../components/groups/GroupSettingsPage/GroupDeadLines/GroupDeadLines";

const instructor = 'INSTRUCTOR';
const student = '_STUDENT';
const info = '_INFO';
const antiPlagiarismStatus = "_ANTIPLAGIARISM_STATUS";
const deadlines = "_DEAD_LINES";
const modeToggle = '_MODE_TOGGLE';
const prohibitFurtherManualChecking = '_PROHIBIT_FURTHER_MANUAL_CHECKING';

export const INSTRUCTOR_STUDENT_MODE_TOGGLE = instructor + student + modeToggle;

export const INSTRUCTOR_STUDENT_PROHIBIT_FURTHER_MANUAL_CHECKING_START = instructor + student + prohibitFurtherManualChecking + START;
export const INSTRUCTOR_STUDENT_PROHIBIT_FURTHER_MANUAL_CHECKING_LOAD = instructor + student + prohibitFurtherManualChecking + LOAD;
export const INSTRUCTOR_STUDENT_PROHIBIT_FURTHER_MANUAL_CHECKING_FAIL = instructor + student + prohibitFurtherManualChecking + FAIL;

export const INSTRUCTOR_STUDENT_INFO_LOAD_START = instructor + student + info + loadStart;
export const INSTRUCTOR_STUDENT_INFO_LOAD_SUCCESS = instructor + student + info + loadSuccess;
export const INSTRUCTOR_STUDENT_INFO_LOAD_FAIL = instructor + student + info + loadFail;

export const ANTIPLAGIARISM_STATUS_LOAD_START = instructor + antiPlagiarismStatus + loadStart;
export const ANTIPLAGIARISM_STATUS_LOAD_SUCCESS = instructor + antiPlagiarismStatus + loadSuccess;

export const INSTRUCTOR_DEAD_LINES_LOAD_START = instructor + deadlines + loadStart;
export const INSTRUCTOR_DEAD_LINES_LOAD_SUCCESS = instructor + deadlines + loadSuccess;
export const INSTRUCTOR_DEAD_LINES_LOAD_FAIL = instructor + deadlines + loadFail;

export const ANTIPLAGIARISM_STATUS_LOAD_FAIL = instructor + antiPlagiarismStatus + loadFail;


export interface StudentModeAction {
	type: typeof INSTRUCTOR_STUDENT_MODE_TOGGLE;
	isStudentMode: boolean;
}

export interface StudentProhibitFurtherManualCheckingStartAction {
	type: typeof INSTRUCTOR_STUDENT_PROHIBIT_FURTHER_MANUAL_CHECKING_START;
	courseId: string;
	slideId: string;
	userId: string;
	prohibit: boolean;
}

export interface StudentProhibitFurtherManualCheckingLoadAction {
	type: typeof INSTRUCTOR_STUDENT_PROHIBIT_FURTHER_MANUAL_CHECKING_LOAD;
	courseId: string;
	slideId: string;
	userId: string;
	prohibit: boolean;
}

export interface StudentProhibitFurtherManualCheckingFailAction extends FailAction {
	type: typeof INSTRUCTOR_STUDENT_PROHIBIT_FURTHER_MANUAL_CHECKING_FAIL;
	courseId: string;
	slideId: string;
	userId: string;
	prohibit: boolean;
}

export interface StudentInfoLoadStartAction {
	type: typeof INSTRUCTOR_STUDENT_INFO_LOAD_START;
	studentId: string;
}

export interface StudentInfoLoadSuccessAction {
	type: typeof INSTRUCTOR_STUDENT_INFO_LOAD_SUCCESS;
	studentId: string;
	userInfo: ShortUserInfo;
}

export interface StudentInfoLoadFailAction extends FailAction {
	type: typeof INSTRUCTOR_STUDENT_INFO_LOAD_FAIL;
	studentId: string;
}

export interface AntiPlagiarismStatusLoadStartAction {
	type: typeof ANTIPLAGIARISM_STATUS_LOAD_START;
	submissionId: number;
}

export interface AntiPlagiarismStatusLoadSuccessAction {
	type: typeof ANTIPLAGIARISM_STATUS_LOAD_SUCCESS;
	submissionId: number;
	response: AntiPlagiarismStatusResponse;
}

export interface AntiPlagiarismStatusLoadFailAction extends FailAction {
	type: typeof ANTIPLAGIARISM_STATUS_LOAD_FAIL;
	submissionId: number;
}

export interface DeadLinesLoadStartAction {
	type: typeof INSTRUCTOR_DEAD_LINES_LOAD_START;
	courseId: string;
	studentId: string;
}

export interface DeadLinesLoadSuccessAction {
	type: typeof INSTRUCTOR_DEAD_LINES_LOAD_SUCCESS;
	courseId: string;
	studentId: string;
	deadLines: DeadLineInfo[];
}

export interface DeadLinesLoadFailAction extends FailAction {
	type: typeof INSTRUCTOR_DEAD_LINES_LOAD_FAIL;
	courseId: string;
	studentId: string;
}

export type InstructorAction =
	StudentModeAction

	| StudentProhibitFurtherManualCheckingStartAction

	| StudentInfoLoadStartAction
	| StudentInfoLoadSuccessAction
	| StudentInfoLoadFailAction

	| AntiPlagiarismStatusLoadStartAction
	| AntiPlagiarismStatusLoadSuccessAction
	| AntiPlagiarismStatusLoadFailAction

	| DeadLinesLoadStartAction
	| DeadLinesLoadSuccessAction
	| DeadLinesLoadFailAction
	;
