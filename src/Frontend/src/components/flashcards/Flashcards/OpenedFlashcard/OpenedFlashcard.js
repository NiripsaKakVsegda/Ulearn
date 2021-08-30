import React, { Component } from "react";
import PropTypes from "prop-types";

import { Button } from "ui";
import Results from "../Results/Results";

import styles from "./openedFlashcard.less";

import translateCode from "src/codeTranslator/translateCode";
import { settingsForFlashcards } from "src/codeTranslator/codemirror";
import { Link } from "react-router-dom";

class OpenedFlashcard extends Component {
	constructor(props) {
		super(props);

		this.state = {
			showAnswer: false,
		}
	}

	componentDidMount() {
		document.addEventListener('keyup', this.handleKeyUp);
		translateCode(this.modal, { 'codeMirror': settingsForFlashcards });
	}

	componentWillUnmount() {
		document.removeEventListener('keyup', this.handleKeyUp);
	}

	componentDidUpdate(prevProps, prevState, snapshot) {
		translateCode(this.modal, { 'codeMirror': settingsForFlashcards });
	}

	handleKeyUp = (e) => {
		const { onClose } = this.props;
		const { showAnswer } = this.state;
		const code = e.key;
		const spaceChar = ' ';
		const escapeChar = 'Escape';

		if(code === spaceChar) {
			this.showAnswer();
		} else if(code >= 1 && code <= 5 && showAnswer) {
			this.handleResultsClick(code);
		} else if(code === escapeChar) {
			onClose();
		}
	};

	handleResultsClick = (rate) => {
		const { onHandlingResultsClick } = this.props;

		onHandlingResultsClick(rate);

		this.setState({
			showAnswer: false,
		})
	};

	render() {
		const { unitTitle, question, answer, onClose } = this.props;
		const { showAnswer } = this.state;

		let content, control;

		if(showAnswer) {
			content = OpenedFlashcard.renderBackContent(question, answer, this.renderLinksToTheorySlides());
			control = this.renderBackControl();
		} else {
			content = OpenedFlashcard.renderFrontContent(question);
			control = this.renderFrontControl();
		}

		return (
			<div className={ styles.wrapper } ref={ ref => this.modal = ref }>

				<button tabIndex={ 1 } className={ styles.closeButton } onClick={ onClose }>
					&times;
				</button>
				<h5 className={ styles.unitTitle }>
					{ unitTitle }
				</h5>
				{ content }
				{ control }
			</div>
		);
	}

	renderLinksToTheorySlides() {
		const { theorySlides } = this.props;
		const slidesCount = theorySlides.length;

		if(slidesCount === 0) {
			return;
		}

		const sourcePluralForm = slidesCount > 1
			? 'Источники'
			: 'Источник';

		return (
			<p className={ styles.theoryLinks }>
				{ sourcePluralForm }: { OpenedFlashcard.mapTheorySlidesToLinks(theorySlides, slidesCount) }
			</p>)
	}

	static mapTheorySlidesToLinks(theorySlides, slidesCount) {
		const links = [];

		for (let i = 0; i < slidesCount; i++) {
			const { slug, title } = theorySlides[i];

			links.push(
				<Link to={ slug } key={ slug }>
					Слайд «{ title }»
				</Link>);

			if(i < slidesCount - 1) {
				links.push(', ');
			}
		}

		return links;
	}

	static renderFrontContent(question) {
		return (
			<div className={ styles.questionFront }
				 dangerouslySetInnerHTML={ { __html: question } }
			/>
		);
	}

	renderFrontControl() {
		return (
			<div className={ styles.showAnswerButtonContainer }>
				<Button size='large' use='primary' onClick={ this.showAnswer }>
					Показать ответ
				</Button>
			</div>
		);
	}

	showAnswer = () => {
		this.props.onShowAnswer();

		this.setState({
			showAnswer: true
		});
	};

	static renderBackContent(question, answer, linksToTheorySlides) {
		return (
			<div className={ styles.backTextContainer }>
				<div className={ styles.questionBack } dangerouslySetInnerHTML={ { __html: question } }/>
				<div className={ styles.answer } dangerouslySetInnerHTML={ { __html: answer } }/>
				{ linksToTheorySlides }
			</div>
		);
	}

	renderBackControl() {
		return (
			<Results handleClick={ this.handleResultsClick }/>
		);
	}
}

OpenedFlashcard.propTypes = {
	question: PropTypes.string,
	answer: PropTypes.string,
	unitTitle: PropTypes.string,
	theorySlides: PropTypes.arrayOf(
		PropTypes.shape({
			slug: PropTypes.string,
			title: PropTypes.string,
		}),
	),

	onClose: PropTypes.func,
	onShowAnswer: PropTypes.func,
	onHandlingResultsClick: PropTypes.func,
};

export default OpenedFlashcard;
