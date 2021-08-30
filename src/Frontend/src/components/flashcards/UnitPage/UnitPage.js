import React, { Component } from 'react';
import PropTypes from "prop-types";

import CourseLoader from "src/components/course/Course/CourseLoader/CourseLoader";
import UnitCard from "./UnitCard/UnitCard";
import Guides from "../Guides/Guides";
import ProgressBar from "../ProgressBar/ProgressBar";
import ShortQuestions from "./ShortQuestions/ShortQuestions";
import Flashcards from "../Flashcards/Flashcards";

import { guides as defaultGuides } from '../consts';
import { RateTypes } from "src/consts/rateTypes";

import countFlashcardsStatistics from "../countFlashcardsStatistics";

import styles from './unitPage.less';


class UnitPage extends Component {
	constructor(props) {
		super(props);
		const { flashcards, unitId } = this.props;

		const unitFlashcards = flashcards.filter(fc => fc.unitId === unitId);

		this.state = {
			unitFlashcards,
			showFlashcards: false,
			statistics: countFlashcardsStatistics(unitFlashcards),
			totalFlashcardsCount: unitFlashcards.length,
		}
	}

	static getDerivedStateFromProps(props, state) {
		const { flashcards, unitId } = props;
		const unitFlashcards = flashcards.filter(flashcard => flashcard.unitId === unitId);
		if(JSON.stringify(unitFlashcards) !== JSON.stringify(state.unitFlashcards)) {
			return {
				unitFlashcards,
				statistics: countFlashcardsStatistics(unitFlashcards),
				totalFlashcardsCount: unitFlashcards.length,
			};
		}
		return null;
	}

	componentDidMount() {
		const { courseId, flashcards, loadFlashcards } = this.props;

		if(flashcards.length === 0) {
			loadFlashcards(courseId);
		}
	}

	render() {
		const {
			courseId,
			unitTitle,
			flashcards,
			flashcardsLoading,
			sendFlashcardRate,
			unitId,
			infoByUnits
		} = this.props;
		const { statistics, totalFlashcardsCount, showFlashcards } = this.state;
		const haveProgress = flashcards && statistics[RateTypes.notRated] !== totalFlashcardsCount;
		const completedUnit = flashcards && statistics[RateTypes.notRated] === 0;
		const dataLoaded = flashcards && !flashcardsLoading;

		if(!dataLoaded) {
			return (<CourseLoader/>);
		}

		return (
			<React.Fragment>
				{ unitTitle &&
				<UnitCard
					haveProgress={ completedUnit }
					totalFlashcardsCount={ totalFlashcardsCount }
					unitTitle={ unitTitle }
					handleStartButton={ this.showFlashcards }
				/> }
				{ unitTitle && this.renderFooter(haveProgress && dataLoaded) }
				{ showFlashcards &&
				<Flashcards
					infoByUnits={ infoByUnits }
					unitId={ unitId }
					onClose={ this.hideFlashcards }
					flashcards={ flashcards }
					courseId={ courseId }
					sendFlashcardRate={ sendFlashcardRate }
				/> }
			</React.Fragment>
		);
	}

	renderFooter(shouldRenderProgress) {
		const { statistics, totalFlashcardsCount, unitFlashcards } = this.state;
		const { guides = defaultGuides, } = this.props;

		if(!shouldRenderProgress) {
			return (
				<div className={ styles.guidesContainer }>
					<Guides guides={ guides }/>
				</div>);
		}

		return (
			<footer className={ styles.footer }>
				<div className={ styles.progressBarContainer }>
					<p className={ styles.progressBarTitle }>
						Результаты последнего прохождения
					</p>
					<ProgressBar
						statistics={ statistics }
						totalFlashcardsCount={ totalFlashcardsCount }
					/>
				</div>
				<ShortQuestions
					questionsWithAnswers={ UnitPage.mapFlashcardsToQuestionWithAnswers(unitFlashcards) }
				/>
			</footer>
		);
	}

	showFlashcards = () => {
		this.setState({
			showFlashcards: true
		});
	};

	hideFlashcards = () => {
		this.setState({
			showFlashcards: false
		});
	};

	static mapFlashcardsToQuestionWithAnswers(unitFlashcards) {
		return unitFlashcards
			.filter(({ rate }) => rate !== RateTypes.notRated)
			.map(({ question, answer, }) => ({ question, answer, }));
	}
}

UnitPage.propTypes = {
	courseId: PropTypes.string,
	unitTitle: PropTypes.string,
	unitId: PropTypes.string,
	flashcards: PropTypes.arrayOf(PropTypes.shape({
		id: PropTypes.string,
		question: PropTypes.string,
		answer: PropTypes.string,
		unitTitle: PropTypes.string,
		rate: PropTypes.string,
		unitId: PropTypes.string,
		lastRateIndex: PropTypes.number,
		theorySlides: PropTypes.arrayOf(
			PropTypes.shape({
				slug: PropTypes.string,
				title: PropTypes.string,
			}),
		),
	})),
	infoByUnits: PropTypes.arrayOf(PropTypes.shape({
		unitTitle: PropTypes.string,
		unlocked: PropTypes.bool,
		cardsCount: PropTypes.number,
		unitId: PropTypes.string,
		flashcardsSlideSlug: PropTypes.string,
	})),
	guides: PropTypes.arrayOf(PropTypes.string),

	loadFlashcards: PropTypes.func,
	sendFlashcardRate: PropTypes.func,
};

export default UnitPage;
