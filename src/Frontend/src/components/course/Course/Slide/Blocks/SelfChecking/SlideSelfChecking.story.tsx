import React from 'react';

import SlideSelfChecking, { SlideSelfCheckingProps } from './SlideSelfChecking';
import { buildListTemplate, GetMock, mock } from "src/storiesUtils";


const ListTemplate = buildListTemplate<SlideSelfCheckingProps>(
	(props) => <SlideSelfChecking { ...props } />
);
export const Default = ListTemplate.bind({});

const args = [
	{
		title: '0 selected',
		props: {
			onCheckupClick: mock,
			checkups: GetMock.OfCheckups.checkups,
		},
	}, {
		title: '1 selected',
		props: {
			onCheckupClick: mock,
			checkups: GetMock.OfCheckups.withSelected(1).checkups,
		},
	},
	{
		title: 'All selected',
		props: {
			onCheckupClick: mock,
			checkups: GetMock.OfCheckups.withAllSelected().checkups,
		},
	},
];

Default.args = args;

export default {
	title: "Slide/Blocks/SelfChecking/Slide",
	component: SlideSelfChecking,
};

