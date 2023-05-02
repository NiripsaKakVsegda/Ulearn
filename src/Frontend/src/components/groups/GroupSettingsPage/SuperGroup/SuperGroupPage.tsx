import React, { useEffect, useState } from "react";
import api from "src/api";

import { Add, Ok, Remove, Warning } from "icons";
import Page from "src/pages";
import { Button, Input, Link, Loader, Modal, Toast, } from "ui";

import { clone } from "src/utils/jsonExtensions";
import { withNavigate } from "src/utils/router";

import { GroupInfo, GroupsListParameters, SuperGroupsListResponse } from "src/models/groups";
import {
	SuperGroupItemActions,
	SuperGroupSheetExtractionResult,
	StudentBelongsToOtherGroup,
	UpdateGroupsRequestParameters,
	ValidatingResult,
	ValidationType
} from "src/models/superGroup";
import { WithNavigate } from "src/models/router";

import texts from "./SuperGroup.texts";
import defaultTexts from "src/components/groups/GroupSettingsPage/GroupSettingsHeader/GroupSettingsHeader.texts";
import defaultStyles from "src/components/groups/GroupSettingsPage/GroupSettingsHeader/groupSettingsHeader.less";
import styles from "./SuperGroup.less";
import SettingsType from "../SettingsType";
import UpdateSubGroupsSettings from "./UpdateSubGroupsSettings";
import InviteBlock from "../GroupMembers/StudentsBlock/InviteBlock/InviteBlock";
import { groupSettingsApi } from "../../../../redux/toolkit/api/groups/groupSettingsApi";
import { RootState } from "../../../../redux/reducers";
import { AppDispatch } from "../../../../setupStore";
import { superGroupsApi } from "../../../../redux/toolkit/api/groups/groupsApi";
import { connect } from "react-redux";
import { ValidationWrapper } from "@skbkontur/react-ui-validations";

interface SuperGroupProps extends WithNavigate {
	groupInfo: GroupInfo;

	updateSuperGroupsState: (params: Partial<GroupsListParameters>,
		recipe: (draft: SuperGroupsListResponse) => void
	) => void;

	updateSuperGroup?: typeof api.superGroups.updateSuperGroup;
	extractGoogleSheet?: typeof api.superGroups.extractFromTable;
	saveGroupSettings?: typeof api.groups.saveGroupSettings;
	saveScoresSettings?: typeof api.groups.saveScoresSettings;
	resortSuperGroupStudents?: typeof api.superGroups.resortSuperGroupStudents;
	getGroupScores?: typeof api.groups.getGroupScores;
	getGroup?: typeof api.groups.getGroup;
	deleteGroup?: typeof api.groups.deleteGroup;
}

const SettingsToTitle = {
	[SettingsType.settings]: 'Настройки',
	[SettingsType.additional]: 'Отложенная публикация',
	[SettingsType.deadlines]: 'Дедлайны',
};

