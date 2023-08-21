import { superGroup, } from "src/consts/routes";
import { buildQuery } from "src/utils";
import api from "./index";
import {
	MoveStudentInfo,
	SuperGroupMoveUserResponse,
	SuperGroupSheetExtractionResult,
	SuperGroupUpdateItem,
	UpdateGroupsRequestParameters
} from "src/models/superGroup";
import { ChangeableGroupSettings, GroupScoringGroupsResponse } from "../models/groups";

export function extractFromTable(spreadsheetUrl: string, groupId: number): Promise<SuperGroupSheetExtractionResult> {
	const url = `${ superGroup }/extract-spreadsheet` + buildQuery({ spreadsheetUrl, groupId });
	return api.get(url);
}

export function updateSuperGroup(
	groupId: number,
	request: UpdateGroupsRequestParameters
): Promise<{ [groupId: string]: SuperGroupUpdateItem }> {
	return api.post(`${ superGroup }/update-groups` + buildQuery({ groupId }), api.createRequestParams({ ...request }));
}

export function resortSuperGroupStudents(
	groupId: number,
	neededMoves: {
		[studentName: string]: MoveStudentInfo;
	}
): Promise<SuperGroupMoveUserResponse> {
	return api.post(
		`${ superGroup }/resort-students` + buildQuery({ groupId }),
		api.createRequestParams({ ...neededMoves })
	);
}

// Scores
export function getGroupScores(superGroupId: number): Promise<GroupScoringGroupsResponse> {
	return api.get(`${ superGroup }/${ superGroupId }/scores`);
}

export function saveScoresSettings(superGroupId: number, checkedScoresSettingsIds: string[]): Promise<Response> {
	return api.post(
		`${ superGroup }/${ superGroupId }/scores`,
		api.createRequestParams({ "scores": checkedScoresSettingsIds })
	);
}

export function getGroupSettings(superGroupId: number): Promise<ChangeableGroupSettings> {
	return api.get(`${ superGroup }/${ superGroupId }/settings`);
}

export function updateGroupSettings(superGroupId: number, settings: ChangeableGroupSettings): Promise<Response> {
	return api.post(`${ superGroup }/${ superGroupId }/settings`, api.createRequestParams({ ...settings }));
}
