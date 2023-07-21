import { ComponentMeta, ComponentStory } from "@storybook/react";
import FlashcardModeration from "./FlashcardModeration";
import styles from "../../storyData/story.styles.less";
import { mockFunc } from "../../../../utils/storyMock";
import { userGeneratedFlashcard } from "../../storyData";
import { FlashcardModerationStatus } from "../../../../models/flashcards";

export default {
	title: "cards/cardContent/FlashcardModeration",
	component: FlashcardModeration,
	argTypes: {
		status: {
			options: [FlashcardModerationStatus.New, FlashcardModerationStatus.Approved, FlashcardModerationStatus.Declined],

		}
	}
} as ComponentMeta<typeof FlashcardModeration>;
const Template: ComponentStory<typeof FlashcardModeration> = (args) => (
	<div className={ styles.flashcardWrapper }>
		<FlashcardModeration
			{ ...args }
		/>
	</div>
);

export const FlashcardEditorApprovedStory = Template.bind({});
FlashcardEditorApprovedStory.storyName = "Approved";
FlashcardEditorApprovedStory.args = {
	question: userGeneratedFlashcard.question,
	answer: userGeneratedFlashcard.answer,
	status: FlashcardModerationStatus.Approved,
	onApproveFlashcard: mockFunc,
	onDeclineFlashcard: mockFunc,
	onSkipFlashcard: mockFunc
};

export const FlashcardEditorNewStory = Template.bind({});
FlashcardEditorNewStory.storyName = "New";
FlashcardEditorNewStory.args = {
	question: userGeneratedFlashcard.question,
	answer: userGeneratedFlashcard.answer,
	status: FlashcardModerationStatus.New,
	onApproveFlashcard: mockFunc,
	onDeclineFlashcard: mockFunc,
	onSkipFlashcard: mockFunc
};

export const FlashcardEditorDeclinedStory = Template.bind({});
FlashcardEditorDeclinedStory.storyName = "Declined";
FlashcardEditorDeclinedStory.args = {
	question: userGeneratedFlashcard.question,
	answer: userGeneratedFlashcard.answer,
	status: FlashcardModerationStatus.Declined,
	onApproveFlashcard: mockFunc,
	onDeclineFlashcard: mockFunc,
	onSkipFlashcard: mockFunc
};
