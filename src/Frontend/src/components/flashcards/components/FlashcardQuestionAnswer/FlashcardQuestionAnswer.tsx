import React, { FC } from 'react';
import styles from './flashcardQuestionAnswer.less';
import cn from "classnames";
import { settingsForFlashcards } from "../../../../codeTranslator/codemirror";
import { EditorConfiguration } from "codemirror";
import Markdown from "../../../common/Markdown/Markdown";
import { ScrollContainer } from "ui";

interface Props {
	question: string;
	answer?: string;
	theorySlides?: React.ReactNode;
	rendered?: boolean;
	className?: string;
}

const FlashcardQuestionAnswer: FC<Props> = ({ question, answer, rendered, theorySlides, className }) => {
	const questionClassName = answer ? styles.question : styles.questionSingle;

	return (
		<ScrollContainer className={ cn(styles.scrollContainer, className) }>
			<div className={styles.contentWrapper }>
				{ rendered
					? <>
						<div className={ questionClassName } dangerouslySetInnerHTML={ { __html: question } }/>
						{ answer &&
							<div className={ styles.answer } dangerouslySetInnerHTML={ { __html: answer } }/>
						}
					</>
					: <>
						<Markdown
							className={ questionClassName }
							codeRenderOptions={ {
								disableStyles: settingsForFlashcards.settings.disableStyles,
								editorConfig: settingsForFlashcards.config as EditorConfiguration
							} }
							children={ question }
						/>
						{ answer &&
							<Markdown
								className={ styles.answer }
								codeRenderOptions={ {
									disableStyles: settingsForFlashcards.settings.disableStyles,
									editorConfig: settingsForFlashcards.config as EditorConfiguration
								} }
								children={ answer }
							/>
						}
					</>
				}
				{ theorySlides }
			</div>
		</ScrollContainer>
	);
};

export default FlashcardQuestionAnswer;
