import React, { useState } from "react";
import CommentsList, { Props } from "./CommentsList";
import { SlideType } from "src/models/slide";
import {
	fakeCommentsApi,
	getMockedComment,
	policyCommentsPostModeration
} from "../storiesData";
import { Comment } from "src/models/comments";
import type { Story } from "@storybook/react";
import { student } from "src/storiesUtils";

const comments: Comment[] = [
	{
		id: 1999,
		text:
			"Решать эти задачи **можно** прямо в браузере, а специальная проверяющая система тут же проверит ваше решение.",
		renderedText:
			"Решать эти задачи <b>можно</b> прямо в браузере, а специальная проверяющая система тут же проверит ваше решение.",
		author: {
			id: "11",
			visibleName: "Louisa",
		},
		publishTime: "2019-01-18T14:12:41.947",
		isApproved: false,
		isPinnedToTop: false,
		isLiked: true,
		likesCount: 10,
		replies: [
			{
				id: 2000,
				author: {
					id: "10",
					visibleName: "Maria",
					avatarUrl: "https://staff.skbkontur.ru/content/images/default-user-woman.png",
				},
				text: "Я **не согласна**",
				replies: [],
				renderedText: "Я <b>не согласна</b>",
				publishTime: "2019-02-18T14:12:41.947",
				isApproved: true,
				isCorrectAnswer: false,
				likesCount: 0,
				isLiked: false,
				parentCommentId: 1999,
			},
			{
				id: 2001,
				author: {
					id: "11",
					visibleName: "Kate",
					avatarUrl:
						"https://staff.skbkontur.ru/content/images/default-user-woman.png",
				},
				text: "Я **согласна**",
				replies: [],
				renderedText: "Я <b>согласна</b>",
				publishTime: "2019-02-18T14:12:41.947",
				isApproved: false,
				isCorrectAnswer: true,
				likesCount: 5,
				isLiked: true,
				parentCommentId: 1999,
			},
		],
	},
	{
		id: 2002,
		text:
			"Решать эти задачи **можно** прямо в браузере, а специальная проверяющая система тут же проверит ваше решение.",
		renderedText:
			"Решать эти задачи <b>можно</b> прямо в браузере, а специальная проверяющая система тут же проверит ваше решение.",
		author: {
			id: "13",
			visibleName: "Henry",
		},
		publishTime: "2019-01-18T14:12:41.947",
		isApproved: false,
		isPinnedToTop: false,
		isLiked: true,
		likesCount: 8,
		replies: [],
	},
].map(getMockedComment);

export default {
	title: "Comments/CommentsList",
};

const Template: Story<Omit<Props, 'headerRef' | 'api'>> = (args) => {
	const ref = React.createRef<HTMLDivElement>();
	return (
		<>
			<div ref={ ref }/>
			<CommentsList
				{ ...args }
				headerRef={ ref }
				api={ fakeCommentsApi }
			/>
		</>
	);
};

export const Default = Template.bind({});
Default.args = {
	handleTabChange: () => ({}),
	isSlideContainsComment: (commentId) => comments.find(c => c.id === commentId) !== undefined,
	slideType: SlideType.Exercise,
	comments,
	user: student,
	courseId: "BasicProgramming",
	slideId: "90bcb61e-57f0-4baa-8bc9-10c9cfd27f58",
	commentPolicy: policyCommentsPostModeration,
};
