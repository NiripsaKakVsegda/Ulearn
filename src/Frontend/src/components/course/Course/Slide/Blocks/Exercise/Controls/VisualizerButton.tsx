import React from "react";

import ShowControlsTextContext from "./ShowControlsTextContext";
import IControlWithText from "./IControlWithText";

import { PC, } from "@skbkontur/react-icons";
import { Visualizer } from "../Visualizer/Visualizer";

import styles from './Controls.less';

import texts from "../Exercise.texts";


interface Props extends IControlWithText {
	code: string;
	onModalClose: (code: string) => void;
}

function VisualizerButton({
	showControlsText,
	code,
	onModalClose,
}: Props): React.ReactElement {
	const [isModalVisible, setModalVisible] = React.useState(false);

	return (
		<span className={ styles.exerciseControls }>
			<ShowControlsTextContext.Consumer>
			{
				(showControlsTextContext) =>
					<span onClick={ openModal }>
						<span className={ styles.exerciseControlsIcon }>
							<PC/>
						</span>
						{ (showControlsTextContext || showControlsText) && texts.controls.visualizer.text }
					</span>
			}
			</ShowControlsTextContext.Consumer>
			{ isModalVisible && <Visualizer code={ code } onModalClose={ closeModal }/> }
		</span>
	);

	function openModal() {
		setModalVisible(true);
	}

	function closeModal(code: string) {
		setModalVisible(false);
		onModalClose(code);
	}
}

export default VisualizerButton;
