import { getNextFlashcardRandomly, sortFlashcardsInAuthorsOrderWithRate } from "./flashcardsStirrer";
import { RateTypes } from "src/consts/rateTypes";

import Flashcard from "./flashcardsStirres.test.base";

const { rate1, rate2, rate3, rate4, rate5 } = RateTypes;

describe('flashcardsStirrer unit sorting should', () => {
	test('sort unseen flashcards in authors order', () => {
		const sequence = [
			new Flashcard(),
			new Flashcard(),
			new Flashcard(),
			new Flashcard(),
			new Flashcard(),
		];

		const result = sortFlashcardsInAuthorsOrderWithRate(sequence);

		expect(result).toEqual(sequence);
	});

	test('sort unseen in authors order, then from worst rate to best rate', () => {
		const sequence = [
			new Flashcard(),
			new Flashcard(),
			new Flashcard(rate4),
			new Flashcard(),
			new Flashcard(rate1),
		];
		const answer = [sequence[0], sequence[1], sequence[3], sequence[4], sequence[2]];

		const result = sortFlashcardsInAuthorsOrderWithRate(sequence);

		expect(result).toStrictEqual(answer);
	});

	test('sort rated flashcards from worst rate to best rate', () => {
		const sequence = [
			new Flashcard(rate1),
			new Flashcard(rate5),
			new Flashcard(rate3),
			new Flashcard(rate2),
			new Flashcard(rate4),
		];
		const answer = [rate1, rate2, rate3, rate4, rate5];

		const result = sortFlashcardsInAuthorsOrderWithRate(sequence);

		const resultRates = result.reduce(
			(rates, flashcard) => [...rates, flashcard.rate], [] as RateTypes[]);
		expect(resultRates).toEqual(answer);
	});

	test('sort rated flashcards with same rate in authors order', () => {
		const sequence = [
			new Flashcard(rate1),
			new Flashcard(rate1),
			new Flashcard(rate1),
			new Flashcard(rate1),
			new Flashcard(rate1),
		];

		const result = sortFlashcardsInAuthorsOrderWithRate(sequence);

		expect(result).toEqual(sequence);
	});

	test('sort rated flashcards with same rate in authors order, from worst rate to best rate', () => {
		const sequence = [
			new Flashcard(rate2),
			new Flashcard(rate2),
			new Flashcard(rate1),
			new Flashcard(rate1),
			new Flashcard(rate1),
		];
		const answer = [sequence[2], sequence[3], sequence[4], sequence[0], sequence[1]];

		const result = sortFlashcardsInAuthorsOrderWithRate(sequence);

		expect(result).toStrictEqual(answer);
	});
});

describe('flashcardsStirrer course flashcards getter should', () => {
	test('not get same card twice', () => {
		const sequence = [
			new Flashcard(rate2, 1),
			new Flashcard(rate3, 4),
			new Flashcard(rate1, 5),
			new Flashcard(rate5, 2),
			new Flashcard(rate4, 3),
		];

		const first = getNextFlashcardRandomly(sequence, 5);
		(first as Flashcard).lastRateIndex = 6;
		const second = getNextFlashcardRandomly(sequence, 6);

		expect(first).not.toBe(second);
	});

	test('get random flashcard(100 iterations for 5 cards)', () => {
		const sequence = [
			new Flashcard(rate2, 1),
			new Flashcard(rate3, 4),
			new Flashcard(rate1, 5),
			new Flashcard(rate5, 2),
			new Flashcard(rate4, 3),
		];

		const meetedIds = new Set<number>();
		let maxTLast = 5;

		for (let i = 0; i < 100; i++) {
			const flashcard = getNextFlashcardRandomly(sequence, maxTLast);
			meetedIds.add((flashcard as Flashcard).id);
			maxTLast++;
			(sequence.find(fc => fc.id === (flashcard as Flashcard).id) as Flashcard).lastRateIndex = maxTLast;
		}

		expect(meetedIds.size).toBe(sequence.length);
	});

	test('return empty string when sequence is empty', () => {
		const result = getNextFlashcardRandomly([], 0);

		expect(result).toBe('');
	});

	test('return card once when sequence contains 1 card', () => {
		const sequence = [
			new Flashcard(rate1, 1),
		];

		const first = getNextFlashcardRandomly(sequence, 2);
		(first as Flashcard).lastRateIndex = 3;
		const second = getNextFlashcardRandomly(sequence, 3);

		expect(first).toBe(sequence[0]);
		expect(second).toBe('');
	});

	test('return both card when sequence contains 2 card', () => {
		const sequence = [
			new Flashcard(rate2, 1),
			new Flashcard(rate1, 2),
		];

		const first = getNextFlashcardRandomly(sequence, 2);
		(first as Flashcard).lastRateIndex = 3;
		const second = getNextFlashcardRandomly(sequence, 3);

		expect(first).not.toBe(second);
		expect(sequence).toContain(first);
		expect(sequence).toContain(second);
	});
});
