import React, { useState } from "react";

import api from "src/api";

import { Button, Hint } from "ui";
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
import { connect } from "react-redux";
import { RootState } from "../../../redux/reducers";


interface Props {
	slideInfo: SlideInfo;
	disabled: boolean;
}

function NavigationButtons({ slideInfo, }: Props): React.ReactElement {
	const { isNavigationVisible, courseId, isReview, } = slideInfo;

	if(isReview) {
		return <ReduxReviewNavigationButtons slideInfo={ slideInfo }/>;
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
	submissionId: number;
	userId: string;
	isLoading: boolean;
}

function ReviewNavigationButtons({ slideInfo, disabled, }: Props): React.ReactElement {
	const [state, setState] = useState<ReviewNavigationState | undefined>();
	const { query, courseId, navigationInfo, slideId, } = slideInfo;

	if(!slideId) {
		throw new Error("Slide id was not provided");
	}

	if(!query.submissionId) {
		throw new Error("Submission id was not provided");
	}

	if(!query.userId) {
		throw new Error("User id was not provided");
	}

	//if any queue params changed -> reload. slide id, user id, submission id
	if(navigationInfo && (!state || slideId !== state.slideId || state.userId !== query.userId || state.submissionId !== query.submissionId)) {
		setState({
			courseId,
			slideId,
			userId: query.userId,
			submissionId: query.submissionId,
			isLoading: true,
		});

		const userId = query.userId;
		const submissionId = query.submissionId;

		api.instructor.getReviewQueue(courseId, query.group || undefined, query.queueSlideId || undefined, undefined,
			query.done)
			.then(c => {

				setState({
					userId,
					submissionId,
					courseId,
					slideId,
					isLoading: false,
					checkings: c.checkings,
				});
			});
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

		const checkings = [...state.checkings];
		const newCheck = checkings.find(c => c.submissionId === query.submissionId);
		if(newCheck && !newCheck.isLocked) {
			api.instructor.lockSubmissionCheck(courseId, query.submissionId, check.type);

			if(newCheck) {
				newCheck.isLocked = true;
			}

			setState({
				...state,
				checkings,
			});
		}

		return (
			<div className={ styles.reviewButtonsWrapper }>
				{
					check && check.submissionId !== state.checkings[currentCheckIndex]?.submissionId
						?
						(disabled
							? <Hint text={ texts.nextSubmissionDisabledHint }>
								<span className={ classnames(styles.slideButton, styles.nextSlideButton,
									styles.reviewButton, styles.reviewButtonDisabled) }>
									{ texts.nextReviewLinkText }
								</span>
							</Hint>
							: <Link
								className={ classnames(styles.slideButton, styles.nextSlideButton,
									styles.reviewButton) }
								to={ disabled ? '' : constructPathToSlide(courseId, check.slideId)
									+ buildQuery({
										checkQueueItemId: check.submissionId,
										submissionId: check.submissionId,
										userId: check.userId,

										queueSlideId: query.queueSlideId || undefined,
										group: query.group || undefined,
										done: query.done,
									}) }>
								{ texts.nextReviewLinkText }
							</Link>)
						:
						<Link
							className={ classnames(styles.slideButton, styles.nextSlideButton, styles.reviewButton) }
							to={ adminCheckingQueuePath + buildQuery({
								courseId,
								slideId: query.queueSlideId || undefined,
								group: query.group || undefined,
								done: query.done,
							}) }
						>
							{ texts.returnToCheckingQueuePage }
						</Link>
				}
				<span className={ styles.reviewQueueNumberLabel }>
					{ texts.buildNextReviewText(
						state.checkings.filter(c => c.submissionId !== query.submissionId).length) }
				</span>
			</div>
		);
	}

	return (
		<div className={ styles.reviewButtonsWrapper }>
			<Button loading>{ texts.loading }</Button>
		</div>
	);
}

const ReduxReviewNavigationButtons = connect(
	(state: RootState) => ({ disabled: state.submissions.nextSubmissionButtonDisabled }), () => ({}))(
	ReviewNavigationButtons);
