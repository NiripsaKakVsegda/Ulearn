import { InstructorReviewTabs } from "./InstructorReviewTabs";
import {
	AutomaticExerciseCheckingProcessStatus,
	AutomaticExerciseCheckingResult,
	SubmissionInfo
} from "src/models/exercise";
import { convertDefaultTimezoneToLocal, momentFromServer, } from "src/utils/momentUtils";
import React from "react";
import getPluralForm from "src/utils/getPluralForm";
import { ShortGroupInfo } from "src/models/comments";
import { DeadLineInfo } from "src/models/deadLines";

const texts = {
	getTabName: (tab: InstructorReviewTabs): string => {
		switch (tab) {
			case InstructorReviewTabs.AuthorSolution:
				return 'Авторское решение';
			case InstructorReviewTabs.Formulation:
				return 'Формулировка';
			case InstructorReviewTabs.Review:
				return 'Решение студента';
		}
	},
	getStudentInfo: (visibleName: string, groups: ShortGroupInfo[]): string => {
		if(groups.length > 0) {
			const archivedGroups = groups.filter(g => g.isArchived);
			const notArchivedGroups = groups.filter(g => !g.isArchived);
			let groupsAsString = notArchivedGroups.map(g => g.name).join(', ');
			if(archivedGroups.length > 0) {
				if(notArchivedGroups.length > 0) {
					groupsAsString += '; ';
				}
				groupsAsString += 'архивные группы: ' + archivedGroups.map(g => g.name).join(', ');
			}
			return `${ visibleName } (${ groupsAsString })`;
		}

		return visibleName;
	},
	deadLineViolated: 'решено после дедлайна',
	getDeadLineViolationInfo: (
		submission: SubmissionInfo,
		deadLine: DeadLineInfo
	): string => `Первое решение было прислано ${ momentFromServer(submission.timestamp).from(
		momentFromServer(deadLine.date)) } после дедлайна`,
	getReviewInfo: (submissions: SubmissionInfo[], prevReviewScore: number | null,
		currentScore: number | null
	): string => {
		if(currentScore !== null) {
			return `${ currentScore }% за ревью`;
		}
		if(prevReviewScore === null) {
			return 'первое ревью';
		}
		return `${ prevReviewScore }% за предыдущее ревью`;
	},
	getSubmissionCaption: (
		submission: SubmissionInfo,
		selectedSubmissionIsLastSuccess: boolean,
		waitingForManualChecking: boolean
	): string => {
		const { timestamp, manualChecking, automaticChecking, } = submission;
		const manualCheckingPassed = (manualChecking?.percent ?? null) !== null;
		const timestampCaption = texts.getSubmissionDate(timestamp);
		if(manualCheckingPassed) {
			return timestampCaption + ", прошло код-ревью";
		} else if(waitingForManualChecking && selectedSubmissionIsLastSuccess) {
			return timestampCaption + ", ожидает код-ревью";
		} else if(automaticChecking?.result !== AutomaticExerciseCheckingResult.RightAnswer) {
			if(automaticChecking?.processStatus === AutomaticExerciseCheckingProcessStatus.Running
				|| automaticChecking?.processStatus === AutomaticExerciseCheckingProcessStatus.Waiting
				|| automaticChecking?.processStatus === AutomaticExerciseCheckingProcessStatus.WaitingTimeLimitExceeded
			) {
				return timestampCaption + ", проверяется";
			}
			if(automaticChecking?.processStatus === AutomaticExerciseCheckingProcessStatus.Done) {
				return timestampCaption + ", не прошло тесты";
			}
			if(automaticChecking?.processStatus === AutomaticExerciseCheckingProcessStatus.ServerError) {
				return timestampCaption + ", ошибка сервера";
			}
		}
		return timestampCaption;
	},
	getSubmissionDate: (timestamp: string): string => {
		return convertDefaultTimezoneToLocal(timestamp).format('DD MMMM YYYY в HH:mm');
	},
	leaveCommentGuideText: 'Выделите участок кода, чтобы оставить комментарий',
	getDiffText: (
		addedCount: number,
		addedColor: string,
		removedCount: number,
		removedColor: string,
		diffForInitialCode?: boolean,
	): React.ReactElement => <>
		{ diffForInitialCode ? 'Diff с начальной версией:' : 'Diff с предыдущим ревью:' }
		<span className={ addedColor }> { addedCount } { getPluralForm(addedCount, 'строку', 'строки',
			'строк') } добавили</span>,
		<span className={ removedColor }> { removedCount } – удалили</span>
	</>,

	submissionAfterDisablingManualChecking: 'Это решение было послано после отключения код-ревью',
	enableManualChecking: 'Возобновить код-ревью',
	saveShowDiff: 'Сделать значением по умолчанию',
	onSaveShowDiffToastMessage: 'Сохранено',
};

export default texts;
