import React from 'react';
import { Button, Link } from "ui";
import getPluralForm from "src/utils/getPluralForm";
import { constructPathToGroup } from "src/consts/routes";
import { SuperGroupSheetExtractionResult, SuperGroupItemActions, MoveStudentInfo } from "src/models/superGroup";
import SettingsType from "../SettingsType";

const submitButtonText = `Применить`;
const instructionLink = `https://docs.google.com/document/d/1jAVLfb5WdXOptZZcsnc4r6gu6UzPftKkz_7_BxzuqhY`;

export default {
	buildMetaTitle: (groupName: string) => `Супер-группа «${ groupName }»`,
	buildGroupsTexts: (
		extractionResult: SuperGroupSheetExtractionResult,
		courseId: string,
		navigateToGroup: (anchor: React.MouseEvent<HTMLAnchorElement>) => void,
		deleteGroup: (anchor: React.MouseEvent<HTMLButtonElement>) => void,
	) => {
		const pluralForStudentsCount = (studentsCount: number) =>
			getPluralForm(studentsCount, 'студент', 'студента', 'студентов');
		const pluralForStudentsJoinedCount = (studentsCount: number) =>
			getPluralForm(studentsCount, 'студент', 'студентов', 'студентов');

		const pluralForStudentsWillJoinCount = (studentsCount: number) =>
			getPluralForm(studentsCount, 'студента', 'студентов', 'студентов');
		const pluralForStudentsVerb = (studentsCount: number) =>
			getPluralForm(studentsCount, 'подключился', 'подключились', 'подключилось');
		const pluralForStudentsResortVerb = (studentsCount: number) =>
			getPluralForm(studentsCount, 'будет перераспределен', 'будут перераспределены', 'будут перераспределены');

		return Object
			.entries(extractionResult.groups)
			.map(([groupName, superGroupItem]) => {
				let text = '';
				let additionalAction = <></>;
				const groupNameContent = superGroupItem.groupId
					? <Link onClick={ navigateToGroup }
							href={ constructPathToGroup(courseId, superGroupItem.groupId) + `/${ SettingsType.settings }` }>
						{ groupName }
					</Link>
					: groupName;
				const count = superGroupItem.studentNames?.length || 0;
				const joinedCount = superGroupItem.joinedStudents?.length || 0;

				switch (superGroupItem.neededAction) {
					case null:
						text += `уже создана`;

						if(count !== 0) {
							text += `, ${ pluralForStudentsVerb(
								joinedCount) } ${ joinedCount } из ${ count } ${ pluralForStudentsJoinedCount(count) }`;
						}
						break;
					case SuperGroupItemActions.ShouldBeCreated:
						text += `будет создана`;
						if(count !== 0) {
							text += `, состоит из ${ count } ${ pluralForStudentsWillJoinCount(count) }`;
						}
						break;
					case SuperGroupItemActions.ShouldBeDeleted:
						text += `не существует в таблице`;

						if(joinedCount !== 0) {
							text += `, ${ pluralForStudentsVerb(
								joinedCount) } ${ joinedCount } ${ pluralForStudentsCount(joinedCount) }`;
						} else if(superGroupItem.groupId) {
							additionalAction = <>
								(<Button
								use={ 'link' }
								data-tid={ superGroupItem.groupId.toString() }
								data-group-name={ groupName }
								onClick={ deleteGroup }>
								удалить
							</Button>)
							</>;
						}
						break;
				}
				return ({
					groupName,
					status: superGroupItem.neededAction,
					content: <>{ groupNameContent } { text } { additionalAction }</>,
				});
			});
	},

	instruction: <>Супер-группа позволяет создавать группы использую гугл-таблицу (<Link
		rel={ "noopener noreferrer" }
		target={ "_blank" }
		href={ instructionLink }>
		инструкция
	</Link>).</>,
	linkPlaceholder: `ссылка на гугл-таблицу с распределением`,
	extractSpreadsheetButton: `Загрузить`,
	noGroupsText: `В таблице нет групп`,
	afterSuccessDeleteToastText: `Группа успешно удалена`,
	settingTabText: `Управление настройками групп`,
	afterSuccessSettingUpdateToastText: `Настройки были успешно применены`,
	groupsTabText: `Управление группами`,
	loadTableFirstHint: `Сначала загрузите таблицу`,
	afterSubmitSuccessToastText: `Изменения были успешно применины`,
	afterResortingSuccessToastText: `Студенты были успешно перераспределены`,

	buildSubmitButtonHint: (actions: SuperGroupItemActions[]) => {
		let text = `При нажатии на кнопку «${ submitButtonText }»`;

		let toCreateCount = 0;
		let toDeleteCount = 0;
		for (const action of actions) {
			switch (action) {
				case SuperGroupItemActions.ShouldBeCreated:
					toCreateCount++;
					break;
				case SuperGroupItemActions.ShouldBeDeleted:
					toDeleteCount++;
					break;
			}
		}

		function getPluralVerb(count: number, toCreate = false) {
			return toCreate
				? getPluralForm(count, 'создана', 'созданы', 'создано')
				: getPluralForm(count, 'удалена', 'удалены', 'удалено');
		}

		function getPluralFutureTense(count: number) {
			return getPluralForm(count, 'будет', 'будут', 'будет');
		}

		function getPluralCreationCount(count: number) {
			return getPluralForm(count, 'группа', 'группы', 'групп');
		}

		if(toCreateCount > 0) {
			text += ` ${ getPluralFutureTense(toCreateCount) } ${ getPluralVerb(toCreateCount,
				true) } ${ toCreateCount } ${ getPluralCreationCount(
				toCreateCount) }`;
		}

		text += `, после этого вы сможете управлять существующими группами.`;

		return text;
	},
	updateGroupsButtonText: submitButtonText,
	updateGroupsSettingsButtonText: `Применить к группам`,

	settingsTabInstructions: 'Для созданных групп можно изменить следующие параметры',

	validating: {
		buildSameStudentInGroups: (studentName: string, groupsNames: string[]) => {
			return `${ studentName } (группы ${ groupsNames.join(", ") })`;
		},
		buildStudentBelongsToOtherGroup: (studentName: string, moveInfo: MoveStudentInfo) => {
			return `${ studentName } должен находиться в группе «${ moveInfo.toGroupName }», но вступил в «${ moveInfo.fromGroupName }»`;
		},

		sameName: `В таблице есть студенты с одинаковыми именами:`,
		sameNameHint: `Эти студенты не смогут воспользоваться ссылкой для вступления в авто группу. Их нужно пригласить вручную через ссылку-приглашение для отдельной группы`,

		studentBelongsToOtherGroup: `Текущее распределение студентов по группам не соответствует таблице:`,
		studentBelongsToOtherGroupButtonText: `Перераспределить студентов`,
		studentBelongsToOtherGroupCreateFirstHint: `Создайте все группы, потом вы сможете перераспределить студентов.`,
		studentBelongsToOtherGroupHint: 'Вы можете перераспределить этих студентов. Тогда они будут удалены из старых групп и добавлены в новые в соответсвии с таблицей.',

		formatIsUnsupported: 'Формат таблицы не распознан',
	},
};
