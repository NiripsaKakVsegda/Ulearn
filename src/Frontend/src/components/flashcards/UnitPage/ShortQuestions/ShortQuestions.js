import React, { Component } from 'react';
import PropTypes from "prop-types";

import { Toggle } from "ui";

import translateCode from "src/codeTranslator/translateCode";
import { settingsForFlashcards } from "src/codeTranslator/codemirror";

import styles from './shortQuestions.less';

class ShortQuestions extends Component {
	constructor(props) {
		super(props);
		const { questionsWithAnswers } = this.props;

		this.state = {
			showAllAnswers: false,
			questionsWithAnswers: questionsWithAnswers.map(el => ({ ...el })),
		};
	}

	componentDidMount() {
		translateCode(this.list, { 'codeMirror': settingsForFlashcards });
	}

	componentDidUpdate(prevProps, prevState, snapshot) {
		const { questionsWithAnswers } = this.props;

		translateCode(this.list, { 'codeMirror': settingsForFlashcards });

		if(prevProps.questionsWithAnswers.length !== questionsWithAnswers.length) {
			this.setState({ showAllAnswers: false, });
		}


		if(this.state.questionsWithAnswers.length !== questionsWithAnswers.length) {
			this.setState({ questionsWithAnswers, showAllAnswers: false, });
		}
	}

	render() {
		const { showAllAnswers } = this.state;

		return (
			<div className={ styles.questionsContainer }>
				{ this.renderQuestions() }
				<div className={ styles.toggleContainer }>
					<Toggle onChange={ this.handleToggleChange } checked={ showAllAnswers }/>
					<span className={ styles.toggleText }>
						Показать ответы
					</span>
				</div>
			</div>
		)
	}

	renderQuestions() {
		const { showAllAnswers, questionsWithAnswers } = this.state;

		return (
			<ol ref={ (ref) => this.list = ref } className={ styles.questionsTextContainer }>
				{ questionsWithAnswers.map(({ question, answer, showAnswer, }, index) =>
					<li className={ styles.listElement }
						key={ index }
						onClick={ () => this.handleQuestionClick(index) }>
						<div dangerouslySetInnerHTML={ { __html: question } }/>
						{
							(showAllAnswers || showAnswer) &&
							<div className={ styles.answerText } dangerouslySetInnerHTML={ { __html: answer } }/>
						}
					</li>)
				}
			</ol>);
	}


	handleQuestionClick = (questionIndex) => {
		const { questionsWithAnswers } = this.state;
		const newQuestionsWithAnswers = questionsWithAnswers.map(el => ({ ...el }));
		const questionWithAnswer = newQuestionsWithAnswers[questionIndex];

		questionWithAnswer.showAnswer = !questionWithAnswer.showAnswer;

		this.setState({
			questionsWithAnswers: newQuestionsWithAnswers,
		})
	};

	handleToggleChange = () => {
		const { showAllAnswers } = this.state;
		const { questionsWithAnswers } = this.props;

		this.setState({
			showAllAnswers: !showAllAnswers,
			questionsWithAnswers: questionsWithAnswers.map(el => ({ ...el })),
		});
	}
}

ShortQuestions.propTypes = {
	questionsWithAnswers: PropTypes.arrayOf(PropTypes.shape({
		question: PropTypes.string,
		answer: PropTypes.string,
	})),
	className: PropTypes.string,
};

export default ShortQuestions;
