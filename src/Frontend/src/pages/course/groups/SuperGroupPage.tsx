import React, { useState } from "react";
import { GroupInfo, GroupScoringGroupInfo } from "../../../models/groups";
import Page from "../../index";
import styles from "./groupPage.less";
import { Button, Input, Link } from "ui";
import texts from "./GroupPage.texts";
import api from "../../../api";
import { AutoGroupMergeError, AutoGroupMergeResult } from "../../../models/autoGroup";
import GroupSettings from "../../../components/groups/GroupSettingsPage/GroupSettings/GroupSettings";
import InviteBlock from "../../../components/groups/GroupSettingsPage/GroupMembers/InviteBlock/InviteBlock";
import cn from "classnames";
import getPluralForm from "../../../utils/getPluralForm";


type SuperGroupProps = {
	groupInfo: GroupInfo;
	goToPrevPage: () => void;
	scores: GroupScoringGroupInfo[];
}

export function SuperGroupPage(props: SuperGroupProps): React.ReactElement {
	const { goToPrevPage, groupInfo } = props;
	const { name } = groupInfo;
	const [tableLink, setTableLink] = useState(groupInfo.distributionTableLink || "");
	const [parsingResult, setParsingResult] = useState<AutoGroupMergeResult | undefined>(undefined);
	const [scores, setScores] = useState(props.scores);
	const [checkedScoresSettingsIds, setCheckedScoresSettingsIds] = useState<string[]>();
	const [updatedFields, setUpdatedFields] = useState<Partial<GroupInfo>>(groupInfo);

	return (
		<Page metaTitle={ `Поток ${ name }` }>
			{renderHeader()}
			{renderDistributionLink()}
			{renderParsingResult()}
			<InviteBlock group={groupInfo}/>
			{renderSubmit()}
		</Page>
	);

	function renderHeader() {
		return (
			<header className={ styles["group-header"] }>
				<div className={ styles["link-to-prev-page-block"] }>
					<div className={ styles["link-to-prev-page"] }>
						<Link onClick={ goToPrevPage }>
							{ texts.allGroups }
						</Link>
					</div>
				</div>
				<h2 className={ styles["group-name"] }>{ name ? name : " " }</h2>
			</header>
		);
	}

	function renderDistributionLink() {
		return (
			<div className={styles.distributionLinkBlock}>
				<p>Ссылка на таблицу распределения студентов по группам (<Link>инструкция</Link>)</p>
				<div className={styles.inline}>
					<Input className={styles.distributionLinkBlockInput}
						   placeholder="ссылка на гугл-таблицу"
						   width="30%"
						   value={tableLink}
						   onValueChange={setTableLink}
					/>
					<Button use="default" onClick={onLoadTable}>Загрузить</Button>
				</div>
			</div>
		);
	}

	function renderParsingResult() {
		if(!parsingResult) {
			return;
		}

		return (
			<div>
				{parsingResult.errors.map(renderError)}
				{parsingResult.newGroups.length > 0 && (
					<div>
						<h4>При нажатии кнопки "Сохранить" будут созданы следующие группы в этом потоке:</h4>
						{parsingResult.newGroups.map(x => <p>- {x}: {parsingResult?.newGroupsLengths[x]} студентов</p>)}
						<h4>Новые группы будут созданы со следующими настройками:</h4>
						<GroupSettings
							loading={false}
							scores={scores}
							onChangeSettings={onChangeSettings}
							onChangeScores={onChangeScores}
							isManualCheckingEnabled={updatedFields?.isManualCheckingEnabled || false}
							canStudentsSeeGroupProgress={updatedFields?.canStudentsSeeGroupProgress || false}
							isManualCheckingEnabledForOldSolutions={updatedFields?.isManualCheckingEnabledForOldSolutions || false}
							defaultProhibitFurtherReview={updatedFields?.defaultProhibitFurtherReview || false}
							canChangeName={false}
						/>
					</div>
				)}
			</div>
		);
	}

	function renderError(error: AutoGroupMergeError) {
		if (error.errorType === "ParsingError") {
			return (
				<div className={styles.errorBlock}>
					<h4>X Формат таблицы не распознан</h4>
				</div>
			);
		}

		if (error.errorType === "GroupsHasSameStudents") {
			return (
				<div className={cn([styles.errorBlock])}>
					<h4>! Есть совпадение по именам:</h4>
					{Object.entries(error.studentToGroupsMap).map(kvp => {
						const [name, groups] = kvp;
						return <p>{`- ${name} (группы ${groups.join(", ")})`}</p>;
					})}
					<p>Этих студентов нужно пригласить через ссылку-приглашение для отдельной группы</p>
				</div>
			);
		}
	}

	function renderSubmit() {
		return (
			<div>
				<Button
					use="default"
					onClick={onSubmit}
					active={parsingResult === undefined}
				>
					Сохранить
				</Button>
			</div>
		);
	}

	function onChangeSettings(field: keyof GroupInfo, value: GroupInfo[keyof GroupInfo]) {
		setUpdatedFields( { ...updatedFields, [field]: value });
	}

	function onChangeScores(
		key: string,
		field: keyof GroupScoringGroupInfo,
		value: GroupScoringGroupInfo[keyof GroupScoringGroupInfo]
	) {
		const updatedScores = scores
			.map(item => item.id === key ? { ...item, [field]: value } : item);

		const checkedScoresSettingsIds = updatedScores
			.filter(item => item[field] === true)
			.map(item => item.id);

		setScores(updatedScores);
		setCheckedScoresSettingsIds(checkedScoresSettingsIds);
	}

	async function onSubmit() {
		const newGroups = await api.groups.updateSupergroup(groupInfo.id, {
			canStudentsSeeGroupProgress: updatedFields.canStudentsSeeGroupProgress || false,
			defaultProhibitFurtherReview: updatedFields.defaultProhibitFurtherReview || false,
			isManualCheckingEnabled: updatedFields.isManualCheckingEnabled || false,
			isManualCheckingEnabledForOldSolutions: updatedFields.isManualCheckingEnabledForOldSolutions || false,
			distributionLink: tableLink,
			merge: parsingResult!
		});
		await Promise.all(newGroups.map(x =>
			api.groups.saveScoresSettings(x, checkedScoresSettingsIds || [])));
	}

	async function onLoadTable() {
		setParsingResult(await api.autoGroups.extractFromTable(tableLink, groupInfo.id));
	}
}
