import { DeadLineInfo } from "src/models/deadLines";
import { momentFromServerToLocal } from "src/utils/momentUtils";
import { AdditionalContentInfo } from "src/models/additionalContent";
import React from "react";
import { Link } from "react-router-dom";
import { constructPathToGroupsPage } from "src/consts/routes";
import { buildQuery } from "src/utils";
import texts from "src/components/course/Course/Course.texts";

export default {
	hiddenSlide: 'Этот слайд скрыт',
	getDeadLineInfo: (
		deadLineInfo: DeadLineInfo,
		maxScore: number
	): React.ReactText => {
		return `На этот слайд установлен дедлайн на ${ momentFromServerToLocal(
			deadLineInfo.date).format(
			'DD.MM.YYYY HH:mm') }. ${ texts.afterDeadLine(deadLineInfo, maxScore) }`;
	},
	slideNotPublished: 'Слайд ещё не опубликован',
	getAdditionalContentPublicationDate: (additionalContentInfo: AdditionalContentInfo) => additionalContentInfo.publicationDate && `Этот слайд будет опубликован ${ momentFromServerToLocal(
		additionalContentInfo.publicationDate, 'DD.MM.YYYY HH:mm:ss').format(
		'DD.MM.YYYY в HH:mm') }`,
	additionalContentPublicationInfoForInstructor: (courseId: string) => <>
		Этот слайд является дополнительным контентом.<br/>
		По умолчанию студенты его не видят.<br/>
		Его можно опубликовать на&nbsp;
		<Link to={ constructPathToGroupsPage(courseId) + buildQuery({ groupsSettings: 'additional-content' }) }>странице
			группы</Link>
	</>,
};
