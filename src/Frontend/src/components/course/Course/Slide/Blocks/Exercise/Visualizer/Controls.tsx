import React, { MouseEventHandler } from "react";

import { ControlButton } from "./ControlButton";
import { Gapped } from "ui";

import { VisualizerStatus } from "./VusualizerStatus";

import texts from './Visualizer.texts';

interface ControlsProps {
	run: MouseEventHandler;
	next: MouseEventHandler;
	previous: MouseEventHandler;
	last: MouseEventHandler;

	visualizerStatus: VisualizerStatus;

	currentStep: number;
	totalSteps: number;
}

export const Controls =
	({ run, next, previous, last, visualizerStatus, currentStep, totalSteps }: ControlsProps):
		React.ReactElement =>
		(
			<Gapped gap={ 16 }>
				<ControlButton
					use={ visualizerStatus === VisualizerStatus.Ready ||
					visualizerStatus === VisualizerStatus.Blocked ? "primary" : "default" }
					onClick={ run }
					text={ visualizerStatus === VisualizerStatus.Ready ?
						texts.controls.run : texts.controls.rerun }
					disabled={ visualizerStatus === VisualizerStatus.Loading }
				/>

				<Gapped gap={ 8 }>
					<ControlButton
						use={ visualizerStatus === VisualizerStatus.Ready ? "default" : "primary" }
						onClick={ previous }
						disabled={ visualizerStatus === VisualizerStatus.Blocked ||
						visualizerStatus === VisualizerStatus.Loading || currentStep === 0 }
						text={ texts.controls.back }
					/>
					<ControlButton
						use={ visualizerStatus === VisualizerStatus.Ready ? "default" : "primary" }
						onClick={ next }
						disabled={ visualizerStatus === VisualizerStatus.Blocked ||
						visualizerStatus === VisualizerStatus.Loading ||
						currentStep === totalSteps - 1 || totalSteps === 0 }
						text={ texts.controls.next }
					/>
				</Gapped>

				<ControlButton
					use={ "default" }
					onClick={ last }
					disabled={ visualizerStatus === VisualizerStatus.Blocked ||
					visualizerStatus === VisualizerStatus.Loading ||
					currentStep === totalSteps - 1 || totalSteps === 0 }
					text={ texts.controls.last }
				/>
			</Gapped>
		);
