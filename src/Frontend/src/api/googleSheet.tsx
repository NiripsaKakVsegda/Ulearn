import api from "./index";
import { buildQuery } from "../utils";
import {
	GoogleSheetsCreateTaskParams,
	GoogleSheetsExportTaskListResponse,
	GoogleSheetsExportTaskResponse,
	GoogleSheetsExportTaskUpdateParams,
} from "../models/googleSheet";
import { courseStatisticsGoogleSheet } from "src/consts/routes";


export function getAllCourseTasks(courseId: string): Promise<GoogleSheetsExportTaskListResponse> {
	const url = courseStatisticsGoogleSheet + buildQuery({ courseId });
	return api.get(url);
}

export function getTaskById(taskId: number): Promise<GoogleSheetsExportTaskResponse> {
	const url = `${ courseStatisticsGoogleSheet }/${ taskId }`;
	return api.get(url);
}

export function addNewTask(params: GoogleSheetsCreateTaskParams): Promise<GoogleSheetsExportTaskResponse> {
	return api.post(
		courseStatisticsGoogleSheet,
		api.createRequestParams({ ...params })
	);
}

export function exportTaskNow(taskId: number): Promise<Response> {
	const url = `${ courseStatisticsGoogleSheet }/${ taskId }`;
	return api.post(url);
}

export function updateCourseTask(
	taskId: number,
	params: Partial<GoogleSheetsExportTaskUpdateParams>
): Promise<Response> {
	const url = `${ courseStatisticsGoogleSheet }/${ taskId }`;
	return api.patch(
		url,
		api.createRequestParams({ ...params })
	);
}

export function deleteTask(taskId: number,): Promise<Response> {
	const url = `${ courseStatisticsGoogleSheet }/${ taskId }`;
	return api.delete(url);
}
