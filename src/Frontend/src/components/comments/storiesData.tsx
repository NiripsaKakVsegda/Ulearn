import { Comment, CommentPolicy, } from "src/models/comments";
import { CommentsApi, FullCommentsApi } from "./utils";
import { ShortUserInfo } from "src/models/users";
import { getMockedShortUser } from "../../storiesUtils";

interface CommentWithPartialAuthor extends Omit<Partial<Comment>, 'author' | 'replies'> {
	author?: Partial<ShortUserInfo>;
	replies?: CommentWithPartialAuthor[];
}

export const policyCommentsPostModeration: CommentPolicy = {
	areCommentsEnabled: true,
	moderationPolicy: "postmoderation",
	onlyInstructorsCanReply: false,
};

export const getMockedComment = (comment: CommentWithPartialAuthor): Comment => {
	return {
		id: comment.id || 100,
		author: getMockedShortUser(comment.author || {}),
		authorGroups: comment.authorGroups || [],
		text: comment.text || 'mocked text',
		renderedText: comment.renderedText || 'mocked text',
		publishTime: comment.publishTime || '2011-05-12',
		isApproved: comment.isApproved || false,
		isCorrectAnswer: comment.isCorrectAnswer || false,
		isPinnedToTop: comment.isPinnedToTop || false,
		isLiked: comment.isLiked || false,
		likesCount: comment.likesCount || 0,
		replies: comment.replies?.map(getMockedComment) || [],
		parentCommentId: comment.parentCommentId,
		isPassed: comment.isApproved || false,
	};
};

export const fakeFullCommentsApi: FullCommentsApi = {
	getComments: () => Promise.resolve(JSON.parse(JSON.stringify([]))),
	addComment: () => Promise.resolve(getMockedComment({})),
	deleteComment: () => Promise.resolve(),
	updateComment: () => Promise.resolve(getMockedComment({})),
	likeComment: () => Promise.resolve(getMockedComment({})),
	dislikeComment: () => Promise.resolve(getMockedComment({})),
	getCommentPolicy: () => Promise.resolve(policyCommentsPostModeration),
};

export const fakeCommentsApi: CommentsApi = {
	addComment: () => Promise.resolve(getMockedComment({})),
	deleteComment: () => Promise.resolve(),
	updateComment: () => Promise.resolve(getMockedComment({})),
	likeComment: () => Promise.resolve(getMockedComment({})),
	dislikeComment: () => Promise.resolve(getMockedComment({})),
};
