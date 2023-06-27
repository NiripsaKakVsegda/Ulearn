import React, { FC } from 'react';
import { ShortUserInfo } from "../../../../../../../../models/users";
import { DropdownMenu, MenuItem, MenuSeparator } from "ui";
import styles from "./ReviewControlsKebab.less";
import { Edit, MenuKebab, Star, Star2, Trash } from "icons";
import texts from "./ReviewControlsKebab.texts";
import { isInstructor, UserInfo } from "../../../../../../../../utils/courseRoles";

interface Props {
	user?: UserInfo;
	id: number;
	author: ShortUserInfo;
	content: string;
	commentId?: number;
	isFavourite?: boolean;

	onToggleReviewFavourite?: () => void;
	onStartEditingReviewOrComment: (value: string, commentId?: number) => void;
	onDeleteReviewOrComment: (commentId?: number) => void;
}

const ReviewControlsKebab: FC<Props> = ({
	user,
	id,
	author,
	content,
	commentId,
	isFavourite,
	...actions
}) => {
	return <DropdownMenu
		className={ styles.kebabMenu }
		caption={ <MenuKebab className={ styles.kebabMenuIcon } size={ 18 }/> }
		positions={ ["left top"] }
		menuWidth={ 216 }
	>
		{ (actions.onToggleReviewFavourite && !commentId) && [
			<MenuItem onClick={ toggleReviewToFavourite } key={ 'toggleFavourite' }>
				{ isFavourite
					? <Star color={ '#F69C00' }/>
					: <Star2/>
				}
				{ ' ' + texts.getToggleFavouriteMarkup(isFavourite ?? false) }
			</MenuItem>,
			<MenuSeparator key={ "separator" }/>
		] }
		{ author.id === user?.id &&
			<MenuItem
				onClick={ startEditingComment }
				data-id={ id }
				data-commentid={ commentId }
				data-text={ content }
			>
				<Edit/>
				{ ' ' + texts.editButton }
			</MenuItem>
		}
		{ (author.id === user?.id || user && isInstructor(user)) &&
			<MenuItem
				data-id={ id }
				data-commentid={ commentId }
				onClick={ deleteReviewOrComment }
			>
				<Trash/>
				{ ' ' + texts.deleteButton }
			</MenuItem>
		}
	</DropdownMenu>;

	function toggleReviewToFavourite(event: React.MouseEvent | React.SyntheticEvent): void {
		if(!actions.onToggleReviewFavourite) {
			return;
		}

		actions.onToggleReviewFavourite();

		event.preventDefault();
		event.stopPropagation();
	}

	function startEditingComment(event: React.MouseEvent | React.SyntheticEvent) {
		const commentId = getCommentId(event);
		const text = (event.currentTarget as HTMLElement).dataset.text ?? "";

		actions.onStartEditingReviewOrComment(text, commentId);
	}

	function deleteReviewOrComment(event: React.MouseEvent | React.SyntheticEvent) {
		const commentId = getCommentId(event);

		actions.onDeleteReviewOrComment(commentId);
	}

	function getCommentId(event: React.MouseEvent | React.SyntheticEvent): number | undefined {
		const { commentid } = (event.currentTarget as HTMLElement).dataset;

		return commentid ? parseInt(commentid) : undefined;

	}
};

export default ReviewControlsKebab;
