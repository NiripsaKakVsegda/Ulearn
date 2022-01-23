import React from "react";
import type { Story } from "@storybook/react";

import GroupDeadLines, { AdditionalContentResponse, Props } from "./GroupDeadLines";
import { getMockedShortUser } from "src/storiesUtils";
import { returnPromiseAfterDelay } from "src/utils/storyMock";

const response: AdditionalContentResponse = {
	modules: [
		{
			id: "c069ba64-e101-40e3-9b76-b65a1ae619ae",
			title: "Интересные практики",
			isAdditional: true,
		},
		{
			id: "e1beb629-6f24-279a-3040-cf111f91e764",
			title: "Дополнительные задания",
			slides: [
				{
					id: "69a2e121-e58f-4cd0-8221-7affb7dc796e",
					title: "Задача \"Крокодил\"",
					isAdditional: true,
					publication: {
						date: '25.12.2012',
						author: getMockedShortUser(),
					}
				},
				{
					id: "69a2e121-e58f-4cd0-8221-7affb7dc796d",
					title: "Задача \"Нетривиальные нули дзета-функции римана\"",
					isAdditional: true,
				}
			]
		},
		{
			id: "c069ba64-e101-40e3-9b76-b65a1ae619as",
			title: "Искусственный интеллект для обрабокт данных",
			isAdditional: true,
			publication: {
				date: '22.12.2012',
				author: getMockedShortUser(),
			}
		},
	]
};

const props: Props = {
	getAdditionalContent: ((courseId, groupId) => returnPromiseAfterDelay(400, response)),
	courseId: 'course',
	groupId: 15,
};

const ListTemplate: Story<Props>
	= (props: Props) => {
	return <>
		<GroupDeadLines { ...props }/>
	</>;
};

export const Default = ListTemplate.bind({});
Default.args = {
	...props
};

export default {
	title: 'Settings/GroupDeadLines',
};
