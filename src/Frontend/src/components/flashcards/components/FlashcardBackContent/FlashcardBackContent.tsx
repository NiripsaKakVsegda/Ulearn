import React, { FC } from 'react';
import styles from './flashcardBackContent.less';
import cn from "classnames";
import { settingsForFlashcards } from "../../../../codeTranslator/codemirror";
import { EditorConfiguration } from "codemirror";
import Markdown from "../../../common/Markdown/Markdown";

interface Props {
	question: string;
	answer: string;
	theorySlides?: React.ReactNode;
	isRendered?: boolean;
	className?: string;
}

const FlashcardBackContent: FC<Props> = ({ question, answer, isRendered, theorySlides, className }) => {
	return (
		<div className={ cn(styles.wrapper, className) }>
			{ isRendered
				? <>
					<div className={ styles.question } dangerouslySetInnerHTML={ { __html: question } }/>
					<div className={ styles.answer } dangerouslySetInnerHTML={ { __html: answer } }/>
				</>
				: <>
					<Markdown
						className={ styles.question }
						codeRenderOptions={ {
							disableStyles: settingsForFlashcards.settings.disableStyles,
							editorConfig: settingsForFlashcards.config as EditorConfiguration
						} }
					>
						{ question }
					</Markdown>
					<Markdown
						className={ styles.answer }
						codeRenderOptions={ {
							disableStyles: settingsForFlashcards.settings.disableStyles,
							editorConfig: settingsForFlashcards.config as EditorConfiguration
						} }
					>
						{ answer }
					</Markdown>
					{ theorySlides }
				</>
			}
		</div>
	);
};

export default FlashcardBackContent;
