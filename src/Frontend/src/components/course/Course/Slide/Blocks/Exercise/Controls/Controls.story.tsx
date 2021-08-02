import React from "react";
import type { Story } from "@storybook/react";

import Controls from "./Controls";
import texts from "../Exercise.texts";
import { Gapped } from "ui";
import { mockFunc } from "src/utils/storyMock";

const defaultHints: string[] = [];

const defaultProps = {
	isEditable: true,
	hasOutput: false,
	hideSolutions: true,
	isShowAcceptedSolutionsAvailable: false,

	valueChanged: false,
	submissionLoading: false,

	hints: defaultHints,
	showedHintsCount: 0,

	showOutput: false,

	attemptsStatistics: {
		attemptedUsersCount: 0,
		usersWithRightAnswerCount: 0,
	},

	onSendExerciseButtonClicked: mockFunc,
	showHint: mockFunc,
	resetCodeAndCache: mockFunc,
	toggleOutput: mockFunc,
	onVisitAcceptedSolutions: mockFunc,
};

const hints: string[] = [
	'small hint',
	'large hint Lorem ipsum dolor sit amet, consectetur adipiscing elit. Pellentesque posuere, justo vitae interdum egestas, nibh purus commodo risus, eget feugiat elit dolor eu eros. Curabitur suscipit est at volutpat facilisis. Aliquam placerat varius aliquet. Vivamus eleifend lorem ut tortor efficitur, sed eleifend nulla rutrum. Cras diam ligula, tincidunt sit amet consectetur non, pellentesque in orci. Sed fermentum risus arcu, ac pharetra lorem sodales eget. In lacinia diam massa, eget mattis leo dignissim a. Aliquam et mattis mauris. Proin vulputate tellus vitae augue luctus, sit amet aliquam magna lacinia. Donec tempor dolor quam, sit amet maximus purus hendrerit vitae. Sed neque dui, scelerisque quis risus eget, porta pretium mi. Aliquam sollicitudin ligula nec posuere aliquam.',
	'medium hint with average number of words in total',
	'testwithlongwordwhichcanbrakelinebrakingantetcetcetcetcetcetcetc',
];

const ListTemplate: Story<{ items: { props: typeof defaultProps, header: string }[] }> = ({ items }) => {
	return <>
		{ items.map(({ header, props }, i) =>
			<div key={ i }>
				<p>{ header }</p>
				<Controls>
					<Controls.SubmitButton
						isLoading={ props.submissionLoading }
						onClick={ props.onSendExerciseButtonClicked }
						text={ props.isEditable ? texts.controls.submitCode.text : texts.controls.submitCode.redactor }
					/>
					{ props.hints.length !== 0 &&
					<Controls.ShowHintButton
						showedHintsCountOnStart={ props.showedHintsCount }
						onAllHintsShowed={ mockFunc }
						renderedHints={ props.hints }
					/> }
					{ props.isEditable && <Controls.ResetButton onResetButtonClicked={ props.resetCodeAndCache }/> }
					{ (!props.isEditable && props.hasOutput) && <Controls.OutputButton
						showOutput={ props.showOutput }
						onShowOutputButtonClicked={ props.toggleOutput }
					/> }
					<Controls.StatisticsHint attemptsStatistics={ props.attemptsStatistics }/>
					{ (!props.hideSolutions && (props.hints.length === props.showedHintsCount || props.isShowAcceptedSolutionsAvailable))
					&& <Controls.AcceptedSolutionsButton
						onVisitAcceptedSolutions={ props.onVisitAcceptedSolutions }
						isShowAcceptedSolutionsAvailable={ props.isShowAcceptedSolutionsAvailable }
					/> }
					{ props.isEditable && <Controls.VisualizerButton
						code={ 'print(\'sample code\')' }
						onModalClose={ mockFunc }
					/> }
				</Controls>
			</div>
		) }
	</>;
};

export const AllControls = ListTemplate;
AllControls.args = {
	items: [
		{ props: { ...defaultProps, isEditable: false }, header: '0 hints, - solutions, review' },
		{ props: { ...defaultProps, isEditable: false, hideSolutions: false }, header: '0 hints, + solutions, review' },
		{ props: { ...defaultProps, isEditable: false, hints: hints, }, header: '0/4 hints, - solutions, review' },
		{
			props: { ...defaultProps, isEditable: false, hints: hints, showedHintsCount: hints.length, },
			header: '4/4 hints, - solutions, review'
		},
		{
			props: {
				...defaultProps,
				isEditable: false,
				hideSolutions: false,
				hints: hints,
				showedHintsCount: hints.length,
			},
			header: '4/4 hints, + solutions, review'
		},

		{ props: defaultProps, header: '0 hints, - solutions, redactor' },

		{ props: { ...defaultProps, hideSolutions: false, }, header: '0 hints, + solutions, redactor' },
		{ props: { ...defaultProps, hideSolutions: false, hints: hints, }, header: '0/4 hints, + solutions, redactor' },
		{
			props: { ...defaultProps, hideSolutions: false, hints: hints, showedHintsCount: hints.length },
			header: '4/4 hints, + solutions, redactor'
		},
		{
			props: { ...defaultProps, hideSolutions: true, hints: hints, showedHintsCount: hints.length },
			header: '4/4 hints, - solutions, redactor'
		},
	]
};

export const OpenedHints = () => {
	return (
		<Controls>
			<Controls.ShowHintButton
				showedHintsCountOnStart={ hints.length }
				onAllHintsShowed={ mockFunc }
				renderedHints={ hints }
				showControlsText={ true }
				subTooltipTrigger={ 'opened' }
				mainTooltipTrigger={ 'opened' }
			/>
		</Controls>
	);
};

export const OpenedStatistics = () => {
	return (
		<Gapped vertical gap={ 200 }>
			<Controls>
				<Controls.StatisticsHint
					tooltipTrigger={ 'opened' }
					attemptsStatistics={ {
						attemptedUsersCount: 50000,
						usersWithRightAnswerCount: 43125,
						lastSuccessAttemptDate: '2010-05-12',
					} }
				/>
			</Controls>
			<Controls>
				<Controls.StatisticsHint
					tooltipTrigger={ 'opened' }
					attemptsStatistics={ {
						attemptedUsersCount: 100,
						usersWithRightAnswerCount: 56,
						lastSuccessAttemptDate: '2020-05-12',
					} }
				/>
			</Controls>
			<Controls>
				<Controls.StatisticsHint
					tooltipTrigger={ 'opened' }
					attemptsStatistics={ {
						attemptedUsersCount: 0,
						usersWithRightAnswerCount: 0,
					} }
				/>
			</Controls>
		</Gapped>
	);
};


export const OpenedAcceptedSolutionsButton = () => {
	return (
		<Controls>
			<Controls.AcceptedSolutionsButton
				tooltipTrigger={ 'opened' }
				onVisitAcceptedSolutions={ mockFunc }
				isShowAcceptedSolutionsAvailable={ true }
			/>
		</Controls>
	);
};


export default {
	title: 'Exercise/Controls',
	component: Controls,
};
