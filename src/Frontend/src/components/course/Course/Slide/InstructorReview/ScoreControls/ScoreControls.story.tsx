import React from "react";
import ScoreControls, { Props } from "./ScoreControls";
import type { Story } from "@storybook/react";
import { Gapped } from "ui";
import { mockFunc } from "../../../../../../utils/storyMock";

interface PropsWithDecorator extends Props {
	title: string;
}

export const List: Story<PropsWithDecorator[]> = (args) =>
	<Gapped vertical>
		{ Object.values(args).map(arg => (
			<div key={ arg.title }>
				<h1>{ arg.title }</h1>
				<ScoreControls { ...arg }/>
			</div>)) }
	</Gapped>;


const args: PropsWithDecorator[] = [
	{
		title: 'toggle checked',

		exerciseTitle: 'Angry Birds',
		toggleChecked: true,
	},
	{
		title: 'angry birds 0',

		exerciseTitle: 'Angry Birds',
		prevReviewScore: 0,
	},
	{
		title: 'angry birds 25',

		exerciseTitle: 'Angry Birds',
		prevReviewScore: 25,
	},
	{
		title: 'angry birds 50',

		exerciseTitle: 'Angry Birds',
		prevReviewScore: 50,
	},
	{
		title: 'angry birds 75',

		exerciseTitle: 'Angry Birds',
		prevReviewScore: 75,
	},
	{
		title: 'angry birds 100',

		exerciseTitle: 'Angry Birds',
		prevReviewScore: 100,
	},
	{
		title: 'no previous review',

		exerciseTitle: 'Practise title',
	},
	{
		title: 'long practice name with spaces and verylongnamewithoutspaces',

		exerciseTitle: 'long practice name with spaces and verylongnamewithoutspaces',
	},
].map(c => ({
	score: null,
	prevReviewScore: c.prevReviewScore || null,
	onSubmit: mockFunc,
	onToggleChange: mockFunc, ...c,
	canChangeScore: true,
	toggleChecked: !!c.toggleChecked,
}));

List.args = args;


export default {
	title: 'Exercise/InstructorReview/ScoreControls',
};
