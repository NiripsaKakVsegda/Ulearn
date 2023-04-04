import React, { useState } from "react";
import { Button, Modal, Toast } from "ui";

import api from "src/api";

import { ChangeableGroupSettings, GroupScoringGroupInfo } from "src/models/groups";
import GroupSettings from "../GroupSettings/GroupSettings";

import texts from "./SuperGroup.texts";

interface Props {
	groupsIds: number[];
	onClose: () => void;
	getGroup?: typeof api.groups.getGroup;
	getScores?: typeof api.groups.getGroupScores;
	saveGroupSettings?: typeof api.groups.saveGroupSettings;
	saveScoresSettings?: typeof api.groups.saveScoresSettings;
}

function UpdateSubGroupsSettings({
	groupsIds,
	onClose,
	getGroup = api.groups.getGroup,
	getScores = api.groups.getGroupScores,
	saveGroupSettings = api.groups.saveGroupSettings,
	saveScoresSettings = api.groups.saveScoresSettings,
}: Props) {
	const [isLoading, setIsLoading] = useState(false);
	const [superGroupScores, setSuperGroupScores] = useState<GroupScoringGroupInfo[]>();
	const [changedGroupScoresIds, setChangedGroupScoresIds] = useState<string[]>();
	const [superGroupSettings, setSuperGroupSettings] = useState<ChangeableGroupSettings>();

	if(!isLoading && !superGroupSettings) {
		loadGroupsInfos();
	}

	return (
		<>
			<Modal.Body>
				<GroupSettings
					loading={ isLoading }
					scores={ superGroupScores || [] }
					onChangeSettings={ onChangeSettings }
					onChangeScores={ onChangeScores }
					{ ...superGroupSettings || {} }
					canChangeName={ false }
				/>
			</Modal.Body>
			<Modal.Footer>
				<Button
					use={ 'primary' }
					onClick={ updateGroupSettings }>
					{ texts.updateGroupsSettingsButtonText }
				</Button>
			</Modal.Footer>
		</>);

	function onChangeSettings(field: keyof ChangeableGroupSettings, value: boolean) {
		setSuperGroupSettings(prevState => ({ ...prevState, [field]: value }));
	}

	function onChangeScores(
		key: string,
		field: keyof GroupScoringGroupInfo,
		value: GroupScoringGroupInfo[keyof GroupScoringGroupInfo]
	) {
		if(!superGroupScores) {
			return;
		}

		const updatedScores = superGroupScores
			.map(item => item.id === key ? { ...item, [field]: value } : item);

		const checkedScoresSettingsIds = updatedScores
			.filter(item => item[field] === true)
			.map(item => item.id);

		setSuperGroupScores(updatedScores);
		setChangedGroupScoresIds(checkedScoresSettingsIds);
	}

	async function loadGroupsInfos() {
		setIsLoading(true);

		const groupsRequests = [];
		for (const groupId of groupsIds) {
			groupsRequests.push(getGroup(groupId));
		}

		const scoresRequests = [];
		for (const groupId of groupsIds) {
			scoresRequests.push(getScores(groupId));
		}

		const groupsPromise = Promise
			.all(groupsRequests);
		const scoresPromise = Promise
			.all(scoresRequests);

		await Promise.all([groupsPromise, scoresPromise]);

		const groupsResponses = await groupsPromise;
		const scoresResponses = await scoresPromise;

		let settings: ChangeableGroupSettings = {};
		let scores: GroupScoringGroupInfo[] = [];

		const merge = <T, >(
			fieldName: keyof T,
			maintainer: T,
			changes: T
		) => {
			if(maintainer[fieldName] === changes[fieldName]) {
				return { ...maintainer };
			}

			return { ...maintainer, [fieldName]: undefined };
		};

		for (let i = 0; i < groupsResponses.length; i++) {
			const group = groupsResponses[i];
			const _scores = scoresResponses[i].scores;

			const _settings = group as ChangeableGroupSettings;

			if(i === 0) {
				settings = {
					isManualCheckingEnabledForOldSolutions: _settings.isManualCheckingEnabledForOldSolutions || false,
					isManualCheckingEnabled: _settings.isManualCheckingEnabled || false,
					defaultProhibitFurtherReview: _settings.defaultProhibitFurtherReview || false,
					canStudentsSeeGroupProgress: _settings.canStudentsSeeGroupProgress || false,
				};
				scores = _scores;

				continue;
			}

			//merging settings of 2 groups
			for (const key of Object.keys(settings)) {
				settings = merge(key as keyof ChangeableGroupSettings, settings, _settings);
			}

			//merging scores of 2 groups
			for (let k = 0; k < scores.length; k++) {
				const score = scores[k];
				const _score = _scores[k];

				if(score.areAdditionalScoresEnabledInThisGroup !== _score.areAdditionalScoresEnabledInThisGroup) {
					score.areAdditionalScoresEnabledInThisGroup = undefined;
				}
				// for (const key of Object.keys(score)) {
				// 	//const was = { ...newScores[k] };
				// 	const _key = key as keyof GroupScoringGroupInfo;
				// 	if(score[_key] !== _score[_key]) {
				// 		scores[k][_key] = undefined as never;
				// 	}
				// 	const mergeResult = merge(key as keyof GroupScoringGroupInfo, score, _score);
				// }
			}
		}

		setSuperGroupSettings(settings);
		setSuperGroupScores(scores);
		setIsLoading(false);
	}

	async function updateGroupSettings() {
		await Promise.all(groupsIds
			.map(id => {
				const settingsPromise = superGroupSettings
					? saveGroupSettings(id, superGroupSettings)
					: Promise.resolve();

				const scoresPromise = changedGroupScoresIds
					? saveScoresSettings(id, changedGroupScoresIds)
					: Promise.resolve();

				return Promise.all([
					settingsPromise,
					scoresPromise
				]);
			}))
			.then(r => {
				Toast.push(texts.afterSuccessSettingUpdateToastText);
			});

		onClose();
	}
}

export default UpdateSubGroupsSettings;
