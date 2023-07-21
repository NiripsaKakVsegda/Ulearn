import { ShortUserInfo } from "./users";
import { CourseAccessType } from "../consts/accessType";

export interface CourseAccess {
	id: number;
	courseId: string;
	user: ShortUserInfo;
	grantedBy: ShortUserInfo;
	accessType: CourseAccessType;
	grantTime: string;
	expiresOn?: string;
	comment: string;
}

export interface ShortCourseAccess {
	id: number;
	courseId: string;
	grantedBy: ShortUserInfo;
	accessType: CourseAccessType;
	grantTime: string;
	expiresOn?: string;
}
