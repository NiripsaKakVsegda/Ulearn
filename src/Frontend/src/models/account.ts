import { AccountProblemType } from "src/consts/accountProblemType";
import { CourseAccessType, CourseRoleType, SystemAccessType } from "src/consts/accessType";
import { ShortUserInfo } from "src/models/users";
import { GroupAsStudentInfo } from "src/models/groups";

export interface LogoutInfo {
	logout: boolean;
}

export interface AccountProblem {
	title: string;
	description: string;
	problemType: AccountProblemType;
}

export interface AccountInfo {
	isAuthenticated: boolean;
	user?: ShortUserInfo;
	accountProblems?: AccountProblem[];
	systemAccesses?: SystemAccessType[];
}

export interface RolesInfo {
	isSystemAdministrator: boolean;
	courseRoles: { courseId: string; role: CourseRoleType }[];
	courseAccesses: { courseId: string; accesses: CourseAccessType[] }[];
	groupsAsStudent: GroupAsStudentInfo[];
}
