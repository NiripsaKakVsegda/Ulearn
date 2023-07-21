import React, { FC } from 'react';
import styles from './flashcardChoose.less';
import texts from './FlashcardChoose.texts';
import { Button } from "ui";
import { FlashcardsState } from "../../Flashcards/Flashcards.types";

interface Props {
	currentState: FlashcardsState;
	onChooseNextState: (newState: FlashcardsState) => void;
}

interface ReplyButton {
	title: string;
	state: FlashcardsState;
	isPrimary?: boolean;
}

const FlashcardChoose: FC<Props> = ({ currentState, onChooseNextState }) => {

	const getMessage = () => {
		switch (currentState) {
			case FlashcardsState.CreateCardBeforeUnit:
				return texts.statesInfo.saveCardBeforeUnit;
			case FlashcardsState.UnitRepeating:
				return texts.statesInfo.unitRepeating;
			case FlashcardsState.CreateCardAfterUnit:
				return texts.statesInfo.createCardAfterUnit;
			case FlashcardsState.ModerateFlashcards:
				return texts.statesInfo.moderateCards;
			case FlashcardsState.NoFlashcards:
				return texts.statesInfo.noFlashcards;
		}
	};

	const getButtons = (): ReplyButton[] => {
		switch (currentState) {
			case FlashcardsState.CreateCardBeforeUnit:
				return [
					{ title: texts.buttons.createCard, state: FlashcardsState.CreateCardBeforeUnit, isPrimary: true },
					{ title: texts.buttons.startUnitCheck, state: FlashcardsState.Unit },
				];
			case FlashcardsState.UnitRepeating:
				return [
					{ title: texts.buttons.createCard, state: FlashcardsState.CreateCardAfterUnit, isPrimary: true },
					{ title: texts.buttons.repeatCourse, state: FlashcardsState.CourseRepeating },
				];
			case FlashcardsState.CreateCardAfterUnit:
				return [
					{ title: texts.buttons.createCard, state: FlashcardsState.CreateCardAfterUnit, isPrimary: true },
					{ title: texts.buttons.repeatCourse, state: FlashcardsState.CourseRepeating },
				];
			case FlashcardsState.ModerateFlashcards:
				return [
					{ title: texts.buttons.startUnitCheck, state: FlashcardsState.Unit },
				];
			case FlashcardsState.NoFlashcards:
				return [
					{ title: texts.buttons.createCard, state: FlashcardsState.CreateCardBeforeUnit, isPrimary: true },
				];
			default:
				return [];
		}
	};

	return (
		<div className={ styles.wrapper }>
			<span className={ styles.message }>{ getMessage() }</span>
			<div className={ styles.buttonsWrapper }>
				{ getButtons().map((button, i) =>
					<Button
						key={ i }
						size={ "large" }
						onClick={ () => onChooseNextState(button.state) }
						use={ button.isPrimary ? 'primary' : "default" }
					>
						{ button.title }
					</Button>
				) }
			</div>
		</div>
	);
};

export default FlashcardChoose;
