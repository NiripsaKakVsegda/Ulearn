import { GoogleSheetsExportTaskResponse } from "src/models/googleSheet";
import moment from "moment/moment";
import React from "react";

export default {
	buildAuthor: (visibleName: string): React.ReactText => `Создатель: ${ visibleName }`,
	buildUploadTimeRange: (task: GoogleSheetsExportTaskResponse): React.ReactText => `Выгрузка с ${ moment(
		task.refreshStartDate).format('DD.MM.yyyy') } по ${ moment(task.refreshEndDate).format('DD.MM.yyyy') }`,
};
