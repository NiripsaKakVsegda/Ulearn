import { BaseFlashcard, CourseFlashcard, FlashcardType, TheorySlideInfo, UnitFlashcards } from "../../../models/flashcards";
import { ShortSlideInfo } from "../../../models/slide";
import { CourseInfo } from "../../../models/course";

export function getCourseSlides(course: CourseInfo) {
	return course.units
		.reduce((result, unit) => [...result, ...unit.slides], [] as ShortSlideInfo[]);
}

export default function getFlashcardsWithTheorySlides(
	courseFlashcards: UnitFlashcards[],
	slides: ShortSlideInfo[]
): UnitFlashcards[] {
	return courseFlashcards.map(unitFlashcards => ({
		...unitFlashcards,
		flashcards: getUnitFlashcardsWithTheorySlides(unitFlashcards.flashcards, slides)
	}));
}

function getUnitFlashcardsWithTheorySlides(
	unitFlashcards: BaseFlashcard[],
	slides: ShortSlideInfo[]
): BaseFlashcard[] {
	return unitFlashcards
		.map(f => {
			if(f.flashcardType !== FlashcardType.CourseFlashcard) {
				return f;
			}
			const courseFlashcard = f as CourseFlashcard;
			if(!courseFlashcard.theorySlidesIds || courseFlashcard.theorySlidesIds.length === 0) {
				return courseFlashcard;
			}
			return {
				...courseFlashcard,
				theorySlides: getTheorySlides(courseFlashcard.theorySlidesIds, slides)
			} as CourseFlashcard;
		});
}

function getTheorySlides(theorySlidesIds: string[], slides: ShortSlideInfo[]): TheorySlideInfo[] {
	return theorySlidesIds
		.reduce((result, id) => {
			const slide = slides.find(s => s.id === id);
			return slide
				? [...result, { title: slide.title, slug: slide.slug }]
				: result;
		}, [] as TheorySlideInfo[]);
}
