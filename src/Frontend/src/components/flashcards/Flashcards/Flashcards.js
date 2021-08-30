import React, { Component } from 'react';
import PropTypes from "prop-types";

import ProgressBar from "../ProgressBar/ProgressBar";
import OpenedFlashcard from "./OpenedFlashcard/OpenedFlashcard";
import { Toast } from "ui";

import { sortFlashcardsInAuthorsOrderWithRate, getNextFlashcardRandomly } from "./flashcardsStirrer/flashcardsStirrer";
import countFlashcardsStatistics from "../countFlashcardsStatistics";
import classNames from 'classnames';

import { RateTypes } from "src/consts/rateTypes";

import styles from './flashcards.less';

const modalsClassNames = {
	first: classNames(styles.modal),
	second: classNames(styles.modal, styles.secondModal),
	third: classNames(styles.modal, styles.thirdModal),
	fourth: classNames(styles.modal, styles.fourthModal)
};

const mapRateToRateType = {
	1: RateTypes.rate1,
	2: RateTypes.rate2,
	3: RateTypes.rate3,
	4: RateTypes.rate4,
	5: RateTypes.rate5,
};

class Flashcards extends Component {
	constructor(props) {
		super(props);
		const { flashcards, unitId, infoByUnits } = this.props;

		const onUnit = unitId !== undefined;
		let sessionFlashcards;
		const maxLastRateIndex = Flashcards.findMaxLastRateIndex(flashcards);

		if (onUnit) {
			const unitFlashcards = flashcards.filter(flashcard => flashcard.unitId === unitId);
			sessionFlashcards = sortFlashcardsInAuthorsOrderWithRate(unitFlashcards);
		} else {
			sessionFlashcards = Flashcards.getUnlockedCourseFlashcards(flashcards, infoByUnits);
		}

		this.state = {
			onUnit,
			onUnitRepeating: false,
			currentFlashcard: null,
			maxLastRateIndex,
			sessionFlashcards,
			totalFlashcardsCount: sessionFlashcards.length,
			statistics: countFlashcardsStatistics(sessionFlashcards),
		}
	}

	componentDidMount() {
		document.querySelector('body')
			.classList.add(styles.overflow);

		this.takeNextFlashcard(this.state.maxLastRateIndex);
	}

	componentWillUnmount() {
		document.querySelector('body')
			.classList.remove(styles.overflow);
	}

	render() {
		const { statistics, totalFlashcardsCount, currentFlashcard } = this.state;

		return (
			<div ref={ (ref) => this.overlay = ref } className={ styles.overlay } onClick={ this.handleOverlayClick }>

				<div className={ modalsClassNames.first } ref={ (ref) => this.firstModal = ref }>
					{ currentFlashcard && this.renderOpenedFlashcard() }
				</div>

				<div className={ modalsClassNames.second } ref={ (ref) => this.secondModal = ref }/>
				<div className={ modalsClassNames.third } ref={ (ref) => this.thirdModal = ref }/>
				<div className={ modalsClassNames.fourth }/>

				{ Flashcards.renderControlGuides() }

				<div className={ styles.progressBarContainer }>
					<ProgressBar statistics={ statistics } totalFlashcardsCount={ totalFlashcardsCount }/>
				</div>
			</div>
		)
	}

	handleOverlayClick = (e) => {
		const { onClose } = this.props;

		if (e.target === this.overlay) {
			onClose();
		}
	};

	renderOpenedFlashcard() {
		const { currentFlashcard } = this.state;
		const { onClose } = this.props;

		return (
			<OpenedFlashcard
				onShowAnswer={ this.resetCardsAnimation }
				question={ currentFlashcard.question }
				answer={ currentFlashcard.answer }
				unitTitle={ currentFlashcard.unitTitle }
				theorySlides={ currentFlashcard.theorySlides }
				onHandlingResultsClick={ this.handleResultsClick }
				onClose={ onClose }
			/>
		);
	}

	static renderControlGuides() {
		return (
			<p className={ styles.controlGuideContainer }>
				Используйте клавиатуру:
				<span>пробел</span> — показать ответ,
				<span>1</span>
				<span>2</span>
				<span>3</span>
				<span>4</span>
				<span>5</span> — поставить оценку
			</p>
		);
	}

	handleResultsClick = (rate) => {
		const { sessionFlashcards, currentFlashcard, maxLastRateIndex, statistics } = this.state;
		const { courseId, sendFlashcardRate } = this.props;

		const newStatistics = { ...statistics };
		const newRate = mapRateToRateType[rate];
		const newLastRateIndex = maxLastRateIndex + 1;
		const indexOfCard = sessionFlashcards.indexOf(currentFlashcard);
		const newSessionFlashcards = sessionFlashcards.map(flashcard => ({ ...flashcard }));

		sendFlashcardRate(courseId, currentFlashcard.unitId, currentFlashcard.id, newRate, newLastRateIndex);

		if (indexOfCard > -1) {
			const flashcard = newSessionFlashcards[indexOfCard];
			flashcard.rate = newRate;
			flashcard.lastRateIndex = newLastRateIndex;
		}

		newStatistics[currentFlashcard.rate]--;
		newStatistics[newRate]++;

		this.setState({
			maxLastRateIndex: newLastRateIndex,
			statistics: newStatistics,
			sessionFlashcards: newSessionFlashcards,
		}, this.takeNextFlashcard);
	};

