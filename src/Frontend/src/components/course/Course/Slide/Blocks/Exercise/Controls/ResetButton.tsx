import React from "react";

import IControlWithText from "./IControlWithText";
import { Refresh } from "icons";

import ShowControlsTextContext from "./ShowControlsTextContext";

import styles from './Controls.less';

import texts from "../Exercise.texts";

export interface Props extends IControlWithText {
	onResetButtonClicked: () => void,
}

function ResetButton({ onResetButtonClicked, showControlsText, }: Props): React.ReactElement {
	return (
		<span className={ styles.exerciseControls } onClick={ onResetButtonClicked }>
			<span className={ styles.exerciseControlsIcon }>
				<Refresh/>
			</span>
			<ShowControlsTextContext.Consumer>
			{
				(showControlsTextContext) => (showControlsTextContext || showControlsText) && texts.controls.reset.text
			}
			</ShowControlsTextContext.Consumer>
	</span>
	);
}

export default ResetButton;
