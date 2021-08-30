import { CourseAccessType, CourseRoleType, SystemAccessType } from "src/consts/accessType";
import { ShortUserInfo } from "../models/users";
import { AccountState } from "../redux/account";

function isCourseAdmin(userRoles: UserRoles): boolean {
	return userRoles.isSystemAdministrator ||
		userRoles.courseRole === CourseRoleType.courseAdmin;
}

function isInstructor(userRoles: UserRoles): boolean {
	return isCourseAdmin(userRoles) ||
		userRoles.courseRole === CourseRoleType.instructor;
}

export function buildUserInfo(user: AccountState, courseId: string,): UserInfo {
	const { isSystemAdministrator, accessesByCourse, roleByCourse, systemAccesses, isAuthenticated, } = user;
	const courseAccesses = accessesByCourse[courseId] ? accessesByCourse[courseId] : [];
	const courseRole = roleByCourse[courseId] ? roleByCourse[courseId] : CourseRoleType.student;

	return {
		...user as ShortUserInfo,

		isAuthenticated,
		isSystemAdministrator,
		courseRole,
		courseAccesses,
		systemAccesses,
	};
}

interface UserRoles {
	isSystemAdministrator: boolean;
	courseRole: CourseRoleType;
}

interface UserInfo extends ShortUserInfo, UserRoles {
	isAuthenticated: boolean;
	systemAccesses: SystemAccessType[];
	courseAccesses: CourseAccessType[];
}

export { isCourseAdmin, isInstructor, UserRoles, UserInfo, };
