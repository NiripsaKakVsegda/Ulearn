import { BaseFlashcard, UnitFlashcards } from "../../../models/flashcards";
import { RateTypes } from "../../../consts/rateTypes";

export function getUnlockedCourseFlashcards(courseFlashcards: UnitFlashcards[]) {
	return courseFlashcards
		.reduce((result, unitFlashcards) =>
				([...result, ...unitFlashcards.flashcards]),
			[] as BaseFlashcard[]
		)
		.filter(f => f.rate !== RateTypes.notRated);
}
