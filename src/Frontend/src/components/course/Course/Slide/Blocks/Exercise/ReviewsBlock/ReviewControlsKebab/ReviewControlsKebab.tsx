import { StarIcon16Regular } from "@skbkontur/icons/StarIcon16Regular";
import { StarIcon16Solid } from "@skbkontur/icons/StarIcon16Solid";
import { ToolPencilLineIcon16Regular } from "@skbkontur/icons/ToolPencilLineIcon16Regular";
import { TrashCanIcon16Regular } from "@skbkontur/icons/TrashCanIcon16Regular";
import React, { FC } from 'react';
import { Kebab, MenuItem, MenuSeparator } from "ui";
import { ShortUserInfo } from "../../../../../../../../models/users";
import { isInstructor, UserInfo } from "../../../../../../../../utils/courseRoles";
import styles from "./reviewControlsKebab.less";
import texts from "./ReviewControlsKebab.texts";

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
	return <Kebab
		className={ styles.kebabMenu }
		size={ 'medium' }
		positions={ ["left top"] }
	>
		{ (actions.onToggleReviewFavourite && !commentId) && [
			<MenuItem
				onClick={ toggleReviewToFavourite }
				key={ 'toggleFavourite' }
				icon={ isFavourite
					? <StarIcon16Solid color={ '#F69C00' }/>
					: <StarIcon16Regular color={ '#000' }/>
				}
			>
				<span className={ styles.toggleFavouritesText }>
					{ texts.getToggleFavouriteMarkup(isFavourite ?? false) }
				</span>
			</MenuItem>,
			<MenuSeparator key={ "separator" }/>
		] }
		{ author.id === user?.id &&
			<MenuItem
				onClick={ startEditingComment }
				data-id={ id }
				data-commentid={ commentId }
				data-text={ content }
				icon={ <ToolPencilLineIcon16Regular/> }
				children={ texts.editButton }
			/>
		}
		{ (author.id === user?.id || user && isInstructor(user)) &&
			<MenuItem
				data-id={ id }
				data-commentid={ commentId }
				onClick={ deleteReviewOrComment }
				icon={ <TrashCanIcon16Regular/> }
				children={ texts.deleteButton }
			/>
		}
	</Kebab>;

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
