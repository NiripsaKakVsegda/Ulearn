import { ShortGroupInfo } from "./comments";
import { ShortUserInfo } from "./users";

export interface GoogleSheetsExportTaskResponse {
	id: number;
	groups: ShortGroupInfo[];
	authorInfo: ShortUserInfo;
	isVisibleForStudents: boolean;
	refreshStartDate?: string;
	refreshEndDate?: string;
	refreshTimeInMinutes: number;
	//time in UTC
	lastUpdateDate: string | null;
	lastUpdateErrorMessage: string | null;
	spreadsheetId: string;
	listId: number;
}

export interface GoogleSheetsCreateTaskParams extends GoogleSheetsExportTaskUpdateParams {
	courseId: string;
	groupsIds: number[];
}

export interface GoogleSheetsExportTaskUpdateParams {
	isVisibleForStudents: boolean;
	refreshStartDate?: string;
	refreshEndDate?: string;
	refreshTimeInMinutes: number;
	spreadsheetId: string;
	listId: number;
}

export interface GoogleSheetsExportTaskListResponse {
	googleSheetsExportTasks: GoogleSheetsExportTaskResponse[];
}
