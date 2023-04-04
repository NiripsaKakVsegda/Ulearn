import api from "src/api/index";
import { groups, resetStudentsLimits } from "src/consts/routes";
import {
	CopyGroupResponse,
	GroupAccessesResponse,
	GroupInfo,
	GroupScoringGroupsResponse, GroupsInfoResponse, GroupsListParameters,
	GroupStudentsResponse
} from "src/models/groups";
import { buildQuery } from "src/utils";
import { Dispatch } from "redux";
import { groupLoadFailAction, groupLoadStartAction, groupLoadSuccessAction } from "src/actions/groups";

// Groups
export function getCourseGroups(courseId: string, userId?: string,): Promise<{ groups: GroupInfo[] }> {
	const url = groups + buildQuery({ courseId, userId, });
	return api.get(url);
}

export function getCourseGroupsRedux(courseId: string, userId: string, archived?: boolean,) {
	return (dispatch: Dispatch): Promise<GroupsInfoResponse | string> => {
		dispatch(groupLoadStartAction(userId));
		const url = groups + buildQuery({ courseId, userId, archived, });
		return api.get<GroupsInfoResponse>(url)
			.then(json => {
				dispatch(groupLoadSuccessAction(userId, json));
				return json;
			})
			.catch(error => {
				dispatch(groupLoadFailAction(userId, error));
				return error;
			});
	};
}

export function getCourseArchivedGroups(courseId: string,
	params: Partial<GroupsListParameters>
): Promise<{ groups: GroupInfo[] }> {
	const url = groups + buildQuery({ courseId, archived: true, ...params, });
	return api.get(url);
}

// Group
export function getGroup(groupId: number): Promise<GroupInfo> {
	return api.get(`${ groups }/${ groupId }`);
}

export function createGroup(courseId: string, name: string): Promise<Response> {
	const url = groups + buildQuery({ courseId });
	return api.post(url, api.createRequestParams({ name }));
}

export function copyGroup(groupId: number, destinationCourseId: string,
	makeMeOwner: boolean
): Promise<CopyGroupResponse> {
	const url = `${ groups }/${ groupId }/copy` + buildQuery({ destinationCourseId, makeMeOwner, });
	return api.post(url);
}

export function saveGroupSettings(groupId: number, groupSettings: Record<string, unknown>): Promise<GroupInfo> {
	return api.patch(`${ groups }/${ groupId }`,
		api.createRequestParams(groupSettings));
}

export function deleteGroup(groupId: number): Promise<Response> {
	return api.delete(`${ groups }/${ groupId }`);
}

export function changeGroupOwner(groupId: number, ownerId: string): Promise<Response> {
	return api.put(`${ groups }/${ groupId }/owner`, api.createRequestParams({ ownerId }));
}

// Scores
export function getGroupScores(groupId: number): Promise<GroupScoringGroupsResponse> {
	return api.get(`${ groups }/${ groupId }/scores`);
}

export function saveScoresSettings(groupId: number, checkedScoresSettingsIds: string[]): Promise<Response> {
	return api.post(`${ groups }/${ groupId }/scores`,
		api.createRequestParams({ 'scores': checkedScoresSettingsIds }));
}

// Accesses
export function getGroupAccesses(groupId: number): Promise<GroupAccessesResponse> {
	return api.get(`${ groups }/${ groupId }/accesses`);
}

export function addGroupAccesses(groupId: number, userId: string): Promise<Response> {
	return api.post(`${ groups }/${ groupId }/accesses/${ userId }`);
}

export function removeAccess(groupId: number, userId: string): Promise<Response> {
	return api.delete(`${ groups }/${ groupId }/accesses/${ userId }`);
}

// Students
export function getStudents(groupId: number): Promise<GroupStudentsResponse> {
	return api.get(`${ groups }/${ groupId }/students`);
}

export function deleteStudents(groupId: number, studentIds: string[]): Promise<Response> {
	return api.delete(`${ groups }/${ groupId }/students`,
		api.createRequestParams({ studentIds }));
}

export function copyStudents(groupId: number, studentIds: string[]): Promise<Response> {
	return api.post(`${ groups }/${ groupId }/students`,
		api.createRequestParams({ studentIds }));
}

export function resetLimitsForStudents(groupId: number, studentIds: string[]): Promise<Response> {
	return api.post(`${ groups }/${ groupId }/${ resetStudentsLimits }`,
		api.createRequestParams({ studentIds }));
}
