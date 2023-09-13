import { createApi } from "@reduxjs/toolkit/dist/query/react";
import { users } from "../../../consts/routes";
import { UsersByIdsResponse, UsersSearchParameters, UsersSearchResponse } from "../../../models/users";
import { fetchBaseQueryWithReauth } from "../utils/baseQueryWithReauth";
import { buildQuery } from "../../../utils";

export const usersApi = createApi({
	reducerPath: 'usersApi',
	baseQuery: fetchBaseQueryWithReauth({
		baseUrl: users,
	}),
	refetchOnMountOrArgChange: true,
	endpoints: (build) => ({
		searchUsers: build.query<UsersSearchResponse, Partial<UsersSearchParameters>>({
			query: (params) => ({
				url: '',
				params
			})
		}),
		findUsersByIds: build.query<UsersByIdsResponse, { userIds: string[], courseId?: string }>({
			query: (params) => ({
				url: `by-ids${ buildQuery(params) }`
			})
		})
	})
});
