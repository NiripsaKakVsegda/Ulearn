import { superGroup, } from "src/consts/routes";
import { buildQuery } from "src/utils";
import api from "./index";
import {
	SuperGroupSheetExtractionResult,
	SuperGroupUpdateItem,
	UpdateGroupsRequestParameters,
	MoveStudentInfo,
	SuperGroupMoveUserResponse
} from "src/models/superGroup";

export function extractFromTable(spreadsheetUrl: string, groupId: number): Promise<SuperGroupSheetExtractionResult> {
	const url = `${ superGroup }/extract-spreadsheet` + buildQuery({ spreadsheetUrl, groupId });
	return api.get(url);
}

export function updateSuperGroup(groupId: number,
	request: UpdateGroupsRequestParameters
): Promise<{ [groupId: string]: SuperGroupUpdateItem }> {
	return api.post(`${ superGroup }/update-groups` + buildQuery({ groupId }), api.createRequestParams({ ...request }));
}

export function resortSuperGroupStudents(groupId: number,
	neededMoves: {
		[studentName: string]: MoveStudentInfo;
	}
): Promise<SuperGroupMoveUserResponse> {
	return api.post(`${ superGroup }/resort-students` + buildQuery({ groupId }), api.createRequestParams({ ...neededMoves }));
}
