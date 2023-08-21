import { ArrowShapeDRadiusDownRightIcon16Light } from '@skbkontur/icons/ArrowShapeDRadiusDownRightIcon16Light';
import { DocsTextIcon16Light } from '@skbkontur/icons/DocsTextIcon16Light';
import { ToolPencilLineIcon16Light } from '@skbkontur/icons/ToolPencilLineIcon16Light';
import React from "react";
import { Link } from 'react-router-dom';

import { CourseAccessType } from "src/consts/accessType";
import { Comment } from "src/models/comments";
import { SlideType } from "src/models/slide";
import { UserInfo } from "src/utils/courseRoles";
import { Button } from "ui";
import { ActionsType } from "../../CommentsList/CommentsList";

import styles from "./CommentActions.less";


interface ActionButtonProps {
	icon: React.ReactElement;

	onClick: () => void;
	children: React.ReactNode;
}

const ActionButton = ({ onClick, icon, children }: ActionButtonProps) =>
	<Button use="link" onClick={ onClick } icon={ icon }>
		{ children }
	</Button>;

interface ActionLinkProps {
	icon: React.ReactElement;
	url: string;

	children: React.ReactNode;
}

const ActionLink = ({ url, icon, children }: ActionLinkProps) =>
	<Link to={ url } className={styles.linkAsButtonLink}>
		<span className={styles.icon}>{ icon }</span>{ children }
	</Link>;

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

	if (canReply && hasReplyAction) {
		commentActions.push(
			<ActionButton
				key="Ответить"
				onClick={ handleShowReplyFormClick }
				icon={ <ArrowShapeDRadiusDownRightIcon16Light/> }
			>
				Ответить
			</ActionButton>);
	}

	if (user.id === comment.author.id || canModerateComments(user, CourseAccessType.editPinAndRemoveComments)) {
		commentActions.push(
			<div className={ styles.visibleOnDesktopAndTablet } key="Редактировать">
				<ActionButton
					onClick={ handleShowEditFormClick }
					icon={ <ToolPencilLineIcon16Light/> }
				>
					Редактировать
				</ActionButton>
			</div>);
	}

	if (slideType === SlideType.Exercise && canModerateComments(user, CourseAccessType.viewAllStudentsSubmissions)) {
		commentActions.push(
			<div className={ styles.visibleOnDesktopAndTablet } key="Решения">
				<ActionLink
					url={ url }
					icon={ <DocsTextIcon16Light/> }
				>
					Посмотреть решения
				</ActionLink>
			</div>);
	}

	if (commentActions.length === 0) {
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
