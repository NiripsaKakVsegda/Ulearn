import {
	GROUPS_LOAD_START,
	GROUPS_LOAD_SUCCESS,
	GROUPS_LOAD_FAIL,
	GroupsLoadStartAction,
	GroupsLoadSuccessAction,
	GroupsLoadFailAction,
} from "./groups.types";
import { GroupsInfoResponse } from "src/models/groups";

export const groupLoadStartAction = (userId: string,): GroupsLoadStartAction => ({
	type: GROUPS_LOAD_START,
	userId,
});

export const groupLoadSuccessAction = (userId: string,
	groupsInfoResponse: GroupsInfoResponse,
): GroupsLoadSuccessAction => ({
	type: GROUPS_LOAD_SUCCESS,
	userId,
	...groupsInfoResponse,
});

export const groupLoadFailAction = (
	userId: string,
	error: string,
): GroupsLoadFailAction => ({
	type: GROUPS_LOAD_FAIL,
	userId,
	error,
});




