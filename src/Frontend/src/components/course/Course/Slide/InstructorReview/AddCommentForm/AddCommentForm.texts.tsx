import React from "react";
import { Star2 } from "icons";

export default {
	commentSectionHeaderText: 'Комментарий',
	favouriteSectionHeaderText: 'Избранные',
	instructorFavouriteSectionHeaderText: 'Комментарии других преподавателей',
	lastUsedReviewsSectionHeaderText: 'Недавно оставленные комментарии',
	addCommentButtonText: 'Добавить',
	addToFavouriteButtonText: 'Добавить комментарий в Избранные',
	noFavouriteCommentsText: (): React.ReactElement => (
		<>
			Чтобы добавить комментарий в Избранные,<br/> нажмите на <Star2/>
		</>),
};
