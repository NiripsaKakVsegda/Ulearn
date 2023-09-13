import { ComponentMeta, ComponentStory } from "@storybook/react";
import { GroupType, JoinGroupInfo } from "../../../models/groups";
import { fullscreenLayout, getMockedUser } from "../../../storiesUtils";
import { mockFunc } from "../../../utils/storyMock";
import JoinGroup from "./JoinGroup";

export default {
	title: 'Group/JoinGroup',
	component: JoinGroup,
	...fullscreenLayout
} as ComponentMeta<typeof JoinGroup>;

const Template: ComponentStory<typeof JoinGroup> = (args) => (
	<JoinGroup { ...args } />
);

export const JoinGroupStory = Template.bind({});
JoinGroupStory.storyName = 'Default';
JoinGroupStory.args = {
	group: getMockedJoinGroupInfo(),
	courseLink: '#',
	accountLink: '#',
	onJoinGroup: mockFunc
};

export const JoinGroupCanSeeProgressStory = Template.bind({});
JoinGroupCanSeeProgressStory.storyName = 'Can see progress';
JoinGroupCanSeeProgressStory.args = {
	group: getMockedJoinGroupInfo({
		canStudentsSeeProgress: true
	}),
	courseLink: '#',
	accountLink: '#',
	onJoinGroup: mockFunc
};

export const JoinGroupJoinedStory = Template.bind({});
JoinGroupJoinedStory.storyName = 'SingleGroup. Joined';
JoinGroupJoinedStory.args = {
	group: getMockedJoinGroupInfo({
		isMember: true
	}),
	courseLink: '#',
	accountLink: '#',
	onJoinGroup: mockFunc
};

export const JoinGroupSuperGroupStory = Template.bind({});
JoinGroupSuperGroupStory.storyName = 'Super group. Join';
JoinGroupSuperGroupStory.args = {
	group: getMockedJoinGroupInfo({
		groupType: GroupType.SuperGroup
	}),
	courseLink: '#',
	accountLink: '#',
	onJoinGroup: mockFunc
};

export const JoinGroupSuperGroupJoinedStory = Template.bind({});
JoinGroupSuperGroupJoinedStory.storyName = 'Super group. Joined';
JoinGroupSuperGroupJoinedStory.args = {
	group: getMockedJoinGroupInfo({
		groupType: GroupType.SuperGroup,
		isMember: true
	}),
	courseLink: '#',
	accountLink: '#',
	onJoinGroup: mockFunc
};

export const JoinGroupLinkDisabledStory = Template.bind({});
JoinGroupLinkDisabledStory.storyName = 'Link disabled';
JoinGroupLinkDisabledStory.args = {
	group: getMockedJoinGroupInfo({
		isInviteLinkEnabled: false
	}),
	courseLink: '#',
	accountLink: '#',
	onJoinGroup: mockFunc
};


function getMockedJoinGroupInfo(joinGroupInfo?: Partial<JoinGroupInfo>): JoinGroupInfo {
	return {
		id: 0,
		groupType: GroupType.SingleGroup,
		name: 'РТФ-Р1-01-2022',
		courseId: 'id',
		courseTitle: 'Основы программирования',
		owner: getMockedUser({
			id: 'userId',
			firstName: 'Иван',
			lastName: 'Иванов',
			visibleName: 'Иван Иванов'
		}),
		isMember: false,
		isInDefaultGroup: false,
		isInviteLinkEnabled: true,
		canStudentsSeeProgress: false,
		...joinGroupInfo
	};
}
