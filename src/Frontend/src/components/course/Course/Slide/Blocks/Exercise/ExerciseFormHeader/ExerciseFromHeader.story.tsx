import React from "react";
import type { Story } from "@storybook/react";
import { ExerciseFormHeader, ExerciseFormHeaderProps } from "./ExerciseFormHeader";
import {
	AutomaticExerciseCheckingProcessStatus,
	AutomaticExerciseCheckingResult,
	SolutionRunStatus,
	SubmissionInfo,
} from "src/models/exercise";
import { Language } from "src/consts/languages";

import { SubmissionColor } from "../ExerciseUtils";
import { skipLoki } from "../../../../../Navigation/stroies.data";

const Template: Story<ExerciseFormHeaderProps> = (args) => <ExerciseFormHeader { ...args } />;

const submissionInfo: SubmissionInfo = {
	id: 1,
	code: "",
	timestamp: "",
	automaticChecking: {
		processStatus: AutomaticExerciseCheckingProcessStatus.Done,
		result: AutomaticExerciseCheckingResult.RightAnswer,
		output: null,
		reviews: null,
		checkerLogs: null,
	},
	manualChecking: {
		reviews: [],
		percent: null,
	},
	language: Language.cSharp
};

export const Editable = Template.bind({});
Editable.args = {
	solutionRunStatus: null,
	selectedSubmission: submissionInfo,
	score: 10
};

export default {
	title: 'Exercise/ExerciseFromHeader',
	component: ExerciseFormHeader,
	argTypes: {
		solutionRunStatus: {
			control: {
				type: 'select',
				options: [...Object.values(SolutionRunStatus), null],
			},
		},
		submissionColor: {
			defaultValue: SubmissionColor.MaxResult,
			control: {
				type: 'select',
				options: [...Object.values(SubmissionColor)],
			},
		},
	},
	...skipLoki,
};
