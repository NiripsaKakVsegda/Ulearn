import { ComponentMeta, ComponentStory } from "@storybook/react";
import FlashcardEditor from "./FlashcardEditor";
import styles from "../../storyData/story.styles.less";
import { mockFunc } from "../../../../utils/storyMock";
import { userGeneratedFlashcard } from "../../storyData";

export default {
	title: "cards/cardContent/FlashcardEditor",
	component: FlashcardEditor
} as ComponentMeta<typeof FlashcardEditor>;

const Template: ComponentStory<typeof FlashcardEditor> = (args) => (
	<div className={ styles.flashcardWrapper }>
		<FlashcardEditor
			{ ...args }
		/>
	</div>
);

export const FlashcardEditorEditingStory = Template.bind({});
FlashcardEditorEditingStory.storyName = "Editing";
FlashcardEditorEditingStory.args = {
	questionInitial: userGeneratedFlashcard.question,
	answerInitial: userGeneratedFlashcard.answer,
	initialShouldBeDifferent: true,
	publishing: false,
	canCreateApproved: false,
	onSave: mockFunc,
	onCancel: mockFunc,
	onUpdateFlashcard: mockFunc,
};

export const FlashcardEditorApprovingStory = Template.bind({});
FlashcardEditorApprovingStory.storyName = "Approving";
FlashcardEditorApprovingStory.args = {
	questionInitial: userGeneratedFlashcard.question,
	answerInitial: userGeneratedFlashcard.answer,
	initialShouldBeDifferent: false,
	publishing: true,
	canCreateApproved: false,
	onSave: mockFunc,
	onCancel: mockFunc,
	onUpdateFlashcard: mockFunc,
};

export const FlashcardEditorCreatingForUserStory = Template.bind({});
FlashcardEditorCreatingForUserStory.storyName = "Creating for user";
FlashcardEditorCreatingForUserStory.args = {
	questionInitial: undefined,
	answerInitial: undefined,
	initialShouldBeDifferent: false,
	publishing: false,
	canCreateApproved: false,
	onSave: mockFunc,
	onCancel: mockFunc,
	onUpdateFlashcard: mockFunc,
};

export const FlashcardEditorCreatingForModeratorStory = Template.bind({});
FlashcardEditorCreatingForModeratorStory.storyName = "Creating for moderator";
FlashcardEditorCreatingForModeratorStory.args = {
	questionInitial: undefined,
	answerInitial: undefined,
	initialShouldBeDifferent: false,
	publishing: false,
	canCreateApproved: true,
	onSave: mockFunc,
	onCancel: mockFunc,
	onUpdateFlashcard: mockFunc,
};
