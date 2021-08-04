import { buildQuery } from "src/utils";

export const coursePath = "course";
export const coursesPath = "/course/courses";
export const flashcards = "flashcards";
export const flashcardsPreview = "preview";
export const commentsPath = "comments";
export const exerciseStudentSubmissions = 'exercise/studentSubmissions';
export const submissions = 'submissions';
export const antiplagiarism = 'antiplagiarism';
export const antiPlagiarismDetailsRoute = '/AntiPlagiarism/Details';
export const favouriteReviews = 'favourite-reviews';
export const reviews = 'reviews';
export const commentPoliciesPath = "comment-policies";
export const analytics = "/analytics";
export const courseStatistics = analytics + '/courseStatistics';
export const users = 'users';
export const userSolutions = analytics + '/userSolutions';
export const slides = "slides";
export const ltiSlide = "ltislide";
export const resetStudentsLimits = "/students/reset-limits";
export const acceptedSolutions = "accepted-solutions";
export const signalrWS = 'ws';
export const login = 'login';
export const account = 'account';
export const accountPath = '/' + account + '/manage';
export const register = account + '/register';
export const logoutPath = account + '/logout';
export const rolesPath = account + '/roles';
export const accountProfile = '/' + account + '/profile';
export const externalLoginConfirmation = login + '/externalLoginConfirmation';
export const externalLoginCallback = login + '/externalLoginCallback';
export const feed = 'feed';
export const notificationsFeed = feed + '/notificationsPartial';
export const groups = 'groups';

export function constructPathToSlide(courseId: string, slideId: string): string {
	return `/${ coursePath }/${ courseId }/${ slideId }`;
}

export function constructPathToComment(commentId: number, isLike?: boolean): string {
	const url = `${ commentsPath }/${ commentId }`;

	if(isLike) {
		return url + "/like";
	}

	return url;
}

export function constructPathToStudentSubmissions(courseId: string, slideId: string): string {
	return `/${ exerciseStudentSubmissions }?courseId=${ courseId }&slideId=${ slideId }`;
}

export function constructPathToFlashcardsPreview(courseId: string, openUnitId?: string | null): string {
	const unitIdQuery = buildQuery({ unitId: openUnitId });
	const url = `/${ coursePath }/${ courseId }/${ flashcards }/${ flashcardsPreview }`;

	return unitIdQuery ? url + unitIdQuery : url;
}

export function constructLinkWithReturnUrl(link: string, returnUrl?: string): string {
	return `/${ link }${ buildQuery({ returnUrl: returnUrl || window.location.pathname }) }`;
}

export function getUserSolutionsUrl(courseId: string, slideId: string, userId: string): string {
	return userSolutions + buildQuery({ courseId, slideId, userId });
}

export function constructPathToAccount(userId: string): string {
	return accountProfile + buildQuery({ userId });
}

export function constructPathToGroup(courseId: string, groupId: number): string {
	return `/${ courseId }/${ groups }/${ groupId }`;
}
