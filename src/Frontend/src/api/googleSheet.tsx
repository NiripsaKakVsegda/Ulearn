import api from "./index";
import { buildQuery } from "../utils";
import { GoogleSheetsExportTaskListResponse, GoogleSheetsExportTaskResponse } from "../models/googleSheet";

const courseStatisticsGoogleSheet = `course-statistics/export/to-google-sheets/tasks`;

export function getAllCourseTasks(courseId: string): Promise<GoogleSheetsExportTaskListResponse> {
	const url = courseStatisticsGoogleSheet + buildQuery({ courseId });
	return api.get(url);
}

export function updateCourseTask(
	taskId: number,
	params: {
		courseId: string,
		isVisibleForStudents: boolean,
		refreshStartDate?: string,
		refreshEndDate?: string,
		refreshTimeInMinutes?: number,
		spreadsheetId: string,
		listId: number
	}
): Promise<GoogleSheetsExportTaskResponse> {
	const url = `${ courseStatisticsGoogleSheet }/${ taskId }`;
	return api.patch(url,
		api.createRequestParams(params));
}

export function exportTaskNow(taskId: number): Promise<GoogleSheetsExportTaskResponse> {
	const url = `${ courseStatisticsGoogleSheet }/${ taskId }`;
	return api.post(url);
}

export function deleteTask(courseId: string, taskId: number,): Promise<GoogleSheetsExportTaskResponse> {
	const url = `${ courseStatisticsGoogleSheet }/${ taskId }`;
	return api.delete(url, api.createRequestParams({ courseId: courseId }));
}
