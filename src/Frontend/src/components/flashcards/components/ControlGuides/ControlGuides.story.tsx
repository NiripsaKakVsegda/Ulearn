import { ComponentMeta, ComponentStory } from "@storybook/react";
import ControlGuides from "./ControlGuides";

export default {
	title: "cards/components/ControlGuides",
	component: ControlGuides
} as ComponentMeta<typeof ControlGuides>;

const Template: ComponentStory<typeof ControlGuides> = (args) => (
	<ControlGuides { ...args } />
);

export const ControlGuidesStory = Template.bind({});
ControlGuidesStory.storyName = "Default";
ControlGuidesStory.args = {};
