import React from "react";
import UnitCard from "./UnitCard";
import { ComponentMeta, ComponentStory } from "@storybook/react";
import { mockFunc } from "../../../../utils/storyMock";

const unitTitle = "Первое знакомство с C#";

export default {
	title: "Cards/UnitPage/UnitCard",
	component: UnitCard
} as ComponentMeta<typeof UnitCard>;

const Template: ComponentStory<typeof UnitCard> = (args) => (
	<UnitCard { ...args } />
);

export const UnitCardStory = Template.bind({});
UnitCardStory.storyName = "Default";
UnitCardStory.args = {
	unitTitle: unitTitle,
	isCompleted: false,
	isAuthenticated: true,
	isModerator: true,
	totalPublishedFlashcardsCount: 20,
	approvedUserFlashcardsCount: 5,
	newUserFlashcardsCount: 8,
	declinedUserFlashcardsCount: 12,
	onStartChecking: mockFunc,
	onCreateNewFlashcard: mockFunc,
	onModerateNewFlashcards: mockFunc,
	onModerateDeclinedFlashcards: mockFunc,
	onModerateApprovedFlashcards: mockFunc
};

export const UnitCardEmptyForUserStory = Template.bind({});
UnitCardEmptyForUserStory.storyName = "Empty for user";
UnitCardEmptyForUserStory.args = {
	unitTitle: unitTitle,
	isCompleted: true,
	isAuthenticated: true,
	isModerator: false,
	totalPublishedFlashcardsCount: 0,
	onStartChecking: mockFunc,
	onCreateNewFlashcard: mockFunc,
};

export const UnitCardEmptyForModeratorStory = Template.bind({});
UnitCardEmptyForModeratorStory.storyName = "Empty for moderator";
UnitCardEmptyForModeratorStory.args = {
	unitTitle: unitTitle,
	isCompleted: true,
	isAuthenticated: true,
	isModerator: true,
	totalPublishedFlashcardsCount: 0,
	approvedUserFlashcardsCount: 0,
	newUserFlashcardsCount: 0,
	declinedUserFlashcardsCount: 0,
	onStartChecking: mockFunc,
	onCreateNewFlashcard: mockFunc,
	onModerateNewFlashcards: mockFunc,
	onModerateDeclinedFlashcards: mockFunc,
	onModerateApprovedFlashcards: mockFunc
};

export const UnitCardEmptyForModeratorStoryWithNewAndDeclinedStory = Template.bind({});
UnitCardEmptyForModeratorStoryWithNewAndDeclinedStory.storyName = "Empty for moderator with new and declined";
UnitCardEmptyForModeratorStoryWithNewAndDeclinedStory.args = {
	unitTitle: unitTitle,
	isCompleted: true,
	isAuthenticated: true,
	isModerator: true,
	totalPublishedFlashcardsCount: 0,
	approvedUserFlashcardsCount: 0,
	newUserFlashcardsCount: 5,
	declinedUserFlashcardsCount: 8,
	onStartChecking: mockFunc,
	onCreateNewFlashcard: mockFunc,
	onModerateNewFlashcards: mockFunc,
	onModerateDeclinedFlashcards: mockFunc,
	onModerateApprovedFlashcards: mockFunc
};

export const UnitCard3Story = Template.bind({});
UnitCard3Story.storyName = "3 Cards";
UnitCard3Story.args = {
	isCompleted: true,
	totalPublishedFlashcardsCount: 3,
	unitTitle: unitTitle,
	isAuthenticated: true,
	isModerator: false,
	onStartChecking: mockFunc,
	onCreateNewFlashcard: mockFunc,
};

export const UnitCard2Story = Template.bind({});
UnitCard2Story.storyName = "2 Cards";
UnitCard2Story.args = {
	isCompleted: true,
	totalPublishedFlashcardsCount: 2,
	unitTitle: unitTitle,
	isAuthenticated: true,
	isModerator: false,
	onStartChecking: mockFunc,
	onCreateNewFlashcard: mockFunc,
};

export const UnitCard1Story = Template.bind({});
UnitCard1Story.storyName = "1 Card";
UnitCard1Story.args = {
	isCompleted: true,
	totalPublishedFlashcardsCount: 1,
	unitTitle: unitTitle,
	isAuthenticated: true,
	isModerator: false,
	onStartChecking: mockFunc,
	onCreateNewFlashcard: mockFunc,
};

export const LongTitleStory = Template.bind({});
LongTitleStory.storyName = "Long title";
LongTitleStory.args = {
	unitTitle: getBigTitle(),
	isCompleted: true,
	totalPublishedFlashcardsCount: 1,
	isAuthenticated: true,
	isModerator: false,
	onStartChecking: mockFunc,
	onCreateNewFlashcard: mockFunc,
};

function getBigTitle() {
	return "Большое название, которое все ломает совсем-совсем, " +
		"не люблю такие, да кто любит? - НИКТО... вооот.фыыdfvbg34tgf4fsdaf23rfewf23ыы";
}

