import { createApi } from "@reduxjs/toolkit/dist/query/react";
import { fetchBaseQueryWithReauth } from "../utils/baseQueryWithReauth";
import { userFlashcards } from "../../../consts/routes";
import { HttpMethods } from "../../../consts/httpMethods";
import {
	FlashcardModerationStatus,
	FlashcardType,
	UserGeneratedFlashcard,
	UserGeneratedFlashcardsResponse
} from "../../../models/flashcards";
import { flashcardsApi } from "./flashcardsApi";
import { RateTypes } from "../../../consts/rateTypes";
import { MaybeDrafted } from "@reduxjs/toolkit/dist/query/core/buildThunks";
import { AppDispatch } from "../../../setupStore";
import { RootState } from "../../reducers";

export const userFlashcardsApi = createApi({
	reducerPath: 'userFlashcardsApi',
	baseQuery: fetchBaseQueryWithReauth({
		baseUrl: userFlashcards,
	}),
	keepUnusedDataFor: 20 * 60,
	endpoints: (build) => ({
		getFlashcards: build.query<
			UserGeneratedFlashcardsResponse,
			{
				courseId: string,
				unitId?: string,
				status?: FlashcardModerationStatus
			}
		>({
			query: (params) => ({
				url: '',
				params: params
			})
		}),
		createFlashcard: build.mutation<UserGeneratedFlashcard, {
			courseId: string,
			unitId: string,
			question: string,
			answer: string,
			approved?: boolean
		}>({
			query: (body) => ({
				url: '',
				method: HttpMethods.POST,
				body: body
			}),
			onQueryStarted(
				{ courseId, unitId },
				{ dispatch, queryFulfilled }
			) {
				queryFulfilled.then(({ data }) => {
					dispatch(flashcardsApi.util.updateQueryData(
						'getFlashcards',
						{ courseId },
						(draft) => {
							const unit = draft.units
								.find(u => u.unitId === unitId);
							if(unit) {
								unit.flashcards.push(data);
								unit.unlocked = false;
							}
						}
					));
					updateQueryData(dispatch, data, draft => {
							draft.flashcards.push(data);
						}
					);
				});
			}
		}),
		removeFlashcard: build.mutation<Response, { flashcard: UserGeneratedFlashcard }>({
			query: ({ flashcard }) => ({
				url: flashcard.id,
				method: HttpMethods.DELETE
			}),
			onQueryStarted(
				{ flashcard },
				{ dispatch, queryFulfilled }
			) {
				queryFulfilled.then(() => {
					dispatch(flashcardsApi.util.updateQueryData(
						'getFlashcards',
						{ courseId: flashcard.courseId },
						(draft) => {
							const unit = draft.units
								.find(u => u.unitId === flashcard.unitId);
							if(!unit) {
								return;
							}
							const prevLength = unit.flashcards.length;
							unit.flashcards = unit.flashcards
								.filter(f => f.id !== flashcard.id);
							if(unit.flashcards.length !== prevLength && !unit.unlocked) {
								unit.unlocked = unit.flashcards
									.every(f => f.rate !== RateTypes.notRated);
							}
						}
					));
					updateQueryData(dispatch, flashcard, draft => {
							draft.flashcards = draft.flashcards.filter(f => f.id !== flashcard.id);
						}
					);
				});
			}
		}),
		editFlashcard: build.mutation<UserGeneratedFlashcard, {
			flashcard: UserGeneratedFlashcard,
			question?: string,
			answer?: string
		}>({
			query: ({ flashcard, ...body }) => ({
				url: flashcard.id,
				method: HttpMethods.PATCH,
				body: body
			}),
			onQueryStarted(
				{ flashcard },
				{ dispatch, queryFulfilled }
			) {
				updateFlashcardsByUnitsQueryFromResponse(dispatch, queryFulfilled, flashcard);
			}
		}),
		approveFlashcard: build.mutation<UserGeneratedFlashcard, {
			flashcard: UserGeneratedFlashcard,
			question?: string,
			answer?: string
		}>({
			query: ({ flashcard, ...body }) => ({
				url: `${ flashcard.id }/approve`,
				method: HttpMethods.PUT,
				body: body
			}),
			onQueryStarted(
				{ flashcard },
				{ dispatch, queryFulfilled }
			) {
				updateFlashcardsByUnitsQueryFromResponse(dispatch, queryFulfilled, flashcard);
			}
		}),
		declineFlashcard: build.mutation<UserGeneratedFlashcard, { flashcard: UserGeneratedFlashcard }>({
			query: ({ flashcard }) => ({
				url: `${ flashcard.id }/decline`,
				method: HttpMethods.PUT
			}),
			onQueryStarted(
				{ flashcard },
				{ dispatch, queryFulfilled, getState }
			) {
				const userId = (getState() as RootState).account.id;
				updateFlashcardsByUnitsQueryFromResponse(dispatch, queryFulfilled, flashcard, userId);
			}
		}),
		restoreFlashcard: build.mutation<UserGeneratedFlashcard, { flashcard: UserGeneratedFlashcard }>({
			query: ({ flashcard }) => ({
				url: `${ flashcard.id }/restore`,
				method: HttpMethods.PUT
			}),
			onQueryStarted(
				{ flashcard },
				{ dispatch, queryFulfilled }
			) {
				updateFlashcardsByUnitsQueryFromResponse(dispatch, queryFulfilled, flashcard);
			}
		}),
	})
});

