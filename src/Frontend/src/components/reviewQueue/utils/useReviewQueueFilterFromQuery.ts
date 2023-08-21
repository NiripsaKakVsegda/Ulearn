import { CourseInfo } from "../../../models/course";
import { ReviewQueueFilterState } from "../RevoewQueue.types";
import { useEffect, useState } from "react";
import { useSearchParams } from "react-router-dom";
import { StudentsFilter } from "../../../models/instructor";
import { usersApi } from "../../../redux/toolkit/api/usersApi";
import { groupsApi } from "../../../redux/toolkit/api/groups/groupsApi";
import getFilterSearchParamsFromQuery from "./getFilterSearchParamsFromQuery";
import buildFilterSearchQueryParams from "./buildFilterSearchQueryParams";

export default function useReviewQueueFilterFromQuery(course?: CourseInfo, loadStudentsAndGroupsInfo?: boolean):
	[filter: ReviewQueueFilterState | undefined, updateFilter: (filter: ReviewQueueFilterState) => void] {

	const [findUsersByIdsQuery] = usersApi.useLazyFindUsersByIdsQuery();
	const [findGroupsByIdsQuery] = groupsApi.useLazyFindGroupsByIdsQuery();

	const [filter, setFilter] = useState<ReviewQueueFilterState>();
	const [searchParams, setSearchParams] = useSearchParams();

	useEffect(() => {
		if(filter) {
			setSearchParams(buildFilterSearchQueryParams(filter), { replace: true });
		}
	}, [filter]);

	useEffect(() => {
		if(course) {
			getFilterFromQuery(searchParams, course)
				.then(f => setFilter(f));
		}
	}, [course]);

	async function getFilterFromQuery(params: URLSearchParams, course: CourseInfo): Promise<ReviewQueueFilterState> {
		const filter = getFilterSearchParamsFromQuery(params, course) as ReviewQueueFilterState;

		if(filter.studentsFilter === StudentsFilter.StudentIds) {
			let studentIds = filter.studentIds ?? [];
			let students = undefined;

			if(loadStudentsAndGroupsInfo) {
				students = (await findUsersByIdsQuery({ userIds: studentIds }).unwrap()).foundUsers;
				studentIds = students.map(s => s.id);
			}

			return {
				...filter,
				studentIds,
				students
			};
		}

		if(filter.studentsFilter === StudentsFilter.GroupIds) {
			let groupIds = filter.groupIds ?? [];
			let groups = undefined;

			if(loadStudentsAndGroupsInfo) {
				groups = (await findGroupsByIdsQuery({ groupIds }).unwrap()).foundGroups;
				groupIds = groups.map(g => g.id);
			}

			return {
				...filter,
				groupIds,
				groups
			};
		}

		return filter;
	}

	return [filter, setFilter];
}
