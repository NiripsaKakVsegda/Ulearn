import { flashcardsApi } from "../../../redux/toolkit/api/flashcardsApi";
import { userFlashcardsApi } from "../../../redux/toolkit/api/userFlashcardsApi";
import { RateTypes } from "../../../consts/rateTypes";
import { UserGeneratedFlashcard } from "../../../models/flashcards";
import { FlashcardsActions } from "../Flashcards/Flashcards.types";

export function useFlashcardsActions(includeCreate?: boolean): FlashcardsActions {
	const [updateFlashcardRate] = flashcardsApi.useUpdateFlashcardStatusMutation();
	const [editFlashcardMutation] = userFlashcardsApi.useEditFlashcardMutation();
	const [removeFlashcardMutation] = userFlashcardsApi.useRemoveFlashcardMutation();
	const [approveFlashcardMutation] = userFlashcardsApi.useApproveFlashcardMutation();
	const [declineFlashcardMutation] = userFlashcardsApi.useDeclineFlashcardMutation();
	const createFlashcardMutation = includeCreate
		? userFlashcardsApi.useCreateFlashcardMutation()[0]
		: undefined;

	return {
		onSendFlashcardRate: (courseId: string, flashcardId: string, rate: RateTypes, newLastRateIndex: number) =>
			updateFlashcardRate({ courseId, flashcardId, rate, newLastRateIndex }),
		onEditFlashcard: (flashcard: UserGeneratedFlashcard, question?: string, answer?: string) =>
			editFlashcardMutation({ flashcard, question, answer }).unwrap(),
		onRemoveFlashcard: (flashcard: UserGeneratedFlashcard) =>
			removeFlashcardMutation({ flashcard }).unwrap(),
		onApproveFlashcard: (flashcard: UserGeneratedFlashcard, question?: string, answer?: string) =>
			approveFlashcardMutation({ flashcard, question, answer }).unwrap(),
		onDeclineFlashcard: (flashcard: UserGeneratedFlashcard) =>
			declineFlashcardMutation({ flashcard }).unwrap(),
		onCreateFlashcard: createFlashcardMutation
			? (courseId: string, unitId: string, question: string, answer: string, approved?: boolean) =>
				createFlashcardMutation({ courseId, unitId, question, answer, approved }).unwrap()
			: undefined
	};
}
