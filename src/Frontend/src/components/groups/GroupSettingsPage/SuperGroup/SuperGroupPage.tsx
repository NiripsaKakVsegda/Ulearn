import { CheckAIcon16Solid } from '@skbkontur/icons/CheckAIcon16Solid';
import { MinusIcon16Solid } from '@skbkontur/icons/MinusIcon16Solid';
import { PlusIcon16Solid } from '@skbkontur/icons/PlusIcon16Solid';
import { WarningCircleIcon16Solid } from '@skbkontur/icons/WarningCircleIcon16Solid';
import { WarningTriangleIcon16Solid } from '@skbkontur/icons/WarningTriangleIcon16Solid';
import { ValidationWrapper } from "@skbkontur/react-ui-validations";
import React, { useEffect, useState } from "react";
import { connect } from "react-redux";
import api, { RequestError } from "src/api";
import defaultStyles from "src/components/groups/GroupSettingsPage/GroupSettingsHeader/groupSettingsHeader.less";
import defaultTexts from "src/components/groups/GroupSettingsPage/GroupSettingsHeader/GroupSettingsHeader.texts";

import {
	GroupInfo,
	GroupScoringGroupsResponse,
	GroupsListParameters,
	SuperGroupsListResponse
} from "src/models/groups";
import { WithNavigate } from "src/models/router";
import {
	StudentBelongsToOtherGroup,
	SuperGroupItemActions,
	SuperGroupSheetExtractionResult,
	UpdateGroupsRequestParameters,
	ValidatingResult,
	ValidationType
} from "src/models/superGroup";
import Page from "src/pages";

import { clone } from "src/utils/jsonExtensions";
import { withNavigate } from "src/utils/router";
import { Button, Input, Link, Loader, Modal, Toast } from "ui";
import { AccountState } from "../../../../redux/account";
import { RootState } from "../../../../redux/reducers";
import { superGroupsApi } from "../../../../redux/toolkit/api/groups/groupsApi";
import { groupSettingsApi } from "../../../../redux/toolkit/api/groups/groupSettingsApi";
import { AppDispatch } from "../../../../setupStore";
import InviteBlock from "../../common/InviteBlock/InviteBlock";
import SettingsType from "../SettingsType";
import ManageMembers from "./ManageMembers";
import styles from "./SuperGroup.less";

import texts from "./SuperGroup.texts";
import UpdateSubGroupsSettings from "./UpdateSubGroupsSettings";

interface SuperGroupProps extends WithNavigate {
	groupInfo: GroupInfo;
	refetchGroup: () => void;
	account: AccountState;
	updateSuperGroupsState: (
		params: Partial<GroupsListParameters>,
		recipe: (draft: SuperGroupsListResponse) => void
	) => void;

	updateSuperGroup?: typeof api.superGroups.updateSuperGroup;
	extractGoogleSheet?: typeof api.superGroups.extractFromTable;
	saveGroupSettings?: (groupId: number, groupSettings: Partial<GroupInfo>) => Promise<GroupInfo>;
	saveScoresSettings?: (groupId: number, checkedScoresSettingsIds: string[]) => Promise<Response>;
	resortSuperGroupStudents?: typeof api.superGroups.resortSuperGroupStudents;
	getGroupScores?: (groupId: number) => Promise<GroupScoringGroupsResponse>;
	getGroup?: (groupId: number) => Promise<GroupInfo>;
	deleteGroup?: typeof api.groups.deleteGroup;
}

const SettingsToTitle = {
	[SettingsType.settings]: 'Настройки',
	[SettingsType.unassignedMembers]: 'Нераспределённые студенты',
	[SettingsType.additional]: 'Отложенная публикация',
	[SettingsType.deadlines]: 'Дедлайны'
};

