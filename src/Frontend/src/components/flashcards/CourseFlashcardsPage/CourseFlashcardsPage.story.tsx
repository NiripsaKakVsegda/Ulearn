import React from "react";
import CourseFlashcardsPage from "./CourseFlashcardsPage";
import { flashcardsActions, flashcards } from "../storyData";
import { ComponentMeta, ComponentStory } from "@storybook/react";


const guides = [
	"Подумайте над вопросом, перед тем как смотреть ответ.",
	"Оцените, насколько хорошо вы знали ответ. Карточки, которые вы знаете плохо, будут показываться чаще",
	"Регулярно пересматривайте карточки, даже если вы уверенны в своих знаниях. Чем чаще использовать карточки, тем лучше они запоминаются.",
	"делай так",
	"не делай так",
];

export default {
	title: "Cards/CoursePage",
	component: CourseFlashcardsPage
} as ComponentMeta<typeof CourseFlashcardsPage>;

const Template: ComponentStory<typeof CourseFlashcardsPage> = (args) => (
	<CourseFlashcardsPage { ...args } />
);

export const CourseFlashcardsPageStory = Template.bind({});
CourseFlashcardsPageStory.storyName = "Default";
CourseFlashcardsPageStory.args = {
	courseId: 'basicProgramming',
	userId: 'userId',
	isModerator: false,
	isFlashcardsLoading: false,
	courseFlashcards: flashcards,
	flashcardSlideSlugsByUnitId: {},
	guides: guides,
	flashcardsActions: flashcardsActions,
};