function updateFlashcardsByUnitsQueryFromResponse(
	dispatch: AppDispatch,
	queryFulfilled: Promise<{ data: UserGeneratedFlashcard }>,
	sourceFlashcard: UserGeneratedFlashcard,
	userId?: string
) {
	queryFulfilled.then(({ data }) => {
		const newFlashcard = {
			...data,
			lastRateIndex: sourceFlashcard.lastRateIndex
		} as UserGeneratedFlashcard;

		dispatch(flashcardsApi.util.updateQueryData(
			'getFlashcards',
			{ courseId: sourceFlashcard.courseId },
			(draft) => {
				const unit = draft.units
					.find(u => u.unitId === newFlashcard.unitId);
				if(!unit) {
					return;
				}
				const flashcardIndex = unit.flashcards
					.findIndex(f => f.id === newFlashcard.id && f.flashcardType === FlashcardType.UserFlashcard);

				if(flashcardIndex !== -1) {
					if(data.moderationStatus === FlashcardModerationStatus.Declined && data.owner?.id !== userId) {
						unit.flashcards = unit.flashcards
							.filter(f => f.id !== data.id);
						unit.unlocked = unit.flashcards
							.every(f => f.rate !== RateTypes.notRated);
					} else {
						unit.flashcards[flashcardIndex] = newFlashcard;
					}
				} else if(newFlashcard.moderationStatus === FlashcardModerationStatus.Approved) {
					unit.flashcards.push(newFlashcard);
					if(newFlashcard.rate === RateTypes.notRated) {
						unit.unlocked = false;
					}
				}
			}
		));

		updateQueryData(dispatch, sourceFlashcard, draft => {
				draft.flashcards = draft.flashcards.filter(f => f.id !== sourceFlashcard.id);
			}
		);
		updateQueryData(dispatch, newFlashcard, draft => {
				draft.flashcards.push(newFlashcard);
			}
		);
	});
}

function updateQueryData(
	dispatch: AppDispatch,
	flashcard: UserGeneratedFlashcard,
	updateRecipe: (draft: MaybeDrafted<UserGeneratedFlashcardsResponse>) => void
) {
	if(flashcard.moderationStatus) {
		dispatch(userFlashcardsApi.util.updateQueryData(
			'getFlashcards',
			{ courseId: flashcard.courseId, status: flashcard.moderationStatus },
			updateRecipe
		));
		dispatch(userFlashcardsApi.util.updateQueryData(
			'getFlashcards',
			{ courseId: flashcard.courseId, unitId: flashcard.unitId, status: flashcard.moderationStatus },
			updateRecipe
		));
	}
}
