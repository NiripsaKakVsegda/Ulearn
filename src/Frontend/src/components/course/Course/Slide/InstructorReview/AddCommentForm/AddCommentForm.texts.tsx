import { StarIcon16Regular } from '@skbkontur/icons/StarIcon16Regular';
import React from "react";

export default {
	preview: 'Превью',
	commentSectionHeaderText: 'Комментарий',
	favouriteSectionHeaderText: 'Избранные',
	instructorFavouriteSectionHeaderText: 'Комментарии других преподавателей',
	lastUsedReviewsSectionHeaderText: 'Ваши последние комментарии',
	addCommentButtonText: 'Добавить',
	addToFavouriteButtonText: 'Добавить комментарий в Избранные',
	noFavouriteCommentsText: (): React.ReactElement => (
		<>
			Чтобы добавить комментарий в Избранные,<br/> нажмите на <StarIcon16Regular
			size={ 14 }
			align={ 'baseline' }
		/>
		</>),
};
