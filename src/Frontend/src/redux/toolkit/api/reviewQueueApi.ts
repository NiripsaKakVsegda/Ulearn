import { createApi } from "@reduxjs/toolkit/dist/query/react";
import { fetchBaseQueryWithReauth } from "../utils/baseQueryWithReauth";
import { reviewQueue } from "../../../consts/routes";
import {
	ReviewQueueFilterParameters,
	ReviewQueueHistoryFilterParameters,
	ReviewQueueResponse
} from "../../../models/instructor";
import { buildQuery } from "../../../utils";
import { HttpMethods } from "../../../consts/httpMethods";

export const reviewQueueApi = createApi({
	reducerPath: 'reviewQueueApi',
	baseQuery: fetchBaseQueryWithReauth(),
	refetchOnMountOrArgChange: true,
	endpoints: (build) => ({
		getReviewQueue: build.query<ReviewQueueResponse, ReviewQueueFilterParameters>({
			query: (params) => ({
				url: `${ reviewQueue }${ buildQuery({ ...params }) }`
			})
		}),
		getReviewQueueHistory: build.query<ReviewQueueResponse, ReviewQueueHistoryFilterParameters>({
			query: (params) => ({
				url: `${ reviewQueue }/history${ buildQuery({ ...params }) }`
			})
		}),
		lockSubmission: build.mutation<Response, { submissionId: number }>({
			query: ({ submissionId }) => ({
				url: `${ reviewQueue }/${ submissionId }/lock`,
				method: HttpMethods.PUT
			})
		})
	})
});
