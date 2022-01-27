import { GroupStudentInfo } from "src/models/groups";
import { CourseInfo } from "src/models/course";
import { DeadLineInfo, DeadLinesResponse } from "src/models/deadLines";

export interface Props {
	getDeadLines: (courseId: string, groupId: number) => Promise<DeadLinesResponse>;
	createDeadLine: (courseId: string, deadLine: DeadLineInfo) => Promise<DeadLineInfo>;
	changeDeadLine: (deadLine: DeadLineInfo) => Promise<Response>;
	deleteDeadLine: (deadLineId: string) => Promise<Response>;

	getStudents: (groupId: number) => Promise<{ students: GroupStudentInfo[] }>;
	getCourse: (courseId: string) => Promise<CourseInfo>;
	courseId: string;
	groupId: number;
}

export interface State {
	responseDeadLines: { [id: string]: StateDeadLineInfo; };
	actualDeadLines: { [id: string]: StateDeadLineInfo; };
	errors: ValidationErrorsContainer;

	unitsMarkup: Markup[];
	studentsMarkup: Markup[];
	slidesMarkupByUnit: {
		[unitId: string]: Markup[];
	};
}

export type Markup =  [id: string, title: string];

export interface ValidationErrorsContainer {
	[id: string]: ValidationErrorType;
}

export type ValidationErrorType = 'time' | 'date' | 'time&date';

export interface DeadLineModuleInfo {
	id: string;
	title: string;
	slides: DeadLineSlideInfo[];
}

export interface DeadLineSlideInfo {
	id: string;
	title: string;
}

export interface StateDeadLineInfo extends Omit<DeadLineInfo, 'date'> {
	date?: string;
	time?: string;
	isOverlappedByOtherDeadLine?: boolean;
}
