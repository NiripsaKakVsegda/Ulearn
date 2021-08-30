import React from "react";
import ShortQuestions from "./ShortQuestions.js";
import { questionsWithAnswers } from "src/components/flashcards/storyData";

export default {
	title: "Cards/UnitPage/ShortQuestions",
};

export const Default = (): React.ReactNode => (
	<ShortQuestions questionsWithAnswers={ questionsWithAnswers }/>
);

Default.storyName = "default";
