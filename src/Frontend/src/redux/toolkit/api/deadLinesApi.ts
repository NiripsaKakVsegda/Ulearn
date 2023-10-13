import { createApi } from "@reduxjs/toolkit/dist/query/react";
import { fetchBaseQueryWithReauth } from "../utils/baseQueryWithReauth";
import { deadLines } from "../../../consts/routes";
import { DeadLineInfo, DeadLineSlideType, DeadLinesResponse } from "../../../models/deadLines";
import { HttpMethods } from "../../../consts/httpMethods";
import { buildQuery } from "../../../utils";

export const deadLinesApi = createApi({
	reducerPath: 'deadLinesApi',
	baseQuery: fetchBaseQueryWithReauth({
		baseUrl: deadLines,
		credentials: 'include'
	}),
	refetchOnMountOrArgChange: true,
	endpoints: (build) => ({
		getDeadLines: build.query<DeadLinesResponse, { courseId: string, groupId: number }>({
			query: ({ courseId, groupId }) => ({
				url: '',
				params: {
					courseId: courseId.toLowerCase(),
					groupId
				}
			})
		}),
		createDeadLine: build.mutation<DeadLineInfo, { deadLine: DeadLineInfo }>({
			query: ({ deadLine }) => ({
				url: '',
				method: HttpMethods.POST,
				params: {
					...deadLine,
					courseId: deadLine.courseId.toLowerCase(),
					userIds: deadLine.userIds === null ? undefined : deadLine.userIds,
					slideType: deadLine.slideType === null ? DeadLineSlideType.All : deadLine.slideType,
					slideValue: deadLine.slideValue === null ? undefined : deadLine.slideValue,
					isOverlappedByOtherDeadLine: undefined,
					time: undefined,
					id: undefined
				}
			}),
			onQueryStarted({ deadLine }, { dispatch, queryFulfilled }) {
				queryFulfilled.then(({ data: createdDeadLine }) => {
					dispatch(deadLinesApi.util.updateQueryData(
						'getDeadLines',
						{ courseId: deadLine.courseId.toLowerCase(), groupId: deadLine.groupId },
						(draft) => {
							draft.deadLines.push(createdDeadLine);
						}
					));
				});
			}
		}),
		changeDeadLine: build.mutation<Response, { deadLine: DeadLineInfo }>({
			query: ({ deadLine }) => ({
				url: `${ deadLine.id }${ buildQuery({
					...deadLine,
					userIds: deadLine.userIds === null ? undefined : deadLine.userIds,
					slideType: deadLine.slideType === null ? DeadLineSlideType.All : deadLine.slideType,
					slideValue: deadLine.slideValue === null ? undefined : deadLine.slideValue,
					isOverlappedByOtherDeadLine: undefined,
					time: undefined,
					id: undefined,
				}) }`,
				method: HttpMethods.PATCH,
			}),
			onQueryStarted({ deadLine }, { dispatch, queryFulfilled }) {
				queryFulfilled.then(() => {
					dispatch(deadLinesApi.util.updateQueryData(
						'getDeadLines',
						{ courseId: deadLine.courseId.toLowerCase(), groupId: deadLine.groupId },
						(draft) => {
							const updatedIndex = draft.deadLines
								.findIndex(sourceDeadLine => sourceDeadLine.id === deadLine.id);
							if(updatedIndex !== -1) {
								draft.deadLines[updatedIndex] = deadLine;
							}
						}
					));
				});
			}
		}),
		deleteDeadLine: build.mutation<Response, { deadLine: DeadLineInfo }>({
			query: ({ deadLine }) => ({
				url: `${ deadLine.id }`,
				method: HttpMethods.DELETE,
			}),
			onQueryStarted({ deadLine }, { dispatch, queryFulfilled }) {
				queryFulfilled.then(() => {
					dispatch(deadLinesApi.util.updateQueryData(
						'getDeadLines',
						{ courseId: deadLine.courseId.toLowerCase(), groupId: deadLine.groupId },
						(draft) => {
							draft.deadLines = draft.deadLines
								.filter(source => source.id !== deadLine.id);
						}
					));
				});
			}
		})
	})
});
