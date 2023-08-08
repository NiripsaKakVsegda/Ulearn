import React, { FC, useState } from 'react';
import { Button, Hint, Textarea } from "ui";
import MarkdownDocLink from "../../../common/Markdown/MarkdownDocLink";
import FlashcardQuestionAnswer from "../../components/FlashcardQuestionAnswer/FlashcardQuestionAnswer";
import styles from './flashcardEditor.less';
import texts from './FlashcardEditor.texts';

interface Props {
	questionInitial?: string;
	answerInitial?: string;
	onUpdateFlashcard?: (question: string, answer: string) => void;
	initialShouldBeDifferent?: boolean;
	publishing?: boolean;
	canCreateApproved?: boolean;
	onSave: (question: string, answer: string, approved?: boolean) => void;
	onCancel: () => void;
}

const FlashcardEditor: FC<Props> = (props) => {
	const { questionInitial, answerInitial } = props;
	const [isPreviewMode, setIsPreviewMode] = useState(false);

	const [question, setQuestion] = useState(questionInitial ?? '');
	const [answer, setAnswer] = useState(answerInitial ?? '');

	const canSave = question.length > 0 && answer.length > 0 &&
		(!props.initialShouldBeDifferent || question !== questionInitial || answer !== answerInitial);

	return (
		<div className={ styles.wrapper }>
			{ isPreviewMode
				? <FlashcardQuestionAnswer
					className={ styles.previewWrapper }
					question={ question }
					answer={ answer }
				/>
				: <div className={ styles.editWrapper }>
					<Textarea
						className={ styles.questionInput }
						value={ question }
						placeholder={ texts.questionPlaceholder }
						onValueChange={ updateQuestion }
						resize={ 'none' }
						maxLength={ 2000 }
						autoFocus
					/>
					<Textarea
						className={ styles.answerInput }
						value={ answer }
						placeholder={ texts.answerPlaceholder }
						onValueChange={ updateAnswer }
						resize={ 'none' }
						maxLength={ 2000 }
					/>
					<MarkdownDocLink className={ styles.markdownDocLink }/>
				</div>
			}

			{ !isPreviewMode
				? <div className={ styles.buttonsWrapper }>
					<Button
						disabled={ !canSave }
						use={ "primary" }
						size={ "large" }
						onClick={ togglePreviewMode }
						children={ texts.previewButton }
					/>
					<Button
						size={ "large" }
						onClick={ props.onCancel }
						children={ texts.cancelButton }
					/>
				</div>
				: <div className={ styles.buttonsWrapper }>
					{ props.canCreateApproved &&
						<Hint
							text={ texts.publishForAllHint }
							pos={ 'top' }
						>
							<Button
								disabled={ !canSave }
								use={ "primary" }
								size={ "large" }
								onClick={ publishApprovedFlashcard }
								children={ texts.publishForAllButton }
							/>
						</Hint>
					}
					<Button
						disabled={ !canSave }
						use={ props.canCreateApproved ? "default" : "primary" }
						size={ "large" }
						onClick={ saveFlashcard }
						children={ props.publishing ? texts.publishButton : texts.saveButton }
					/>
					<Button
						size={ "large" }
						onClick={ togglePreviewMode }
						children={ texts.backButton }
					/>
				</div>
			}
		</div>
	);

	function updateQuestion(value: string) {
		setQuestion(value);
		props.onUpdateFlashcard?.(value, answer);
	}

	function updateAnswer(value: string) {
		setAnswer(value);
		props.onUpdateFlashcard?.(question, value);
	}

	function saveFlashcard() {
		props.onSave(question, answer);
	}

	function publishApprovedFlashcard() {
		props.onSave(question, answer, true);
	}

	function togglePreviewMode() {
		setIsPreviewMode(!isPreviewMode);
	}
};

export default FlashcardEditor;
