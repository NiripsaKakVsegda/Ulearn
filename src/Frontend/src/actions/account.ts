import { CourseAccessType, CourseRoleType, } from "src/consts/accessType";
import { AccountInfo, RolesInfo, } from "src/models/account";
import {
	ACCOUNT__USER_HIJACK,
	ACCOUNT__USER_INFO_UPDATED,
	ACCOUNT__USER_ROLES_UPDATED,
	AccountAction,
} from "src/actions/account.types";

export const userProgressHijackAction = (isHijacked: boolean): AccountAction => ({
	type: ACCOUNT__USER_HIJACK,
	isHijacked,
});

export const accountInfoUpdateAction = ({
		isAuthenticated,
		user,
		accountProblems,
		systemAccesses,
	}: AccountInfo
): AccountAction => ({
	type: ACCOUNT__USER_INFO_UPDATED,
	isAuthenticated,
	user,
	accountProblems,
	systemAccesses,
});

export const rolesUpdateAction = ({
	courseRoles,
	courseAccesses,
	groupsAsStudent,
	isSystemAdministrator,
}: RolesInfo): AccountAction => {
	const courseRolesObject: { [courseId: string]: CourseRoleType } = {};
	if(courseRoles) {
		courseRoles.forEach(c => courseRolesObject[c.courseId.toLowerCase()] = c.role);
	}

	const courseAccessesObject: { [courseId: string]: CourseAccessType[] } = {};
	if(courseAccesses) {
		courseAccesses.forEach(c => courseAccessesObject[c.courseId.toLowerCase()] = c.accesses);
	}

	return {
		type: ACCOUNT__USER_ROLES_UPDATED,
		groupsAsStudent,
		isSystemAdministrator,
		roleByCourse: courseRolesObject,
		accessesByCourse: courseAccessesObject,
	};
};
