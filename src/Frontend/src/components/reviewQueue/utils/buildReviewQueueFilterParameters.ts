import {
	ReviewQueueFilterParameters,
	ReviewQueueHistoryFilterParameters,
	ReviewQueueMetaFilterParameters,
	StudentsFilter
} from "../../../models/instructor";
import { CourseInfo } from "../../../models/course";
import { Grouping, InstructorReviewFilterSearchParams, ReviewQueueFilterSearchParams } from "../RevoewQueue.types";
import getTimestampFromTimespan from "./getTimestampFromTimespan";

export function buildReviewQueueFilterParameters(
	filter: ReviewQueueFilterSearchParams,
	courseInfo: CourseInfo,
	count = 500
):
	ReviewQueueFilterParameters | ReviewQueueHistoryFilterParameters {
	const slideIds = filter.unitId
		? filter.slideId
			? [filter.slideId]
			: courseInfo.units.find(u => u.id === filter.unitId)?.slides
				.map(s => s.id)
		: undefined;

	const minTimestamp = filter.reviewed
		? getTimestampFromTimespan(filter.timeSpan)
		: undefined;

	return {
		courseId: courseInfo.id,
		sort: filter.sort,
		studentsFilter: filter.studentsFilter,
		groupIds: filter.groupIds,
		studentIds: filter.studentIds,
		slideIds,
		minTimestamp,
		count
	};
}

export function buildInstructorReviewQueueMetaFilterParameters(
	filter: InstructorReviewFilterSearchParams,
	courseInfo: CourseInfo,
	count = 500
): ReviewQueueMetaFilterParameters {
	const parameters = buildReviewQueueFilterParameters(filter, courseInfo, count) as ReviewQueueMetaFilterParameters;
	parameters.history = filter.reviewed;

	if(filter.grouping === Grouping.GroupStudents && filter.groupingItemId) {
		parameters.studentsFilter = StudentsFilter.StudentIds;
		parameters.studentIds = [filter.groupingItemId];
	}

	if(filter.grouping === Grouping.GroupExercises && filter.groupingItemId) {
		parameters.slideIds = [filter.groupingItemId];
	}

	return parameters;
}
