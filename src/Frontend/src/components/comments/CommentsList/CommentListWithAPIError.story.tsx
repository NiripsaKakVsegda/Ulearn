import React from "react";
import CommentsList, { Props } from "./CommentsList";
import { getMockedComment, } from "../storiesData";
import { CommentsApi, } from "../utils";
import { CommentPolicy } from "src/models/comments";
import type { Story } from "@storybook/react";
import { SlideType } from "src/models/slide";
import { student } from "src/storiesUtils";

const commentPolicy: CommentPolicy = {
	areCommentsEnabled: true,
	moderationPolicy: "postmoderation",
	onlyInstructorsCanReply: false,
};

const comments = [
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
					avatarUrl:
						"https://staff.skbkontur.ru/content/images/default-user-woman.png",
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

const brokenMethodsCommentsApi: CommentsApi = {
	addComment: () => Promise.reject(new Error("произошла ошибка")),
	deleteComment: () => Promise.reject(new Error("произошла ошибка")),
	updateComment: () => Promise.reject(new Error("произошла ошибка")),
	likeComment: () => Promise.reject(new Error("произошла ошибка")),
	dislikeComment: () => Promise.reject(new Error("произошла ошибка")),
};

export default {
	title: "Comments/CommentsList",
};

const Template: Story<Props> = (args) => {
	return (
		<CommentsList
			{ ...args }
			slideType={ SlideType.Exercise }
			courseId={ "BasicProgramming" }
			slideId={ "90bcb61e-57f0-4baa-8bc9-10c9cfd27f58" }
			commentPolicy={ commentPolicy }
			user={ student }
			comments={ comments }
		/>
	);
};

export const ApiRequestError = Template.bind({});
ApiRequestError.args = {
	api: brokenMethodsCommentsApi,
};
