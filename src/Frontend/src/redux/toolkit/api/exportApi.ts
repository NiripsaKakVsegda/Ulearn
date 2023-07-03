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
		downloadQuizResults: build.query<void, {
			groupId: number, quizSlideId?: string, fileName?: string,
			gender?: boolean,
			telegram?: boolean,
			vk?: boolean,
			email?: boolean
		}>(
			{
				query: ({ groupId, quizSlideId, fileName, telegram, vk, email, gender }) => ({
					url: 'users-info-and-results',
					method: HttpMethods.GET,
					params: {
						groupId,
						quizSlideId,
						telegram,
						vk,
						email,
						gender,
					},
					cache: "no-cache",
					responseHandler: response => responseDownloadHandler(response, fileName)
				})
			})
	})
});
