import { Flashcard, UnitFlashcardsInfo } from "src/models/flashcards";
import { ShortSlideInfo } from "src/models/slide";

export default function getFlashcardsWithTheorySlides(
	infoByUnits: UnitFlashcardsInfo[],
	courseFlashcards: { [flashcardId: string]: Flashcard },
	courseSlides: ShortSlideInfo[]
): FlashcardWithTheorySlides[] {
	const flashcards: FlashcardWithTheorySlides[] = [];

	for (const { flashcardsIds } of infoByUnits) {
		flashcards.push(...getUnitFlashcardsWithTheorySlides(flashcardsIds, courseFlashcards, courseSlides));
	}

	return flashcards;
}

interface FlashcardWithTheorySlides extends Flashcard {
	theorySlides: { slug: string, title: string }[];
}

function getUnitFlashcardsWithTheorySlides(
	flashcardsIds: string[],
	courseFlashcards: { [flashcardId: string]: Flashcard },
	courseSlides: ShortSlideInfo[]
) {
	const flashcards: FlashcardWithTheorySlides[] = [];

	for (const id of flashcardsIds) {
		const flashcardInRedux = courseFlashcards[id];
		const flashcard: Flashcard = { ...flashcardInRedux };
		const { theorySlidesIds } = flashcard;
		const theorySlides = getTheorySlides(theorySlidesIds, courseSlides);

		flashcards.push({
			...flashcard,
			theorySlides,
		});
	}

	return flashcards;
}

function getTheorySlides(theorySlidesIds: string[], courseSlides: ShortSlideInfo[]) {
	const theorySlides = [];

	for (const slideId of theorySlidesIds) {
		const courseSlide = courseSlides.find(slide => slide.id === slideId);

		if(courseSlide) {
			theorySlides.push({
				slug: courseSlide.slug,
				title: courseSlide.title,
			});
		}
	}

	return theorySlides;
}
