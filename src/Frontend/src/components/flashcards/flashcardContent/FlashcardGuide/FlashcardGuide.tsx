import React, { FC } from 'react';
import Flashcard from "../Flashcard/Flashcard";
import texts from './FlashcardGuide.texts';

interface Props {
	onRateClick: () => void;
	onClose: () => void;
}

const FlashcardGuide: FC<Props> = ({ onRateClick, onClose }) => {
	return <Flashcard
		rendered={ false }
		question={ texts.questionMarkdown }
		answer={ texts.answerMarkdown }
		onClose={ onClose }
		onRateClick={ rateClick }
	/>;

	function rateClick() {
		onRateClick();
	}
};

export default FlashcardGuide;
