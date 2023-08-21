import { GroupsSearchParameters } from "../../../models/groups";
import { groupsApi } from "../../../redux/toolkit/api/groups/groupsApi";
import { ShortGroupInfo } from "../../../models/comments";

export function useGroupsSearch(
	courseId?: string,
	params?: Partial<Omit<GroupsSearchParameters, 'courseId' | 'query'>>
) {
	const [searchGroupsQuery] = groupsApi.useLazySearchGroupsQuery();

	return searchGroups;

	async function searchGroups(query: string): Promise<ShortGroupInfo[]> {
		const response = await searchGroupsQuery({
			courseId,
			query,
			includeArchived: true,
			count: 10,
			...params,
		}).unwrap();
		return response.groups;
	}
}
