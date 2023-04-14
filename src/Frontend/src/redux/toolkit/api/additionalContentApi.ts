import { createApi } from "@reduxjs/toolkit/dist/query/react";
import { fetchBaseQueryWithReauth } from "../utils/baseQueryWithReauth";
import { additionalContent } from "../../../consts/routes";
import {
	AdditionalContentPublicationResponse,
	AdditionalContentPublicationsResponse
} from "../../../models/additionalContent";
import { HttpMethods } from "../../../consts/httpMethods";

export const additionalContentApi = createApi({
	reducerPath: 'additionalContentApi',
	baseQuery: fetchBaseQueryWithReauth({
		baseUrl: additionalContent,
		credentials: 'include',
	}),
	refetchOnMountOrArgChange: true,
	endpoints: (build) => ({
		getAdditionalContent: build.query<AdditionalContentPublicationsResponse, { courseId: string, groupId: number }>
		({
			query: ({ courseId, groupId }) => ({
				url: '',
				params: {
					courseId,
					groupId
				},
			}),
		}),
		addPublication: build.mutation<
			AdditionalContentPublicationResponse,
			{ courseId: string, groupId: number, unitId: string, slideId?: string, date: string }
		>({
			query: (params) => ({
				url: '',
				method: HttpMethods.POST,
				params: params
			}),
			onQueryStarted({ courseId, groupId }, { dispatch, queryFulfilled }) {
				queryFulfilled.then(({ data: addedContent }) => {
					dispatch(additionalContentApi.util.updateQueryData(
						'getAdditionalContent',
						{ courseId, groupId },
						(draft) => {
							draft.publications.push(addedContent);
						}
					));
				});
			}
		}),
		updatePublication: build.mutation<
			Response,
			{ publication: AdditionalContentPublicationResponse, date: string }
		>({
			query: ({ publication, date }) => ({
				url: `${ publication.id }`,
				method: HttpMethods.PATCH,
				params: {
					date
				}
			}),
			onQueryStarted({ publication, date }, { dispatch, queryFulfilled }) {
				queryFulfilled.then(() => {
					dispatch(additionalContentApi.util.updateQueryData(
						'getAdditionalContent',
						{ courseId: publication.courseId, groupId: publication.groupId },
						(draft) => {
							draft.publications.forEach(source =>{
								if (source.id === publication.id){
									source.date = date;
								}
							});
						}
					));
				});
			}
		}),
		deletePublication: build.mutation<Response, { publication: AdditionalContentPublicationResponse }>({
			query: ({ publication }) => ({
				url: `${ publication.id }`,
				method: HttpMethods.DELETE
			}),
			onQueryStarted({ publication }, { dispatch, queryFulfilled }) {
				queryFulfilled.then(() => {
					dispatch(additionalContentApi.util.updateQueryData(
						'getAdditionalContent',
						{ courseId: publication.courseId, groupId: publication.groupId },
						(draft) => {
							draft.publications = draft.publications
								.filter(source => source.id !== publication.id);
						}
					));
				});
			}
		}),
	})
});
