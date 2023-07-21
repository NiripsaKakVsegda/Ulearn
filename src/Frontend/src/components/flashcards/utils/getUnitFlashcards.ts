import { UnitFlashcards } from "../../../models/flashcards";
import { RateTypes } from "../../../consts/rateTypes";

export function getUnitFlashcards(
	courseFlashcards: UnitFlashcards[],
	unitId: string,
	onlyFailedFlashcards = false
) {
	const unitFlashcards = courseFlashcards.find(f => f.unitId === unitId)?.flashcards ?? [];

	return onlyFailedFlashcards
		? unitFlashcards
			.filter(f => f.rate === RateTypes.rate1 || f.rate === RateTypes.rate2)
		: unitFlashcards;
}
