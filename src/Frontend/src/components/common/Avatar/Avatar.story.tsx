import { ComponentMeta, ComponentStory } from '@storybook/react';
import { botId } from '../../../consts/common';
import { getMockedUser } from '../../../storiesUtils';
import Avatar from './Avatar';

export default {
	title: 'Common/Avatar',
	component: Avatar
} as ComponentMeta<typeof Avatar>;

const Template: ComponentStory<typeof Avatar> = (args) => (
	<Avatar { ...args } />
);

export const AvatarStory = Template.bind({});
AvatarStory.storyName = 'Default';
AvatarStory.args = {
	user: getMockedUser({
		id: 'id',
		visibleName: 'A'
	}),
	size: 'big'
};

export const AvatarImageStory = Template.bind({});
AvatarImageStory.storyName = 'Image';
AvatarImageStory.args = {
	user: getMockedUser({
		id: 'id',
		visibleName: 'A',
		avatarUrl: 'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTDJzEaxLN-jGRYYUO65pWu7Q9GXoNt4LUSSA'
	}),
	size: 'big'
};

export const AvatarBotStory = Template.bind({});
AvatarBotStory.storyName = 'Bot';
AvatarBotStory.args = {
	user: getMockedUser({
		id: botId,
	}),
	size: 'big'
};

export const AvatarInvalidNameStory = Template.bind({});
AvatarInvalidNameStory.storyName = 'Invalid name';
AvatarInvalidNameStory.args = {
	user: getMockedUser({
		id: 'id',
		visibleName: '165.{}'
	}),
	size: 'big'
};
