import { ComponentMeta, ComponentStory } from "@storybook/react";
import PreviewUnitPageFromAllCourse from "./PreviewUnitPageFromAllCourse";
import { flashcards } from "../storyData";
import { mockFunc } from "../../../utils/storyMock";

export default {
	title: "cards/PreviewUnitPageFromAllCourse",
	component: PreviewUnitPageFromAllCourse
} as ComponentMeta<typeof PreviewUnitPageFromAllCourse>;

const Template: ComponentStory<typeof PreviewUnitPageFromAllCourse> = (args) => (
	<PreviewUnitPageFromAllCourse { ...args } />
);

export const PreviewUnitPageFromAllCourseStory = Template.bind({});
PreviewUnitPageFromAllCourseStory.storyName = "Default";
PreviewUnitPageFromAllCourseStory.args = {
	courseId: 'basicprogramming',
	courseFlashcards: flashcards,
	unitId: flashcards[0].unitId,
	flashcardSlideSlugsByUnitId: {},
	isFlashcardsLoading: false,
	onChangeUnit: mockFunc
};
