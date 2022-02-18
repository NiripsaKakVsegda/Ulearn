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

	unitsMarkup: Markup<string>[];
	studentsMarkup: Markup<string>[];
	slidesMarkupByUnit: {
		[unitId: string]: Markup<SlidesMarkupValue>[];
	};
}

export type Markup<T> = [value: T, title: string];

export interface SlidesMarkupValue {
	id: string;
	isScoringGroup?: boolean;
}

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
	scoringGroupId: string | null;
}

export interface StateDeadLineInfo extends Omit<DeadLineInfo, 'date'> {
	//format DD.MM.YYYY, local timezone
	date?: string;
	//format HH:mm, local timezone
	time?: string;
	isOverlappedByOtherDeadLine?: boolean;
}
