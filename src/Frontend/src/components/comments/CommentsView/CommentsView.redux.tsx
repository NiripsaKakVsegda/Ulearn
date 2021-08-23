import { connect } from "react-redux";

import {
	addComment, deleteComment,
	dislikeComment,
	getCommentPolicy,
	getComments,
	likeComment,
	updateComment
} from "src/api/comments";

import CommentsView from "./CommentsView";

import { sortComments } from "../utils";

import { RootState } from "src/redux/reducers";
import { Dispatch } from "redux";
import { Comment } from "src/models/comments";
import { CommentRedux } from "src/redux/comments";


const mapStateToProps = (state: RootState, { courseId, slideId }: { courseId: string, slideId: string, }) => {
	const slideCommentsIds = state.comments.byCourses[courseId]?.bySlides?.[slideId];
	const byIds = state.comments.byIds;
	let comments = undefined;
	let instructorComments = undefined;

	const convertReduxCommentToComment = ({ repliesIds, ...rest }: CommentRedux) => ({
		...rest,
		replies: repliesIds?.map(id => byIds[id]) || []
	} as Comment);

	const filterDeletedAndSort = (commentsIds: number[],) => {
		const comments:Comment[] = [];
		let repliesCount = 0;

		for (const commentId of commentsIds) {
			const comment = { ...byIds[commentId] };
			if(!comment.isDeleted) {
				comment.repliesIds = comment.repliesIds?.filter(replyId => !byIds[replyId].isDeleted);
				repliesCount += comment.repliesIds?.length || 0;
				comments.push(convertReduxCommentToComment(comment));
			}
		}

		sortComments(comments);

		return { comments, count: comments.length + repliesCount };
	};

	if(slideCommentsIds) {
		if(slideCommentsIds.commentsIds) {
			comments = filterDeletedAndSort(slideCommentsIds.commentsIds);
		}

		if(slideCommentsIds.instructorCommentsIds) {
			instructorComments = filterDeletedAndSort(slideCommentsIds.instructorCommentsIds);
		}
	}

	return {
		deviceType: state.device.deviceType,
		comments: comments?.comments,
		commentsCount: comments?.count || 0,
		instructorComments: instructorComments?.comments,
		instructorCommentsCount: instructorComments?.count || 0,
		commentPolicy: state.comments.byCourses[courseId]?.policy,
	};
};

const mapDispatchToProps = (dispatch: Dispatch) => ({
	api: {
		getComments: (courseId: string, slideId: string, forInstructor: boolean) =>
			getComments(courseId, slideId, forInstructor)(dispatch).then(c => c.comments),
		getCommentPolicy: (courseId: string) => getCommentPolicy(courseId)(dispatch).then(c => c.policy),

		addComment: (courseId: string, slideId: string, text: string, forInstructor: boolean,
			parentCommentId?: number
		) =>
			addComment(courseId, slideId, text, parentCommentId, forInstructor)(dispatch).then(c => c.comment),
		deleteComment: (courseId: string, slideId: string, commentId: number, forInstructor: boolean,) =>
			deleteComment(courseId, slideId, commentId, forInstructor)(dispatch),

		likeComment: (commentId: number) => likeComment(commentId)(dispatch),
		dislikeComment: (commentId: number) => dislikeComment(commentId)(dispatch),

		updateComment: (commentId: number,
			updatedFields?: Pick<Partial<Comment>, 'text' | 'isApproved' | 'isCorrectAnswer' | 'isPinnedToTop'>
		) => updateComment(commentId, updatedFields)(dispatch).then(c => c.comment),
	}
});

export default connect(
	mapStateToProps,
	mapDispatchToProps,
)(CommentsView);
