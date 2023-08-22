import React, { FC, useEffect, useMemo } from "react";
import ReviewNavigationButtons from "./ReviewNavigationButtons";
import { SlideInfo } from "./CourseUtils";
import { useAppSelector } from "../../../redux/toolkit/hooks/useAppSelector";
import { buildInstructorReviewQueueMetaFilterParameters } from "../../reviewQueue/utils/buildReviewQueueFilterParameters";
import { isInstructorFromAccount } from "../../../utils/courseRoles";
import { reviewQueueApi } from "../../../redux/toolkit/api/reviewQueueApi";
import { useAppDispatch } from "../../../redux/toolkit/hooks/useAppDispatch";
import { ShortUserInfo } from "../../../models/users";
import moment from "moment-timezone";
import { momentFromServerToLocal, momentToServerFormat } from "../../../utils/momentUtils";
import { InstructorReviewFilterSearchParams } from "../../reviewQueue/RevoewQueue.types";
import { ShortReviewQueueItem } from "../../../models/instructor";


interface Props {
	slideInfo: SlideInfo;
}

const itemsToLoadCount = 500;

const ReviewNavigationButtonsConnected: FC<Props> = ({ slideInfo }) => {
	const courseId = slideInfo.courseId;
	const isReviewed = slideInfo.query.reviewed;
	const currentSubmissionId = slideInfo.query.submissionId ?? -1;

	const [account, course, disabled] = useAppSelector(state => [
		state.account,
		state.courses.fullCoursesInfo[courseId],
		state.submissions.nextSubmissionButtonDisabled
	]);
	const dispatch = useAppDispatch();

	const isInstructor = isInstructorFromAccount(account, courseId);

	const searchParams: InstructorReviewFilterSearchParams = {
		...slideInfo.query,
		slideId: slideInfo.query.queueSlideId
	};
	const filter = useMemo(
		() =>
			buildInstructorReviewQueueMetaFilterParameters(searchParams, course, itemsToLoadCount),
		[searchParams, course]
	);

	const {
		reviewQueueItems,
		isLoading,
		refetch
	} = reviewQueueApi.useGetReviewQueueMetaQuery(filter, {
		selectFromResult: ({ data, isLoading, isFetching }) => ({
				reviewQueueItems: data?.checkings,
				isLoading: isLoading || isFetching
			}
		),
		skip: !isInstructor
	});

	useEffect(() => {
		if(reviewQueueItems && !isLoading) {
			refetch();
		}
	}, [currentSubmissionId]);

	const items = reviewQueueItems ?? [];
	const totalItemsCount = items.length;

	const currentItemIndex = items
		.findIndex(item => item.submissionId === currentSubmissionId);
	const currentItem = currentItemIndex === -1 ? undefined : items[currentItemIndex];
	const isLocked = !currentItem || isItemLocked(currentItem);

	const itemsToCheckCount = items
		.filter(item => item.submissionId !== currentSubmissionId && !isItemLocked(item, account.id))
		.length;

	let nextItem;
	if(itemsToCheckCount > 0) {
		for (
			let i = (currentItemIndex + 1) % totalItemsCount;
			i !== currentItemIndex;
			i = (i + 1) % totalItemsCount
		) {
			const item = items[i];
			if(!isItemLocked(item, account.id)) {
				nextItem = item;
				break;
			}
		}
	}

	const [lockSubmissionMutation] = reviewQueueApi.useLockSubmissionMutation();

	return <ReviewNavigationButtons
		courseId={ courseId }
		filterSearchParams={ searchParams }

		itemsToCheckCount={ itemsToCheckCount }
		currentSubmissionId={ currentSubmissionId }
		currentLocked={ isLocked }

		nextSubmissionId={ nextItem?.submissionId }
		nextSlideId={ nextItem?.slideId }
		nextUserId={ nextItem?.userId }

		loading={ isLoading }
		disabled={ disabled }
		notAllLoaded={ items.length === itemsToLoadCount }

		lockSubmission={ lockSubmission }
	/>;

	function lockSubmission(submissionId: number) {
		lockSubmissionMutation({ submissionId })
			.then(() => {
				const queryToUpdate = isReviewed
					? 'getReviewQueue'
					: 'getReviewQueueHistory';

				dispatch(reviewQueueApi.util.updateQueryData(
					queryToUpdate,
					filter,
					draft => {
						const toLock = draft.checkings.find(item => item.submissionId === submissionId);
						if(toLock) {
							toLock.lockedBy = account as ShortUserInfo;
							toLock.lockedUntil = momentToServerFormat(moment().add(30, 'minutes'));
						}
					}
				));
			});
	}

	function isItemLocked(item: ShortReviewQueueItem, notByUserId?: string) {
		const locked = !!item?.lockedById && !!item.lockedUntil &&
			moment().isBefore(momentFromServerToLocal(item.lockedUntil));
		return notByUserId
			? locked && item.lockedById !== notByUserId
			: locked;
	}
};

export default ReviewNavigationButtonsConnected;
