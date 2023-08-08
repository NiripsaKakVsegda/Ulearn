import React, { FC, useEffect, useRef, useState } from 'react';
import { TheorySlideInfo } from "../../../../models/flashcards";
import { RateTypes } from "../../../../consts/rateTypes";
import { Link } from "react-router-dom";
import { Button } from "ui";
import Results from "../../components/Results/Results";
import translateCode from "../../../../codeTranslator/translateCode";
import { settingsForFlashcards } from "../../../../codeTranslator/codemirror";
import FlashcardQuestionAnswer from "../../components/FlashcardQuestionAnswer/FlashcardQuestionAnswer";
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

	const renderBackControl = () => <Results onResultClick={ handleResultsClick }/>;

	const renderFrontControl = () =>
		<div className={ styles.showAnswerButtonContainer }>
			<Button size="large" use="primary" onClick={ showAnswer }>
				{ texts.showAnswerButton }
			</Button>
		</div>;


	return <div className={ styles.wrapper } ref={ modal }>
		<FlashcardQuestionAnswer
			className={ styles.questionAnswerWrapper }
			question={ question }
			answer={ isAnswerShown ? answer : undefined }
			rendered={ rendered }
			theorySlides={ renderLinksToTheorySlides(theorySlides) }
		/>
		{ isAnswerShown
			? renderBackControl()
			: renderFrontControl()
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
