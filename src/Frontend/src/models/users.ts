import { CourseRoleType, LmsRoleType } from "../consts/accessType";

export enum Gender {
	Male = 'male',
	Female = 'female',
}

export interface ShortUserInfo {
	id: string;
	login?: string;
	email?: string;
	firstName: string;
	lastName: string;
	visibleName: string;
	avatarUrl: string | null;
	gender?: Gender;
}

export interface UsersByIdsResponse {
	foundUsers: ShortUserInfo[];
	notFoundUserIds: string[];
}

export interface UsersSearchResponse {
	users: FoundUserResponse[];
}

export interface FoundUserResponse {
	user: ShortUserInfo;
	fields: SearchField[];
}

export enum SearchField {
	UserId,
	Login,
	Name,
	Email,
	SocialLogin,
}

export interface UsersSearchParameters {
	userId: string;
	query: string;
	courseId: string;
	courseRole: CourseRoleType;
	lmsRole: LmsRoleType;
	offset: number;
	count: number;
}
