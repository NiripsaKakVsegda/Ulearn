import { ComponentMeta, ComponentStory } from "@storybook/react";
import Flashcard from "./Flashcard";
import { userGeneratedFlashcard } from "../../storyData";
import { mockFunc } from "../../../../utils/storyMock";
import styles from '../../storyData/story.styles.less';

export default {
	title: "cards/cardContent/Flashcard",
	component: Flashcard
} as ComponentMeta<typeof Flashcard>;

const Template: ComponentStory<typeof Flashcard> = (args) => (
	<div className={ styles.flashcardWrapper }>
		<Flashcard
			{ ...args }
		/>
	</div>
);

const args = {
	courseId: 'basicprogramming',
	theorySlides: [],
	onClose: mockFunc,
	onRateClick: mockFunc,
};

export const FlashcardRenderedStory = Template.bind({});
FlashcardRenderedStory.storyName = "Rendered";
FlashcardRenderedStory.args = {
	question: userGeneratedFlashcard.question,
	answer: userGeneratedFlashcard.answer,
	rendered: false,
	...args
};

export const FlashcardNotRenderedStory = Template.bind({});
FlashcardNotRenderedStory.storyName = "Not rendered";
FlashcardNotRenderedStory.args = {
	question: "<p>Как преобразовать строковое представление числа в <code>double</code>?</p>",
	answer: "<p><code>double.Parse</code> или\r\n<code>Convert.ToDouble</code> или\r\n<code>double.TryParse</code></p>",
	rendered: true,
	...args
};
