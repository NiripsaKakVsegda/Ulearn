import React, { FC, useEffect, useMemo, useState } from "react";
import ReviewQueuePage from "./ReviewQueuePage";
import { useAppSelector } from "../../redux/toolkit/hooks/useAppSelector";
import CourseLoader from "../course/Course/CourseLoader";
import Error404 from "../common/Error/Error404";
import {
	CourseSlidesInfo,
	defaultFilterState,
	Grouping,
	InstructorReviewFilterSearchParams,
	SlideInfo,
	UnitSlidesInfo
} from "./RevoewQueue.types";
import { CourseInfo } from "../../models/course";
import { changeCurrentCourseAction, loadCourse } from "../../actions/course";
import { useAppDispatch } from "../../redux/toolkit/hooks/useAppDispatch";
import { reviewQueueApi } from "../../redux/toolkit/api/reviewQueueApi";
import useReviewQueueFilterFromQuery from "./utils/useReviewQueueFilterFromQuery";
import { buildReviewQueueFilterParameters } from "./utils/buildReviewQueueFilterParameters";
import { useRequiredCourseIdParam } from "../../hooks/useCourseIdParam";
import { isInstructorFromAccount } from "../../utils/courseRoles";
import { ReviewQueueItem } from "../../models/instructor";
import { constructPathToSlide } from "../../consts/routes";
import { buildQuery } from "../../utils";
import { buildInstructorReviewFilterSearchQueryParams } from "./utils/buildFilterSearchQueryParams";
import { useGroupsSearch } from "../common/GroupsSearch/useGroupsSearch";
import { useUsersSearch } from "../common/UsersSearch/useUsersSearch";
import {
	getFilterFromLocalStorage,
	isFilterUpdated,
	ReviewQueueFilterLocalStorage,
	updateLocalStorageFilter
} from "./utils/localStorageManager";

const itemsToLoadCount = 500;

const ReviewQueuePageConnected: FC = () => {
	const courseId = useRequiredCourseIdParam();

	const { account, courses } = useAppSelector(state => ({
		account: state.account,
		courses: state.courses
	}));

	const dispatch = useAppDispatch();

	useEffect(() => {
		if(!courses.fullCoursesInfo[courseId] && !courses.courseLoading) {
			loadCourse(courseId)(dispatch);
			dispatch(changeCurrentCourseAction(courseId));
		}
	}, [courseId]);

	const course = courses.fullCoursesInfo[courseId];
	const courseSlidesInfo = useMemo(
		() => course
			? buildCourseSlidesInfo(course)
			: undefined,
		[course]
	);

	const isInstructor = isInstructorFromAccount(account, courseId);

	const filterLocalStorage = getFilterFromLocalStorage();
	const defaultFilter = {
		...defaultFilterState,
		...filterLocalStorage
	};
	const [filter, updateFilter] = useReviewQueueFilterFromQuery(course, defaultFilter);
	const filterParameters = useMemo(
		() => filter && course
			? buildReviewQueueFilterParameters(filter, course, itemsToLoadCount)
			: { courseId },
		[filter, course]
	);

	const [grouping, setGrouping] = useState(filterLocalStorage.grouping ?? Grouping.NoGrouping);

	useEffect(() => {
		if(!filter) {
			return;
		}
		const updated: Partial<ReviewQueueFilterLocalStorage> = {};
		updated.sort = filter.sort;
		if(filter.reviewed) {
			updated.timeSpan = filter.timeSpan;
		} else {
			updated.grouping = grouping;
		}
		if(isFilterUpdated(updated)) {
			updateLocalStorageFilter(updated);
		}
	}, [filter, grouping]);

	const { reviewQueueItems, isLoading } = reviewQueueApi.useGetReviewQueueQuery(
		filterParameters,
		{
			selectFromResult: ({ data, isFetching }) => ({
				reviewQueueItems: data?.checkings ?? [],
				isLoading: isFetching
			}),
			skip: !isInstructor || !filter || filter.reviewed
		}
	);

	const { reviewQueueItemsHistory, isHistoryLoading } = reviewQueueApi.useGetReviewQueueHistoryQuery(
		filterParameters,
		{
			selectFromResult: ({ data, isFetching }) => ({
				reviewQueueItemsHistory: data?.checkings ?? [],
				isHistoryLoading: isFetching
			}),
			skip: !isInstructor || !filter || !filter.reviewed
		}
	);

	const searchGroups = useGroupsSearch(courseId);
	const searchStudents = useUsersSearch();

	if(courses.courseLoading || !courseSlidesInfo || !filter) {
		return <CourseLoader/>;
	}

	if(courses.courseLoadingErrorStatus || !account.id || !isInstructor) {
		return <Error404/>;
	}

	const items = filter.reviewed
		? reviewQueueItemsHistory
		: reviewQueueItems;

	return (
		<ReviewQueuePage
			reviewQueueItems={ items }
			notAllItemsLoaded={ items.length === itemsToLoadCount }
			loading={ isLoading || isHistoryLoading }
			filter={ filter }
			grouping={ grouping }
			courseSlidesInfo={ courseSlidesInfo }
			userId={ account.id }
			onChangeGrouping={ setGrouping }
			onUpdateFilter={ updateFilter }
			getStudents={ searchStudents }
			getGroups={ searchGroups }
			buildLinkToInstructorReview={ buildLinkToInstructorReview }
		/>
	);

	function buildLinkToInstructorReview(
		item: ReviewQueueItem,
		filter: InstructorReviewFilterSearchParams
	): string {
		return constructPathToSlide(courseId, item.slideId) + buildQuery({
			submissionId: item.submissionId,
			userId: item.user.id,
			...buildInstructorReviewFilterSearchQueryParams(filter)
		});
	}

	function buildCourseSlidesInfo(course: CourseInfo): CourseSlidesInfo {
		const units: UnitSlidesInfo[] = [];
		for (const unit of course.units) {
			const slides: SlideInfo[] = [];
			for (const slide of unit.slides) {
				if(slide.requiresReview) {
					slides.push({
						id: slide.id,
						title: slide.title
					});
				}
			}

			if(slides.length) {
				units.push({
					id: unit.id,
					title: unit.title,
					slides: slides,
				});
			}
		}
		return { units };
	}
};

export default ReviewQueuePageConnected;
