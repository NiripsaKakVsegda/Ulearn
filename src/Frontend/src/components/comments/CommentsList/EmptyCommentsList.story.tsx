import React from "react";
import CommentsList, { Props } from "./CommentsList";
import { CommentPolicy } from "src/models/comments";
import { Story } from "@storybook/react";
import { SlideType } from "src/models/slide";
import { fakeCommentsApi } from "../storiesData";
import { student, unAuthUser } from "src/storiesUtils";


const commentPolicy: CommentPolicy = {
	areCommentsEnabled: true,
	moderationPolicy: "postmoderation",
	onlyInstructorsCanReply: false,
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
			api={ fakeCommentsApi }
			comments={ [] }
		/>
	);
};


export const EmptyCommentsListForUnauthorizedUser = Template.bind({});
EmptyCommentsListForUnauthorizedUser.args = {
	user: unAuthUser,
};

export const EmptyCommentsListForStudent = Template.bind({});
EmptyCommentsListForStudent.args = {
	user: student,
};
