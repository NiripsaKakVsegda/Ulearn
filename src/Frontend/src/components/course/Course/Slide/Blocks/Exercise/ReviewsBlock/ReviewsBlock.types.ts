import { InstructorReviewInfo, InstructorReviewInfoWithAnchor } from "../../../InstructorReview/InstructorReview.types";
import React from "react";
import { UserInfo } from "src/utils/courseRoles";

export interface CommentReplies {
	[id: number]: string;
}

export interface RenderedReview {
	margin: number;
	prevMargin?: number;
	review: InstructorReviewInfoWithAnchor;
	ref: React.RefObject<HTMLLIElement>;
}

export interface LineRenderedReviews {
	reviews: InstructorReviewInfo[];
	startLine: number;
	anchor: number;
	margin: number;
	prevMargin?: number;
	ref: React.RefObject<HTMLLIElement>;
}

export interface EditingReviewState {
	reviewId: number;
	commentId?: number;
	value: string;
}

export interface Props {
	reviews: InstructorReviewInfoWithAnchor[];
	selectedReviewId: number;
	user?: UserInfo;

	replies: CommentReplies;
	onEditReply: (value: string) => void;

	className?: string;
	backgroundColor?: 'orange' | 'gray';

	onSelectReview: (id: number) => void;
	onSelectReviewClick: (e: React.MouseEvent | React.FocusEvent) => void;
	onSendComment: () => void;
	onDeleteReviewOrComment: (commentId?: number) => void;

	editingReview?: EditingReviewState;
	onStartEditingReviewOrComment: (value: string, commentId?: number) => void;
	onStopEditingComment: () => void;
	onEditingTextareaValueChange: (value: string) => void;
	onSaveEditingReviewOrComment: () => void;

	onAssignBotComment?: () => void;
	onToggleReviewFavourite?: () => void;
	onCopySelectedReviewTextToClipboard: () => void;
}
