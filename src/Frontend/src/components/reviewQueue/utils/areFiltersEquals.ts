import { ReviewQueueModalFilterState } from "../RevoewQueue.types";
import { StudentsFilter } from "../../../models/instructor";

export default function areFiltersEquals(f1: ReviewQueueModalFilterState, f2: ReviewQueueModalFilterState) {
	if(f1.unitId !== f2.unitId || f1.slideId !== f2.slideId || f1.studentsFilter !== f2.studentsFilter) {
		return false;
	}

	if(f1.studentsFilter === StudentsFilter.MyGroups || f1.studentsFilter === StudentsFilter.All) {
		return true;
	}

	const ids1 = f1.studentsFilter === StudentsFilter.StudentIds
		? f1.studentIds ?? []
		: f1.groupIds ?? [];

	const ids2 = f2.studentsFilter === StudentsFilter.StudentIds
		? f2.studentIds ?? []
		: f2.groupIds ?? [];

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