	takeNextFlashcard() {
		const { sessionFlashcards, onUnit, onUnitRepeating } = this.state;

		if (onUnit) {
			if (sessionFlashcards.length === 0) {
				if (onUnitRepeating) {
					this.startCourseRepeating();
				} else {
					this.startUnitRepeating();
				}
			} else {
				this.getNextCardInUnit();
			}
		} else {
			this.getNextCardInCourse();
		}

		this.animateCards();
	}

	startUnitRepeating() {
		const { flashcards, unitId } = this.props;

		const failedUnitFlashcards = sortFlashcardsInAuthorsOrderWithRate(Flashcards.getUnitFlashcards(flashcards, unitId, true));

		if (failedUnitFlashcards.length > 0) {
			const firstFlashcard = failedUnitFlashcards.shift();

			this.setState({
				onUnitRepeating: true,
				currentFlashcard: firstFlashcard,
				sessionFlashcards: failedUnitFlashcards,
			})
		} else {
			this.startCourseRepeating();
		}
	}

	static getUnitFlashcards(courseFlashcards, unitId, onlyFailedFlashcards = false) {
		const isUnitFlashcard = (flashcard) => {
			return flashcard.unitId === unitId
		};
		const isFailedUnitFlashcard = (flashcard) => {
			return flashcard.unitId === unitId
				&& (flashcard.rate === RateTypes.rate1 || flashcard.rate === RateTypes.rate2)
		};

		const filter = onlyFailedFlashcards
			? isFailedUnitFlashcard
			: isUnitFlashcard;

		return courseFlashcards.filter(filter);
	}

	startCourseRepeating() {
		const { flashcards, infoByUnits } = this.props;

		const unlockedCourseFlashcards = Flashcards.getUnlockedCourseFlashcards(flashcards, infoByUnits);
		const maxLastRateIndex = Flashcards.findMaxLastRateIndex(unlockedCourseFlashcards);

		Toast.push('Переход к повторению по курсу');

		this.setState({
			onUnit: false,
			onUnitRepeating: false,
			sessionFlashcards: unlockedCourseFlashcards,
			totalFlashcardsCount: unlockedCourseFlashcards.length,
			maxLastRateIndex,
			statistics: countFlashcardsStatistics(unlockedCourseFlashcards),
			currentFlashcard: getNextFlashcardRandomly(unlockedCourseFlashcards, maxLastRateIndex),
		});
	}

	static getUnlockedCourseFlashcards(flashcards, infoByUnits) {
		const unlocksByUnits = infoByUnits.reduce(
			(unlocks, { unitId, unlocked }) => ({ ...unlocks, [unitId]: unlocked }), {});

		return flashcards
			.filter(({ rate, unitId }) => rate !== RateTypes.notRated && unlocksByUnits[unitId])
			.map(flashcard => ({ ...flashcard }));
	}

	static findMaxLastRateIndex(flashcards) {
		let maxLastRateIndex = -1;

		for (const { lastRateIndex } of flashcards) {
			if (maxLastRateIndex < lastRateIndex) {
				maxLastRateIndex = lastRateIndex;
			}
		}

		return maxLastRateIndex;
	}

	getNextCardInUnit() {
		const { sessionFlashcards } = this.state;

		const newSessionFlashcards = sessionFlashcards.map(fc => ({ ...fc }));
		const newCurrentFlashcard = newSessionFlashcards.shift();

		this.setState({
			currentFlashcard: newCurrentFlashcard,
			sessionFlashcards: newSessionFlashcards,
		})
	}

	getNextCardInCourse() {
		const { sessionFlashcards, maxLastRateIndex } = this.state;

		this.setState({
			currentFlashcard: getNextFlashcardRandomly(sessionFlashcards, maxLastRateIndex),
		})
	}

	animateCards() {
		const { firstModal, secondModal, thirdModal } = this;
		const animationDuration = 700;

		firstModal.className = classNames(modalsClassNames.second, styles.move);
		secondModal.className = classNames(modalsClassNames.third, styles.move);
		thirdModal.className = classNames(modalsClassNames.fourth, styles.move);

		setTimeout(() => {
			this.resetCardsAnimation();
		}, animationDuration - animationDuration / 10);
	}

	resetCardsAnimation = () => {
		const { firstModal, secondModal, thirdModal } = this;

		if (!firstModal) {
			return;
		}

		firstModal.className = modalsClassNames.first;
		secondModal.className = modalsClassNames.second;
		thirdModal.className = modalsClassNames.third;
	}
}


Flashcards.propTypes = {
	courseId: PropTypes.string,
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
		unlocked: PropTypes.bool,
		unitId: PropTypes.string,
	})),

	onClose: PropTypes.func,
	sendFlashcardRate: PropTypes.func,
};

export default Flashcards;
