import { DocTextIcon16Regular } from "@skbkontur/icons/DocTextIcon16Regular";
import cn from 'classnames';
import React from "react";

import texts from "../Exercise.texts";

import styles from './Controls.less';
import IControlWithText from "./IControlWithText";
import ShowControlsTextContext from "./ShowControlsTextContext";

export interface Props extends IControlWithText {
	showOutput: boolean,

	onShowOutputButtonClicked: () => void,
}

function OutputButton({
	showOutput,
	onShowOutputButtonClicked,
	showControlsText
}: Props): React.ReactElement {
	return (
		<span
			className={ cn(styles.exerciseControls, styles.exerciseControlsGapped) }
			onClick={ onShowOutputButtonClicked }
		>
			<DocTextIcon16Regular/>
			<ShowControlsTextContext.Consumer>
				{
					(showControlsTextContext) => (showControlsTextContext || showControlsText) &&
												 (showOutput ? texts.controls.output.hide : texts.controls.output.show)
				}
			</ShowControlsTextContext.Consumer>
		</span>
	);
}

export default OutputButton;
