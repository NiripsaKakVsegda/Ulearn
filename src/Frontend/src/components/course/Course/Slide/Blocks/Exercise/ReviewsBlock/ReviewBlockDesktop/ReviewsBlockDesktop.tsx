import React, { FC, ReactNode, useEffect, useState } from "react";
import cn from "classnames";
import { Props, RenderedReview } from "../ReviewsBlock.types";
import styles from "./ReviewsBlockDesktop.less";
import { getDataFromReviewToCompareChanges } from "../../../../InstructorReview/utils";
import { areReviewsSame, reviewsComparerByStart } from "../../ExerciseUtils";
import ReviewItem from "../ReviewItem/ReviewItem";
import { clamp } from "src/utils/clamp";

const ReviewsBlockDesktop: FC<Props> = (props) => {
	const minDistanceBetweenReviews = 5;
	const { backgroundColor = "orange", className } = props;

	const { reviews, selectedReviewId } = props;

	const [renderedReviews, setRenderedReviews] = useState<RenderedReview[]>([]);

	useEffect(() => {
		if(renderedReviews.length > 0 && renderedReviews[0].prevMargin === undefined) {
			setRenderedReviews(addMarginsToReviews(renderedReviews, selectedReviewId));
		}
	}, [renderedReviews]);

	useEffect(() => {
		if(selectedReviewId !== -1) {
			setRenderedReviews(addMarginsToReviews(renderedReviews, selectedReviewId));
		}
	}, [selectedReviewId]);

	useEffect(() => {
		const newReviews = [...reviews].sort(reviewsComparerByStart);
		const oldReviews = renderedReviews.map(rendered => rendered.review);
		const newReviewsChanges = newReviews.map(getDataFromReviewToCompareChanges);
		const oldReviewsChanges = oldReviews.map(getDataFromReviewToCompareChanges);
		const sameReviews = areReviewsSame(newReviewsChanges, oldReviewsChanges);

		if(sameReviews === "containsChangedReviews") {
			setRenderedReviews(renderedReviews.map((r, i) =>
				({ ...r, review: newReviews[i] })
			));
		} else if(sameReviews === "containsNewReviews") {
			setRenderedReviews(
				newReviews
					.map((review) => ({
						ref: React.createRef<HTMLLIElement>(),
						margin: 0,
						review: review
					}))
			);
		}

	}, [reviews]);

	const renderReview = ({ review, margin, prevMargin, ref }: RenderedReview): ReactNode => {
		const {
			id,
		} = review;
		const { selectedReviewId } = props;
		const outdated = review.instructor?.outdated;

		const className = cn(
			styles.comment,
			{ [styles.outdatedComment]: outdated },
			{ [styles.commentMounted]: prevMargin !== undefined },
			{ [styles.selectedReviewCommentWrapper]: selectedReviewId === id }
		);

		return <li
			key={ review.id }
			className={ className }
			ref={ ref }
			data-id={ review.id }
			onClick={ props.onSelectReviewClick }
			style={ {
				marginTop: `${ margin }px`
			} }
		>
			<ReviewItem
				review={ review }
				reply={ props.replies[review.id] }
				user={ props.user }
				isSelected={ selectedReviewId === id }

				onEditingReply={ props.onEditReply }
				onSendComment={ props.onSendComment }
				onDeleteReviewOrComment={ props.onDeleteReviewOrComment }

				editingReview={ props.editingReview }
				onStartEditingReviewOrComment={ props.onStartEditingReviewOrComment }
				onStopEditingComment={ props.onStopEditingComment }
				onEditingTextareaValueChange={ props.onEditingTextareaValueChange }
				onSaveEditingReviewOrComment={ props.onSaveEditingReviewOrComment }

				onToggleReviewFavourite={ props.onToggleReviewFavourite }
				onAssignBotComment={ props.onAssignBotComment }
				onCopySelectedReviewTextToClipboard={ props.onCopySelectedReviewTextToClipboard }
			/>
		</li>;
	};

	return <ul className={
		cn(styles.reviewsContainer,
			backgroundColor === "orange"
				? styles.reviewOrange
				: styles.reviewGray,
			className
		) }
	>
		{ renderedReviews.map(renderReview) }
	</ul>;

	function addMarginsToReviews(reviews: RenderedReview[], selectedReviewId: number): RenderedReview[] {
		if(reviews.length === 0) {
			return [];
		}

		const commentsWithMargin = reviews.map(r => ({
			...r,
			prevMargin: r.margin,
		} as RenderedReview));
		const selectedReviewIndex = commentsWithMargin.findIndex(c => c.review.id === selectedReviewId);
		let curPosition = 0;

		if(selectedReviewIndex > -1) {
			const selectedComment = commentsWithMargin[selectedReviewIndex];
			const distanceToSelectedReviewFromTop = Math.max(
				minDistanceBetweenReviews,
				selectedComment.review.anchor
			);

			let spaceWhichReviewsWillConsume = calculateMinSpaceForReviews(
				commentsWithMargin,
				selectedReviewIndex,
				minDistanceBetweenReviews
			);
			const comment = commentsWithMargin[0];
			const anchorTop = Math.max(minDistanceBetweenReviews, comment.review.anchor);
			const height = comment.ref.current?.offsetHeight || 0;

			if(spaceWhichReviewsWillConsume >= distanceToSelectedReviewFromTop) {
				comment.margin = distanceToSelectedReviewFromTop - spaceWhichReviewsWillConsume;
			} else {
				const availableSpace = distanceToSelectedReviewFromTop - spaceWhichReviewsWillConsume;
				if(availableSpace >= anchorTop) {
					comment.margin = anchorTop;
				} else {
					comment.margin = availableSpace;
				}
			}

			curPosition += comment.margin + height;
			spaceWhichReviewsWillConsume -= (height + minDistanceBetweenReviews);

			for (let i = 1; i <= selectedReviewIndex; i++) {
				const comment = commentsWithMargin[i];
				const anchorTop = comment.review.anchor;
				const height = comment.ref.current?.offsetHeight || 0;

				const availableSpace = distanceToSelectedReviewFromTop - (curPosition + spaceWhichReviewsWillConsume);
				comment.margin = clamp(anchorTop - curPosition, minDistanceBetweenReviews, availableSpace);

				curPosition += comment.margin + height;
				spaceWhichReviewsWillConsume -= (height + minDistanceBetweenReviews);
			}
		}

		for (let i = selectedReviewIndex + 1; i < commentsWithMargin.length; i++) {
			const comment = commentsWithMargin[i];
			const anchorTop = comment.review.anchor;
			const height = comment.ref.current?.offsetHeight || 0;

			comment.margin = Math.max(anchorTop - curPosition, minDistanceBetweenReviews);
			curPosition += comment.margin + height;
		}

		return commentsWithMargin;
	}

	function calculateMinSpaceForReviews(
		reviews: RenderedReview[],
		stopAtIndex: number,
		minDistanceBetweenReviews: number
	): number {
		let totalCommentsHeight = 0;
		for (let i = 0; i < stopAtIndex; i++) {
			const review = reviews[i];
			const height = review.ref.current?.offsetHeight || 0;
			totalCommentsHeight += height + minDistanceBetweenReviews;
		}

		return totalCommentsHeight;
	}
};

export default ReviewsBlockDesktop;
