import React, { FC, useEffect, useState } from "react";
import { CommentReplies, EditingReviewState } from "./ReviewsBlock.types";
import { useMediaQuery } from "react-responsive";
import ReviewsBlockDesktop from "./ReviewBlockDesktop/ReviewsBlockDesktop";
import ReviewsBlockMobile from "./ReivewBlockMobile/ReviewsBlockMobile";
import { ReviewInfo } from "../../../../../../../models/exercise";
import { InstructorReviewInfoWithAnchor } from "../../../InstructorReview/InstructorReview.types";
import { UserInfo } from "../../../../../../../utils/courseRoles";
import { Toast } from "ui";

export interface Props {
	reviews: InstructorReviewInfoWithAnchor[];
	selectedReviewId: number;
	user?: UserInfo;

	className?: string;
	backgroundColor?: "orange" | "gray";

	onSelectReview: (id: number) => void;
	onSendComment: (reviewId: number, comment: string) => void;
	onDeleteReviewOrComment: (reviewId: number, commentId?: number) => void;
	onEditReviewOrComment: (text: string, reviewId: number, parentReviewId?: number) => void;
	onAssignBotComment?: (botReviewId: number) => void;
	onToggleReviewFavourite?: (favouriteReviewId: number) => void;
}

const ReviewsBlock: FC<Props> = (props) => {
	const isDesktopOrLaptop = useMediaQuery({ minWidth: 1280 });

	const { selectedReviewId, reviews } = props;
	const [replies, setReplies] = useState<CommentReplies>(buildCommentsReplies(reviews));

	useEffect(() => {
		setReplies(buildCommentsReplies(reviews, replies));
	}, [reviews]);

	const [editingReview, setEditingReview] = useState<EditingReviewState>();

	return (
		<>
			{ isDesktopOrLaptop
				? <ReviewsBlockDesktop
					reviews={ reviews }
					selectedReviewId={ props.selectedReviewId }
					user={ props.user }

					replies={ replies }
					onEditReply={ editReply }

					className={ props.className }
					backgroundColor={ props.backgroundColor }

					onSelectReview={ props.onSelectReview }
					onSelectReviewClick={ selectReviewClick }
					onSendComment={ sendComment }
					onDeleteReviewOrComment={ deleteReviewOrComment }

					editingReview={ editingReview }
					onStartEditingReviewOrComment={ startEditingReviewOrComment }
					onStopEditingComment={ stopEditingComment }
					onEditingTextareaValueChange={ editingTextareaValueChange }
					onSaveEditingReviewOrComment={ saveEditingReviewOrComment }

					onAssignBotComment={ props.onAssignBotComment && assignBotComment }
					onToggleReviewFavourite={ props.onToggleReviewFavourite && toggleReviewToFavourite }
					onCopySelectedReviewTextToClipboard={ copySelectedReviewTextToClipboard }
				/>
				: <ReviewsBlockMobile
					reviews={ reviews }
					selectedReviewId={ props.selectedReviewId }
					user={ props.user }

					replies={ replies }
					onEditReply={ editReply }

					className={ props.className }
					backgroundColor={ props.backgroundColor }

					onSelectReview={ props.onSelectReview }
					onSelectReviewClick={ selectReviewClick }
					onSendComment={ sendComment }
					onDeleteReviewOrComment={ deleteReviewOrComment }

					editingReview={ editingReview }
					onStartEditingReviewOrComment={ startEditingReviewOrComment }
					onStopEditingComment={ stopEditingComment }
					onEditingTextareaValueChange={ editingTextareaValueChange }
					onSaveEditingReviewOrComment={ saveEditingReviewOrComment }

					onAssignBotComment={ props.onAssignBotComment && assignBotComment }
					onToggleReviewFavourite={ props.onToggleReviewFavourite && toggleReviewToFavourite }
					onCopySelectedReviewTextToClipboard={ copySelectedReviewTextToClipboard }
				/>
			}
		</>
	);

	function selectReviewClick(e: React.MouseEvent | React.FocusEvent) {
		e.stopPropagation();
		const id = getReviewId(e);
		if(selectedReviewId !== id) {
			props.onSelectReview(id);
		}
	}

	function editReply(value: string) {
		setReplies({ ...replies, [selectedReviewId]: value });
	}

	function sendComment() {
		props.onSendComment(selectedReviewId, replies[selectedReviewId]);
		setReplies({ ...replies, [selectedReviewId]: "" });
	}

	function deleteReviewOrComment(commentId?: number) {
		props.onDeleteReviewOrComment(selectedReviewId, commentId);
	}

	function startEditingReviewOrComment(value: string, commentId?: number) {
		setEditingReview({ reviewId: selectedReviewId, commentId: commentId, value: value });
	}

	function stopEditingComment(): void {
		setEditingReview(undefined);
	}

	function editingTextareaValueChange(value: string) {
		if(!editingReview) {
			return;
		}
		setEditingReview({ ...editingReview, value: value });
	}

	function saveEditingReviewOrComment(): void {
		if(!editingReview) {
			return;
		}

		if(editingReview.commentId) {
			props.onEditReviewOrComment(editingReview.value, editingReview.commentId, editingReview.reviewId);
		} else {
			props.onEditReviewOrComment(editingReview.value, editingReview.reviewId);
		}

		stopEditingComment();
	}

	function assignBotComment() {
		if(!props.onAssignBotComment) {
			return;
		}

		props.onAssignBotComment(selectedReviewId);
	}

	function toggleReviewToFavourite(): void {
		if(!props.onToggleReviewFavourite) {
			return;
		}

		props.onToggleReviewFavourite(selectedReviewId);
	}

	function copySelectedReviewTextToClipboard() {
		const review = reviews.find(r => r.id === selectedReviewId);
		if(review) {
			// noinspection JSIgnoredPromiseFromCall
			navigator.clipboard.writeText(review.comment);
			Toast.push("Текст скопирован");
		}
	}

	function buildCommentsReplies(reviews: ReviewInfo[], oldReplies?: CommentReplies): CommentReplies {
		const newCommentReplies: CommentReplies = {};

		for (const { id } of reviews) {
			newCommentReplies[id] = oldReplies?.[id] || "";
		}

		return newCommentReplies;
	}

	function getReviewId(event: React.MouseEvent | React.SyntheticEvent): number {
		const { id } = (event.currentTarget as HTMLElement).dataset;

		return parseInt(id || "-1");
	}
};

export default ReviewsBlock;
