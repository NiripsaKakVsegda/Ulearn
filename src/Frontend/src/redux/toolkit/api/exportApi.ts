import { createApi } from "@reduxjs/toolkit/dist/query/react";
import { fetchBaseQueryWithReauth } from "../utils/baseQueryWithReauth";
import { HttpMethods } from "../../../consts/httpMethods";
import responseDownloadHandler from "../utils/responseDownloadHandler";

export const exportApi = createApi({
	reducerPath: 'exportApi',
	baseQuery: fetchBaseQueryWithReauth({
		baseUrl: 'export',
	}),
	refetchOnMountOrArgChange: true,
	endpoints: (build) => ({
		downloadQuizResults: build.query<void, { groupId: number, quizSlideId?: string, fileName?: string }>({
			query: ({ groupId, quizSlideId, fileName }) => ({
				url: 'users-info-and-results',
				method: HttpMethods.GET,
				params: {
					groupId,
					quizSlideId
				},
				cache: "no-cache",
				responseHandler: response => responseDownloadHandler(response, fileName)
			})
		})
	})
});
