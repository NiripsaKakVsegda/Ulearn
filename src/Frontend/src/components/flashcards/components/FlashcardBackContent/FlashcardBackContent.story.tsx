import { ComponentMeta, ComponentStory } from "@storybook/react";
import FlashcardBackContent from "./FlashcardBackContent";
import { userGeneratedFlashcard } from "../../storyData";
import styles from '../../storyData/story.styles.less';

export default {
	title: "cards/components/FlashcardBackContent",
	component: FlashcardBackContent
} as ComponentMeta<typeof FlashcardBackContent>;
const Template: ComponentStory<typeof FlashcardBackContent> = (args) => (
	<FlashcardBackContent { ...args } />
);
export const FlashcardBackContentNotRenderedStory = Template.bind({});
FlashcardBackContentNotRenderedStory.storyName = "Not rendered";
FlashcardBackContentNotRenderedStory.args = {
	question: userGeneratedFlashcard.question,
	answer: userGeneratedFlashcard.answer,
	isRendered: false,
	className: styles.flashcardBackContentWrapper
};

export const FlashcardBackContentRenderedStory = Template.bind({});
FlashcardBackContentRenderedStory.storyName = "Rendered";
FlashcardBackContentRenderedStory.args = {
	question: "<p>Как преобразовать строковое представление числа в <code>double</code>?</p>",
	answer: "<p><code>double.Parse</code> или\r\n<code>Convert.ToDouble</code> или\r\n<code>double.TryParse</code></p>",
	isRendered: true,
	className: styles.flashcardBackContentWrapper
};
