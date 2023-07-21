import React from "react";
import Results from "./Results";
import { mockFunc } from "../../../../utils/storyMock";
import { ComponentMeta, ComponentStory } from "@storybook/react";
import { disableViewport } from "../../../course/Navigation/stroies.data";

export default {
	title: "Cards/components/Results",
	component: Results,
	...disableViewport
} as ComponentMeta<typeof Results>;

const Template: ComponentStory<typeof Results> = (args) => (
	<Results { ...args } />
);

export const ResultsStory = Template.bind({});
ResultsStory.storyName = "Default";
ResultsStory.args = {
	onResultClick: mockFunc
};
