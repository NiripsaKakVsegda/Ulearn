import { defaultFilterState, Grouping, HistoryTimeSpan } from "../RevoewQueue.types";
import { reviewQueueFilter } from "../../../utils/localStorageManager";
import { DateSort } from "../../../models/instructor";

export interface ReviewQueueFilterLocalStorage {
	grouping: Grouping;
	timeSpan: HistoryTimeSpan;
	sort: DateSort;
}

export function getFilterFromLocalStorage(): Partial<ReviewQueueFilterLocalStorage> {
	const filter = JSON.parse(localStorage.getItem(reviewQueueFilter) ?? '{}');
	const resultFilter: Partial<ReviewQueueFilterLocalStorage> = {};
	if(filter.grouping && Object.values(Grouping).includes(filter.grouping)) {
		resultFilter.grouping = filter.grouping;
	}
	if(filter.timeSpan && Object.values(HistoryTimeSpan).includes(filter.timeSpan)) {
		resultFilter.timeSpan = filter.timeSpan;
	}
	if(filter.sort && Object.values(DateSort).includes(filter.sort)) {
		resultFilter.sort = filter.sort;
	}
	return resultFilter;
}

export function updateLocalStorageFilter(filter: Partial<ReviewQueueFilterLocalStorage>) {
	const current = getFilterFromLocalStorage();
	const updated = {
		...current,
		...filter
	};
	localStorage.setItem(reviewQueueFilter, JSON.stringify(updated));
}

export function isFilterUpdated(filter: Partial<ReviewQueueFilterLocalStorage>): boolean {
	const current = getFilterFromLocalStorage();
	return (!!filter.grouping && (
			(current.grouping && filter.grouping !== current.grouping) ||
			(filter.grouping !== Grouping.NoGrouping)
		)) ||
		(!!filter.timeSpan && (
			(current.timeSpan && filter.timeSpan !== current.timeSpan) ||
			(filter.timeSpan !== defaultFilterState.timeSpan)
		)) ||
		(!!filter.sort && (
			(current.sort && filter.sort !== current.sort) ||
			(filter.sort !== defaultFilterState.sort)
		));
}
