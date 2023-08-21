import { TechPcLaptopIcon } from '@skbkontur/icons/TechPcLaptopIcon';
import React from "react";

import defaultTheme from "src/uiTheme";
import { ThemeContext } from "ui";

import texts from "../Exercise.texts";

import { Visualizer } from "../Visualizer/Visualizer";

import styles from './Controls.less';
import IControlWithText from "./IControlWithText";

import ShowControlsTextContext from "./ShowControlsTextContext";


interface Props extends IControlWithText {
	code: string;
	onModalClose?: (code: string) => void;

	size?: number;
	className?: string;
}

function VisualizerButton({
	showControlsText,
	code,
	className,
	size,
	onModalClose
}: Props): React.ReactElement {
	const [isModalVisible, setModalVisible] = React.useState(false);

	return (
		<ThemeContext.Provider value={ defaultTheme }>
			<span className={ className ?? styles.exerciseControls }>
				<ShowControlsTextContext.Consumer>
				{
					(showControlsTextContext) =>
						<span className={ styles.exerciseControlsGapped } onClick={ openModal }>
							<TechPcLaptopIcon size={ size }/>
							{ (showControlsTextContext || showControlsText) && texts.controls.visualizer.text }
						</span>
				}
				</ShowControlsTextContext.Consumer>
				{ isModalVisible && <Visualizer code={ code } onModalClose={ closeModal }/> }
			</span>
		</ThemeContext.Provider>
	);

	function openModal() {
		setModalVisible(true);
	}

	function closeModal(code: string) {
		setModalVisible(false);
		onModalClose?.(code);
	}
}

export default VisualizerButton;
