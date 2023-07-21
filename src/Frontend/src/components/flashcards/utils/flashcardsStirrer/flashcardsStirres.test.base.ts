import { RateTypes } from "../../../../consts/rateTypes";
import { BaseFlashcard, FlashcardType, TheorySlideInfo } from '../../../../models/flashcards';

const { notRated, } = RateTypes;

let idCounter = 0;

export default class Flashcard implements BaseFlashcard {
	public id: string;
	public rate: RateTypes;
	public lastRateIndex: number;

	constructor(rate = notRated, lastRateIndex = 0) {
		this.id = (idCounter++).toString();
		this.rate = rate;
		this.lastRateIndex = lastRateIndex;
	}

	flashcardType = FlashcardType.CourseFlashcard;
	answer = '';
	question = '';
	theorySlides: TheorySlideInfo[] = [];
	theorySlidesIds: string[] = [];
	courseId = '';
	unitId = '';
}
