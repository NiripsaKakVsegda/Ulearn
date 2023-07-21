import React from "react";
import Guides from "./Guides";
import { guides } from "../../storyData";
import { ComponentMeta, ComponentStory } from "@storybook/react";

export default {
	title: "cards/components/Guides",
	component: Guides
} as ComponentMeta<typeof Guides>;
const Template: ComponentStory<typeof Guides> = (args) => (
	<Guides { ...args } />
);
export const GuidesStory = Template.bind({});
GuidesStory.storyName = "Default";
GuidesStory.args = {
	guides: guides
};
