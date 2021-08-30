import React from "react";

import { DocumentLite } from "icons";
import IControlWithText from "./IControlWithText";

import styles from './Controls.less';

import texts from "../Exercise.texts";
import ShowControlsTextContext from "./ShowControlsTextContext";

export interface Props extends IControlWithText {
	showOutput: boolean,

	onShowOutputButtonClicked: () => void,
}

function OutputButton({
	showOutput,
	onShowOutputButtonClicked,
	showControlsText,
}: Props): React.ReactElement {
	return (
		<span className={ styles.exerciseControls } onClick={ onShowOutputButtonClicked }>
			<span className={ styles.exerciseControlsIcon }>
				<DocumentLite/>
			</span>
			<ShowControlsTextContext.Consumer>
				{
					(showControlsTextContext) => (showControlsTextContext || showControlsText) && (showOutput ? texts.controls.output.hide : texts.controls.output.show)
				}
			</ShowControlsTextContext.Consumer>
		</span>
	);
}

export default OutputButton;
