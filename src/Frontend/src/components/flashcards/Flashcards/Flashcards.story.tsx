import { ComponentMeta, ComponentStory } from "@storybook/react";
import Flashcards from "./Flashcards";
import { flashcards, flashcardsActions, userGeneratedFlashcard } from "../storyData";
import { mockFunc } from "../../../utils/storyMock";
import { disableViewport } from "../../course/Navigation/stroies.data";
import { FlashcardModerationStatus } from "../../../models/flashcards";
import { FlashcardsState } from "./Flashcards.types";

export default {
	title: "cards/Flashcards",
	component: Flashcards,
	argTypes: {
		initialState: {
			options: [
				FlashcardsState.NoFlashcards,
				FlashcardsState.CreateCardBeforeUnit,
				FlashcardsState.Unit,
				FlashcardsState.CourseRepeating,
				FlashcardsState.ModerateFlashcards
			],
			control: { type: 'radio' }
		}
	},
	...disableViewport
} as ComponentMeta<typeof Flashcards>;

const Template: ComponentStory<typeof Flashcards> = (args) => (
	<Flashcards { ...args } />
);

export const FlashcardsGuideStory = Template.bind({});
FlashcardsGuideStory.storyName = "FlashcardGuide";
FlashcardsGuideStory.args = {
	courseId: 'basicprogramming',
	unitId: flashcards[0].unitId,
	unitTitle: flashcards[0].unitTitle,
	userId: 'userId',
	isModerator: false,
	initialState: FlashcardsState.NoFlashcards,
	courseFlashcards: [],
	moderationFlashcards: [userGeneratedFlashcard],
	showModerationGuides: false,
	onClose: mockFunc,
	flashcardsActions: flashcardsActions
};

export const FlashcardsUnitStory = Template.bind({});
FlashcardsUnitStory.storyName = "Unit";
FlashcardsUnitStory.args = {
	courseId: 'basicprogramming',
	unitId: flashcards[0].unitId,
	unitTitle: flashcards[0].unitTitle,
	userId: 'userId',
	isModerator: false,
	initialState: FlashcardsState.Unit,
	courseFlashcards: flashcards,
	moderationFlashcards: [userGeneratedFlashcard],
	showModerationGuides: false,
	onClose: mockFunc,
	flashcardsActions: flashcardsActions
};

export const FlashcardsCreateCardBeforeUnitStory = Template.bind({});
FlashcardsCreateCardBeforeUnitStory.storyName = "CreateCardBeforeUnit state";
FlashcardsCreateCardBeforeUnitStory.args = {
	courseId: 'basicprogramming',
	unitId: flashcards[0].unitId,
	unitTitle: flashcards[0].unitTitle,
	userId: 'userId',
	isModerator: false,
	initialState: FlashcardsState.CreateCardBeforeUnit,
	courseFlashcards: flashcards,
	moderationFlashcards: [userGeneratedFlashcard],
	showModerationGuides: false,
	onClose: mockFunc,
	flashcardsActions: flashcardsActions
};

export const FlashcardsCourseRepeatingStory = Template.bind({});
FlashcardsCourseRepeatingStory.storyName = "CourseRepeating state";
FlashcardsCourseRepeatingStory.args = {
	courseId: 'basicprogramming',
	unitId: flashcards[0].unitId,
	unitTitle: flashcards[0].unitTitle,
	userId: 'userId',
	isModerator: false,
	initialState: FlashcardsState.CourseRepeating,
	courseFlashcards: flashcards,
	moderationFlashcards: [userGeneratedFlashcard],
	showModerationGuides: false,
	onClose: mockFunc,
	flashcardsActions: flashcardsActions
};

export const FlashcardsModerationGuidesShownStory = Template.bind({});
FlashcardsModerationGuidesShownStory.storyName = "ModerateFlashcards state. Guides shown";
FlashcardsModerationGuidesShownStory.args = {
	courseId: 'basicprogramming',
	unitId: flashcards[0].unitId,
	unitTitle: flashcards[0].unitTitle,
	userId: 'userId',
	isModerator: false,
	initialState: FlashcardsState.ModerateFlashcards,
	courseFlashcards: flashcards,
	moderationFlashcards: [{ ...userGeneratedFlashcard, moderationStatus: FlashcardModerationStatus.New }],
	showModerationGuides: true,
	onClose: mockFunc,
	flashcardsActions: flashcardsActions
};

export const FlashcardsModerationGuidesHiddenStory = Template.bind({});
FlashcardsModerationGuidesHiddenStory.storyName = "ModerateFlashcards state. Guides hidden";
FlashcardsModerationGuidesHiddenStory.args = {
	courseId: 'basicprogramming',
	unitId: flashcards[0].unitId,
	unitTitle: flashcards[0].unitTitle,
	userId: 'userId',
	isModerator: false,
	initialState: FlashcardsState.ModerateFlashcards,
	courseFlashcards: flashcards,
	moderationFlashcards: [userGeneratedFlashcard],
	showModerationGuides: false,
	onClose: mockFunc,
	flashcardsActions: flashcardsActions
};
