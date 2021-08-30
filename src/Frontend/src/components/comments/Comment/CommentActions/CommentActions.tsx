import React from "react";
import { Button, } from "ui";
import { Link } from 'react-router-dom';
import { ArrowCorner1, Edit, DocumentLite } from "icons";

import { CourseAccessType, } from "src/consts/accessType";
import { SlideType, } from "src/models/slide";
import { Comment } from "src/models/comments";
import { ActionsType } from "../../CommentsList/CommentsList";
import { UserInfo, } from "src/utils/courseRoles";

import styles from "./CommentActions.less";


interface ActionButtonProps {
	icon: React.ReactElement;

	onClick: () => void;
	children: React.ReactNode;
}

const ActionButton = ({ onClick, icon, children }: ActionButtonProps) => (
	<div className={ styles.action }>
		<Button use="link" onClick={ onClick } icon={ icon }>
			{ children }
		</Button>
	</div>
);

interface ActionLinkProps {
	icon: React.ReactElement;
	url: string;

	children: React.ReactNode;
}

const ActionLink = ({ url, icon, children }: ActionLinkProps) => (
	<div className={ styles.action }>
		<Link to={ url }>
			{ icon }{ children }
		</Link>
	</div>
);

interface Props {
	url: string;
	slideType: SlideType;

	user: UserInfo;

	comment: Comment;

	hasReplyAction: boolean;
	canReply: boolean;

	actions: ActionsType;
	canModerateComments: (user: UserInfo, access: CourseAccessType) => boolean;
}

export default function CommentActions(props: Props): React.ReactElement | null {
	const {
		user, comment, url, hasReplyAction, canModerateComments,
		actions, slideType, canReply
	} = props;

	const commentActions: React.ReactElement[] = [];

	if(canReply && hasReplyAction) {
		commentActions.push(
			<ActionButton
				key="Ответить"
				onClick={ handleShowReplyFormClick }
				icon={ <ArrowCorner1/> }>
				Ответить
			</ActionButton>);
	}

	if(user.id === comment.author.id || canModerateComments(user, CourseAccessType.editPinAndRemoveComments)) {
		commentActions.push(
			<div className={ styles.visibleOnDesktopAndTablet } key="Редактировать">
				<ActionButton
					onClick={ handleShowEditFormClick }
					icon={ <Edit/> }>
					Редактировать
				</ActionButton>
			</div>);
	}

	if(slideType === SlideType.Exercise && canModerateComments(user, CourseAccessType.viewAllStudentsSubmissions)) {
		commentActions.push(
			<div className={ styles.visibleOnDesktopAndTablet } key="Решения">
				<ActionLink
					url={ url }
					icon={ <DocumentLite/> }>
					Посмотреть решения
				</ActionLink>
			</div>);
	}

	if(commentActions.length === 0) {
		return null;
	}

	return (
		<div className={ styles.actions }>
			{ commentActions }
		</div>
	);

	function handleShowReplyFormClick() {
		const commentId = comment.parentCommentId ? comment.parentCommentId : comment.id;

		actions.handleShowReplyForm(commentId);
	}

	function handleShowEditFormClick() {
		actions.handleShowEditForm(comment.id);
	}
}
