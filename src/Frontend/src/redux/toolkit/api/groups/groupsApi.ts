import { createApi } from "@reduxjs/toolkit/query/react";
import { groups } from "../../../../consts/routes";
import {
	CopyGroupResponse,
	CreateGroupResponse,
	GroupInfo,
	GroupsInfoResponse,
	GroupsListParameters
} from "../../../../models/groups";
import { fetchBaseQueryWithReauth } from "../../utils/baseQueryWithReauth";
import { HttpMethods } from "../../../../consts/httpMethods";

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
			}),
		}),

		createGroup: build.mutation<CreateGroupResponse, { courseId: string, name: string }>({
			query: ({ courseId, name }) => ({
				url: '',
				method: HttpMethods.POST,
				params: { courseId },
				body: { name }
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
