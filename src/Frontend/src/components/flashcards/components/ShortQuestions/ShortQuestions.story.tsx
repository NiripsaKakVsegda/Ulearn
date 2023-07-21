import React from "react";
import ShortQuestions from "./ShortQuestions";
import { flashcards, userGeneratedFlashcard } from "../../storyData";
import { ComponentMeta, ComponentStory } from "@storybook/react";

export default {
	title: "Cards/components/ShortQuestions",
	component: ShortQuestions
} as ComponentMeta<typeof ShortQuestions>;
const Template: ComponentStory<typeof ShortQuestions> = (args) => (
	<ShortQuestions { ...args } />
);
export const ShortQuestionsStory = Template.bind({});
ShortQuestionsStory.storyName = "Default";
ShortQuestionsStory.args = {
	questionsWithAnswers: [
		...(flashcards[0].flashcards
				.map(f => ({
					question: f.question,
					answer: f.answer,
					isRendered: true
				}))
				.slice(0, 3)
		),
		{
			question: userGeneratedFlashcard.question,
			answer: userGeneratedFlashcard.answer,
			isRendered: false
		}
	]
};
