import React, { FC, useEffect } from "react";
import styles from "./Course.less";
import { Button, Hint } from "ui";
import texts from "./NavigationButtons.texts";
import classnames from "classnames";
import { Link } from "react-router-dom";
import { constructPathToReviewQueue, constructPathToSlide } from "../../../consts/routes";
import { buildQuery } from "../../../utils";
import buildFilterSearchQueryParams, { buildInstructorReviewFilterSearchQueryParams } from "../../reviewQueue/utils/buildFilterSearchQueryParams";
import { InstructorReviewFilterSearchParams } from "../../reviewQueue/RevoewQueue.types";

interface Props {
	courseId: string;
	filterSearchParams: InstructorReviewFilterSearchParams;

	itemsToCheckCount: number;
	currentSubmissionId: number;
	currentLocked: boolean;

	nextSubmissionId?: number;
	nextSlideId?: string;
	nextUserId?: string;

	loading?: boolean;
	disabled?: boolean;
	notAllLoaded?: boolean;

	lockSubmission: (submissionId: number) => void;
}

const ReviewNavigationButtons: FC<Props> = (props) => {
	if(props.loading) {
		return <div className={ styles.reviewButtonsWrapper }>
			<Button loading>{ texts.loading }</Button>
		</div>;
	}

	useEffect(() => {
		if(!props.currentLocked) {
			props.lockSubmission(props.currentSubmissionId);
		}
	}, [props.currentSubmissionId]);

	return <div className={ styles.reviewButtonsWrapper }>
		{ props.itemsToCheckCount && props.nextSubmissionId && props.nextSlideId && props.nextUserId
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
					to={ constructPathToSlide(props.courseId, props.nextSlideId)
						+ buildQuery({
							submissionId: props.nextSubmissionId,
							userId: props.nextUserId,

							...buildInstructorReviewFilterSearchQueryParams(props.filterSearchParams)
						}) }
					children={ texts.nextReviewLinkText }
				/>)
			: <Link
				className={ classnames(styles.slideButton, styles.nextSlideButton, styles.reviewButton) }
				to={ constructPathToReviewQueue(props.courseId) +
					buildQuery(buildFilterSearchQueryParams(props.filterSearchParams))
				}
				children={ texts.returnToCheckingQueuePage }
			/>
		}
		<span className={ styles.reviewQueueNumberLabel }>
			{ texts.buildNextReviewText(props.itemsToCheckCount, props.notAllLoaded) }
		</span>
	</div>;
};

export default ReviewNavigationButtons;
