import React from "react";
import UnitPage from "./UnitPage";
import { flashcardsActions, flashcards, guides, userGeneratedFlashcard, } from "src/components/flashcards/storyData";
import { RateTypes } from "src/consts/rateTypes";
import { UnitFlashcards } from "../../../models/flashcards";
import { ComponentMeta, ComponentStory } from "@storybook/react";

export default {
	title: "Cards/UnitPage",
	component: UnitPage
} as ComponentMeta<typeof UnitPage>;

const Template: ComponentStory<typeof UnitPage> = (args) => (
	<UnitPage { ...args } />
);

export const UnitPageStory = Template.bind({});
UnitPageStory.storyName = "Default";
UnitPageStory.args = {
	userId: 'userId',
	isModerator: false,
	courseId: 'basicProgramming',
	unitId: flashcards[0].unitId,
	unitTitle: flashcards[0].unitTitle,
	courseFlashcards: flashcards,
	isFlashcardsLoading: false,
	newUserFlashcards: [userGeneratedFlashcard],
	declinedUserFlashcards: [userGeneratedFlashcard],
	isUserFlashcardsLoading: false,
	guides: guides,
	flashcardsActions: flashcardsActions
};

export const UnitPageEmptyForUserStory = Template.bind({});
UnitPageEmptyForUserStory.storyName = "Empty for user";
UnitPageEmptyForUserStory.args = {
	userId: 'userId',
	isModerator: false,
	courseId: 'basicProgramming',
	unitId: flashcards[0].unitId,
	unitTitle: flashcards[0].unitTitle,
	courseFlashcards: flashcards.map((u, i) => i === 0
		? { ...u, flashcards: [], unlocked: true }
		: u
	),
	isFlashcardsLoading: false,
	newUserFlashcards: [],
	declinedUserFlashcards: [],
	isUserFlashcardsLoading: false,
	guides: guides,
	flashcardsActions: flashcardsActions
};

export const UnitPageEmptyForModeratorStory = Template.bind({});
UnitPageEmptyForModeratorStory.storyName = "Empty for moderator";
UnitPageEmptyForModeratorStory.args = {
	userId: 'userId',
	isModerator: true,
	courseId: 'basicProgramming',
	unitId: flashcards[0].unitId,
	unitTitle: flashcards[0].unitTitle,
	courseFlashcards: flashcards.map((u, i) => i === 0
		? { ...u, flashcards: [], unlocked: true }
		: u
	),
	isFlashcardsLoading: false,
	newUserFlashcards: [],
	declinedUserFlashcards: [],
	isUserFlashcardsLoading: false,
	guides: guides,
	flashcardsActions: flashcardsActions
};

export const UnitPageEmptyForModeratorWithNewAndDeclinedStory = Template.bind({});
UnitPageEmptyForModeratorWithNewAndDeclinedStory.storyName = "Empty for moderator with new and declined";
UnitPageEmptyForModeratorWithNewAndDeclinedStory.args = {
	userId: 'userId',
	isModerator: true,
	courseId: 'basicProgramming',
	unitId: flashcards[0].unitId,
	unitTitle: flashcards[0].unitTitle,
	courseFlashcards: flashcards.map((u, i) => i === 0
		? { ...u, flashcards: [], unlocked: true }
		: u
	),
	isFlashcardsLoading: false,
	newUserFlashcards: [userGeneratedFlashcard],
	declinedUserFlashcards: [userGeneratedFlashcard],
	isUserFlashcardsLoading: false,
	guides: guides,
	flashcardsActions: flashcardsActions
};

export const UnitPageCompletedStory = Template.bind({});
UnitPageCompletedStory.storyName = "Completed";
UnitPageCompletedStory.args = {
	userId: 'userId',
	isModerator: false,
	courseId: 'basicProgramming',
	unitId: flashcards[0].unitId,
	unitTitle: flashcards[0].unitTitle,
	courseFlashcards: filterFlashcards(flashcards, true, 3),
	isFlashcardsLoading: false,
	newUserFlashcards: [],
	declinedUserFlashcards: [],
	isUserFlashcardsLoading: false,
	guides: guides,
	flashcardsActions: flashcardsActions
};

export const UnitPageNoProgressStory = Template.bind({});
UnitPageNoProgressStory.storyName = "No progress";
UnitPageNoProgressStory.args = {
	userId: 'userId',
	isModerator: false,
	courseId: 'basicProgramming',
	unitId: flashcards[0].unitId,
	unitTitle: flashcards[0].unitTitle,
	courseFlashcards: filterFlashcards(flashcards, false, 3),
	isFlashcardsLoading: false,
	newUserFlashcards: [],
	declinedUserFlashcards: [],
	isUserFlashcardsLoading: false,
	guides: guides,
	flashcardsActions: flashcardsActions
};

function filterFlashcards(flashcards: UnitFlashcards[], rated: boolean, count: number): UnitFlashcards[] {
	const filtered = [...flashcards];
	filtered[0] = {
		...filtered[0],
		unlocked: rated,
		flashcards: filtered[0].flashcards
			.map(f => ({ ...f, rate: rated ? RateTypes.rate5 : RateTypes.notRated }))
			.slice(0, count)
	};

	return filtered;
}
