import React, { FC } from 'react';
import { Link } from "react-router-dom";
import { ReviewQueueItem, ShortReviewInfo } from "../../../models/instructor";
import { getNameWithLastNameFirst } from "../../common/Profile/Profile";
import { CourseSlidesInfo } from "../RevoewQueue.types";
import { getReviewQueueTimestamp } from "../utils/getReviewQueueTimestamp";
import { getSlideTitlesByIds } from "../utils/getSlideTitlesByIds";
import styles from './reviewQueueHistoryList.less';
import texts from './ReviewQueueHistoryList.texts';
import MockString from "../../common/MockString/MockString";

interface Props {
	reviewQueueItems: ReviewQueueItem[];
	courseSlidesInfo: CourseSlidesInfo;
	showComments?: boolean;

	loading?: boolean;

	buildLinkToInstructorReview: (item: ReviewQueueItem) => string;
}

const mockedItemsCount = 3;

const ReviewQueueHistoryList: FC<Props> = (props) => {
	const renderPlaceholderItem = (key: React.Key) => {
		const mockUserName = <>
			<MockString length={ 10 }/> <MockString length={ 5 }/>
		</>;
		return <li className={ styles.reviewQueueItem } key={ key }>
			<div className={ styles.submissionLink }>
				<div className={ styles.userSlideWrapper }>
					<span className={ styles.user }>
						{ mockUserName }
					</span>
					<span className={ styles.slide }>
						<MockString length={ 15 }/>
					</span>
				</div>
				<span className={ styles.score }>
					<MockString length={ 5 }/>
				</span>
				<div className={ styles.reviewerTimestampWrapper }>
					<span>{ mockUserName }</span>
					<span className={ styles.timestamp }>
						<MockString length={ 10 }/>
					</span>
				</div>
			</div>
			<div className={ styles.splitter }/>
		</li>;
	};

	if(!props.reviewQueueItems.length) {
		return props.loading
			? <ul className={ styles.reviewQueueHistoryList }>
				{ [...Array(mockedItemsCount).keys()].map(renderPlaceholderItem) }
			</ul>
			: <div className={ styles.noSubmissionsWrapper }>
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
		const checkedByUser = item.checkedBy ?? item.lockedBy;
		const checkedTimestamp = item.checkedTimestamp ?? item.lockedUntil ?? item.timestamp;
		//TODO для поддрежки legacy без нужной информации, удалить после обновления базы данных

		return <li className={ styles.reviewQueueItem } key={ item.submissionId }>
			<Link
				className={ styles.submissionLink }
				to={ props.buildLinkToInstructorReview(item) }
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
						{ texts.getScoringInfo(item.score ?? 0, item.maxScore) }
					</span>
				<div className={ styles.reviewerTimestampWrapper }>
					{ checkedByUser &&
						<span>{ getNameWithLastNameFirst(checkedByUser) }</span>
					}
					<span className={ styles.timestamp }>{ getReviewQueueTimestamp(checkedTimestamp) }</span>
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
