import React from "react";
import UnitPage from "./UnitPage.js";
import { flashcards, guides, infoByUnits, } from "src/components/flashcards/storyData";
import { RateTypes } from "src/consts/rateTypes";
import { Flashcard } from "src/models/flashcards";

export default {
	title: "Cards/UnitPage",
};

export const _3CardsWithSuccess = (): React.ReactNode => (
	<UnitPage
		infoByUnits={ infoByUnits }
		guides={ guides }
		flashcards={ filterFlashcards(flashcards, true, 3) }
		courseId={ '' }
		unitId={ flashcards[0].unitId }
		unitTitle={ flashcards[0].unitTitle }
	/>
);

_3CardsWithSuccess.storyName = "3 cards with success";

export const _3Cards = (): React.ReactNode => (
	<UnitPage
		infoByUnits={ infoByUnits }
		guides={ guides }
		flashcards={ filterFlashcards(flashcards, false, 3) }
		courseId={ '' }
		unitId={ flashcards[0].unitId }
		unitTitle={ flashcards[0].unitTitle }
	/>
);

_3Cards.storyName = "3 cards";

export const _2CardsWithSuccess = (): React.ReactNode => (
	<UnitPage
		infoByUnits={ infoByUnits }
		guides={ guides }
		flashcards={ filterFlashcards(flashcards, true, 2) }
		courseId={ '' }
		unitId={ flashcards[0].unitId }
		unitTitle={ flashcards[0].unitTitle }
	/>
);

_2CardsWithSuccess.storyName = "2 cards with success";

export const _2Cards = (): React.ReactNode => (
	<UnitPage
		infoByUnits={ infoByUnits }
		guides={ guides }
		flashcards={ filterFlashcards(flashcards, false, 2) }
		courseId={ '' }
		unitId={ flashcards[0].unitId }
		unitTitle={ flashcards[0].unitTitle }
	/>
);

_2Cards.storyName = "2 cards";

export const _1CardWithSuccess = (): React.ReactNode => (
	<UnitPage
		infoByUnits={ infoByUnits }
		guides={ guides }
		flashcards={ filterFlashcards(flashcards, true, 1) }
		courseId={ '' }
		unitId={ flashcards[0].unitId }
		unitTitle={ flashcards[0].unitTitle }
	/>
);

_1CardWithSuccess.storyName = "1 card with success";

export const _1Card = (): React.ReactNode => (
	<UnitPage
		infoByUnits={ infoByUnits }
		guides={ guides }
		flashcards={ filterFlashcards(flashcards, false, 1) }
		courseId={ '' }
		unitId={ flashcards[0].unitId }
		unitTitle={ flashcards[0].unitTitle }
	/>
);

_1Card.storyName = "1 card";

function filterFlashcards(flashcards: Flashcard[], rated: boolean, count: number,) {
	const unitId = flashcards[0].unitId;
	const result = flashcards
		.filter(f =>
			rated
				? f.rate !== RateTypes.notRated
				: f.rate === RateTypes.notRated
		)
		.slice(0, count);
	result.forEach(f => f.unitId = unitId);

	return result;
}
