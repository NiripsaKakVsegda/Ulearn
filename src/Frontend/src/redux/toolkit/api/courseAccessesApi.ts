import { createApi } from "@reduxjs/toolkit/dist/query/react";
import { fetchBaseQueryWithReauth } from "../utils/baseQueryWithReauth";
import { courseAccesses } from "../../../consts/routes";
import { CourseAccess } from "../../../models/courseAccess";
import { CourseAccessType } from "../../../consts/accessType";
import { HttpMethods } from "../../../consts/httpMethods";

export const courseAccessesApi = createApi({
	reducerPath: 'courseAccessesApi',
	baseQuery: fetchBaseQueryWithReauth({
		baseUrl: courseAccesses
	}),
	refetchOnMountOrArgChange: true,
	endpoints: (build) => ({
		grantAccess: build.mutation<
			CourseAccess,
			{
				courseId: string,
				userId: string,
				accessType: CourseAccessType,
				comment: string
			}>({
			query: ({ courseId, ...body }) => ({
				url: `${ courseId }`,
				method: HttpMethods.POST,
				body: body
			})
		}),
		revokeAccess: build.mutation<
			Response,
			{
				courseId: string,
				userId: string,
				accessType: CourseAccessType,
				comment: string
			}>({
			query: ({ courseId, ...body }) => ({
				url: `${ courseId }`,
				method: HttpMethods.DELETE,
				body: body
			})
		}),
	})
});
