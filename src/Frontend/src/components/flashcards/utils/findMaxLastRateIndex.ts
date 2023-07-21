import { BaseFlashcard } from "../../../models/flashcards";

export function findMaxLastRateIndex(flashcards: BaseFlashcard[]) {
	return flashcards.length === 0
		? -1
		: Math.max(...flashcards.map(f => f.lastRateIndex));
}
