import { RateTypes } from "../../../consts/rateTypes";
import { UserGeneratedFlashcard } from "../../../models/flashcards";

export const enum FlashcardsState {
	NoFlashcards = 'NoFlashcards',
	CreateCardBeforeUnit = 'CreateCardBeforeUnit',
	Unit = 'Unit',
	UnitRepeating = 'UnitRepeating',
	CreateCardAfterUnit = 'CreateCardAfterUnit',
	CourseRepeating = 'CourseRepeating',
	ModerateFlashcards = 'ModerateFlashcards'
}

export type InitialFlashcardsState =
	FlashcardsState.NoFlashcards |
	FlashcardsState.CreateCardBeforeUnit |
	FlashcardsState.Unit |
	FlashcardsState.CourseRepeating |
	FlashcardsState.ModerateFlashcards;

export const enum EditingState {
	EditingFlashcard = 1,
	ApprovingFlashcard = 2
}

export interface FlashcardsActions {
	onSendFlashcardRate: (courseId: string, id: string, rate: RateTypes, newLastRateIndex: number) =>
		void;
	onEditFlashcard: (flashcard: UserGeneratedFlashcard, question?: string, answer?: string) =>
		Promise<UserGeneratedFlashcard>;
	onRemoveFlashcard: (flashcard: UserGeneratedFlashcard) =>
		Promise<Response>;
	onApproveFlashcard: (flashcard: UserGeneratedFlashcard, question?: string, answer?: string) =>
		Promise<UserGeneratedFlashcard>;
	onDeclineFlashcard: (flashcard: UserGeneratedFlashcard) =>
		Promise<UserGeneratedFlashcard>;
	onCreateFlashcard?: (courseId: string, unitId: string, question: string, answer: string, approved?: boolean) =>
		Promise<UserGeneratedFlashcard>;
}
