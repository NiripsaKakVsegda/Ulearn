import React from "react";

import { UserInfo } from "src/utils/courseRoles";
import { ShortGroupInfo } from "src/models/comments";
import {
	AntiPlagiarismStatusResponse,
	FavouriteReview,
	FavouriteReviewResponse
} from "src/models/instructor";
import { ShortUserInfo } from "src/models/users";
import { ReviewCommentResponse, ReviewInfo, SubmissionInfo } from "src/models/exercise";
import { GroupsInfoResponse } from "src/models/groups";
import { SlideContext } from "../Slide";
import { ReviewInfoWithMarker, TextMarkersByReviewId } from "../Blocks/Exercise/ExerciseUtils";
import { InstructorReviewTabs } from "./InstructorReviewTabs";
import { DiffInfo } from "./utils";
import CodeMirror, { Editor } from "codemirror";
import { FavouriteReviewRedux, } from "src/redux/instructor";

export interface PropsFromRedux {
	user?: UserInfo;

	favouriteReviews?: FavouriteReviewRedux[];

	student?: ShortUserInfo;
	studentGroups?: ShortGroupInfo[];
	studentSubmissions?: SubmissionInfo[];
	scoresBySubmissionId?: { [submissionId: number]: number | undefined; };

	antiPlagiarismStatus?: AntiPlagiarismStatusResponse;
	prohibitFurtherManualChecking: boolean;
}

export interface ApiFromRedux {
	getStudentInfo: (studentId: string,) => Promise<ShortUserInfo | string>;
	getStudentSubmissions: (studentId: string, courseId: string,
		slideId: string,
	) => Promise<SubmissionInfo[] | string>;
	getAntiPlagiarismStatus: (courseId: string,
		submissionId: number,
	) => Promise<AntiPlagiarismStatusResponse | string>;
	getFavouriteReviews: (courseId: string, slideId: string,) => Promise<FavouriteReviewResponse | string>;
	getStudentGroups: (courseId: string, studentId: string,) => Promise<GroupsInfoResponse | string>;

	onScoreSubmit: (submissionId: number, userId: string, score: number,
		oldScore: number | undefined,
	) => Promise<Response | string>;
	prohibitFurtherReview: (courseId: string, slideId: string, userId: string, prohibit: boolean) => Promise<Response>;

	addFavouriteReview: (courseId: string, slideId: string, text: string) => Promise<FavouriteReview>;
	deleteFavouriteReview: (courseId: string, slideId: string, favouriteReviewId: number) => Promise<Response>;

	addReview: (
		submissionId: number,
		comment: string,
		startLine: number,
		startPosition: number,
		finishLine: number,
		finishPosition: number,
	) => Promise<ReviewInfo>;
	addReviewComment: (submissionId: number, reviewId: number,
		comment: string
	) => Promise<ReviewCommentResponse | string>;
	deleteReview: (submissionId: number, reviewId: number) => Promise<Response>;
	deleteReviewComment: (submissionId: number, reviewId: number, commentId: number) => Promise<Response>;
	editReviewOrComment: (submissionId: number, reviewId: number,
		parentReviewId: number | undefined, text: string, oldText: string,
	) => Promise<ReviewInfo | ReviewCommentResponse | string>;
}

export interface Props extends PropsFromRedux, ApiFromRedux {
	authorSolution?: React.ReactNode;
	formulation?: React.ReactNode;
	slideContext: SlideContext;
	studentId: string;
}

export interface InstructorExtraFields {
	outdated?: boolean;
	isFavourite?: boolean;
}

export interface ReviewCompare {
	comment: string;
	comments: string[];
	startLine: number;
	anchor?: number;
	instructor?: InstructorExtraFields;
}

export interface InstructorReviewInfo extends ReviewInfo {
	instructor?: InstructorExtraFields;
}

export interface InstructorReviewInfoWithAnchor extends InstructorReviewInfo {
	anchor: number;
}

export type InstructorReviewInfoWithMarker = InstructorReviewInfo & ReviewInfoWithMarker;

export interface State {
	currentTab: InstructorReviewTabs;

	currentSubmission: SubmissionInfo | undefined;

	showDiff: boolean;
	diffInfo?: DiffInfo;

	selectedReviewId: number;
	reviews: ReviewInfo[];
	outdatedReviews: ReviewInfo[];
	markers: TextMarkersByReviewId;

	curScore?: number;
	prevScore?: number;

	editor: null | Editor;

	addCommentValue: string;
	addCommentFormCoords?: { left: number; top: number; bottom: number };
	addCommentFormExtraSpace?: number;
	addCommentRanges?: { startRange: CodeMirror.Position; endRange: CodeMirror.Position; };

	initialCode?: string;

	favouriteReviewsSet: Set<string>;
	favouriteByUserSet: Set<string>;
}
