import { ComponentMeta, ComponentStory } from "@storybook/react";
import FlashcardGuide from "./FlashcardGuide";
import { mockFunc } from "../../../../utils/storyMock";
import styles from "../../storyData/story.styles.less";

export default {
	title: "cards/cardContent/FlashcardGuide",
	component: FlashcardGuide
} as ComponentMeta<typeof FlashcardGuide>;

const Template: ComponentStory<typeof FlashcardGuide> = (args) => (
	<div className={ styles.flashcardWrapper }>
		<FlashcardGuide { ...args } />
	</div>
);

export const FlashcardGuideStory = Template.bind({});
FlashcardGuideStory.storyName = "Default";
FlashcardGuideStory.args = {
	onRateClick: mockFunc
};
