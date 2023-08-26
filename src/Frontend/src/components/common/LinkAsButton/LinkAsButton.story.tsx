import LinkAsButton from './LinkAsButton';
import { ComponentMeta, ComponentStory } from "@storybook/react";

export default {
	title: 'Common/LinkAsButton',
	component: LinkAsButton,
	argTypes: {
		href: {
			defaultValue: ''
		},
		children: {
			type: 'string',
			defaultValue: 'Link'
		}
	}
} as ComponentMeta<typeof LinkAsButton>;

const Template: ComponentStory<typeof LinkAsButton> = (args) => (
	<LinkAsButton { ...args } />
);

export const LinkAsButtonStory = Template.bind({});
LinkAsButtonStory.storyName = 'Default';
LinkAsButtonStory.args = {
	href: ''
};

export const LinkAsButtonPrimaryStory = Template.bind({});
LinkAsButtonPrimaryStory.storyName = 'Primary';
LinkAsButtonPrimaryStory.args = {
	use: 'primary'
};

export const LinkAsButtonDisabledStory = Template.bind({});
LinkAsButtonDisabledStory.storyName = 'Disabled';
LinkAsButtonDisabledStory.args = {
	disabled: true
};

export const LinkAsButtonSmallStory = Template.bind({});
LinkAsButtonSmallStory.storyName = 'Small';
LinkAsButtonSmallStory.args = {
	size: 'small'
};

export const LinkAsButtonMediumStory = Template.bind({});
LinkAsButtonMediumStory.storyName = 'Medium';
LinkAsButtonMediumStory.args = {
	size: 'medium'
};

export const LinkAsButtonLargeStory = Template.bind({});
LinkAsButtonLargeStory.storyName = 'Large';
LinkAsButtonLargeStory.args = {
	size: 'large'
};
