import React from "react";
import ProgressBar from "./ProgressBar";
import { ComponentMeta, ComponentStory } from "@storybook/react";

export default {
	title: "Cards/components/ProgressBar",
	component: ProgressBar
} as ComponentMeta<typeof ProgressBar>;
const Template: ComponentStory<typeof ProgressBar> = (args) => (
	<ProgressBar { ...args } />
);
export const ProgressBar222222Story = Template.bind({});
ProgressBar222222Story.storyName = "Scores 2 2 2 2 2 2";
ProgressBar222222Story.args = {
	statistics: {
		notRated: 2,
		rate1: 2,
		rate2: 2,
		rate3: 2,
		rate4: 2,
		rate5: 2,
	},
	totalFlashcardsCount: 12
};

export const ProgressBar11251018Story = Template.bind({});
ProgressBar11251018Story.storyName = "Scores 1 1 2 5 10 18";
ProgressBar11251018Story.args = {
	statistics: {
		notRated: 1,
		rate1: 1,
		rate2: 2,
		rate3: 5,
		rate4: 10,
		rate5: 18,
	},
	totalFlashcardsCount: 37
};

export const ProgressBar1000000Story = Template.bind({});
ProgressBar1000000Story.storyName = "Scores 10 0 0 0 0 0";
ProgressBar1000000Story.args = {
	statistics: {
		notRated: 10,
		rate1: 0,
		rate2: 0,
		rate3: 0,
		rate4: 0,
		rate5: 0,
	},
	totalFlashcardsCount: 10
};
