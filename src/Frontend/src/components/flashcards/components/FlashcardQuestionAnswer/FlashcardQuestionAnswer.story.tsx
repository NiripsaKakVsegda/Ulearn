import { ComponentMeta, ComponentStory } from "@storybook/react";
import FlashcardQuestionAnswer from "./FlashcardQuestionAnswer";
import { userGeneratedFlashcard } from "../../storyData";
import styles from '../../storyData/story.styles.less';

export default {
	title: "cards/components/FlashcardQuestionAnswer",
	component: FlashcardQuestionAnswer
} as ComponentMeta<typeof FlashcardQuestionAnswer>;
const Template: ComponentStory<typeof FlashcardQuestionAnswer> = (args) => (
	<FlashcardQuestionAnswer { ...args } />
);
export const FlashcardBackContentNotRenderedStory = Template.bind({});
FlashcardBackContentNotRenderedStory.storyName = "Not rendered";
FlashcardBackContentNotRenderedStory.args = {
	question: userGeneratedFlashcard.question,
	answer: userGeneratedFlashcard.answer,
	rendered: false,
	className: styles.flashcardBackContentWrapper
};

export const FlashcardBackContentRenderedStory = Template.bind({});
FlashcardBackContentRenderedStory.storyName = "Rendered";
FlashcardBackContentRenderedStory.args = {
	question: "<p>Как преобразовать строковое представление числа в <code>double</code>?</p>",
	answer: "<p><code>double.Parse</code> или\r\n<code>Convert.ToDouble</code> или\r\n<code>double.TryParse</code></p>",
	rendered: true,
	className: styles.flashcardBackContentWrapper
};
