import { Grouping, InstructorReviewFilterSearchParams, ReviewQueueFilterSearchParams } from "../RevoewQueue.types";

export default function buildFilterSearchQueryParams(filter: ReviewQueueFilterSearchParams): Record<string, string | string[]> {
	const searchParams: Record<string, string | string[]> = {};
	if(filter.reviewed) {
		searchParams['reviewed'] = 'true';
	}
	searchParams['sort'] = filter.sort;
	if(filter.unitId) {
		searchParams['unitId'] = filter.unitId;
	}
	if(filter.slideId) {
		searchParams['slideId'] = filter.slideId;
	}
	if(filter.reviewed && filter.timeSpan) {
		searchParams['timeSpan'] = filter.timeSpan;
	}
	searchParams['studentsFilter'] = filter.studentsFilter;
	if(filter.studentIds?.length) {
		searchParams['studentIds'] = filter.studentIds;
	}
	if(filter.groupIds?.length) {
		searchParams['groupIds'] = filter.groupIds.map(id => id.toString());
	}

	return searchParams;
}

export function buildInstructorReviewFilterSearchQueryParams(
	filter: InstructorReviewFilterSearchParams
): Record<string, string | string[]> {
	const searchParams = buildFilterSearchQueryParams(filter);
	if(filter.grouping !== Grouping.NoGrouping && filter.groupingItemId) {
		searchParams['grouping'] = filter.grouping;
		searchParams['groupingItemId'] = filter.groupingItemId;
	}

	return searchParams;
}
