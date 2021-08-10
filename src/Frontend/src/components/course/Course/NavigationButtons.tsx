import React, { useState } from "react";

import api from "src/api";

import { Button } from "ui";
import { Link } from "react-router-dom";

import { SlideInfo } from "./CourseUtils";
import {
	adminCheckingQueuePath,
	constructPathToSlide,
	constructPathWithAutoplay,
	getPreviousSlideUrl
} from "src/consts/routes";
import { buildQuery, } from "src/utils";
import classnames from "classnames";

import { ReviewQueueResponse } from "src/models/instructor";

import styles from "./Course.less";
import texts from "./NavigationButtons.texts";


interface Props {
	slideInfo: SlideInfo;
}

function NavigationButtons({ slideInfo, }: Props): React.ReactElement {
	const { isNavigationVisible, courseId, isReview, } = slideInfo;

	if(isReview) {
		return <ReviewNavigationButtons slideInfo={ slideInfo }/>;
	}

	if(isNavigationVisible) {
		return renderNavigationButtons(courseId, slideInfo);
	}

	return <></>;

	function renderNavigationButtons(courseId: string, slideInfo: SlideInfo): React.ReactElement {
		const prevSlideHref = getPreviousSlideUrl(courseId, slideInfo);
		const nextSlideHref = slideInfo.navigationInfo && slideInfo.navigationInfo.next
			? constructPathToSlide(courseId, slideInfo.navigationInfo.next.slug)
			: null;

		const previousButtonText = slideInfo.navigationInfo && slideInfo.navigationInfo.current.firstInModule
			? texts.previousModule
			: texts.previous;

		const nextButtonText = slideInfo.navigationInfo && slideInfo.navigationInfo.current.lastInModule
			? texts.nextModule
			: texts.next;

		return (
			<div className={ styles.navigationButtonsWrapper }>
				{
					prevSlideHref
						? <Link className={ classnames(styles.slideButton, styles.previousSlideButton) }
								to={ constructPathWithAutoplay(prevSlideHref) }>
							{ previousButtonText }
						</Link>
						: <div className={ classnames(styles.slideButton, styles.disabledSlideButton) }>
							{ previousButtonText }
						</div>
				}
				{
					nextSlideHref
						?
						<Link className={ classnames(styles.slideButton, styles.nextSlideButton) }
							  to={ constructPathWithAutoplay(nextSlideHref) }>
							{ nextButtonText }
						</Link>
						: <div className={ classnames(styles.slideButton, styles.disabledSlideButton) }>
							{ nextButtonText }
						</div>
				}
			</div>
		);
	}
}

export default NavigationButtons;

interface ReviewNavigationState extends Partial<ReviewQueueResponse> {
	courseId: string;
	slideId: string;
	isLoading: boolean;
}

function ReviewNavigationButtons({ slideInfo, }: Props): React.ReactElement {
	const [state, setState] = useState<ReviewNavigationState | undefined>();
	const { query, slideId, courseId, navigationInfo, } = slideInfo;

	const groupsIds: string[] | "all" | "not-in-group" = Array.isArray(query.group)
		? query.group
		: query.group == null
			? 'all'
			: 'not-in-group';

	if(!query.submissionId || !slideId) {
		throw new Error("Submission id was not provided");
	}

	if(navigationInfo && (!state || slideId !== state.slideId)) {
		setState({
			courseId,
			slideId,
			isLoading: true,
		});

		api.instructor.lockSubmissionCheck(courseId, query.submissionId);
		api.instructor.getReviewQueue(courseId, groupsIds, slideId, undefined, query.done)
			.then(c => setState({
				courseId,
				slideId,
				isLoading: false,
				checkings: c.checkings
			}));
	}

	if(state && state.checkings) {
		const currentCheckIndex = state.checkings
			.findIndex(r => r.submissionId === slideInfo.query.submissionId);
		let check = state.checkings[(1 + currentCheckIndex) % state.checkings.length];

		for (let i = 1; i < state.checkings.length; i++) {
			const curCheck = state.checkings[(i + currentCheckIndex) % state.checkings.length];
			if(!curCheck.isLocked) {
				check = curCheck;
				break;
			}
		}

		return (
			<div className={ styles.reviewButtonsWrapper }>
				{
					check.submissionId !== state.checkings[currentCheckIndex]?.submissionId
						?
						<Link className={ classnames(styles.slideButton, styles.nextSlideButton, styles.reviewButton) }
							  to={ constructPathToSlide(courseId, slideId)
							  + buildQuery({
								  checkQueueItemId: check.submissionId,
								  submissionId: check.submissionId,
								  userId: check.userId,
							  }) }>
							{ texts.nextReviewLinkText }
						</Link>
						:
						<Link className={ classnames(styles.slideButton, styles.nextSlideButton, styles.reviewButton) }
							  to={ adminCheckingQueuePath + buildQuery({ courseId, }) }
						>
							{ texts.returnToCheckingQueuePage }
						</Link>
				}
				<span className={ styles.reviewQueueNumberLabel }>
					{ texts.buildNextReviewText(state.checkings.length - 1) }
				</span>
			</div>
		);
	}


	return (
		<div className={ styles.reviewButtonsWrapper }>
			<Button loading>Идет загрузка</Button>
		</div>
	);
}
