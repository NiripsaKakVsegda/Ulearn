import React, { FC, ReactNode, useEffect, useState } from "react";
import cn from "classnames";
import { LineRenderedReviews, Props } from "../ReviewsBlock.types";
import styles from "./ReviewsBlockMobile.less";
import { ThemeContext, Tooltip } from "ui";
import { tooltipReview } from "../../../../../../../../uiTheme";
import { InstructorReviewInfoWithAnchor } from "../../../../InstructorReview/InstructorReview.types";
import { getDataFromReviewToCompareChanges } from "../../../../InstructorReview/utils";
import { areReviewsSameLineCombined, reviewsComparerByStart } from "../../ExerciseUtils";
import ReviewItem from "../ReviewItem/ReviewItem";
import { CommentRectIcon20Solid } from '@skbkontur/icons/CommentRectIcon20Solid';

const ReviewsBlockMobile: FC<Props> = (props) => {
	const { reviews, selectedReviewId } = props;
	const { className } = props;

	const [renderedReviews, setRenderedReviews] = useState<LineRenderedReviews[]>([]);

	const selectedReviewLineIndex = selectedReviewId === -1
		? -1
		: renderedReviews.findIndex(line => line.reviews.some(r => r.id === selectedReviewId));
	const selectedReviewInlineIndex = selectedReviewLineIndex === -1
		? -1
		: renderedReviews[selectedReviewLineIndex].reviews.findIndex(r => r.id === selectedReviewId);
	const selectedReview = selectedReviewLineIndex === -1
		? undefined
		: renderedReviews[selectedReviewLineIndex].reviews[selectedReviewInlineIndex];

	useEffect(() => {
		if(renderedReviews.length > 0 && renderedReviews[0].prevMargin === undefined) {
			setRenderedReviews(addMarginsToReviews(renderedReviews));
		}
	}, [renderedReviews]);


	useEffect(() => {
		const newReviews = [...reviews].sort(reviewsComparerByStart);
		const oldReviews = renderedReviews
			.map(rendered => rendered.reviews)
			.reduce((result, reviews) => result.concat(reviews), []);
		const newReviewsChanges = newReviews.map(getDataFromReviewToCompareChanges);
		const oldReviewsChanges = oldReviews.map(getDataFromReviewToCompareChanges);
		const sameReviews = areReviewsSameLineCombined(newReviewsChanges, oldReviewsChanges);

		if(sameReviews === "containsChangedReviews") {
			let i = 0;
			const updated = renderedReviews
				.map(line => ({
					...line,
					reviews: line.reviews.map(() => newReviews[i++])
				}));
			setRenderedReviews(updated);
		} else if(sameReviews === 'containsChangedAnchors') {
			let i = 0;
			const updated = renderedReviews
				.map(line => ({
					...line,
					anchor: newReviews[i].anchor,
					reviews: line.reviews.map(() => newReviews[i++]),
					prevMargin: undefined
				}));
			setRenderedReviews(updated);
		} else if(sameReviews === "containsNewReviews") {
			setRenderedReviews(groupReviewsByLine(newReviews));
		}

	}, [reviews]);

	const renderTooltip = (): ReactNode => {
		if(!selectedReview) {
			return;
		}

		const navNext = selectedReviewInlineIndex + 1 >= renderedReviews[selectedReviewLineIndex].reviews.length
			? undefined
			: navigateNextReviewInLine;

		const navPrev = selectedReviewInlineIndex - 1 < 0
			? undefined
			: navigatePrevReviewInLine;

		return <ReviewItem
			review={ selectedReview }
			reply={ props.replies[selectedReviewId] }
			user={ props.user }
			isSelected={ true }
			className={ styles.tooltipWrapper }

			renderNavMenu={ renderedReviews[selectedReviewLineIndex].reviews.length > 1 }
			reviewIndexInLine={ selectedReviewInlineIndex }
			totalReviewsInLine={ renderedReviews[selectedReviewLineIndex].reviews.length }
			onNavigateNext={ navNext }
			onNavigatePrevious={ navPrev }

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
		/>;
	};

	const renderLineReviews = (lineReviews: LineRenderedReviews, index: number): ReactNode => {
		const click = selectedReviewLineIndex === index
			? undefined
			: props.onSelectReviewClick;

		return <li
			key={ index }
			ref={ lineReviews.ref }
			className={ styles.reviewItem }
			style={ {
				marginTop: `${ lineReviews.margin }px`
			} }
		>
			<ThemeContext.Provider value={ tooltipReview }>
				<Tooltip
					pos={ "bottom right" }
					allowedPositions={ ["bottom right"] }
					render={ renderTooltip }
					closeButton={ false }
					trigger={ selectedReviewLineIndex === index ? "opened" : "closed" }
					onCloseRequest={ unselectComment }
				>
					<div
						className={ styles.reviewIconWrapper }
						data-id={ lineReviews.reviews[0].id }
						onClick={ click }
					>
						<CommentRectIcon20Solid className={ styles.reviewIcon }/>
						{ lineReviews.reviews.length > 1 &&
							<span className={ styles.reviewCount }>
								{ lineReviews.reviews.length > 9 ? "9+" : lineReviews.reviews.length }
							</span>
						}
					</div>
				</Tooltip>
			</ThemeContext.Provider>
		</li>;
	};

	return <ul className={ cn(styles.reviewsContainerMobile, className) }>
		{ renderedReviews.map(renderLineReviews) }
	</ul>;

	function unselectComment(e: Event | React.MouseEvent<Element, MouseEvent> | undefined) {
		e?.stopPropagation();
		if(selectedReviewId !== -1) {
			props.onSelectReview(-1);
		}
	}

	function navigateNextReviewInLine() {
		if(selectedReviewLineIndex === -1) {
			return;
		}

		const line = renderedReviews[selectedReviewLineIndex];
		if(selectedReviewInlineIndex + 1 >= line.reviews.length) {
			return;
		}

		props.onSelectReview(line.reviews[selectedReviewInlineIndex + 1].id);
	}

	function navigatePrevReviewInLine() {
		if(selectedReviewLineIndex === -1) {
			return;
		}

		const line = renderedReviews[selectedReviewLineIndex];
		if(selectedReviewInlineIndex - 1 < 0) {
			return;
		}

		props.onSelectReview(line.reviews[selectedReviewInlineIndex - 1].id);
	}

	function groupReviewsByLine(reviews: InstructorReviewInfoWithAnchor[]): LineRenderedReviews[] {
		const result = [...reviews]
			.sort(reviewsComparerByStart)
			.reduce(
				(result, review) => {
					const current = result[review.startLine];
					result[review.startLine] = current === undefined
						? {
							reviews: [review],
							startLine: review.startLine,
							anchor: review.anchor,
							margin: 0,
							ref: React.createRef<HTMLLIElement>()
						}
						: { ...current, reviews: [...current.reviews, review] };
					return result;
				},
				{} as { [line: string]: LineRenderedReviews }
			);
		return Object.values(result)
			.sort((r1, r2) => r1.startLine - r2.startLine);
	}

	function addMarginsToReviews(lineReviews: LineRenderedReviews[]): LineRenderedReviews[] {
		if(lineReviews.length === 0) {
			return [];
		}
		const commentsWithMargin = lineReviews.map(r => ({
			...r,
			prevMargin: r.margin,
		}));

		let currentHeight = 0;
		for (let i = 0; i < commentsWithMargin.length; i++) {
			commentsWithMargin[i].margin = commentsWithMargin[i].anchor - currentHeight;
			currentHeight = commentsWithMargin[i].anchor + (commentsWithMargin[i].ref.current?.offsetHeight ?? 0);
		}

		return commentsWithMargin;
	}
};

export default ReviewsBlockMobile;
