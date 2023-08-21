import api from "src/api/index";
import { groups } from "src/consts/routes";
import { CreateGroupResponse, GroupInfo, GroupsInfoResponse, GroupType } from "src/models/groups";
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

// Group

export function createGroup(courseId: string, name: string, groupType: GroupType): Promise<CreateGroupResponse> {
	const url = groups + buildQuery({ courseId });
	return api.post(url, api.createRequestParams({ name, groupType }));
}

export function deleteGroup(groupId: number): Promise<Response> {
	return api.delete(`${ groups }/${ groupId }`);
}

// Invite hash
export function joinGroupByInviteHash(inviteHash: string): Promise<Response> {
	const url = `${ groups }/${ inviteHash }/join`;
	return api.post(url);
}

export function getGroupByInviteHash(inviteHash: string): Promise<GroupInfo> {
	const url = `${ groups }/${ inviteHash }`;
	return api.get(url);
}
