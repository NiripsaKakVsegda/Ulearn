import { Story } from "@storybook/react";
import React from "react";
import { Route, Router } from "react-router-dom";
import {
	getMockedShortUser,
	shortGroupExample,
	shortGroupWithLongNameExample,
	shortGroupWithLongNameExample2
} from "../../../storiesUtils";
import { mockFunc, returnPromiseAfterDelay } from "../../../utils/storyMock";
import { apiMocked, getMockedTask } from "../storyUtils";
import UnloadingSettings, { Props } from './UnloadingSettings';

export default {
	title: 'GoogleSheet/Settings'
};

const ListTemplate: Story<{ items: { props: Props, header: string, }[] }>
	= ({ items }) => {
	return <>
		{ items.map((item) =>
			<>
				<p>{ item.header }</p>
				<Router location={ item.props.location } navigator={ {} as any }>
					<Route
						path={ "/:courseId/google-sheet-tasks/:taskId" }
						element={ <UnloadingSettings { ...item.props }/> }
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
				courseTitle: 'Course',
				api: apiMocked,
				location: window.location,
				navigate: mockFunc,
				params: {
					courseId: 'courseId',
					taskId: 'taskId'
				}
			},
			header: 'Default'
		}
	]
};

export const ManyGroups = ListTemplate.bind({});

ManyGroups.args = {
	items: [
		{
			props: {
				courseTitle: 'Course',
				api: {
					...apiMocked,
					getTaskById: (taskId) => {
						return returnPromiseAfterDelay(0, getMockedTask({
							groups: [
								shortGroupExample,
								shortGroupWithLongNameExample,
								shortGroupWithLongNameExample2
							]
						}));
					}
				},
				location: window.location,
				navigate: mockFunc,
				params: {
					courseId: 'courseId',
					taskId: 'taskId'
				}
			},
			header: 'Default'
		}
	]
};

export const LongNameAuthor = ListTemplate.bind({});

LongNameAuthor.args = {
	items: [
		{
			props: {
				courseTitle: 'Course',
				api: {
					...apiMocked,
					getTaskById: (taskId) => {
						return returnPromiseAfterDelay(0, getMockedTask({
							authorInfo: getMockedShortUser(
								{ visibleName: 'Абдулай Шахид АА\'ль Фахид Сараха Фуум Джик Бек Алым Агы' })
						}));
					}
				},
				location: window.location,
				navigate: mockFunc,
				params: {
					courseId: 'courseId',
					taskId: 'taskId'
				}
			},
			header: 'Default'
		}
	]
};

