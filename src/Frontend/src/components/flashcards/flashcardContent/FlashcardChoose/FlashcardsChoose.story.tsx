import { ComponentMeta, ComponentStory } from "@storybook/react";
import FlashcardChoose from "./FlashcardChoose";
import { mockFunc } from "../../../../utils/storyMock";
import styles from "../../storyData/story.styles.less";
import { FlashcardsState } from "../../Flashcards/Flashcards.types";

export default {
	title: "cards/cardContent/FlashcardChoose",
	component: FlashcardChoose,
	argTypes: {
		currentState: {
			options: [
				FlashcardsState.CreateCardBeforeUnit,
				FlashcardsState.CreateCardAfterUnit,
				FlashcardsState.UnitRepeating,
				FlashcardsState.ModerateFlashcards
			],
			control: { type: 'radio' }
		}
	}
} as ComponentMeta<typeof FlashcardChoose>;

const Template: ComponentStory<typeof FlashcardChoose> = (args) => (
	<div className={ styles.flashcardWrapper }>
		<FlashcardChoose { ...args } />
	</div>
);

export const FlashcardChooseCreateCardBeforeUnitStory = Template.bind({});
FlashcardChooseCreateCardBeforeUnitStory.storyName = "CreateCardBeforeUnit state";
FlashcardChooseCreateCardBeforeUnitStory.args = {
	currentState: FlashcardsState.CreateCardBeforeUnit,
	onChooseNextState: mockFunc
};

export const FlashcardChooseUnitRepeatingStory = Template.bind({});
FlashcardChooseUnitRepeatingStory.storyName = "UnitRepeating state";
FlashcardChooseUnitRepeatingStory.args = {
	currentState: FlashcardsState.UnitRepeating,
	onChooseNextState: mockFunc
};

export const FlashcardChooseCreateCardAfterUnitStory = Template.bind({});
FlashcardChooseCreateCardAfterUnitStory.storyName = "CreateCardAfterUnit state";
FlashcardChooseCreateCardAfterUnitStory.args = {
	currentState: FlashcardsState.CreateCardAfterUnit,
	onChooseNextState: mockFunc
};

export const FlashcardChooseModerateFlashcardsStory = Template.bind({});
FlashcardChooseModerateFlashcardsStory.storyName = "ModerateFlashcards state";
FlashcardChooseModerateFlashcardsStory.args = {
	currentState: FlashcardsState.ModerateFlashcards,
	onChooseNextState: mockFunc
};
