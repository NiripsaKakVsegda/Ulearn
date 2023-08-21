import { AttachPinIcon16Regular } from "@skbkontur/icons/AttachPinIcon16Regular";
import { CheckCircleIcon16Regular } from "@skbkontur/icons/CheckCircleIcon16Regular";
import { DocsTextIcon16Light } from '@skbkontur/icons/DocsTextIcon16Light';
import { EyeOffIcon16Regular } from "@skbkontur/icons/EyeOffIcon16Regular";
import { EyeOpenIcon16Regular } from "@skbkontur/icons/EyeOpenIcon16Regular";
import { ToolPencilIcon16Regular } from "@skbkontur/icons/ToolPencilIcon16Regular";
import { TrashCanIcon16Regular } from "@skbkontur/icons/TrashCanIcon16Regular";
import React from "react";
import { CourseAccessType } from "src/consts/accessType";
import { Comment } from "src/models/comments";

import { SlideType } from "src/models/slide";
import { UserInfo } from "src/utils/courseRoles";
import { Kebab, MenuItem } from "ui";
import { ActionsType } from "../../CommentsList/CommentsList";

import styles from "./KebabActions.less";


interface Props {
	url: string;
	slideType: SlideType;

	user: UserInfo;

	comment: Comment;

	actions: ActionsType;
	canModerateComments: (user: UserInfo, access: CourseAccessType) => boolean;
}

export default function KebabActions(props: Props): React.ReactElement {
	const { user, comment, url, canModerateComments, actions, slideType } = props;
	const canModerate = canModerateComments(user, CourseAccessType.editPinAndRemoveComments);
	const canDeleteAndEdit = (user.id === comment.author.id || canModerate);
	const canSeeSubmissions = (slideType === SlideType.Exercise &&
							   canModerateComments(user, CourseAccessType.viewAllStudentsSubmissions));

	return (
		<div className={ styles.instructorsActions }>
			<Kebab positions={ ["bottom right"] } size="large" disableAnimations={ true }>
				{ canModerate &&
				  <MenuItem
					  data-id={ comment.id }
					  data-approved={ comment.isApproved }
					  icon={ !comment.isApproved ? <EyeOpenIcon16Regular/> : <EyeOffIcon16Regular/> }
					  onClick={ handleApprovedMarkClick }
				  >
					  { !comment.isApproved ? "Опубликовать" : "Скрыть" }
				  </MenuItem> }
				{ canDeleteAndEdit &&
				  <MenuItem
					  icon={ <TrashCanIcon16Regular/> }
					  onClick={ handleDeleteCommentClick }
				  >
					  Удалить
				  </MenuItem> }
				{ (canModerate && !comment.parentCommentId) &&
				  <MenuItem
					  onClick={ handlePinnedToTopMarkClick }
					  icon={ <AttachPinIcon16Regular/> }
				  >
					  { comment.isPinnedToTop ? "Открепить" : "Закрепить" }
				  </MenuItem> }
				{ canDeleteAndEdit &&
				  <MenuItem
					  icon={ <ToolPencilIcon16Regular/> }
					  onClick={ handleShowEditFormClick }
				  >
					  Редактировать
				  </MenuItem> }
				{ canSeeSubmissions &&
				  <div className={ styles.visibleOnPhone }>
					  <MenuItem
						  href={ url }
						  icon={ <DocsTextIcon16Light/> }
					  >
						  Посмотеть решения
					  </MenuItem>
				  </div> }
				{ (canModerate && comment.parentCommentId) &&
				  <MenuItem
					  onClick={ handleCorrectAnswerMarkClick }
					  icon={ <CheckCircleIcon16Regular/> }
				  >
					  { comment.isCorrectAnswer ? "Снять отметку" : "Отметить правильным" }
				  </MenuItem> }
			</Kebab>
		</div>
	);

	function handleApprovedMarkClick(): void {
		actions.handleApprovedMark(comment.id, !comment.isApproved);
	}

	function handleDeleteCommentClick() {
		actions.handleDeleteComment(comment.id);
	}

	function handlePinnedToTopMarkClick() {
		actions.handlePinnedToTopMark(comment.id, !comment.isPinnedToTop);
	}

	function handleShowEditFormClick() {
		actions.handleShowEditForm(comment.id);
	}

	function handleCorrectAnswerMarkClick() {
		actions.handleCorrectAnswerMark(comment.id, !comment.isCorrectAnswer);
	}
}
