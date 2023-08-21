import { DateSort, StudentsFilter } from "../../../models/instructor";
import { CourseInfo } from "../../../models/course";
import {
	defaultFilterState,
	Grouping,
	HistoryTimeSpan,
	InstructorReviewFilterSearchParams,
	ReviewQueueFilterSearchParams
} from "../RevoewQueue.types";

export default function getFilterSearchParamsFromQuery(
	params?: URLSearchParams,
	course?: CourseInfo,
	defaultFilter?: ReviewQueueFilterSearchParams
): ReviewQueueFilterSearchParams {
	const lowerCaseParams = params
		? searchParamsToLowerCase(params)
		: new URLSearchParams();

	const filter = defaultFilter ?? { ...defaultFilterState };

	const reviewed = lowerCaseParams.get('reviewed');
	if(reviewed === 'true') {
		filter.reviewed = true;
	} else if(reviewed === 'false') {
		filter.reviewed = false;
	}

	const sort = lowerCaseParams.get('sort') as DateSort;
	if(sort && Object.values(DateSort).includes(sort)) {
		filter.sort = sort;
	}

	const unitId = lowerCaseParams.get('unitid');
	const unit = course && unitId
		? course.units.find(u => u.id === unitId)
		: undefined;
	filter.unitId = course
		? unit?.id
		: unitId ?? undefined;

	const slideId = lowerCaseParams.get('slideid');
	const slide = unit && slideId
		? unit.slides.find(s => s.id === slideId)
		: undefined;
	filter.slideId = course
		? slide?.id
		: slideId ?? undefined;

	if(filter.reviewed) {
		const timeSpan = lowerCaseParams.get('timespan') as HistoryTimeSpan;
		if(timeSpan && Object.values(HistoryTimeSpan).includes(timeSpan)) {
			filter.timeSpan = timeSpan;
		}
	}

	const studentsFilter = lowerCaseParams.get('studentsfilter') as StudentsFilter;
	if(studentsFilter && Object.values(StudentsFilter).includes(studentsFilter)) {
		filter.studentsFilter = studentsFilter;
	}

	if(filter.studentsFilter === StudentsFilter.StudentIds) {
		return {
			...filter,
			studentIds: lowerCaseParams.getAll('studentids')
		};
	}

	if(filter.studentsFilter === StudentsFilter.GroupIds) {
		return {
			...filter,
			groupIds: lowerCaseParams.getAll('groupids')
				.map(id => parseInt(id))
				.filter(id => !isNaN(id))
		};
	}

	return filter;
}

export function getInstructorReviewFilterSearchParamsFromQuery(
	params?: URLSearchParams,
	course?: CourseInfo
): InstructorReviewFilterSearchParams {
	const lowerCaseParams = params
		? searchParamsToLowerCase(params)
		: new URLSearchParams();

	const filter = getFilterSearchParamsFromQuery(params, course);

	const grouping = lowerCaseParams.get('grouping') as Grouping;
	const groupingItemId = lowerCaseParams.get('groupingitemid');

	if(!grouping || !Object.values(Grouping).includes(grouping)) {
		return {
			...filter,
			grouping: Grouping.NoGrouping
		};
	}

	if(grouping === Grouping.NoGrouping || !groupingItemId) {
		return {
			...filter,
			grouping: Grouping.NoGrouping
		};
	}

	return {
		...filter,
		grouping,
		groupingItemId
	};
}

export function searchParamsToLowerCase(params: URLSearchParams): URLSearchParams {
	const newParams = new URLSearchParams();
	for (const [name, value] of params) {
		newParams.append(name.toLowerCase(), value.toLowerCase());
	}
	return newParams;
}
