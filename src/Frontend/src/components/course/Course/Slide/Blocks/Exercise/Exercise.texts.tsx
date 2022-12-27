import React from "react";

import getPluralForm from "src/utils/getPluralForm";
import { SubmissionInfo } from "src/models/exercise";
import { convertDefaultTimezoneToLocal, getMoment } from "src/utils/momentUtils";
import { Language } from "src/consts/languages";
import { capitalize } from "src/utils/stringUtils";
import { LanguageLaunchInfo } from "src/models/slide";
import { Link } from "@skbkontur/react-ui";

const texts = {
	submissions: {
		newTry: 'Новая версия',
		getSubmissionCaption: (
			submission: SubmissionInfo,
			selectedSubmissionIsLastSuccess: boolean,
			waitingForManualChecking: boolean
		): string => {
			const { timestamp, manualChecking } = submission;
			const manualCheckingPassed = (manualChecking?.percent ?? null) !== null;
			const timestampCaption = texts.getSubmissionDate(timestamp);
			if(manualCheckingPassed) {
				return timestampCaption + ", прошло код-ревью";
			} else if(waitingForManualChecking && selectedSubmissionIsLastSuccess) {
				return timestampCaption + ", ожидает код-ревью";
			}
			return timestampCaption;
		},
	},

	getLanguageLaunchInfo: (language: Language,
		languageInfos: EnumDictionary<string, LanguageLaunchInfo> | null
	): LanguageLaunchInfo => {
		const capitalizeLanguage = capitalize(language);
		if(languageInfos !== null && languageInfos[capitalizeLanguage] !== undefined) {
			return languageInfos[capitalizeLanguage];
		}
		return {
			compiler: language === Language.cSharp ? "C#" : capitalize(language),
			compileCommand: "",
			runCommand: ""
		};
	},

	editCommentError: 'При редактировании комментария к ревью произошла ошибка',

	getLanguageLaunchMarkup: (languageLaunchInfo: LanguageLaunchInfo): React.ReactNode => {
		function renderLine(name: string, value: string) {
			return value && (<><h5>{ name }</h5><p>{ value }</p></>);
		}

		return (
			<div>
				{ renderLine("Операционная система: ", "Ubuntu 20.04 x86_64") }
				{ renderLine("Компиляция: ", languageLaunchInfo.compileCommand) }
				{ renderLine("Запуск: ", languageLaunchInfo.runCommand) }
			</div>);
	},

	compilationText: 'Как компилируется код?',

	controls: {
		submitCode: {
			text: 'Отправить',
			redactor: 'Открыть редактор',
			hint: 'Начните писать код',
		},
		hints: {
			text: 'Посмотреть подсказку',
			hint: 'Подсказки закончились',
			showHintText: 'Показать ещё',
		},

		reset: {
			text: 'Начать сначала',
		},

		output: {
			show: 'Показать вывод',
			hide: 'Скрыть вывод',
		},

		acceptedSolutions: {
			text: 'Посмотреть решения',
			buildWarning: (): React.ReactNode => <React.Fragment>
				Вы не получите баллы за задачу,<br/>
				если посмотрите чужие решения.<br/>
				<br/>
			</React.Fragment>,
			continue: 'Всё равно посмотреть',
		},

		showAllCode: {
			text: 'Показать код полностью',
		},

		copyCode: {
			text: 'Скопировать код',
			onCopy: 'Код скопирован в буфер обмена',
		},

		visualizer: {
			text: 'Выполнить пошагово',
		},

		statistics: {
			buildShortText: (usersWithRightAnswerCount: number): React.ReactNode =>
				<span>Решило: { usersWithRightAnswerCount }</span>,
			buildStatistics: (attemptedUsersCount: number, usersWithRightAnswerCount: number,
				lastSuccessAttemptDate?: string
			): React.ReactNode =>
				lastSuccessAttemptDate
					? <React.Fragment>
						За всё время:<br/>
						{ attemptedUsersCount } { getPluralForm(attemptedUsersCount, 'студент пробовал',
						'студента пробовали', 'студентов пробовали') } решить
						задачу.<br/>
						{ getPluralForm(attemptedUsersCount, 'Решил', 'Решили', 'Решили') } { usersWithRightAnswerCount }
						<br/>
						<br/>
						Последний раз решили { getMoment(lastSuccessAttemptDate) }
					</React.Fragment>
					: 'Эту задачу ещё никто не решал',
		},
	},

	getSubmissionDate: (timestamp: string): string => {
		return convertDefaultTimezoneToLocal(timestamp).format('DD MMMM YYYY в HH:mm');
	}
};

export default texts;
