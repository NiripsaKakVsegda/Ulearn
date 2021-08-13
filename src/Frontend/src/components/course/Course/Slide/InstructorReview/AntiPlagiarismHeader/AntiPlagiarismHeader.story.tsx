import React from "react";
import AntiPlagiarismHeader, { Props } from "./AntiPlagiarismHeader";

import { mockFunc, } from "src/utils/storyMock";
import { Gapped } from "ui";

import { Story } from "@storybook/react";
import { AntiPlagiarismStatusResponse } from "src/models/instructor";

interface PropsWithDecorator extends Props {
	title: string;
}

const Template: Story<PropsWithDecorator[]> = (args) => (
	<Gapped vertical>
		{ Object.values(args).map(arg => (
			<div key={ arg.title }>
				<h1>{ arg.title }</h1>
				<AntiPlagiarismHeader { ...arg } />
			</div>)) }
	</Gapped>);


export const List = Template.bind({});
List.args = ([
	{
		title: 'notChecking',
		status: { suspicionCount: 0, suspicionLevel: 'none', status: 'notChecked' },
	},
	{
		title: 'checking -> running',
	},
	{
		title: 'checking -> accepted',
		status: { suspicionCount: 0, suspicionLevel: 'none', status: 'checked' },
	},
	{
		title: 'checking -> suspicions with 5',
		status: { suspicionCount: 5, suspicionLevel: 'faint', status: 'checked' },
	},
	{
		title: 'checking -> strong suspicions with 15',
		status: { suspicionCount: 15, suspicionLevel: 'strong', status: 'checked' },
	},
] as { title: string; status?: AntiPlagiarismStatusResponse; }[]).map(a => ({
	...a,
	fixed: false,
	onZeroScoreButtonPressed: mockFunc,
}));

export default {
	title: 'Exercise/InstructorReview/AntiplagiarismHeader',
};
