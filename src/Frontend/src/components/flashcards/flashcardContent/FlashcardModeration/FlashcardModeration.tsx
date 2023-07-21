import React, { FC } from 'react';
import { FlashcardModerationStatus } from "../../../../models/flashcards";
import FlashcardBackContent from "../../components/FlashcardBackContent/FlashcardBackContent";
import { Button } from "ui";
import { EyeClosed, EyeOpened, Skip } from "icons";
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
					icon={ <EyeOpened/> }
					use={ 'primary' }
					size={ "large" }
					onClick={ actions.onApproveFlashcard }
				>
					{ texts.publishButton }
				</Button>
			}
			{ status !== FlashcardModerationStatus.Declined &&
				<Button
					icon={ <EyeClosed/> }
					size={ "large" }
					use={ 'danger' }
					children={ texts.declineButton }
					onClick={ actions.onDeclineFlashcard }
				/>
			}
			<Button
				icon={ <Skip/> }
				size={ "large" }
				children={ texts.skipButton }
				onClick={ skipFlashcard }
			/>
		</div>;
	};

	return (
		<div className={ styles.wrapper }>
			<FlashcardBackContent
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
