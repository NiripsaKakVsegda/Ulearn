import React from "react";
import { TransitionGroup, CSSTransition } from "react-transition-group";
import Comment from "../Comment/Comment";
import CommentSendForm from "../CommentSendForm/CommentSendForm";

import { UserInfo, } from "src/utils/courseRoles";
import { CommentStatus } from "src/consts/comments";
import { SlideType } from "src/models/slide";
import { Comment as CommentType, CommentPolicy } from "src/models/comments";
import { ActionsType } from "../CommentsList/CommentsList";

import styles from "./Thread.less";


export interface Props {
	courseId: string;
	slideId: string;
	slideType: SlideType;

	user: UserInfo;

	comment: CommentType;
	reply: CommentStatus;
	commentEditing: CommentStatus;
	commentPolicy: CommentPolicy | null;

	animation: boolean;
	isSlideReady: boolean;

	actions: ActionsType;
}

function Thread({
	comment, user, reply, commentEditing, actions, slideType,
	commentPolicy, courseId, isSlideReady, animation, slideId,
}: Props): React.ReactElement {
	const replies = comment.replies || [];
	return renderComment(comment, replies.length === 0);

	function renderComment(comment: CommentType, isLastChild: boolean) {
		const isLastCommentInThread = isLastChild;
		const isParentComment = !comment.parentCommentId;
		const focusedReplyForm = { inReplyForm: isParentComment && comment.id === reply.commentId, };

		return (
			<Comment
				key={ comment.id }
				comment={ comment }
				hasReplyAction={ isLastCommentInThread }
				commentEditing={ commentEditing }
				commentPolicy={ commentPolicy }
				actions={ actions }
				slideType={ slideType }
				courseId={ courseId }
				slideId={ slideId }
				user={ user }
				isSlideReady={ isSlideReady }>
				{ comment.replies.length > 0 && renderReplies(comment) }
				{ (isParentComment && comment.id === reply.commentId) &&
				<CommentSendForm
					className={ styles.replyForm }
					isShowFocus={ focusedReplyForm }
					sending={ reply.sending }
					author={ user }
					submitTitle="Отправить"
					handleCancel={ handleShowReplyFormClick }
					handleSubmit={ handleAddReply }/> }
			</Comment>
		);
	}

	function handleAddReply(text: string) {
		actions.handleAddReplyComment(comment.id, text);
	}

	function handleShowReplyFormClick() {
		actions.handleShowReplyForm(null);
	}

	function renderReplies(comment: CommentType) {
		const replies = comment.replies || [];
		const transitionStyles = {
			enter: styles.enter,
			exit: styles.exit,
			enterActive: styles.enterActive,
			exitActive: styles.exitActive,
		};

		const duration = {
			enter: 1000,
			exit: 500,
		};

		return (
			<div className={ styles.repliesWrapper }>
				<TransitionGroup enter={ animation }>
					{ replies.map((reply, index) =>
						<CSSTransition
							key={ reply.id }
							mountOnEnter
							unmountOnExit
							in={ animation }
							classNames={ transitionStyles }
							timeout={ duration }>
							<div key={ reply.id } className={ styles.reply }>
								{ renderComment(reply, index + 1 === replies.length) }
							</div>
						</CSSTransition>
					) }
				</TransitionGroup>
			</div>
		);
	}
}

export default Thread;

