import classnames from "classnames";
import React, { FC, useEffect } from "react";
import { Link } from "react-router-dom";
import { Button, Hint } from "ui";
import ReviewQueueFiltersTooltip from "../../reviewQueue/ReviewQueueFiltersTooltip/ReviewQueueFiltersTooltip";
import { CourseSlidesInfo, Grouping, ReviewQueueFilterState } from "../../reviewQueue/RevoewQueue.types";
import styles from "./Course.less";
import texts from "./NavigationButtons.texts";

interface Props {
	courseId: string;

	filter: ReviewQueueFilterState;
	courseSlidesInfo: CourseSlidesInfo;
	grouping?: Grouping;
	groupingItemId?: string;

	itemsToCheckCount: number;
	currentSubmissionId: number;

	nextReviewItemLink?: string;
	reviewQueueLink: string;

	loading?: boolean;
	disabled?: boolean;
	notAllLoaded?: boolean;

	lockSubmission: (submissionId: number) => void;
}

const ReviewNavigationButtons: FC<Props> = (props) => {
	useEffect(() => {
		if (props.currentSubmissionId !== -1) {
			props.lockSubmission(props.currentSubmissionId);
		}
	}, [props.currentSubmissionId]);

	if (props.loading) {
		return <div className={ styles.reviewButtonsWrapper }>
			<Button loading>{ texts.loading }</Button>
		</div>;
	}

	return <div className={ styles.reviewButtonsWrapper }>
		{ props.itemsToCheckCount && props.nextReviewItemLink
			? (props.disabled
				? <Hint text={ texts.nextSubmissionDisabledHint }>
					<span
						className={ classnames(
							styles.slideButton,
							styles.nextSlideButton,
							styles.reviewButton,
							styles.reviewButtonDisabled
						) }
						children={ texts.nextReviewLinkText }
					/>
				</Hint>
				: <Link
					className={ classnames(
						styles.slideButton,
						styles.nextSlideButton,
						styles.reviewButton
					) }
					to={ props.nextReviewItemLink }
					children={ texts.nextReviewLinkText }
				/>)
			: <Link
				className={ classnames(styles.slideButton, styles.nextSlideButton, styles.reviewButton) }
				to={ props.reviewQueueLink }
				children={ texts.returnToCheckingQueuePage }
			/>
		}
		{ !props.loading &&
		  <div className={ styles.nextReviewTextWrapper }>
			<span>
				{ texts.buildNextReviewText(props.itemsToCheckCount, props.notAllLoaded) }
			</span>
			  <ReviewQueueFiltersTooltip
				  filter={ props.filter }
				  courseSlidesInfo={ props.courseSlidesInfo }
				  grouping={ props.grouping }
				  groupingItemId={ props.groupingItemId }
				  showTimeSpanInfo
				  pos={ "right bottom" }
				  size={ 16 }
			  />
		  </div>
		}
	</div>;
};

export default ReviewNavigationButtons;
