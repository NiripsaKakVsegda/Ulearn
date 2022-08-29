import React from "react";
import { getDateDDMMYY, } from "src/utils/momentUtils";

export default {
	scoresText: 'Оценка за работу в % ',
	submitButtonText: 'Оценить',
	resetScoreButtonText: 'Не оценивать',
	changeScoreText: 'Изменить оценку',
	lastReviewScoreText: 'за пред. ревью',
	getCodeReviewToggleText: (exerciseTitle: string): string => `Принимать ещё код-ревью у этого студента по практике «${ exerciseTitle }»`,
	getScoreText: (percent: number, date?: string,): string => {
		if(date) {
			return `Работа от ${ getDateDDMMYY(date) }. Оценена на ${ percent }%`;
		}
		return `Работа оценена на ${ percent }%`;
	},
};
