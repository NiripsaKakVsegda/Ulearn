import { SubmissionInfo, } from "./exercise";

export interface SubmissionsResponse {
	submissions: SubmissionInfo[];
	submissionsScores: { [submissionId: number]: number; };
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
}

export interface ReviewQueueResponse {
	checkings: ReviewQueueItem[];
}

export interface ReviewQueueItem {
	isLocked: boolean;
	submissionId: number;
	userId: string;
	type: QueueItemType;
}

export enum QueueItemType {
	Exercise,
	Quiz,
}
