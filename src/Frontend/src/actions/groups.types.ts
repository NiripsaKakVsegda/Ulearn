import { FailAction, loadFail, loadStart, loadSuccess, } from "src/consts/actions";
import { GroupsInfoResponse } from "src/models/groups";
import { Action } from "redux";

const groups = 'GROUP_';
export const GROUPS_LOAD_START = groups + loadStart;
export const GROUPS_LOAD_SUCCESS = groups + loadSuccess;
export const GROUPS_LOAD_FAIL = groups + loadFail;


interface GroupsLoadAction<T> extends Action<T> {
	userId: string;
}

export type GroupsLoadStartAction = GroupsLoadAction<typeof GROUPS_LOAD_START>;
export type GroupsLoadSuccessAction = GroupsLoadAction<typeof GROUPS_LOAD_SUCCESS> & GroupsInfoResponse;
export type GroupsLoadFailAction = GroupsLoadAction<typeof GROUPS_LOAD_FAIL> & FailAction;

export type GroupsAction =
	GroupsLoadStartAction
	| GroupsLoadSuccessAction
	| GroupsLoadFailAction
	;
