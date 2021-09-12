import React from "react";
import { Button, Gapped, Switcher, Toggle, } from "ui";

import styles from './ScoreControls.less';
import texts from './ScoreControls.texts';


const defaultScores = ['0', '25', '50', '75', '100'];

export interface Props {
	scores?: string[];
	exerciseTitle: string;

	prevReviewScore: number | 0 | 25 | 50 | 75 | 100 | null;
	score: number | 0 | 25 | 50 | 75 | 100 | null;
	date?: string;

	toggleChecked: boolean;
	canChangeScore: boolean;

	onSubmit: (score: number) => void;
	onToggleChange: (value: boolean) => void;
}

interface State {
	curScore: number | null;
	scoreSaved: boolean;
	toggleChecked: boolean;
}

class ScoreControls extends React.Component<Props, State> {
	constructor(props: Props) {
		super(props);
		const {
			score,
			toggleChecked,
		} = this.props;

		this.state = {
			curScore: score,
			scoreSaved: score !== null,
			toggleChecked: toggleChecked,
		};
	}

	componentDidUpdate(prevProps: Readonly<Props>): void {
		const { score, toggleChecked, canChangeScore, } = this.props;

		if(prevProps.score !== score) {
			this.setState({
				toggleChecked,
				curScore: score,
				scoreSaved: score !== null || !canChangeScore,
			});
		}

		if(this.state.toggleChecked !== toggleChecked) {
			this.setState({
				toggleChecked,
			});
		}
	}

	render(): React.ReactNode {
		const {
			scores = defaultScores,
			exerciseTitle,
			prevReviewScore,
			canChangeScore,
			date,
		} = this.props;
		const {
			scoreSaved,
			curScore,
			toggleChecked,
		} = this.state;

		return (
			<Gapped gap={ 24 } vertical>
				{ scoreSaved && curScore !== null
					? this.renderControlsAfterSubmit(curScore, date, canChangeScore,)
					: this.renderControls(scores, curScore, prevReviewScore)
				}
				{ canChangeScore && this.renderKeepReviewingToggle(toggleChecked, exerciseTitle,) }
			</Gapped>
		);
	}

	renderControlsAfterSubmit = (score: number, date?: string, canChangeScore?: boolean): React.ReactElement => {
		return (
			<Gapped gap={ 16 } vertical={ false }>
				<span className={ styles.successLabel }>
					{ texts.getScoreText(score, date) }
				</span>
				{ canChangeScore && <Button
					size={ "medium" }
					use={ "link" }
					onClick={ this.resetScore }
				>
					{ texts.changeScoreText }
				</Button> }
			</Gapped>
		);
	};

	renderControls = (scores: string[], score: number | null, prevReviewScore: number | null,): React.ReactElement => {
		return (
			<Gapped gap={ 24 } vertical={ false } className={ styles.controlsWrapper }>
				{ this.renderSwitcherWithLastReviewMarker(scores, score, prevReviewScore,) }
				<Button
					size={ 'medium' }
					disabled={ score === null }
					use={ 'primary' }
					onClick={ this.onSubmitClick }
				>
					{ texts.submitButtonText }
				</Button>
			</Gapped>
		);
	};

	renderSwitcherWithLastReviewMarker = (scores: string[], score: number | null,
		prevReviewScore: number | null,
	): React.ReactElement => {
		if(prevReviewScore === null) {
			return (
				<Switcher
					className={ styles.scoresLabel }
					label={ texts.scoresText }
					size={ "medium" }
					value={ score?.toString() }
					items={ scores }
					onValueChange={ this.onValueChange }/>
			);
		}
		/*
			injecting/changing something down here? check injectPrevReviewScore first or you can break something
		*/
		return (
			<span ref={ this.injectPrevReviewScore }>
				<Switcher
					className={ styles.scoresLabel }
					label={ texts.scoresText }
					size={ "medium" }
					value={ score?.toString() }
					items={ scores }
					onValueChange={ this.onValueChange }/>
				</span>
		);
	};

	//hardcoded function which injecting an last review span in parent span using absolute position
	//[1] parent span should not have any childs except switcher, or it will not render last review span
	//[2] switcher should be the first child of span
	//[3] switcher should have buttons with correct positions
	injectPrevReviewScore = (element: HTMLSpanElement): void => {
		const {
			scores = defaultScores,
			prevReviewScore,
		} = this.props;

		if(prevReviewScore === null || !element?.children[0] || element.children.length > 1) { //[1]
			return;
		}

		const index = scores.findIndex(s => s === prevReviewScore.toString());

		if(index === -1) {
			return;
		}

		const buttons = element.children[0].getElementsByTagName('button'); //[2]
		const button = buttons[index];

		const lastReviewNode = document.createElement('span');
		lastReviewNode.className = styles.lastReviewWrapper;
		lastReviewNode.textContent = texts.lastReviewScoreText;

		element.appendChild(lastReviewNode);

		//[3]
		lastReviewNode.style.top = `${ button.offsetHeight + 6 }px`; // 6 = 2px borders + 4px margin
		lastReviewNode.style.left = `${ button.offsetLeft + (button.offsetWidth - lastReviewNode.offsetWidth) / 2 }px`;
	};

	renderKeepReviewingToggle(toggleChecked: boolean, exerciseTitle: string,): React.ReactElement {
		return (
			<Toggle checked={ toggleChecked } captionPosition={ "right" } onValueChange={ this.onToggleValueChange }>
				<span className={ styles.toggleLabel }>
					{ texts.getCodeReviewToggleText(exerciseTitle) }
				</span>
			</Toggle>
		);
	}

	resetScore = (): void => {
		this.setState({
			scoreSaved: false,
		});
	};

	onValueChange = (score: string): void => {
		const { toggleChecked } = this.state;
		const scoreAsNumber = parseInt(score);

		if(toggleChecked && (scoreAsNumber === 0 || scoreAsNumber === 100)) {
			this.onToggleValueChange(false);
		}

		this.setState({
			curScore: scoreAsNumber,
		});
	};

	onToggleValueChange = (value: boolean): void => {
		this.props.onToggleChange(value);
		this.setState({
			toggleChecked: value,
		});
	};

	onSubmitClick = (): void => {
		const {
			curScore,
		} = this.state;
		const {
			onSubmit,
		} = this.props;
		if(curScore !== null) {
			onSubmit(curScore);
		}
		this.setState({
			scoreSaved: true,
		});
	};
}

export default ScoreControls;
