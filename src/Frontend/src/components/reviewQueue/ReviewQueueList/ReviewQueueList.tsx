import React, { FC, useMemo } from 'react';
import { ReviewQueueItem } from "../../../models/instructor";
import { CourseSlidesInfo, Grouping, SlideInfo } from "../RevoewQueue.types";
import { getSlideTitlesByIds } from "../utils/getSlideTitlesByIds";
import ReviewQueueGroup from "../ReviewQueueGroup/ReviewQueueGroup";
import styles from './reviewQueueList.less';
import texts from './ReviewQueueList.texts';
import { getNameWithLastNameFirst } from "../../common/Profile/Profile";

interface GroupInfo {
	id: string;
	title?: string;
	items: ReviewQueueItem[];
}

interface Props {
	reviewQueueItems: ReviewQueueItem[];
	courseSlidesInfo: CourseSlidesInfo;
	userId: string;
	grouping: Grouping;
	notAllLoaded?: boolean;

	buildLinkToInstructorReview: (item: ReviewQueueItem, groupingItemId: string) => string;
}

const ReviewQueueList: FC<Props> = (props) => {
	const slidesOrderInCourseById = props.courseSlidesInfo.units
		.reduce(
			(slides, unit) => [...slides, ...unit.slides],
			[] as SlideInfo[]
		)
		.map((slide, index) => ({ id: slide.id, index }))
		.reduce(
			(orderById, slide) => ({ ...orderById, [slide.id]: slide.index }),
			{} as Record<string, number>
		);

	if(!props.reviewQueueItems.length) {
		return <div className={ styles.noSubmissionsWrapper }>
			<span className={ styles.noSubmissionsHintColor }>{ texts.noSubmissionsFoundHint }</span>
			<span>{ texts.noSubmissionsFound }</span>
		</div>;
	}

	const slideTitlesByIds = getSlideTitlesByIds(props.courseSlidesInfo);

	const groups = useMemo(
		() => buildGroups(props.reviewQueueItems, props.grouping),
		[props.reviewQueueItems, props.grouping]
	);

	const renderGroup = (group: GroupInfo) =>
		<li key={ group.id }>
			<ReviewQueueGroup
				reviewQueueItems={ group.items }
				slideTitlesByIds={ slideTitlesByIds }
				userId={ props.userId }
				title={ group.title }
				notAllLoaded={ props.notAllLoaded }
				alwaysOpened={ props.grouping === Grouping.NoGrouping }
				noStudent={ props.grouping === Grouping.GroupStudents }
				noSlide={ props.grouping === Grouping.GroupExercises }
				buildLinkToInstructorReview={ item => props.buildLinkToInstructorReview(item, group.id) }
			/>
		</li>;

	return (
		<ul className={ styles.groupingList }>
			{ groups.map(renderGroup) }
		</ul>
	);

	function buildGroups(items: ReviewQueueItem[], grouping: Grouping): GroupInfo[] {
		switch (grouping) {
			case Grouping.NoGrouping:
				return [{ id: 'all', items: items }];

			case Grouping.GroupStudents:
				return Object.values(items
						.reduce((result, item) => {
							const current = result[item.user.id] ?? {
								id: item.user.id,
								title: getNameWithLastNameFirst(item.user),
								items: []
							};
							current.items.push(item);
							return { ...result, [item.user.id]: current };
						}, {} as Record<string, GroupInfo>)
					)
					// eslint-disable-next-line @typescript-eslint/no-non-null-assertion
					.sort((a, b) => a.title!.localeCompare(b.title!));

			case Grouping.GroupExercises:
				return Object.values(items
						.reduce((result, item) => {
							const current = result[item.slideId] ?? {
								id: item.slideId,
								title: slideTitlesByIds[item.slideId],
								items: []
							};
							current.items.push(item);
							return { ...result, [item.slideId]: current };
						}, {} as Record<string, GroupInfo & { id: string }>)
					)
					.sort((a, b) =>
						slidesOrderInCourseById[a.id] - slidesOrderInCourseById[b.id]
					);
		}
	}
};

export default ReviewQueueList;
