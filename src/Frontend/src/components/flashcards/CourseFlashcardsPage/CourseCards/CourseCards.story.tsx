import React from "react";
import CourseCards from "./CourseCards";
import { infoByUnits } from "../../storyData";
import { ComponentMeta, ComponentStory } from "@storybook/react";

export default {
	title: "Cards/CoursePage/CourseCards",
	component: CourseCards
} as ComponentMeta<typeof CourseCards>;

const Template: ComponentStory<typeof CourseCards> = (args) => (
	<CourseCards { ...args } />
);

export const CourseCardsStory = Template.bind({});
CourseCardsStory.storyName = "Default";
CourseCardsStory.args = {
	courseId: 'basicprogramming',
	infoByUnits: infoByUnits
};
