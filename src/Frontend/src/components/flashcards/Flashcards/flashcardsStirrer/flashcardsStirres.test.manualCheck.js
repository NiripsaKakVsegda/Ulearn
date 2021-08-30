import { getNextFlashcardRandomly } from "./flashcardsStirrer";
import { RateTypes } from "src/consts/rateTypes";

import Flashcard from "./flashcardsStirres.test.base";

const { rate1, rate2, rate3, rate4, rate5 } = RateTypes;

const mapNumberToRateType = {
	1: rate1,
	2: rate2,
	3: rate3,
	4: rate4,
	5: rate5,
};

describe('flashcardsStirrer course flashcards getter should', () => {
	test('built good sequence ( !!!NOT AUTO VALIDATION, NEEDS MANUAL CHECK!!! )', () => {
		const getRandomRate = () => mapNumberToRateType[Math.ceil(Math.random() * 5)];
		const sequence = [
			new Flashcard(getRandomRate(), 1),
			new Flashcard(getRandomRate(), 4),
			new Flashcard(getRandomRate(), 5),
			new Flashcard(getRandomRate(), 2),
			new Flashcard(getRandomRate(), 3),
		];

		console.log(sequence);
		let maxTLast = 5;
		const builtedSequenceHistory = [];

		for (let i = 0; i < 15; i++) {
			const flashcard = getNextFlashcardRandomly(sequence, maxTLast);
			const historyInfo = { ...flashcard };
			const flashcardInSequence = sequence.find(fc => fc.id === flashcard.id);

			maxTLast++;

			flashcardInSequence.lastRateIndex = maxTLast;
			flashcardInSequence.rate = getRandomRate();

			historyInfo.newRate = flashcardInSequence.rate;
			historyInfo.newRateIndex = flashcardInSequence.lastRateIndex;

			builtedSequenceHistory.push(historyInfo);
		}

		console.log(builtedSequenceHistory);
	});
});
