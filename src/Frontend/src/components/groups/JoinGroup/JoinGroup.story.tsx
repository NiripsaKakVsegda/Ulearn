import React from 'react';
import JoinGroup from "./JoinGroup";
import { buildListTemplate, getMockedGroup } from "../../../storiesUtils";
import { returnPromiseAfterDelay } from "src/utils/storyMock";
import { Meta } from "@storybook/react";
import { Props } from './JoinGroup.types';

const group = getMockedGroup();

const joinGroupMock = (inviteHash: string) => {
	return returnPromiseAfterDelay<Response>(300);
};

const getGroupMock = (inviteHash: string) => {
	return returnPromiseAfterDelay(300, group);
};

const defaultProps: Props = {
	joinGroup: joinGroupMock,
	getGroupByHash: getGroupMock,
	navigate: () => ({}),
	params: { hash: '123123', courseId: 'bp', taskId: '' },
};

const ListTemplate = buildListTemplate<Props>(
	(props) => <JoinGroup { ...props } />
);
export const Default = ListTemplate.bind({});
Default.args = [{ title: "Join simple", props: { ...defaultProps } }];


export default {
	title: 'Group/JoinGroup',
} as Meta;
