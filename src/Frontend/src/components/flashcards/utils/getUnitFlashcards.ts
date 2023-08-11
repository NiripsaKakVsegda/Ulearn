import { BaseFlashcard, UnitFlashcards } from "../../../models/flashcards";
import { RateTypes } from "../../../consts/rateTypes";

export function getUnitFlashcards(	courseFlashcards: UnitFlashcards[],	unitId: string) {
	return  courseFlashcards.find(f => f.unitId === unitId)?.flashcards ?? [];
}

export function getFailedFlashcards(flashcards: BaseFlashcard[]){
	return flashcards
		.filter(f => f.rate === RateTypes.rate1 || f.rate === RateTypes.rate2)
}
