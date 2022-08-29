import { GoogleSheetsExportTaskResponse } from "src/models/googleSheet";
import moment from "moment/moment";
import React from "react";

export default {
	buildUploadTimeRange: (task: GoogleSheetsExportTaskResponse): string => `Выгрузка с ${ moment(
		task.refreshStartDate).format('DD.MM.yyyy') } по ${ moment(task.refreshEndDate).format('DD.MM.yyyy') }`,
	isVisibleForStudents:'Видна студентам',
	isInvisibleForStudents:'Не видна студентам',
	unloadingActive:'Выгрузка активна',
	unloadingInactive:'Выгрузка неактивна',
};
