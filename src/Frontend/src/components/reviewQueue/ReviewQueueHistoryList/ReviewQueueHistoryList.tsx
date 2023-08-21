import React, { FC } from 'react';
import { ReviewQueueItem, ShortReviewInfo } from "../../../models/instructor";
import { CourseSlidesInfo } from "../RevoewQueue.types";
import { getSlideTitlesByIds } from "../utils/getSlideTitlesByIds";
import styles from './reviewQueueHistoryList.less';
import texts from './ReviewQueueHistoryList.texts';
import { getReviewQueueTimestamp } from "../utils/getReviewQueueTimestamp";
import { getNameWithLastNameFirst } from "../../common/Profile/Profile";
import { Link } from "ui";

interface Props {
	reviewQueueItems: ReviewQueueItem[];
	courseSlidesInfo: CourseSlidesInfo;
	showComments?: boolean;

	buildLinkToInstructorReview: (item: ReviewQueueItem) => string;
}

const ReviewQueueHistoryList: FC<Props> = (props) => {
	if(!props.reviewQueueItems.length) {
		return <div className={ styles.noSubmissionsWrapper }>
			<span className={ styles.noSubmissionsHintColor }>{ texts.noSubmissionsFoundHint }</span>
			<span>{ texts.noSubmissionsFound }</span>
		</div>;
	}

	const slideTitlesByIds = getSlideTitlesByIds(props.courseSlidesInfo);

	const renderReviewComment = (review: ShortReviewInfo) =>
		<li className={ styles.reviewComment } key={ review.commentId }>
			<code className={ styles.codeFragment }>{ review.codeFragment }</code>
			<span>{ review.comment }</span>
		</li>;

	const renderReviewQueueItem = (item: ReviewQueueItem) => {
		if(!item.checkedBy || !item.checkedTimestamp || item.score === undefined) {
			return;
		}

		return <li className={ styles.reviewQueueItem } key={ item.submissionId }>
			<Link
				className={ styles.submissionLink }
				href={ props.buildLinkToInstructorReview(item) }
			>
				<div className={ styles.userSlideWrapper }>
						<span className={ styles.user }>
							{ getNameWithLastNameFirst(item.user) }
						</span>
					<span className={ styles.slide }>
							{ slideTitlesByIds[item.slideId] }
						</span>
				</div>
				<span className={ styles.score }>
						{ texts.getScoringInfo(item.score, item.maxScore) }
					</span>
				<div className={ styles.reviewerTimestampWrapper }>
					<span>{ getNameWithLastNameFirst(item.checkedBy) }</span>
					<span className={ styles.timestamp }>{ getReviewQueueTimestamp(item.checkedTimestamp) }</span>
				</div>
			</Link>
			{ props.showComments && !!item.reviews?.length &&
				<ul className={ styles.reviewCommentsList }>
					{ item.reviews.map(renderReviewComment) }
				</ul>
			}
			<div className={ styles.splitter }/>
		</li>;
	};

	return (
		<ul className={ styles.reviewQueueHistoryList }>
			{ props.reviewQueueItems.map(renderReviewQueueItem) }
		</ul>
	);
};

export default ReviewQueueHistoryList;
