import { createApi } from "@reduxjs/toolkit/dist/query/react";
import { users } from "../../../consts/routes";
import { FoundUserResponse, UsersSearchResponse } from "../../../models/users";
import { fetchBaseQueryWithReauth } from "../utils/baseQueryWithReauth";

export const usersApi = createApi({
	reducerPath: 'usersApi',
	baseQuery: fetchBaseQueryWithReauth({
		baseUrl: users,
	}),
	refetchOnMountOrArgChange: true,
	endpoints: (build) => ({
		getCourseInstructors: build.query<FoundUserResponse[], { courseId: string, query?: string, count?: number }>({
			query: ({ courseId, query, count }) => ({
				url: '',
				params: {
					courseId,
					courseRole: 'Instructor',
					query,
					count
				}
			}),
			transformResponse: (response: UsersSearchResponse) => response.users,
		})
	})
});