function SuperGroupPage(props: SuperGroupProps): React.ReactElement {
	const {
		groupInfo,
		navigate,
		extractGoogleSheet = api.superGroups.extractFromTable,
		updateSuperGroup = api.superGroups.updateSuperGroup,
		resortSuperGroupStudents = api.superGroups.resortSuperGroupStudents,
		deleteGroup = api.groups.deleteGroup,
		updateSuperGroupsState,
	} = props;
	const { name } = groupInfo;
	const [tableLink, setTableLink] = useState(groupInfo.distributionTableLink || "");
	useEffect(() => {
		setTableLink(groupInfo.distributionTableLink || "");
	}, [groupInfo]);
	const [extractionResult, setExtractionResult] = useState<SuperGroupSheetExtractionResult>();
	const [isLoading, setLoading] = useState(false);
	const [settingsTab, setSettingsSettingsTab] = useState<SettingsType>();
	const [saveSettings] = groupSettingsApi.useSaveGroupSettingsMutation();

	const groups = extractionResult
		? extractionResult.groups
		: null;
	const neededActionsAsDictionary = extractionResult
		? mapExtractionToAction(extractionResult)
		: null;
	const neededActions = neededActionsAsDictionary
		? Object.values(neededActionsAsDictionary)
			.filter(a => a !== SuperGroupItemActions.ShouldBeDeleted)
		: null;
	const neededActionsCount = neededActions ? neededActions.length : -1;

	if(tableLink && !isLoading && !extractionResult) {
		_extractGoogleSheet();
	}

	return (
		<Page metaTitle={ texts.buildMetaTitle(name) }>
			{ renderHeader() }
			{ renderGoogleSheetLink() }
			<InviteBlock onToggleInviteLink={ onToggleInviteLink } group={ groupInfo }/>
			<Loader active={ isLoading }>
				{ extractionResult && renderExtractionResult(extractionResult) }
			</Loader>
		</Page>
	);

	function onToggleInviteLink(isEnabled: boolean) {
		if(!groupInfo) {
			return;
		}
		saveSettings({
			groupId: groupInfo.id,
			groupSettings: { 'isInviteLinkEnabled': isEnabled }
		});
	}

	function renderHeader() {
		return (
			<header className={ defaultStyles["group-header"] }>
				<div className={ defaultStyles["link-to-prev-page-block"] }>
					<div className={ defaultStyles["link-to-prev-page"] }>
						<Link onClick={ goToPrevPage }>
							{ defaultTexts.allGroups }
						</Link>
					</div>
				</div>
				<h2 className={ defaultStyles["group-name"] }>
					{ name }
				</h2>
			</header>
		);
	}

	function goToPrevPage() {
		navigate(-1);
	}

	function getDateValidationResult(): { message: string } | null {
		if(tableLink === '' || tableLink === null) {
			return { message: "Поле не может быть пустым" };
		}
		return null;
	}

	function renderGoogleSheetLink() {
		return (
			<div className={ defaultStyles.distributionLinkBlock }>
				<p>{ texts.instruction }</p>
				<div className={ defaultStyles.inline }>
					<ValidationWrapper validationInfo={ getDateValidationResult() }>
						<Input className={ defaultStyles.distributionLinkBlockInput }
							   placeholder={ texts.linkPlaceholder }
							   width={ "30%" }
							   value={ tableLink }
							   onValueChange={ setTableLink }
							   error={ tableLink === '' || tableLink === null }
						/>
					</ValidationWrapper>
					<Button
						use={ "default" }
						disabled={ tableLink === '' || tableLink === null }
						onClick={ _extractGoogleSheet }>
						{ texts.extractSpreadsheetButton }
					</Button>
				</div>
			</div>
		);
	}

	function renderExtractionResult(extractionResult: SuperGroupSheetExtractionResult) {
		if(!groups || Object.keys(groups).length === 0) {
			if(extractionResult.validatingResults.length === 0) {
				return (
					<p>
						{ texts.noGroupsText }
					</p>
				);
			}

			return (
				<div>
					{ renderValidatingResults(extractionResult.validatingResults) }
				</div>
			);
		}

		return (
			<div>
				{ renderGroupsInfo(extractionResult) }
				{ renderValidatingResults(extractionResult.validatingResults) }
				{
					neededActions && neededActionsCount > 0 && <>
						<p>{ texts.buildSubmitButtonHint(neededActions) }</p>
						{ renderUpdateGroupsButton() }
					</>
				}
				{
					neededActions && neededActionsCount === 0 &&
					<>
						<p>{ texts.settingsTabInstructions }</p>
						{ renderSettings() }
					</>
				}
			</div>
		);
	}

	function renderGroupsInfo(extractionResult: SuperGroupSheetExtractionResult) {
		return texts
			.buildGroupsTexts(extractionResult, groupInfo.courseId, navigateToGroup, _deleteGroup)
			.map(({ groupName, content, status }) => {
				let icon = null;
				switch (status) {
					case SuperGroupItemActions.ShouldBeCreated:
						icon = <Add className={ styles.groupToCreate }/>;
						break;
					case SuperGroupItemActions.ShouldBeDeleted:
						icon = <Remove className={ styles.groupToDelete }/>;
						break;
					case null:
						icon = <Ok className={ styles.createdGroup }/>;
						break;
				}
				return (<p key={ groupName }>{ icon } { content }</p>);
			});
	}

	function _deleteGroup(event: React.MouseEvent<HTMLButtonElement>) {
		const parent = event.currentTarget.parentElement;
		if(!parent) {
			console.error("Parent element of this button wasn't found!");
			return;
		}
		const id = parseInt(parent.dataset.tid || '');
		const groupName = parent.dataset.groupName || '';

		if(!id || !extractionResult) {
			console.error("Couldn't parse id and/or group name from the button data");
			return;
		}

		event.preventDefault();
		event.stopPropagation();

		deleteGroup(id)
			.then(r => {
				Toast.push(texts.afterSuccessDeleteToastText);
				const newResults = clone(extractionResult);
				delete newResults.groups[groupName];

				setExtractionResult(newResults);
				return r;
			});
	}

	function navigateToGroup(event: React.MouseEvent<HTMLAnchorElement>) {
		const href = event.currentTarget.pathname;

		event.preventDefault();
		event.stopPropagation();
		navigate(href);
	}

	function renderValidatingResults(validatings: ValidatingResult[]) {
		return validatings.map((validatingResult, index) => {
			const { title, content } = getValidatingInfo(validatingResult);
			return (
				<div key={ index } className={ defaultStyles.errorBlock }>
					<h4><Warning className={ styles.validatingWarning }/> { title }</h4>
					{ content && content }
				</div>
			);
		});
	}

	function getValidatingInfo(validationResult: ValidatingResult) {
		switch (validationResult.type) {
			case ValidationType.invalidSheetStructure:
				return {
					title: texts.validating.formatIsUnsupported,
					content: <p>
						Проверьте правильность заполнения строк по номерам: { validationResult.rawsIndexes.map(
						r => r + 1).join(', ') }.
					</p>
				};
			case ValidationType.groupsHasSameStudents:
				return {
					title: <>{ texts.validating.sameName }</>,
					content: <>
						<ul>
							{ Object
								.entries(validationResult.sameNamesInGroups)
								.map(([userName, groupsNames]) => {
									const text = texts.validating.buildSameStudentInGroups(userName, groupsNames);
									return <li key={ userName }>{ text }</li>;
								}) }
						</ul>
						<p>{ texts.validating.sameNameHint }</p>
					</>,
				};
			case ValidationType.studentBelongsToOtherGroup: {
				const neededNoActions = (!neededActions || neededActionsCount === 0);
				return {
					title: <>{ texts.validating.studentBelongsToOtherGroup }</>,
					content: <>
						<ul>
							{ Object
								.entries(validationResult.neededMoves)
								.map(([studentName, moveInfo]) => {
									const text = texts.validating.buildStudentBelongsToOtherGroup(studentName,
										moveInfo);
									return <li key={ studentName }>{ text }</li>;
								}) }
						</ul>
						{ neededNoActions &&
							<>
								<p>{ texts.validating.studentBelongsToOtherGroupHint }</p>
								<Button onClick={ resortStudentsButtonClick }>
									{ texts.validating.studentBelongsToOtherGroupButtonText }
								</Button>
							</>
						}
					</>,
				};
			}
		}
	}

	function resortStudentsButtonClick() {
		if(!extractionResult) {
			return;
		}
		const belongsToOtherGroup = extractionResult.validatingResults
			.find(v => v.type === ValidationType.studentBelongsToOtherGroup) as StudentBelongsToOtherGroup;
		if(!belongsToOtherGroup) {
			return;
		}

		resortSuperGroupStudents(groupInfo.id, belongsToOtherGroup.neededMoves)
			.then(r => {
				Toast.push(texts.afterResortingSuccessToastText);

				_extractGoogleSheet();
				return r;
			});
	}

	function renderUpdateGroupsButton() {
		return (
			<Button
				use={ "primary" }
				onClick={ onUpdateGroupsButtonClick }
			>
				{ texts.updateGroupsButtonText }
			</Button>
		);
	}

	function renderSettings() {
		return (
			<>
				<p>
					<Link id={ SettingsType.settings } onClick={ onSettingsTabChangeHandler }>
						{ SettingsToTitle[SettingsType.settings] }
					</Link>
				</p>
				<p>
					<Link disabled id={ SettingsType.additional } onClick={ onSettingsTabChangeHandler }>
						{ SettingsToTitle[SettingsType.additional] }
					</Link>
				</p>
				<p>
					<Link disabled id={ SettingsType.deadlines } onClick={ onSettingsTabChangeHandler }>
						{ SettingsToTitle[SettingsType.deadlines] }
					</Link>
				</p>
				{
					// eslint-disable-next-line @typescript-eslint/ban-ts-comment
					// @ts-ignore
					settingsTab && <Modal onClose={ closeSettingsModal }>
						<Modal.Header>
							{ SettingsToTitle[settingsTab] }
						</Modal.Header>
						{/*eslint-disable-next-line @typescript-eslint/ban-ts-comment*/ }
						{/*@ts-ignore*/ }
						{ renderSettingsModalContent(settingsTab) }
					</Modal>
				}
			</>
		);
	}

	function closeSettingsModal() {
		onSettingsModalChange(undefined);
	}

	function renderSettingsModalContent(type: SettingsType) {
		if(!groups) {
			return;
		}
		switch (type) {
			case SettingsType.settings: {
				return (
					<UpdateSubGroupsSettings
						superGroupId={ groupInfo.id }
						onClose={ closeSettingsModal }
					/>
				);
			}
			case SettingsType.additional:
				return 'Функциональность пока недоступна';
			case SettingsType.deadlines:
				return 'Функциональность пока недоступна';
		}
	}

	function onSettingsTabChangeHandler(event: React.MouseEvent<HTMLAnchorElement>) {
		const settingType = event.currentTarget.id;
		onSettingsModalChange(settingType);
	}

	function onSettingsModalChange(value?: string) {
		if(!value) {
			setSettingsSettingsTab(undefined);
		}

		setSettingsSettingsTab(value as SettingsType);
	}

	async function onUpdateGroupsButtonClick() {
		if(!neededActionsAsDictionary || !extractionResult) {
			return;
		}

		const updates = await updateSuperGroup(groupInfo.id, {
			groupsToUpdate: neededActionsAsDictionary,
		});

		Toast.push(texts.afterSubmitSuccessToastText);

		const afterUpdates = clone(extractionResult);
		for (const update of Object.values(updates)) {
			//group exists === created
			if(update.groupId) {
				const group = afterUpdates.groups[update.groupName];
				group.groupId = update.groupId;
				group.neededAction = null;
			} else {
				//group doesnt exists === deleted
				delete (afterUpdates.groups[update.groupName]);
			}
		}

		setExtractionResult(afterUpdates);
	}

	function mapExtractionToAction(extractionResult: SuperGroupSheetExtractionResult) {
		const groupsToUpdate: UpdateGroupsRequestParameters['groupsToUpdate'] = {};
		for (const [groupId, superGroupItem] of Object.entries(extractionResult.groups)) {
			if(superGroupItem.neededAction) {
				groupsToUpdate[groupId] = superGroupItem.neededAction;
			}
		}
		return groupsToUpdate;
	}

	async function _extractGoogleSheet() {
		updateSuperGroupsState({ courseId: groupInfo.courseId }, draft => {
			const index = draft.superGroups.findIndex(source => source.id === groupInfo.id);
			draft.superGroups[index] = {
				...draft.superGroups[index],
				distributionTableLink: tableLink,
			};
		});
		setLoading(true);
		setExtractionResult(await extractGoogleSheet(tableLink, groupInfo.id));
		setLoading(false);
	}
}

const mapStateToProps = (state: RootState) => ({});

const mapDispatchToProps = (dispatch: AppDispatch) => ({
	updateSuperGroupsState: (params: Partial<GroupsListParameters>, recipe: (draft: SuperGroupsListResponse) => void) =>
		dispatch(superGroupsApi.util.updateQueryData('getGroups', params, recipe)),
});

const connected = connect(mapStateToProps, mapDispatchToProps)(SuperGroupPage);
export default withNavigate(connected);
