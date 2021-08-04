import { InstructorReviewInfoWithAnchor } from "../../../InstructorReview/InstructorReview.types";
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

export interface ReviewState {
	renderedReviews: RenderedReview[];
	replies: CommentReplies;

	editingParentReviewId?: number;
	editingReviewId?: number;
	editingCommentValue?: string;
}

export interface ReviewProps {
	reviews: InstructorReviewInfoWithAnchor[];
	selectedReviewId: number;
	user?: UserInfo;

	onReviewClick: (e: React.MouseEvent | React.FocusEvent, id: number,) => void;
	addReviewComment: (parentReviewId: number, comment: string) => void;
	deleteReviewOrComment: (reviewId: number, parentReviewId?: number) => void;
	editReviewOrComment: (text: string, reviewId: number, parentReviewId?: number) => void;
	assignBotComment?: (botReviewId: number,) => void;

	backgroundColor?: 'orange' | 'gray';

	toggleReviewFavourite?: (favouriteReviewId: number,) => void;
}
