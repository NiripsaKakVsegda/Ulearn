import { createApi } from "@reduxjs/toolkit/query/react";
import { groups, superGroups } from "../../../../consts/routes";
import {
	CopyGroupResponse,
	CreateGroupResponse,
	GroupInfo,
	GroupsByIdsResponse,
	GroupsInfoResponse,
	GroupsListParameters, GroupsSearchParameters, GroupsSearchResponse,
	GroupType,
	SuperGroupsListResponse
} from "../../../../models/groups";
import { fetchBaseQueryWithReauth } from "../../utils/baseQueryWithReauth";
import { HttpMethods } from "../../../../consts/httpMethods";
import { buildQuery } from "../../../../utils";

export const groupsApi = createApi({
	reducerPath: 'groupsApi',
	baseQuery: fetchBaseQueryWithReauth({
		baseUrl: groups,
	}),
	refetchOnMountOrArgChange: true,
	endpoints: (build) => ({
		getGroups: build.query<GroupsInfoResponse, Partial<GroupsListParameters>>({
			query: (params) => ({
				url: '',
				params: {
					...params
				}
			})
		}),
		findGroupsByIds: build.query<GroupsByIdsResponse, { groupIds: number[] }>({
			query: (params) => ({
				url: `by-ids${ buildQuery(params) }`
			})
		}),
		searchGroups: build.query<GroupsSearchResponse, Partial<GroupsSearchParameters>>({
			query: (params) => ({
				url: `search${ buildQuery(params) }`
			})
		}),

		createGroup: build.mutation<CreateGroupResponse, { courseId: string, name: string }>({
			query: ({ courseId, name }) => ({
				url: '',
				method: HttpMethods.POST,
				params: { courseId },
				body: { name, groupType: GroupType.SingleGroup }
			})
		}),
		copyGroup: build.mutation<
			CopyGroupResponse,
			{ groupId: number, destinationCourseId: string, makeMeOwner: boolean }
		>({
			query: ({ groupId, destinationCourseId, makeMeOwner }) => ({
				url: `${ groupId }/copy`,
				method: HttpMethods.POST,
				params: {
					destinationCourseId,
					makeMeOwner
				}
			})
		}),
		deleteGroup: build.mutation<Response, { group: GroupInfo }>({
			query: ({ group }) => ({
				url: `${ group.id }`,
				method: HttpMethods.DELETE,
			})
		})
	})
});

export const superGroupsApi = createApi({
	reducerPath: 'superGroupsApi',
	baseQuery: fetchBaseQueryWithReauth({
		baseUrl: superGroups,
	}),
	refetchOnMountOrArgChange: true,
	endpoints: (build) => ({
		getGroups: build.query<SuperGroupsListResponse, Partial<GroupsListParameters>>({
			query: (params) => ({
				url: '',
				params: {
					...params
				}
			})
		}),
	})
});
