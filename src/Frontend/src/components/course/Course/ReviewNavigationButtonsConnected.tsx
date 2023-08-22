import React, { FC, useEffect, useMemo, useState } from "react";
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
import {
	Grouping,
	InstructorReviewFilterSearchParams,
	ReviewQueueFilterState
} from "../../reviewQueue/RevoewQueue.types";
import { ShortReviewQueueItem, StudentsFilter } from "../../../models/instructor";
import { constructPathToReviewQueue, constructPathToSlide } from "../../../consts/routes";
import { buildQuery } from "../../../utils";
import buildFilterSearchQueryParams, { buildInstructorReviewFilterSearchQueryParams } from "../../reviewQueue/utils/buildFilterSearchQueryParams";
import buildCourseSlidesInfo from "../../reviewQueue/utils/buildCourseSlidesInfo";
import { usersApi } from "../../../redux/toolkit/api/usersApi";
import { groupsApi } from "../../../redux/toolkit/api/groups/groupsApi";


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

	const [searchParams, setSearchParams] = useState<InstructorReviewFilterSearchParams>({
		...slideInfo.query,
		slideId: slideInfo.query.queueSlideId
	});
	useEffect(() => {
		const updatedParams = {
			...slideInfo.query,
			slideId: slideInfo.query.queueSlideId
		};
		if(areParamsEqual(searchParams, updatedParams)) {
			return;
		}
		setSearchParams(updatedParams);
	}, [slideInfo]);

	const [filter, setFilter] = useState<ReviewQueueFilterState>();
	useEffect(() => {
		buildFilterState(searchParams)
			.then(f => setFilter(f));
	}, [searchParams]);

	const filterParameters = useMemo(
		() => buildInstructorReviewQueueMetaFilterParameters(searchParams, course, itemsToLoadCount),
		[searchParams, course]
	);

	const courseSlidesInfo = useMemo(
		() => buildCourseSlidesInfo(course),
		[course]
	);

	const {
		reviewQueueItems,
		isLoading,
		refetch
	} = reviewQueueApi.useGetReviewQueueMetaQuery(filterParameters, {
		selectFromResult: ({ data, isLoading, isFetching }) => ({
				reviewQueueItems: data?.checkings,
				isLoading: isLoading || isFetching
			}
		),
		skip: !isInstructor,
	});

	useEffect(() => {
		if(reviewQueueItems && !isLoading) {
			refetch();
		}
	}, [currentSubmissionId]);

	const [findUsersByIdsQuery] = usersApi.useLazyFindUsersByIdsQuery();
	const [findGroupsByIdsQuery] = groupsApi.useLazyFindGroupsByIdsQuery();

	const items = reviewQueueItems ?? [];
	const totalItemsCount = items.length;

	const currentItemIndex = items
		.findIndex(item => item.submissionId === currentSubmissionId);

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

	const nextReviewItemLink = nextItem
		? constructPathToSlide(courseId, nextItem.slideId)
		+ buildQuery({
			submissionId: nextItem.submissionId,
			userId: nextItem.userId,

			...buildInstructorReviewFilterSearchQueryParams(searchParams)
		})
		: undefined;

	const reviewQueueLink = constructPathToReviewQueue(courseId) +
		buildQuery(buildFilterSearchQueryParams(searchParams));

	return <ReviewNavigationButtons
		courseId={ courseId }

		filter={ filter ?? searchParams }
		courseSlidesInfo={ courseSlidesInfo }
		grouping={ searchParams.grouping }
		groupingItemId={ searchParams.groupingItemId }

		itemsToCheckCount={ itemsToCheckCount }
		currentSubmissionId={ currentSubmissionId }

		nextReviewItemLink={ nextReviewItemLink }
		reviewQueueLink={ reviewQueueLink }

		loading={ isLoading || !filter }
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
					filterParameters,
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

	function areParamsEqual(p1: InstructorReviewFilterSearchParams, p2: InstructorReviewFilterSearchParams) {
		if(
			p1.unitId !== p2.unitId ||
			p1.slideId !== p2.slideId ||
			p1.studentsFilter !== p2.studentsFilter ||
			p1.grouping !== p2.grouping ||
			p1.groupingItemId !== p2.groupingItemId
		) {
			return false;
		}

		if(p1.studentsFilter === StudentsFilter.MyGroups || p1.studentsFilter === StudentsFilter.All) {
			return true;
		}

		const ids1 = p1.studentsFilter === StudentsFilter.StudentIds
			? p1.studentIds ?? []
			: p1.groupIds ?? [];

		const ids2 = p2.studentsFilter === StudentsFilter.StudentIds
			? p2.studentIds ?? []
			: p2.groupIds ?? [];

		if(ids1.length !== ids2.length) {
			return false;
		}

		for (let i = 0; i < ids1.length; i++) {
			if(ids1[i] !== ids2[i]) {
				return false;
			}
		}

		return true;
	}

	async function buildFilterState(params: InstructorReviewFilterSearchParams): Promise<ReviewQueueFilterState> {
		if(params.grouping === Grouping.GroupStudents && params.groupingItemId) {
			const students =
				(await findUsersByIdsQuery({ userIds: [params.groupingItemId] }).unwrap()).foundUsers;
			const studentIds = students.map(s => s.id);

			return {
				...params,
				studentIds,
				students
			};
		}

		if(params.studentsFilter === StudentsFilter.StudentIds) {
			const students = params.studentIds?.length
				? (await findUsersByIdsQuery({ userIds: params.studentIds }).unwrap()).foundUsers
				: [];
			const studentIds = students.map(s => s.id);

			return {
				...params,
				studentIds,
				students
			};
		}

		if(params.studentsFilter === StudentsFilter.GroupIds) {
			const groups = params.groupIds?.length
				? (await findGroupsByIdsQuery({ groupIds: params.groupIds }).unwrap()).foundGroups
				: [];
			const groupIds = groups.map(g => g.id);

			return {
				...params,
				groupIds,
				groups
			};
		}

		return params;
	}
};

export default ReviewNavigationButtonsConnected;
