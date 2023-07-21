import { createApi } from "@reduxjs/toolkit/dist/query/react";
import { fetchBaseQueryWithReauth } from "../utils/baseQueryWithReauth";
import { HttpMethods } from "../../../consts/httpMethods";
import { FlashcardsByUnits } from "../../../models/flashcards";
import { RateTypes } from "../../../consts/rateTypes";

export const flashcardsApi = createApi({
	reducerPath: 'flashcardsApi',
	baseQuery: fetchBaseQueryWithReauth(),
	keepUnusedDataFor: 20 * 60,
	endpoints: (build) => ({
		getFlashcards: build.query<FlashcardsByUnits, { courseId: string }>({
			query: ({ courseId }) => ({
				url: `courses/${ courseId }/flashcards-by-units`
			})
		}),
		updateFlashcardStatus: build.mutation<Response, {
			courseId: string,
			flashcardId: string,
			rate: RateTypes,
			newLastRateIndex: number
		}>({
			query: ({ courseId, flashcardId, rate }) => ({
				url: `courses/${ courseId }/flashcards/${ flashcardId }/status`,
				method: HttpMethods.PUT,
				body: JSON.stringify(rate),
				headers: { 'Content-Type': 'application/json' }
			}),
			onQueryStarted(
				{ courseId, flashcardId, rate, newLastRateIndex },
				{ dispatch, queryFulfilled }
			) {
				queryFulfilled.then(() => {
					dispatch(flashcardsApi.util.updateQueryData(
						'getFlashcards',
						{ courseId },
						(draft) => {
							for (const unit of draft.units) {
								const flashcard = unit.flashcards
									.find(f => f.id === flashcardId);
								if(flashcard) {
									flashcard.rate = rate;
									flashcard.lastRateIndex = newLastRateIndex;
									if(!unit.unlocked) {
										unit.unlocked = unit.flashcards
											.every(f => f.rate !== RateTypes.notRated);
									}
								}
							}
						}
					));
				});
			}
		})
	})
});
