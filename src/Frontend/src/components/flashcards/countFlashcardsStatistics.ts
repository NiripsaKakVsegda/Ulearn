import { RateTypes } from "src/consts/rateTypes";
import { Flashcard } from "src/models/flashcards";

export default function countFlashcardsStatistics(flashcards: Flashcard[]): Record<RateTypes, number> {
	const statistics: Record<string, number> = Object
		.values(RateTypes)
		.reduce((stats, rateType) => ({ ...stats, [rateType]: 0 }), {});

	for (const flashcard of flashcards) {
		statistics[flashcard.rate]++;
	}

	return statistics;
}
