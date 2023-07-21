import { RateTypes } from "../../../consts/rateTypes";
import { BaseFlashcard } from "../../../models/flashcards";

export default function countFlashcardsStatistics(flashcards: BaseFlashcard[]): Record<RateTypes, number> {
	const statistics: Record<string, number> = Object.values(RateTypes)
		.reduce((stats, rateType) => ({ ...stats, [rateType]: 0 }), {});

	for (const flashcard of flashcards) {
		statistics[flashcard.rate]++;
	}

	return statistics;
}
