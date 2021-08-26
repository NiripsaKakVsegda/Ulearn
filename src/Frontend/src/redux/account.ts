import {
	ACCOUNT__USER_HIJACK,
	ACCOUNT__USER_INFO_UPDATED,
	ACCOUNT__USER_ROLES_UPDATED,
	AccountAction,
} from "src/actions/account.types";
import { CourseAccessType, CourseRoleType, SystemAccessType } from "src/consts/accessType";

import { ShortUserInfo } from "src/models/users";
import { AccountProblem } from "src/models/account";
import { GroupAsStudentInfo } from "../models/groups";

export interface AccountState extends Partial<ShortUserInfo> {
	accountLoaded: boolean;
	isAuthenticated: boolean;
	isSystemAdministrator: boolean;
	isHijacked: boolean;

	accountProblems: AccountProblem[];
	systemAccesses: SystemAccessType[];
	roleByCourse: { [courseId: string]: CourseRoleType };
	accessesByCourse: { [courseId: string]: CourseAccessType[] };
	groupsAsStudent: GroupAsStudentInfo[];
}

const initialAccountState: AccountState = {
	accountLoaded: false,
	isAuthenticated: false,
	isSystemAdministrator: false,
	isHijacked: false,

	accountProblems: [],
	systemAccesses: [],
	roleByCourse: {},
	accessesByCourse: {},
	groupsAsStudent: [],
};

function accountReducer(state = initialAccountState, action: AccountAction): AccountState {
	switch (action.type) {
		case ACCOUNT__USER_INFO_UPDATED: {
			const newState = { ...state };
			newState.isAuthenticated = action.isAuthenticated;
			newState.accountLoaded = true;
			if(newState.isAuthenticated) {
				newState.id = action.user?.id;
				newState.login = action.user?.login;
				newState.firstName = action.user?.firstName;
				newState.lastName = action.user?.lastName;
				newState.visibleName = action.user?.visibleName;
				newState.avatarUrl = action.user?.avatarUrl;
				newState.accountProblems = action.accountProblems || [];
				newState.systemAccesses = action.systemAccesses || [];
				newState.gender = action.user?.gender;
			} else {
				newState.id = undefined;
				newState.login = undefined;
				newState.firstName = undefined;
				newState.lastName = undefined;
				newState.visibleName = undefined;
				newState.avatarUrl = undefined;
				newState.accountProblems = [];
				newState.systemAccesses = [];
				newState.gender = undefined;
			}
			return newState;
		}
		case ACCOUNT__USER_ROLES_UPDATED:
			return {
				...state,
				isSystemAdministrator: action.isSystemAdministrator,
				roleByCourse: action.roleByCourse,
				accessesByCourse: action.accessesByCourse,
				groupsAsStudent: action.groupsAsStudent,
			};
		case ACCOUNT__USER_HIJACK:
			return {
				...state,
				isHijacked: action.isHijacked,
			};
		default:
			return state;
	}
}

export default accountReducer;
