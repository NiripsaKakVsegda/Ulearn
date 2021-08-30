import { Comment, CommentPolicy } from "src/models/comments";

export const config = {
	commentHash: 'comment',
};

export interface CommentsApi {
	addComment: (text: string, parentCommentId?: number) => Promise<Comment>;
	deleteComment: (commentId: number,) => Promise<void>;

	likeComment: (commentId: number) => Promise<unknown>;
	dislikeComment: (commentId: number) => Promise<unknown>;

	updateComment: (commentId: number,
		updatedFields?: Pick<Partial<Comment>, 'text' | 'isApproved' | 'isCorrectAnswer' | 'isPinnedToTop'>
	) => Promise<Comment>;
}

export interface FullCommentsApi {
	getCommentPolicy: (courseId: string) => Promise<CommentPolicy>;
	getComments: (courseId: string, slideId: string, forInstructor: boolean) => Promise<Comment[]>;

	addComment: (courseId: string, slideId: string, text: string, forInstructor: boolean,
		parentCommentId?: number
	) => Promise<Comment>;
	deleteComment: (courseId: string, slideId: string, commentId: number, forInstructor: boolean,
	) => Promise<unknown>;

	likeComment: (commentId: number) => Promise<unknown>;
	dislikeComment: (commentId: number) => Promise<unknown>;

	updateComment: (commentId: number,
		updatedFields?: Pick<Partial<Comment>, 'text' | 'isApproved' | 'isCorrectAnswer' | 'isPinnedToTop'>
	) => Promise<Comment>;
}

export const compareComments = (a: Comment, b: Comment, timeByAscending?: boolean): number => {
	if(a.isPinnedToTop && !b.isPinnedToTop) {
		return -1;
	}

	if(!a.isPinnedToTop && b.isPinnedToTop) {
		return 1;
	}
	const aTime = new Date(a.publishTime).getTime();
	const bTime = new Date(b.publishTime).getTime();
	return timeByAscending ? bTime - aTime : aTime - bTime;
};

export const sortComments = (comments: Comment[]): void => {
	comments.sort((a, b) => compareComments(a, b, true));
	comments.forEach(c => c.replies = c.replies.sort(compareComments));
};

export const countAllComments = (comments: Comment[]): number =>
	(comments.length + comments.reduce((pV, c) => pV + c.replies.length, 0));

export const getAllCommentsInRow = <T>(
	comments: Comment[],
	mapper: (comment: Comment) => T
): T[] => (comments.reduce((pV: T[], c) => pV.concat([mapper(c), ...c.replies.map(mapper)]), []));

export const getCommentsIdsInRow = (comments: Comment[]): number[] =>
	(getAllCommentsInRow(comments, (c) => c.id));

export const findIndexOfComment = (commentId: number, comments: Comment[]): number =>
	(getCommentsIdsInRow(comments).indexOf(commentId));

export const getCommentsByCount = (count: number, comments: Comment[]): Comment[] => {
	const resultComments = [];
	let topLevelCommentIndex = 0;

	while (count > 0) {
		if(topLevelCommentIndex >= comments.length) {
			break;
		}

		const comment = { ...comments[topLevelCommentIndex] };

		resultComments.push(comment);
		topLevelCommentIndex++;
		count--;

		if(comment.replies.length > count) {
			comment.replies = comment.replies.slice(0, count);
			break;
		}
		count -= comment.replies.length;
	}

	return resultComments;
};

export const parseCommentIdFromHash = (hash: string, commentHash = config.commentHash): number => {
	if(!hash.includes(commentHash)) {
		return -1;
	}

	const startIndex = hash.indexOf('-') + 1;
	return Number.parseInt(hash.slice(startIndex));
};
