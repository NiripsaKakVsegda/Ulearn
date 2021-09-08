import UnloadingSettings, { Props } from './UnloadingSettings';
import { Story } from "@storybook/react";
import React from "react";
import { Route, RouteComponentProps, Router } from "react-router-dom";
import { createMemoryHistory } from 'history';
import { MatchParams } from "src/models/router";
import { apiMocked, } from "../storyUtils";

export default {
	title: 'GoogleSheet/Settings',
};

const ListTemplate: Story<{ items: { props: Omit<Props, keyof RouteComponentProps<MatchParams>>, header: string, }[] }>
	= ({ items }) => {
	return <>
		{ items.map((item) =>
			<>
				<p>{ item.header }</p>
				<Router history={ createMemoryHistory({ initialEntries: ['/basicprogramming/google-sheet-tasks/0'] }) }>
					<Route
						path={ "/:courseId/google-sheet-tasks/:taskId" }
						render={ (props) => <UnloadingSettings { ...props } { ...item.props } /> }
					/>
				</Router>
			</>
		) }
	</>;
};

export const Default = ListTemplate.bind({});

Default.args = {
	items: [
		{
			props: {
				api: apiMocked,
			},
			header: 'Default',
		}
	]
};
