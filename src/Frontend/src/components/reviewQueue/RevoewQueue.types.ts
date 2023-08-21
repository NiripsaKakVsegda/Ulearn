import { DateSort, StudentsFilter } from "../../models/instructor";
import { ShortUserInfo } from "../../models/users";
import { ShortGroupInfo } from "../../models/comments";

export enum Grouping {
	NoGrouping = "all",
	GroupStudents = "students",
	GroupExercises = "exercises",
}

export enum HistoryTimeSpan {
	Day = 'day',
	Week = 'week',
	Month = 'month',
	All = 'all'
}

export const defaultFilterState: ReviewQueueFilterSearchParams = {
	reviewed: false,
	sort: DateSort.Ascending,
	studentsFilter: StudentsFilter.MyGroups,
	timeSpan: HistoryTimeSpan.Day
};

export interface ReviewQueueFilterSearchParams {
	reviewed: boolean;
	sort: DateSort;
	timeSpan: HistoryTimeSpan;

	unitId?: string;
	slideId?: string;

	studentsFilter: StudentsFilter;
	studentIds?: string[];
	groupIds?: number[];
}

export interface InstructorReviewFilterSearchParams extends ReviewQueueFilterSearchParams {
	grouping: Grouping;
	groupingItemId?: string;
}

export interface ReviewQueueFilterState extends ReviewQueueFilterSearchParams {
	students?: ShortUserInfo[];
	groups?: ShortGroupInfo[];
}

export type ReviewQueueModalFilterState = Omit<ReviewQueueFilterState, "reviewed" | "sort" | "timeSpan">

export interface CourseSlidesInfo {
	units: UnitSlidesInfo[];
}

export interface UnitSlidesInfo {
	id: string;
	title: string;
	slides: SlideInfo[];
}

export interface SlideInfo {
	id: string;
	title: string;
}
