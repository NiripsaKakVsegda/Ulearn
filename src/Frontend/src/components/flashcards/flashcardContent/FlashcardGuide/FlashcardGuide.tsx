import React, { FC } from 'react';
import Flashcard from "../Flashcard/Flashcard";
import texts from './FlashcardGuide.texts';

interface Props {
	onRateClick: () => void;
}

const FlashcardGuide: FC<Props> = ({ onRateClick }) => {
	return <Flashcard
		rendered={ false }
		question={ texts.questionMarkdown }
		answer={ texts.answerMarkdown }
		onRateClick={ rateClick }
	/>;

	function rateClick() {
		onRateClick();
	}
};

export default FlashcardGuide;
