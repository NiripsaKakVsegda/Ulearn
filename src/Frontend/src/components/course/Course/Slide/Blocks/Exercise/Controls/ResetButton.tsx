import { ArrowRoundTimeForwardIcon16Regular } from "@skbkontur/icons/ArrowRoundTimeForwardIcon16Regular";
import cn from 'classnames';
import React from "react";

import texts from "../Exercise.texts";

import styles from './Controls.less';

import IControlWithText from "./IControlWithText";

import ShowControlsTextContext from "./ShowControlsTextContext";

export interface Props extends IControlWithText {
	onResetButtonClicked: () => void,
}

function ResetButton({ onResetButtonClicked, showControlsText }: Props): React.ReactElement {
	return (
		<span className={ cn(styles.exerciseControls, styles.exerciseControlsGapped) } onClick={ onResetButtonClicked }>
			<ArrowRoundTimeForwardIcon16Regular/>
			<ShowControlsTextContext.Consumer>
			{
				(showControlsTextContext) => (showControlsTextContext || showControlsText) && texts.controls.reset.text
			}
			</ShowControlsTextContext.Consumer>
	</span>
	);
}

export default ResetButton;
