import React from "react";
import getPluralForm from "src/utils/getPluralForm";

export default {
	getSlideScore: (score: number, maxScore: number, showZeroScore = false): React.ReactNode =>
		score !== 0 || showZeroScore
			? `${ score } ${ getPluralForm(score, 'балл', 'балла', 'баллов') } из ${ maxScore }`
			: `Максимум ${ maxScore } ${ getPluralForm(maxScore, 'балл', 'балла', 'баллов') }`,
	skippedHeaderText: "Вы посмотрели чужие решения, поэтому не получите баллы за эту задачу",
	pendingReview: "Решение ожидает код-ревью",
	prohibitFurtherReview: 'По этой задаче код-ревью больше не проводится',
	reviewWaitForCorrection: 'Решение прошло код-ревью',
	hiddenSlideText: "Студенты не видят этот слайд",
	showAcceptedSolutionsText: 'Посмотреть решения студентов',
	showAcceptedSolutionsHeaderText: 'Решения студентов',
	linkToFlashcardsPreviewText: 'Показать превью по всем флешкартам за курс',
	//hiddenSlideTextWithStudents: (showedGroupsIds) => `Студенты ${ showedGroupsIds.join(', ') } видят этот слайд.`,
};
