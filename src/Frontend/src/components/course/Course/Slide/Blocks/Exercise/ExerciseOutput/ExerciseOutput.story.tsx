import React from "react";
import type { Story } from "@storybook/react";
import { ExerciseOutput, OutputTypeProps } from "./ExerciseOutput";

import {
	AutomaticExerciseCheckingProcessStatus as ProcessStatus,
	AutomaticExerciseCheckingResult as CheckingResult,
	SolutionRunStatus
} from "src/models/exercise";
import { SubmissionColor } from "../ExerciseUtils";

const Template: Story<OutputTypeProps> = (args) => <ExerciseOutput { ...args } />;

const ListTemplate: Story<{ items: { props: OutputTypeProps, header: string }[] }> = ({ items }) => {
	return <>
		{ items.map((item) =>
			<>
				<p>{ item.header }</p>
				<ExerciseOutput { ...item.props } />
			</>
		) }
	</>;
};

const outputTypeProps: OutputTypeProps = {
	solutionRunStatus: SolutionRunStatus.Success,
	message: "message text",
	expectedOutput: "1\n2\n      3\nНазвание теста: MoveManipulatorTo_ActuallyBringsManipulatorToDesiredLocation(-207.950052059698d,109.42692431548d,62.2635171658309d)",
	submissionColor: SubmissionColor.WrongAnswer,
	automaticChecking: {
		output: "3\n2\n3",
		processStatus: ProcessStatus.Done,
		result: CheckingResult.WrongAnswer,
		reviews: null,
		checkerLogs: null,
	},
};

export const Editable = Template.bind({});
Editable.args = outputTypeProps;

const allSolutionRunStatusesItems = Object.values(SolutionRunStatus)
	.map(s => ({
		header: s, props: {
			...outputTypeProps,
			solutionRunStatus: s,
			submissionColor: s === SolutionRunStatus.CompilationError ? SubmissionColor.WrongAnswer : SubmissionColor.NeedImprovements
		}
	}));
export const AllSolutionRunStatuses = ListTemplate.bind({});
AllSolutionRunStatuses.args = { items: allSolutionRunStatusesItems };

const allProcessStatusesItems = Object.values(ProcessStatus)
	.map(s => ({
		header: s as string, props: {
			...outputTypeProps,
			message: null,
			submissionColor: SubmissionColor.NeedImprovements,
			automaticChecking: {
				...outputTypeProps.automaticChecking,
				processStatus: s,
			},
		} as OutputTypeProps
	}));
export const AllProcessStatuses = ListTemplate.bind({});
AllProcessStatuses.args = { items: allProcessStatusesItems };

const allCheckingResultsItems = Object.values(CheckingResult)
	.map(s => ({
		header: s as string, props: {
			...outputTypeProps,
			message: null,
			submissionColor: s === CheckingResult.CompilationError || s === CheckingResult.WrongAnswer || s === CheckingResult.RuntimeError
				? SubmissionColor.WrongAnswer
				: SubmissionColor.MaxResult,
			automaticChecking: {
				...outputTypeProps.automaticChecking,
				result: s,
			}
		} as OutputTypeProps
	}));
export const allCheckingResults = ListTemplate.bind({});
allCheckingResults.args = { items: allCheckingResultsItems };

export default {
	title: 'Exercise/ExerciseOutput',
	component: ExerciseOutput,
	argTypes: {
		solutionRunStatus: {
			control: {
				type: 'select',
				options: Object.values(SolutionRunStatus),
			},
		},
		submissionColor: {
			control: {
				type: 'select',
				options: [...Object.values(SubmissionColor)],
			},
		},
		message: {
			control: {
				type: 'text'
			},
		},
		expectedOutput: {
			control: {
				type: 'text'
			},
		},
	},
};
