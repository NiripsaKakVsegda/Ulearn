import React, { useState } from "react";
import { connect, ConnectedProps } from "react-redux";
import classNames from 'classnames';

import DownloadedHtmlContent from 'src/components/common/DownloadedHtmlContent.js';
import { Link, Loader, Modal, } from "ui";

import { isInstructor } from "src/utils/courseRoles";

import { SubmissionInfo } from "src/models/exercise";
import { getSubmissionsWithReviews, SlideInfo } from "../../CourseUtils";
import { SlideType } from "src/models/slide";
import { constructPathToStudentSubmissions } from "src/consts/routes";
import { RootState } from "src/models/reduxState";

import texts from "./SlideHeader.texts";
import styles from "../SlideHeader/SlideHeader.less";

interface State {
	isModalShowed: boolean;
	isContentInModalReady: boolean;
}

const ScoreHeaderInternal = (props: PropsFromRedux & ScoreHeaderProps) => {
	const [{ isModalShowed, isContentInModalReady }, setState] = useState<State>(
		{ isModalShowed: false, isContentInModalReady: false, });

	const {
		scoreHeader,
		maxScore,
		isSkipped,
		waitingForManualChecking,
		prohibitFurtherManualChecking,
		hasReviewedSubmissions,
		courseId,
		slideId,
		showStudentSubmissions,
		anyAttemptsUsed,
	} = props;
	if(scoreHeader === null || maxScore === null) {
		return null;
	}

	const isMaxScore = scoreHeader === maxScore;
	let message: string | null = null;
	if(!isMaxScore) {
		if(isSkipped) {
			message = texts.skippedHeaderText;
		} else if(waitingForManualChecking) {
			message = texts.pendingReview;
		} else if(prohibitFurtherManualChecking) {
			message = texts.prohibitFurtherReview;
		} else if(hasReviewedSubmissions) {
			message = texts.reviewWaitForCorrection;
		}
	}

	const maxModalWidth = window.innerWidth - 40;
	const modalWidth: undefined | number = maxModalWidth > 880 ? 880 : maxModalWidth; //TODO пока что это мок, в будущем width будет другой
	const anyTryUsed = isSkipped || hasReviewedSubmissions || waitingForManualChecking || anyAttemptsUsed;

	return (
		<div className={ styles.header }>
			<span className={ classNames(styles.headerText, styles.scoreTextWeight, styles.scoreTextColor) }>
				{ texts.getSlideScore(scoreHeader, maxScore, anyTryUsed) }
			</span>
			{ message && <span className={ styles.headerStatusText }>{ message }</span> }
			{ showStudentSubmissions &&
			<Link loading={ !isContentInModalReady && isModalShowed } onClick={ openModal }
				  className={ styles.headerLinkText }>
				<Loader type={ 'mini' } caption={ '' }
						active={ !isContentInModalReady && isModalShowed }>
					{ texts.showAcceptedSolutionsText }
				</Loader>
			</Link>
			}
			{ isModalShowed &&
			<DownloadedHtmlContent
				url={ constructPathToStudentSubmissions(courseId, slideId) }
				injectInWrapperAfterContentReady={ injectInWrapperAfterContentReady }
			/>
			}
		</div>
	);

	function injectInWrapperAfterContentReady(html: React.ReactNode) {
		if(!isContentInModalReady) {
			setState({ isContentInModalReady: true, isModalShowed: true });
		}
		return (
			<Modal width={ modalWidth } onClose={ closeModal }>
				<Modal.Header>
					<h2>
						{ texts.showAcceptedSolutionsHeaderText }
					</h2>
				</Modal.Header>
				<Modal.Body>
					{ html }
				</Modal.Body>
			</Modal>);
	}

	function closeModal() {
		setState({ isContentInModalReady: false, isModalShowed: false });
	}

	function openModal() {
		if(!isModalShowed) {
			setState({ isContentInModalReady: false, isModalShowed: true });
		}
	}
};

interface ScoreHeaderProps {
	slideInfo: SlideInfo;
}

const mapState = (state: RootState, ownProps: ScoreHeaderProps) => {
	const { userProgress, account, } = state;
	const { slideInfo, } = ownProps;
	const { slideId, courseId, slideType, } = slideInfo;

	if(!slideId) {
		throw new Error('No slide id provided');
	}

	const slideProgress = userProgress.progress[courseId]?.[slideId];
	const submissions: SubmissionInfo[] | undefined =
		getSubmissionsWithReviews(
			courseId,
			slideId,
			state.account.id,
			state.submissions.submissionsIdsByCourseIdBySlideIdByUserId,
			state.submissions.submissionsById, state.submissions.reviewsBySubmissionId
		);
	const hasReviewedSubmissions = submissions
		? submissions.some(s => (s.manualChecking?.percent ?? null) !== null)
		: false;
	const instructor = isInstructor(
		{ isSystemAdministrator: account.isSystemAdministrator, courseRole: account.roleByCourse[courseId] });
	return {
		courseId,
		slideId,
		scoreHeader: slideProgress?.score ?? 0,
		isSkipped: slideProgress?.isSkipped ?? false,
		waitingForManualChecking: slideProgress?.waitingForManualChecking ?? false,
		prohibitFurtherManualChecking: slideProgress?.prohibitFurtherManualChecking ?? false,
		maxScore: slideInfo.navigationInfo?.current.maxScore || 0,
		hasReviewedSubmissions: hasReviewedSubmissions,
		showStudentSubmissions: slideType === SlideType.Exercise && instructor,
		anyAttemptsUsed: slideProgress?.usedAttempts > 0,
	};
};
const connector = connect(mapState);
type PropsFromRedux = ConnectedProps<typeof connector>;


type ScoreHeaderPropsFromRedux = Omit<PropsFromRedux, "dispatch">;
const ScoreHeader = connector(ScoreHeaderInternal);
export { ScoreHeader, type ScoreHeaderProps, type ScoreHeaderPropsFromRedux };
