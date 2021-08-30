import { CourseAccessType, CourseRoleType, } from "src/consts/accessType";
import { AccountInfo, } from "src/models/account";
import { GroupAsStudentInfo } from "src/models/groups";

export const ACCOUNT__USER_HIJACK = "ACCOUNT__USER_HIJACK";
export const ACCOUNT__USER_INFO_UPDATED = 'ACCOUNT__USER_INFO_UPDATED';
export const ACCOUNT__USER_ROLES_UPDATED = 'ACCOUNT__USER_ROLES_UPDATED';

export interface UserProgressHijackAction {
	type: typeof ACCOUNT__USER_HIJACK,
	isHijacked: boolean,
}

export interface AccountInfoUpdateAction extends AccountInfo {
	type: typeof ACCOUNT__USER_INFO_UPDATED,
}

export interface AccountUserRolesUpdateAction {
	type: typeof ACCOUNT__USER_ROLES_UPDATED,

	isSystemAdministrator: boolean,
	roleByCourse: { [courseId: string]: CourseRoleType };
	accessesByCourse: { [courseId: string]: CourseAccessType[] };
	groupsAsStudent: GroupAsStudentInfo[];
}

export type AccountAction = UserProgressHijackAction | AccountInfoUpdateAction | AccountUserRolesUpdateAction;
