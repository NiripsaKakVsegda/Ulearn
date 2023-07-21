import React, { FC, useEffect, useRef, useState } from 'react';
import translateCode from "../../../../codeTranslator/translateCode";
import { settingsForFlashcards } from "../../../../codeTranslator/codemirror";
import styles from './shortQuestions.less';
import { Toggle } from "ui";
import { QuestionWithAnswer } from "../../../../models/flashcards";
import Markdown from "../../../common/Markdown/Markdown";
import { EditorConfiguration } from "codemirror";

interface Props {
	questionsWithAnswers: QuestionWithAnswer[];
}

interface QuestionWithAnswerState extends QuestionWithAnswer {
	isAnswerShown: boolean;
}

const ShortQuestions: FC<Props> = ({ questionsWithAnswers }) => {
	const [allAnswersShown, setAllAnswersShown] = useState(false);

	const [questionsWithAnswersState, setQuestionsWithAnswersState] =
		useState<QuestionWithAnswerState[]>([]);

	const list = useRef<HTMLOListElement>(null);

	useEffect(() => {
		if(questionsWithAnswers.length !== questionsWithAnswersState.length) {
			setAllAnswersShown(false);
			setQuestionsWithAnswersState(questionsWithAnswers.map(qa => ({ ...qa, isAnswerShown: false })));
		}
	}, [questionsWithAnswers]);

	useEffect(() => {
		if(list.current) {
			translateCode(list.current, { 'codeMirror': settingsForFlashcards });
		}
	}, [questionsWithAnswersState, allAnswersShown]);

	const renderQuestions = () =>
		<ol ref={ list } className={ styles.questionsTextContainer }>
			{ questionsWithAnswersState
				.map(({ question, answer, isRendered, isAnswerShown }, index) =>
					<li className={ styles.listElement }
						key={ index }
						data-index={ index }
						onClick={ toggleAnswerShown }>
						{ isRendered
							? <div dangerouslySetInnerHTML={ { __html: question } }/>
							: <Markdown
								codeRenderOptions={ {
									disableStyles: settingsForFlashcards.settings.disableStyles,
									editorConfig: settingsForFlashcards.config as EditorConfiguration
								} }
							>
								{ question }
							</Markdown>
						}
						{ (allAnswersShown || isAnswerShown) && (isRendered
								? <div className={ styles.answerText } dangerouslySetInnerHTML={ { __html: answer } }/>
								: <Markdown
									className={ styles.answerText }
									codeRenderOptions={ {
										disableStyles: settingsForFlashcards.settings.disableStyles,
										editorConfig: settingsForFlashcards.config as EditorConfiguration
									} }
								>
									{ answer }
								</Markdown>
						) }
					</li>
				)
			}
		</ol>;

	return (
		<div className={ styles.questionsContainer }>
			{ renderQuestions() }
			<div className={ styles.toggleContainer }>
				<Toggle onChange={ toggleAllAnswersShown } checked={ allAnswersShown }/>
				<span className={ styles.toggleText }>
					Показать ответы
				</span>
			</div>
		</div>
	);

	function toggleAnswerShown(e: React.MouseEvent) {
		const index = parseInt((e.currentTarget as HTMLElement).dataset.index ?? '-1');
		if(isNaN(index) || index < 0) {
			return;
		}
		const updatedState = questionsWithAnswersState
			.map((qa, i) => i === index
				? { ...qa, isAnswerShown: !qa.isAnswerShown }
				: qa
			);
		setQuestionsWithAnswersState(updatedState);
	}

	function toggleAllAnswersShown() {
		setAllAnswersShown(!allAnswersShown);
	}
};

export default ShortQuestions;
