import React, { Component } from 'react';
import PropTypes from "prop-types";

import CourseCards from "./CourseCards/CourseCards";
import Guides from "../Guides/Guides";
import { Button, } from "ui";
import Flashcards from "../Flashcards/Flashcards";

import styles from './courseFlashcardsPage.less';
import { guides as defaultGuides } from '../consts';
import CourseLoader from "src/components/course/Course/CourseLoader/CourseLoader";

class CourseFlashcardsPage extends Component {
	constructor(props) {
		super(props);

		this.state = {
			showFlashcards: false,
		}
	}

	componentDidMount() {
		const { courseId, flashcards, loadFlashcards } = this.props;

		if(flashcards.length === 0) {
			loadFlashcards(courseId);
		}
	}

	render() {
		const { showFlashcards } = this.state;
		const { courseId, flashcardsLoading, flashcards, infoByUnits, sendFlashcardRate, guides = defaultGuides } = this.props;

		if(!flashcards || flashcardsLoading) {
			return (<CourseLoader/>);
		}

		return (
			<React.Fragment>
				{ this.renderHeader() }

				{ !flashcardsLoading &&
				<CourseCards
					infoByUnits={ infoByUnits }
					courseId={ courseId }
				/> }
				{ !flashcardsLoading &&
				<Guides guides={ guides }/>
				}

				{ showFlashcards &&
				<Flashcards
					infoByUnits={ infoByUnits }
					flashcards={ flashcards }
					courseId={ courseId }
					onClose={ this.hideFlashcards }
					sendFlashcardRate={ sendFlashcardRate }
				/> }
			</React.Fragment>
		);
	}

	renderHeader() {
		const hasUnlockedUnit = this.props.infoByUnits
			.some(unit => unit.unlocked);

		return (
			<header className={ styles.header }>
				<div>
					<p className={ styles.description }>
						Помогут лучше запомнить материал курса и подготовиться к экзаменам
					</p>
				</div>
				<Button disabled={ !hasUnlockedUnit }
						use="primary"
						size='large'
						onClick={ this.showFlashcards }>
					Проверить себя
				</Button>
			</header>
		);
	}

	showFlashcards = () => {
		this.setState({
			showFlashcards: true,
		});
	};

	hideFlashcards = () => {
		this.setState({
			showFlashcards: false,
		});
	};
}

CourseFlashcardsPage.propTypes = {
	courseId: PropTypes.string,
	infoByUnits: PropTypes.arrayOf(PropTypes.shape({
		unitTitle: PropTypes.string,
		unlocked: PropTypes.bool,
		cardsCount: PropTypes.number,
		unitId: PropTypes.string,
		flashcardsSlideSlug: PropTypes.string,
	})),
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
	guides: PropTypes.arrayOf(PropTypes.string),

	loadFlashcards: PropTypes.func,
	sendFlashcardRate: PropTypes.func,
};

export default CourseFlashcardsPage;
