import React from "react";
import CommentsView, { Props } from "./CommentsView";
import { Comment, CommentPolicy } from "src/models/comments";
import { SlideType } from "src/models/slide";
import { DeviceType } from "src/consts/deviceType";
import {
	fakeFullCommentsApi,
	getMockedComment
} from "../storiesData";
import type { Story } from "@storybook/react";
import { CourseRoleType } from "src/consts/accessType";
import { isInstructor, UserInfo } from "src/utils/courseRoles";
import { FullCommentsApi } from "../utils";
import {
	accessesToSeeProfiles,
	courseAccessesToEditComments,
	courseAccessesToViewSubmissions,
	courseAdmin,
	instructor,
	student,
	sysAdmin,
	unAuthUser
} from "src/storiesUtils";

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
					id: "1",
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
		isApproved: true,
		isPinnedToTop: false,
		isLiked: true,
		likesCount: 8,
		replies: [],
	},
].map(getMockedComment);

const commentPolicy: CommentPolicy = {
	areCommentsEnabled: false,
	moderationPolicy: "postmoderation",
	onlyInstructorsCanReply: false,
};

export default {
	title: "Comments/CommentsView",
};

//adding new field here? add it to testCasesSet below so story would use test class
interface TestCases {
	commentsLoadTimeMs?: number;
	switchUser?: { inMs: number, newRole: CourseRoleType, };
}

const testCasesSet = new Set(['commentsLoadTimeMs', 'switchUser']);

interface TestState {
	comments?: Comment[];
	instructorComments?: Comment[];
	user: UserInfo;
	api: FullCommentsApi;
}

//special test class allowing
// -- imitate comments loading via setTimeouts in api
// -- imitate user role change during loading
// used to track unusual scenarios, such as ULEARN-835
class TestCasesClass extends React.Component<Props & TestCases, TestState> {
	constructor(props: Props & TestCases) {
		super(props);
		const apiWithDelayedCommentsLoad = { ...fakeFullCommentsApi };
		//adding timeouts in api calls with state update. it's looks like redux worker for inner component
		if(props.commentsLoadTimeMs) {
			apiWithDelayedCommentsLoad.getComments = (_, __, forInstructor) => {
				if(forInstructor) {
					return new Promise(resolve => setTimeout(() => {
						resolve(props.instructorComments || []);
						this.setState({
							instructorComments: props.instructorComments || [],
						});
					}, props.commentsLoadTimeMs));
				}
				return new Promise(resolve => setTimeout(() => {
					resolve(props.comments || []);
					this.setState({
						comments: props.comments || [],
					});
				}, props.commentsLoadTimeMs));
			};
		}

		this.state = {
			comments: props.commentsLoadTimeMs === undefined ? props.comments : undefined,
			instructorComments: props.commentsLoadTimeMs === undefined
				? isInstructor(props.user)
					? props.instructorComments
					: undefined
				: undefined,
			user: { ...props.user },
			api: apiWithDelayedCommentsLoad,
		};
	}

	componentDidMount() {
		if(this.props.switchUser) {
			const { inMs, newRole, } = this.props.switchUser;
			setTimeout(() => this.setState({ user: { ...this.state.user, courseRole: newRole, } }), inMs);
		}
	}

	render() {
		return (
			<CommentsView
				{ ...this.props }
				commentsCount={ this.state.comments?.length || 0 }
				instructorCommentsCount={ 0 }
				instructorComments={ this.state.instructorComments }
				deviceType={ DeviceType.desktop }
				isSlideReady={ true }
				slideType={ SlideType.Exercise }
				comments={ this.state.comments }
				user={ this.state.user }
				slideId={ "90bcb61e-57f0-4baa-8bc9-10c9cfd27f58" }
				courseId={ "BasicProgramming" }
				api={ this.state.api }
				commentPolicy={ commentPolicy }
			/>
		);
	}
}

const Template: Story<Props & TestCases> = args => {
	//detecting if extra test args been added, then use test class instead of simple component
	if(Object.keys(args).some(k => testCasesSet.has(k))) {
		return <TestCasesClass { ...args }/>;
	}

	return (
		<CommentsView
			{ ...args }
			commentsCount={ comments.length }
			instructorCommentsCount={ 0 }
			instructorComments={ args.instructorComments || [] }
			deviceType={ DeviceType.desktop }
			isSlideReady={ true }
			slideType={ SlideType.Exercise }
			comments={ args.comments || comments }
			slideId={ "90bcb61e-57f0-4baa-8bc9-10c9cfd27f58" }
			courseId={ "BasicProgramming" }
			api={ fakeFullCommentsApi }
			commentPolicy={ commentPolicy }
		/>
	);
};

export const UnauthorizedUser = Template.bind({});
UnauthorizedUser.args = {
	user: unAuthUser,
};

export const UserIsStudent = Template.bind({});
UserIsStudent.args = {
	user: student,
};

export const UserIsInstructor = Template.bind({});
UserIsInstructor.args = {
	user: instructor,
};

export const UserIsInstructorWithAccessesToSeeProfiles = Template.bind({});
UserIsInstructorWithAccessesToSeeProfiles.args = {
	user: { ...instructor, systemAccesses: accessesToSeeProfiles },
};

export const UserIsInstructorWithModerateAccesses = Template.bind({});
UserIsInstructorWithModerateAccesses.args = {
	user: { ...instructor, courseAccesses: courseAccessesToEditComments },
};

export const UserIsInstructorWithAccessesViewSubmission = Template.bind({});
UserIsInstructorWithAccessesViewSubmission.args = {
	user: { ...instructor, courseAccesses: courseAccessesToViewSubmissions },
};

export const UserIsCourseAdmin = Template.bind({});
UserIsCourseAdmin.args = {
	user: courseAdmin,
};

export const UserIsSysadmin = Template.bind({});
UserIsSysadmin.args = {
	user: sysAdmin,
};

export const LongCommentsLoading = Template.bind({});
LongCommentsLoading.args = {
	user: sysAdmin,
	comments,
	instructorComments: comments.slice(0, comments.length - 1),
	commentsLoadTimeMs: 2500,
};

export const UserFromStudentToInstructor = Template.bind({});
UserFromStudentToInstructor.args = {
	user: student,
	comments,
	instructorComments: comments.slice(0, comments.length - 1),
	commentsLoadTimeMs: 2500,
	switchUser: {
		inMs: 1000,
		newRole: CourseRoleType.instructor,
	},
};

export const UserFromInstructorToStudent = Template.bind({});
UserFromInstructorToStudent.args = {
	user: instructor,
	comments,
	instructorComments: comments.slice(0, comments.length - 1),
	commentsLoadTimeMs: 2500,
	switchUser: {
		inMs: 1000,
		newRole: CourseRoleType.student,
	},
};


