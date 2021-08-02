import api from "src/api";
import { ShortUserInfo, UsersSearchResponse } from "src/models/users";
import { buildQuery } from "src/utils";
import { users } from "src/consts/routes";

export function getCourseInstructors(courseId: string, query?: string, count?: number): Promise<UsersSearchResponse> {
	const buildedQuery = buildQuery({ courseId, courseRole: 'Instructor', query, count, });
	const url = users + buildedQuery;
	return api.get(url);
}

export function getUserInfo(userId: string,): Promise<UsersSearchResponse> {
	const query = buildQuery({ userId, });
	const url = users + query;
	return api.get(url);
}

export function getOtherUserInfo(userId: string,): Promise<ShortUserInfo> {
	const url = `${ users }/${ userId }`;
	return api.get(url);
}

