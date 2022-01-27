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
	students: GroupStudentInfo[];
	modules: { [id: string]: DeadLineModuleInfo };
	responseDeadLines: { [id: string]: StateDeadLineInfo; };
	actualDeadLines: { [id: string]: StateDeadLineInfo; };
	errors: { [id: string]: 'time' | 'date' | 'time&date' };
}

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
	error?: boolean;
}
