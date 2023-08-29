import { ComponentMeta, ComponentStory } from "@storybook/react";
import { JoinGroupInfo, SuperGroupError } from "../../../models/groups";
import { fullscreenLayout, getMockedUser } from "../../../storiesUtils";
import JoinGroup from "./JoinGroup";
import { mockFunc } from "../../../utils/storyMock";

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

export const JoinGroupLinkAlreadyMemberStory = Template.bind({});
JoinGroupLinkAlreadyMemberStory.storyName = 'Already member';
JoinGroupLinkAlreadyMemberStory.args = {
	group: getMockedJoinGroupInfo({
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

export const JoinGroupLinkNoDistributionLinkErrorStory = Template.bind({});
JoinGroupLinkNoDistributionLinkErrorStory.storyName = 'Super group. No distribution link';
JoinGroupLinkNoDistributionLinkErrorStory.args = {
	group: getMockedJoinGroupInfo({
		superGroupError: SuperGroupError.NoDistributionLink
	}),
	courseLink: '#',
	accountLink: '#',
	onJoinGroup: mockFunc
};

export const JoinGroupLinkNoGroupFoundStory = Template.bind({});
JoinGroupLinkNoGroupFoundStory.storyName = 'Super group. No group found for student';
JoinGroupLinkNoGroupFoundStory.args = {
	group: getMockedJoinGroupInfo({
		superGroupError: SuperGroupError.NoGroupFoundForStudent
	}),
	courseLink: '#',
	accountLink: '#',
	onJoinGroup: mockFunc
};


function getMockedJoinGroupInfo(joinGroupInfo?: Partial<JoinGroupInfo>): JoinGroupInfo {
	return {
		id: 0,
		name: 'Мок группа',
		courseId: 'id',
		courseTitle: 'Мок курс',
		owner: getMockedUser({
			id: 'userId',
			firstName: 'Иван',
			lastName: 'Иванов',
			visibleName: 'Иван Иванов'
		}),
		isMember: false,
		isInviteLinkEnabled: true,
		canStudentsSeeProgress: false,
		superGroupError: undefined,
		...joinGroupInfo
	};
}
