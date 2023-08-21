import { ShortUserInfo } from "src/models/users";

export interface SlideComments {
	topLevelComments: Comment[];
	pagination: Pagination;
}

export type SlideCommentsResponse = SlideComments & Response;

export interface Pagination {
	offset: number;
	count: number;
	totalCount: number;
}

export interface ShortGroupInfo {
	id: number;
	courseId: string;
	name: string;
	isArchived: boolean;
	apiUrl: string;
	owner: ShortUserInfo;
	membersCount: number;
}

export interface Comment {
	id: number;
	author: ShortUserInfo;
	authorGroups: ShortGroupInfo[] | null;
	text: string;
	renderedText: string;
	publishTime: string;
	isApproved: boolean;
	isCorrectAnswer?: boolean;
	isPinnedToTop?: boolean;
	isLiked: boolean;
	likesCount: number;
	replies: Comment[];
	parentCommentId?: number;
	isPassed: boolean;
}

export interface CommentPolicy {
	areCommentsEnabled: boolean;
	moderationPolicy: string;
	onlyInstructorsCanReply: boolean;
}

export interface CommentPolicyResponse extends Response {
	areCommentsEnabled: boolean;
	moderationPolicy: string;
	onlyInstructorsCanReply: boolean;
}
