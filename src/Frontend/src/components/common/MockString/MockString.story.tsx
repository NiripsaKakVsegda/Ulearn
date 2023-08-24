import { ComponentMeta, ComponentStory } from '@storybook/react';
import MockString from './MockString';

export default {
	title: 'Common/MockString',
	component: MockString,
	argTypes: {
		children: {
			type: 'string'
		},
		opacity: {
			defaultValue: 0.1,
			control: {
				type: 'range',
				min: 0,
				max: 1,
				step: 0.01
			}
		}
	}
} as ComponentMeta<typeof MockString>;

const Template: ComponentStory<typeof MockString> = (args) => (
	<MockString { ...args } />
);

export const MockStringStory = Template.bind({});
MockStringStory.storyName = 'Default';
MockStringStory.args = {};

export const MockStringPatternStory = Template.bind({});
MockStringPatternStory.storyName = 'Pattern';
MockStringPatternStory.args = {
	children: 'Hello World!   123\n' +
			  '\n' +
			  '      abc',
	length: undefined
};

export const MockStringLengthStory = Template.bind({});
MockStringLengthStory.storyName = 'Length';
MockStringLengthStory.args = {
	length: 15
};
