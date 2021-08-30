import React from 'react';
import { VisualizerStatus } from "./VusualizerStatus";

import texts from './Visualizer.texts';

export interface DataAreaProps {
	totalSteps: number;
	currentStep: number;
	status: VisualizerStatus;
}

export const StepsCounter = ({ totalSteps, currentStep, status } : DataAreaProps): React.ReactElement<DataAreaProps> =>
	 (
		<div>
			<p>
				{ texts.stepsCounter.currentStepNumber(currentStep, totalSteps) }
				<b> { texts.stepsCounter.status(status) === null ?
					'' : texts.stepsCounter.status(status) }</b>
			</p>
		</div>
	);
