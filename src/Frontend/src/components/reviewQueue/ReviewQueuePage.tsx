import React, { FC, useState } from "react";
import { ReviewQueueItem } from "../../models/instructor";
import { Helmet } from "react-helmet";
import texts from "./ReviewQueuePage.texts";
import styles from "./reviewQueuePage.less";
import { Tabs } from "ui";
import {
	CourseSlidesInfo,
	Grouping,
	InstructorReviewFilterSearchParams,
	ReviewQueueFilterState
} from "./RevoewQueue.types";
import ReviewQueueFilters from "./ReviewQueueFilters/ReviewQueueFilters";
import ReviewQueueList from "./ReviewQueueList/ReviewQueueList";
import ReviewQueueHistoryList from "./ReviewQueueHistoryList/ReviewQueueHistoryList";
import { ShortUserInfo } from "../../models/users";
import { ShortGroupInfo } from "../../models/comments";
import GlobalLoaderFixed from "../common/GlobalLoaderFixed";

interface Props {
	reviewQueueItems: ReviewQueueItem[];
	notAllItemsLoaded?: boolean;
	loading?: boolean;

	filter: ReviewQueueFilterState;
	grouping: Grouping;
	courseSlidesInfo: CourseSlidesInfo;
	userId: string;

	onChangeGrouping: (grouping: Grouping) => void;
	onUpdateFilter: (newValues: ReviewQueueFilterState) => void;

	getStudents: (query: string) => Promise<ShortUserInfo[]>;
	getGroups: (query: string) => Promise<ShortGroupInfo[]>;

	buildLinkToInstructorReview: (item: ReviewQueueItem, filter: InstructorReviewFilterSearchParams) => string;
}

const ReviewQueuePage: FC<Props> = (props) => {
	const [showComments, setShowComments] = useState(false);

	return <div className={ styles.pageWrapper }>
		<Helmet defer={ false }>
			<title>{ texts.title }</title>
		</Helmet>
		<GlobalLoaderFixed
			expectedResponseTime={ 1000 }
			loading={ props.loading ?? false }
		/>
		<div className={ styles.pageContentWrapper }>
			<h1 className={ styles.pageTitle }>{ texts.title }</h1>
			<Tabs
				className={ styles.tabs }
				value={ props.filter.reviewed ? "history" : "reviewQueue" }
				onValueChange={ handleTabChange }
			>
				<Tabs.Tab id={ "reviewQueue" } className={ styles.tab }>
					{ texts.tabs.reviewQueue }
				</Tabs.Tab>
				<Tabs.Tab id={ "history" } className={ styles.tab }>
					{ texts.tabs.reviewed }
				</Tabs.Tab>
			</Tabs>
			<div className={ styles.filtersListWrapper }>
				<ReviewQueueFilters
					grouping={ props.filter.reviewed ? undefined : props.grouping }
					onChangeGrouping={ props.filter.reviewed ? undefined : props.onChangeGrouping }
					showComments={ props.filter.reviewed ? showComments : undefined }
					onChangeShowComments={ props.filter.reviewed ? setShowComments : undefined }
					filter={ props.filter }
					courseSlidesInfo={ props.courseSlidesInfo }
					onUpdateFilter={ props.onUpdateFilter }
					getStudents={ props.getStudents }
					getGroups={ props.getGroups }
				/>
				{ (props.reviewQueueItems.length > 0 || !props.loading) && (props.filter.reviewed
						? <ReviewQueueHistoryList
							reviewQueueItems={ props.reviewQueueItems }
							courseSlidesInfo={ props.courseSlidesInfo }
							showComments={ showComments }
							buildLinkToInstructorReview={ buildLinkToInstructorReview }
						/>
						: <ReviewQueueList
							reviewQueueItems={ props.reviewQueueItems }
							courseSlidesInfo={ props.courseSlidesInfo }
							userId={ props.userId }
							grouping={ props.grouping }
							notAllLoaded={ props.notAllItemsLoaded }
							buildLinkToInstructorReview={ buildLinkToInstructorReview }
						/>
				) }
				{ props.notAllItemsLoaded &&
					<div className={ styles.notAllItemsLoadedInfo }>
						{ texts.buildNotAllItemsLoadedInfo(props.reviewQueueItems.length) }
					</div>
				}
			</div>
		</div>
	</div>;


	function handleTabChange(value: string) {
		props.onUpdateFilter({
			...props.filter,
			reviewed: value === "history"
		});
	}

	function buildLinkToInstructorReview(item: ReviewQueueItem, groupingItemId?: string) {
		return props.buildLinkToInstructorReview(item, {
			...props.filter,
			grouping: props.grouping,
			groupingItemId: props.grouping === Grouping.NoGrouping ? undefined : groupingItemId
		});
	}
};

export default ReviewQueuePage;
