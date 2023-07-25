import React, { FC, useEffect, useRef, useState } from 'react';
import { TheorySlideInfo } from "../../../../models/flashcards";
import { RateTypes } from "../../../../consts/rateTypes";
import { Link } from "react-router-dom";
import { Button } from "ui";
import Results from "../../components/Results/Results";
import translateCode from "../../../../codeTranslator/translateCode";
import { settingsForFlashcards } from "../../../../codeTranslator/codemirror";
import Markdown from "../../../common/Markdown/Markdown";
import { EditorConfiguration } from "codemirror";
import FlashcardBackContent from "../../components/FlashcardBackContent/FlashcardBackContent";
import styles from "./flashcard.less";
import texts from './Flashcard.texts';
import { constructPathToSlide } from "../../../../consts/routes";

interface Props {
	courseId?: string;
	question: string,
	answer: string,
	rendered: boolean,
	theorySlides?: TheorySlideInfo[]
	onRateClick: (rate: RateTypes) => void;
}

const mapRateToRateType: { [rate: number]: RateTypes } = {
	1: RateTypes.rate1,
	2: RateTypes.rate2,
	3: RateTypes.rate3,
	4: RateTypes.rate4,
	5: RateTypes.rate5,
};

const Flashcard: FC<Props> = ({
	courseId,
	question,
	answer,
	rendered
	, theorySlides,
	...actions
}) => {
	const [isAnswerShown, setIsAnswerShown] = useState(false);
	const modal = useRef<HTMLDivElement>(null);

	useEffect(() => {
		document.addEventListener('keyup', handleKeyUp);
		return () => document.removeEventListener('keyup', handleKeyUp);
	}, [isAnswerShown]);

	useEffect(() => {
		if(modal.current && rendered) {
			translateCode(modal.current, { 'codeMirror': settingsForFlashcards });
		}
	}, [question, answer, rendered, isAnswerShown, modal]);

	const mapTheorySlidesToLinks = (theorySlides: TheorySlideInfo[], courseId: string) => {
		const links = [];

		for (let i = 0; i < theorySlides.length; i++) {
			const { slug, title } = theorySlides[i];

			links.push(
				<Link to={ constructPathToSlide(courseId, slug) } key={ slug }>
					{ texts.getTheorySlideName(title) }
				</Link>);

			if(i < theorySlides.length - 1) {
				links.push(', ');
			}
		}

		return links;
	};

	const renderLinksToTheorySlides = (theorySlides?: TheorySlideInfo[]) => {
		const slidesCount = theorySlides?.length;

		if(!slidesCount || !courseId) {
			return;
		}

		return (
			<p className={ styles.theoryLinks }>
				{ texts.getTheorySlidesPlural(slidesCount) }: { mapTheorySlidesToLinks(theorySlides, courseId) }
			</p>);
	};

	const renderBackContent = (linksToTheorySlides: React.ReactNode) =>
		<FlashcardBackContent
			question={ question }
			answer={ answer }
			isRendered={ rendered }
			className={ styles.backTextContainer }
			theorySlides={ linksToTheorySlides }
		/>;

	const renderFrontContent = (question: string) => {
		if(rendered) {
			return <div className={ styles.questionFront }
						dangerouslySetInnerHTML={ { __html: question } }
			/>;
		}
		return <Markdown
			className={ styles.questionFront }
			codeRenderOptions={ {
				disableStyles: settingsForFlashcards.settings.disableStyles,
				editorConfig: settingsForFlashcards.config as EditorConfiguration
			} }
		>
			{ question }
		</Markdown>;
	};

	const renderBackControl = () => <Results onResultClick={ handleResultsClick }/>;

	const renderFrontControl = () =>
		<div className={ styles.showAnswerButtonContainer }>
			<Button size="large" use="primary" onClick={ showAnswer }>
				{ texts.showAnswerButton }
			</Button>
		</div>;


	return <div className={ styles.wrapper } ref={ modal }>
		{ isAnswerShown
			? <>
				{ renderBackContent(renderLinksToTheorySlides(theorySlides)) }
				{ renderBackControl() }
			</>
			: <>
				{ renderFrontContent(question) }
				{ renderFrontControl() }
			</>
		}
	</div>;


	function handleKeyUp(e: KeyboardEvent) {
		const code = e.key;
		const spaceChar = ' ';
		const rateNum = parseInt(code);

		if(code === spaceChar) {
			showAnswer();
		} else if(!isNaN(rateNum) && rateNum >= 1 && rateNum <= 5 && isAnswerShown) {
			handleResultsClick(rateNum);
		}
	}

	function handleResultsClick(rate: number) {
		const { onRateClick } = actions;

		onRateClick(mapRateToRateType[rate]);
		setIsAnswerShown(false);
	}

	function showAnswer() {
		setIsAnswerShown(true);
	}
};

export default Flashcard;
