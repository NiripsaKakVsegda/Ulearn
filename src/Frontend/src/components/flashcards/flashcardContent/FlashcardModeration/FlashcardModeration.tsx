import React, { FC } from 'react';
import { FlashcardModerationStatus } from "../../../../models/flashcards";
import FlashcardQuestionAnswer from "../../components/FlashcardQuestionAnswer/FlashcardQuestionAnswer";
import { Button } from "ui";
import styles from './flashcardModeration.less';
import texts from './FlashcardModeration.texts';

interface Props {
	question: string;
	answer: string;
	status: FlashcardModerationStatus;
	onApproveFlashcard: () => void;
	onDeclineFlashcard: () => void;
	onSkipFlashcard: () => void;
}

const FlashcardModeration: FC<Props> = ({ question, answer, status, ...actions }) => {
	const renderReplyButtons = (): React.ReactNode => {
		return <div className={ styles.buttonsWrapper }>
			{ status !== FlashcardModerationStatus.Approved &&
				<Button
					use={ 'primary' }
					size={ "large" }
					onClick={ actions.onApproveFlashcard }
				>
					{ texts.publishButton }
				</Button>
			}
			{ status !== FlashcardModerationStatus.Declined &&
				<Button
					size={ "large" }
					use={ 'danger' }
					children={ texts.declineButton }
					onClick={ actions.onDeclineFlashcard }
				/>
			}
			<Button
				size={ "large" }
				children={ texts.skipButton }
				onClick={ skipFlashcard }
			/>
		</div>;
	};

	return (
		<div className={ styles.wrapper }>
			<FlashcardQuestionAnswer
				className={ styles.flashcardContentWrapper }
				question={ question }
				answer={ answer }
			/>
			{ renderReplyButtons() }
		</div>
	);

	function skipFlashcard() {
		actions.onSkipFlashcard();
	}
};

export default FlashcardModeration;
