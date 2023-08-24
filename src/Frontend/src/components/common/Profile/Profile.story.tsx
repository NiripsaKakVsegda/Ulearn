import { ComponentMeta, ComponentStory } from '@storybook/react';
import { getMockedUser } from '../../../storiesUtils';
import Profile from './Profile';

export default {
	title: 'Common/Profile',
	component: Profile
} as ComponentMeta<typeof Profile>;

const Template: ComponentStory<typeof Profile> = (args) => (
	<Profile { ...args } />
);

export const ProfileStory = Template.bind({});
ProfileStory.storyName = 'Default';
ProfileStory.args = {
	user: getMockedUser({
		id: 'id',
		firstName: 'Иван',
		lastName: 'Иванов',
		visibleName: 'Иван Иванов',
		avatarUrl: 'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTDJzEaxLN-jGRYYUO65pWu7Q9GXoNt4LUSSA'
	}),
	withAvatar: false,
	canViewProfiles: false,
	showLastNameFirst: false,
};

export const ProfileLastNameFirstStory = Template.bind({});
ProfileLastNameFirstStory.storyName = 'Last name first';
ProfileLastNameFirstStory.args = {
	user: getMockedUser({
		id: 'id',
		firstName: 'Иван',
		lastName: 'Иванов',
		visibleName: 'Иван Иванов',
		avatarUrl: 'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTDJzEaxLN-jGRYYUO65pWu7Q9GXoNt4LUSSA'
	}),
	withAvatar: false,
	canViewProfiles: false,
	showLastNameFirst: true,
};

export const ProfileCanViewProfilesStory = Template.bind({});
ProfileCanViewProfilesStory.storyName = 'Can view profiles';
ProfileCanViewProfilesStory.args = {
	user: getMockedUser({
		id: 'id',
		firstName: 'Иван',
		lastName: 'Иванов',
		visibleName: 'Иван Иванов',
		avatarUrl: 'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTDJzEaxLN-jGRYYUO65pWu7Q9GXoNt4LUSSA'
	}),
	withAvatar: false,
	canViewProfiles: true,
	showLastNameFirst: false,
};

export const ProfileWithAvatarStory = Template.bind({});
ProfileWithAvatarStory.storyName = 'With avatar';
ProfileWithAvatarStory.args = {
	user: getMockedUser({
		id: 'id',
		firstName: 'Иван',
		lastName: 'Иванов',
		visibleName: 'Иван Иванов',
		avatarUrl: 'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTDJzEaxLN-jGRYYUO65pWu7Q9GXoNt4LUSSA'
	}),
	withAvatar: true,
	canViewProfiles: false,
	showLastNameFirst: false,
};
