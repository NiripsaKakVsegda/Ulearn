import { SubmissionInfo, } from "./exercise";
import { ShortUserInfo } from "./users";

export interface SubmissionsResponse {
	submissions: SubmissionInfo[];
	prohibitFurtherManualChecking: boolean;
}

export type SuspicionLevel = 'none' | 'faint' | 'strong';

export interface AntiPlagiarismInfo {
	suspicionLevel: SuspicionLevel;
	suspiciousAuthorsCount: number;
}

export interface AntiPlagiarismStatusResponse extends AntiPlagiarismInfo {
	status: "notChecked" | "checked";
}

export interface FavouriteReview {
	renderedText: string;
	text: string;
	id: number;
}


export interface FavouriteReviewResponse {
	favouriteReviews: FavouriteReview[];
	userFavouriteReviews: FavouriteReview[];
	lastUsedReviews: string[];
}

export interface ReviewQueueFilterParameters {
	courseId: string;
	studentsFilter?: StudentsFilter;
	groupIds?: number[];
	studentIds?: string[];
	slideIds?: string[];
	sort?: DateSort;
	count?: number;
}

export interface ReviewQueueHistoryFilterParameters extends ReviewQueueFilterParameters {
	minTimestamp?: string;
}
export interface ReviewQueueMetaFilterParameters extends ReviewQueueFilterParameters {
	history?: boolean;
	minTimestamp?: string;
}

export enum StudentsFilter {
	All = 'all',
	MyGroups = 'mygroups',
	GroupIds = 'groupids',
	StudentIds = 'studentids'
}

export enum DateSort {
	Ascending = 'ascending',
	Descending = 'descending'
}

export interface ReviewQueueResponse {
	checkings: ReviewQueueItem[];
}

export interface ReviewQueueItem {
	type: QueueItemType;
	submissionId: number;
	slideId: string;
	user: ShortUserInfo;
	timestamp: string;
	score?: number;
	maxScore: number;
	lockedBy?: ShortUserInfo;
	lockedUntil?: string;
	checkedTimestamp?: string;
	checkedBy?: ShortUserInfo;
	reviews?: ShortReviewInfo[];
}

export interface ReviewQueueMetaResponse {
	checkings: ShortReviewQueueItem[];
}

export interface ShortReviewQueueItem {
	submissionId: number;
	slideId: string;
	userId: string;
	lockedById?: string;
	lockedUntil?: string;
}

export interface ShortReviewInfo {
	commentId: number;
	author: ShortUserInfo;
	codeFragment: string;
	comment: string;
}

export enum QueueItemType {
	Exercise,
	Quiz,
}