function SuperGroupPage(props: SuperGroupProps): React.ReactElement {
	const {
		groupInfo,
		navigate,
		extractGoogleSheet = api.superGroups.extractFromTable,
		updateSuperGroup = api.superGroups.updateSuperGroup,
		resortSuperGroupStudents = api.superGroups.resortSuperGroupStudents,
		deleteGroup = api.groups.deleteGroup,
		updateSuperGroupsState
	} = props;
	const { name } = groupInfo;
	const [tableLink, setTableLink] = useState(groupInfo.distributionTableLink || "");
	useEffect(() => {
		setTableLink(groupInfo.distributionTableLink || "");
	}, [groupInfo]);
	const [extractionResult, setExtractionResult] = useState<SuperGroupSheetExtractionResult>();
	const [isLoading, setLoading] = useState(false);
	const [error, setError] = useState<string | null>(null);
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

	if(tableLink && !isLoading && !extractionResult && !error) {
		_extractGoogleSheet();
	}

	return (
		<Page metaTitle={ texts.buildMetaTitle(name) }>
			{ renderHeader() }
			{ renderGoogleSheetLink() }
			<InviteBlock onToggleInviteLink={ onToggleInviteLink } group={ groupInfo }/>
			<p key={ SettingsType.unassignedMembers }>
				{ groupInfo.studentsCount
					? <WarningCircleIcon16Solid color={ "#b49100" } size={ 14 }/>
					: <CheckAIcon16Solid size={ 14 }/>
				}&nbsp;
				<Button
					use={ 'link' }
					data-id={ SettingsType.unassignedMembers }
					onClick={ onSettingsTabChangeHandler }
					disabled={ !groupInfo.studentsCount }
				>
					{ texts.getUnassignedMembersInfo(groupInfo.studentsCount) }
				</Button>
			</p>
			<Loader active={ isLoading }>
				{ extractionResult && renderExtractionResult(extractionResult) }
			</Loader>
			{ settingsTab &&
				<Modal onClose={ closeSettingsModal }>
					<Modal.Header>
						{ SettingsToTitle[settingsTab] }
					</Modal.Header>
					{ renderSettingsModalContent(settingsTab) }
				</Modal>
			}
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

	function getDateValidationResult(): {
		message: string
	} | null {
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
						<Input
							className={ defaultStyles.distributionLinkBlockInput }
							placeholder={ texts.linkPlaceholder }
							width={ "30%" }
							value={ tableLink }
							onValueChange={ setTableLink }
							error={ tableLink === "" || tableLink === null }
						/>
					</ValidationWrapper>
					<Button
						use={ "default" }
						error={ !!error }
						disabled={ tableLink === '' || tableLink === null }
						onClick={ updateGroups }
					>
						{ texts.extractSpreadsheetButton }
					</Button>
					{ error && <span className={ styles.requestErrorText }>{ error }</span> }
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
						icon = <PlusIcon16Solid size={ 14 } className={ styles.groupToCreate }/>;
						break;
					case SuperGroupItemActions.ShouldBeDeleted:
						icon = <MinusIcon16Solid size={ 14 } className={ styles.groupToDelete }/>;
						break;
					case null:
						icon = <CheckAIcon16Solid size={ 14 } className={ styles.createdGroup }/>;
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
					<h4><WarningTriangleIcon16Solid className={ styles.validatingWarning }/> { title }</h4>
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
					</>
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
									const text = texts.validating.buildStudentBelongsToOtherGroup(
										studentName,
										moveInfo
									);
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
					</>
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

				updateGroups();
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
					<Button use={'link'} data-id={ SettingsType.settings } onClick={ onSettingsTabChangeHandler }>
						{ SettingsToTitle[SettingsType.settings] }
					</Button>
				</p>
				<p>
					<Button disabled use={'link'} data-id={ SettingsType.additional } onClick={ onSettingsTabChangeHandler }>
						{ SettingsToTitle[SettingsType.additional] }
					</Button>
				</p>
				<p>
					<Button disabled use={'link'} data-id={ SettingsType.deadlines } onClick={ onSettingsTabChangeHandler }>
						{ SettingsToTitle[SettingsType.deadlines] }
					</Button>
				</p>
			</>
		);
	}

	function closeSettingsModal() {
		onSettingsModalChange(undefined);
	}

	function renderSettingsModalContent(type: SettingsType) {
		switch (type) {
			case SettingsType.settings: {
				return (
					<UpdateSubGroupsSettings
						superGroupId={ groupInfo.id }
						onClose={ closeSettingsModal }
					/>
				);
			}
			case SettingsType.unassignedMembers: {
				return <ManageMembers
					groupId={ groupInfo.id }
					courseId={ groupInfo.courseId }
				/>
			}
			case SettingsType.additional:
				return 'Функциональность пока недоступна';
			case SettingsType.deadlines:
				return 'Функциональность пока недоступна';
		}
	}

	function onSettingsTabChangeHandler(event: React.MouseEvent) {
		event.stopPropagation();
		event.preventDefault();
		const { id } = (event.currentTarget as HTMLElement).parentElement?.dataset ?? {};
		onSettingsModalChange(id);
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
			groupsToUpdate: neededActionsAsDictionary
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

	function updateGroups() {
		props.refetchGroup();
		return _extractGoogleSheet();
	}

	async function _extractGoogleSheet() {
		updateSuperGroupsState({ courseId: groupInfo.courseId }, draft => {
			const index = draft.superGroups.findIndex(source => source.id === groupInfo.id);
			draft.superGroups[index] = {
				...draft.superGroups[index],
				distributionTableLink: tableLink
			};
		});
		setLoading(true);
		setError(null);

		await extractGoogleSheet(tableLink, groupInfo.id)
			.then(r => {
				setExtractionResult(r);
			})
			.catch(err => {
				const reqError = err as RequestError;
				reqError.response
					.text()
					.then(t => setError(t));
			});
		setLoading(false);
	}
}

const mapStateToProps = (state: RootState) => ({
	account: state.account
});

const mapDispatchToProps = (dispatch: AppDispatch) => ({
	updateSuperGroupsState: (params: Partial<GroupsListParameters>, recipe: (draft: SuperGroupsListResponse) => void) =>
		dispatch(superGroupsApi.util.updateQueryData('getGroups', params, recipe))
});

const connected = connect(mapStateToProps, mapDispatchToProps)(SuperGroupPage);
export default withNavigate(connected);
