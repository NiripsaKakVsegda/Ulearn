import React from "react";
import Thread, { Props } from "./Thread";
import { Comment as CommentType, CommentPolicy } from "src/models/comments";
import { getMockedComment } from "../storiesData";
import { SlideType } from "src/models/slide";
import type { Story } from "@storybook/react";
import { sysAdmin } from "src/storiesUtils";

const actions = {
	handleLikeClick: () => console.log("likeComment"),
	handleCorrectAnswerMark: () => console.log("correctMark"),
	handleApprovedMark: () => console.log("approvedMark"),
	handlePinnedToTopMark: () => console.log("pinnedToTopMark"),
	handleEditComment: () => console.log("editComment"),
	handleAddReplyComment: () => console.log("addReplyComment"),
	handleDeleteComment: () => console.log("deleteComment"),
	handleShowEditForm: () => console.log("showEditForm"),
	handleShowReplyForm: () => console.log("showReplyForm"),
};

const commentEditing = {
	commentId: null,
	sending: false,
};

const reply = {
	commentId: null,
	sending: false,
};

const replies = [];

for (let i = 0; i < 5; i++) {
	replies.push({
		id: 2 + i,
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
	});
}

const commentPolicy: CommentPolicy = {
	areCommentsEnabled: true,
	moderationPolicy: "postmoderation",
	onlyInstructorsCanReply: false,
};

const comment: CommentType = getMockedComment({
	id: 1,
	text:
		"Решать эти задачи **можно** прямо в браузере, а специальная проверяющая система тут же проверит ваше решение.",
	renderedText:
		"Решать эти задачи <b>можно</b> прямо в браузере, а специальная проверяющая система тут же проверит ваше решение.",
	author: {
		id: "11",
		visibleName: "Louisa",
		firstName: "Louisa",
		lastName: '',
	},
	publishTime: "2019-01-18T14:12:41.947",
	isApproved: false,
	isPinnedToTop: false,
	isLiked: true,
	likesCount: 10,
	replies: replies,
});


const Template: Story<Props> = (args) => (
	<Thread { ...args }/>
);

export const CommentWithReplies = Template.bind({});
CommentWithReplies.args = {
	slideType: SlideType.Exercise,
	slideId: 'slideId',
	courseId: 'courseId',
	isSlideReady: true,
	animation: true,
	commentPolicy: commentPolicy,
	comment: comment,
	user: sysAdmin,
	commentEditing: commentEditing,
	reply: reply,
	actions: actions,
};

export default {
	title: "Comments/Thread",
	component: CommentWithReplies,
};
