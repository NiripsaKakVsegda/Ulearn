import React, { useState } from "react";
import { Button, Modal, Toast } from "ui";

import api from "src/api";

import { ChangeableGroupSettings, GroupScoringGroupInfo } from "src/models/groups";

import texts from "./SuperGroup.texts";
import ReviewSettings from "../GroupSettings/GroupReviewSettings/GroupReviewSettings";
import GroupScoresSettingsList from "../GroupSettings/GroupScoresSettings/GroupScoresSettingsList";

interface Props {
	superGroupId: number;
	onClose: () => void;
	getGroupSettings?: typeof api.superGroups.getGroupSettings;
	getScores?: typeof api.superGroups.getGroupScores;
	updateGroupSettings?: typeof api.superGroups.updateGroupSettings;
	saveScoresSettings?: typeof api.superGroups.saveScoresSettings;
}

function UpdateSubGroupsSettings({
	superGroupId,
	onClose,
	getGroupSettings = api.superGroups.getGroupSettings,
	getScores = api.superGroups.getGroupScores,
	updateGroupSettings = api.superGroups.updateGroupSettings,
	saveScoresSettings = api.superGroups.saveScoresSettings,
}: Props) {
	const [isLoading, setIsLoading] = useState(false);

	const [scoringInfo, setScoringInfo] = useState<GroupScoringGroupInfo[]>();
	const [updatedScoringInfo, setUpdatedScoringInfo] = useState<GroupScoringGroupInfo[]>();

	const [superGroupSettings, setSuperGroupSettings] = useState<ChangeableGroupSettings>();

	if(!isLoading && !superGroupSettings) {
		loadGroupsInfos();
	}

	return (
		<>
			<Modal.Body>
				<ReviewSettings
					settings={ superGroupSettings || {} }
					onChangeSettings={ onChangeSettings }
				/>
				<GroupScoresSettingsList
					initialIndeterminateOnNull
					scoringInfo={ updatedScoringInfo || [] }
					onChangeScoringInfo={ onChangeScores }
				/>
			</Modal.Body>
			<Modal.Footer>
				<Button
					use={ 'primary' }
					onClick={ _updateGroupSettings }>
					{ texts.updateGroupsSettingsButtonText }
				</Button>
			</Modal.Footer>
		</>);

	function onChangeSettings(field: keyof ChangeableGroupSettings,
		value: ChangeableGroupSettings[keyof ChangeableGroupSettings]
	) {
		setSuperGroupSettings(prevState => ({ ...prevState, [field]: value }));
	}

	function onChangeScores(updated: GroupScoringGroupInfo) {
		if(!updatedScoringInfo) {
			return;
		}

		const updatedScores = updatedScoringInfo
			.map(item => item.id === updated.id ? updated : item);

		setUpdatedScoringInfo(updatedScores);
	}

	async function loadGroupsInfos() {
		setIsLoading(true);

		const scoresPromise = getScores(superGroupId);
		const groupsPromise = getGroupSettings(superGroupId);

		await Promise.all([groupsPromise, scoresPromise]);

		const settingsResponse = await groupsPromise;
		const scoresResponses = await scoresPromise;

		setSuperGroupSettings(settingsResponse);
		setScoringInfo(scoresResponses.scores);
		setUpdatedScoringInfo(scoresResponses.scores);
		setIsLoading(false);
	}

	async function _updateGroupSettings() {
		const checkedScoringInfoIds = isScoringInfosChanged() && updatedScoringInfo
			?.filter(scoreInfo =>
				scoreInfo.areAdditionalScoresEnabledInThisGroup &&
				!scoreInfo.areAdditionalScoresEnabledForAllGroups &&
				scoreInfo.canInstructorSetAdditionalScoreInSomeUnit)
			.map(item => item.id);
		const scoresPromise = checkedScoringInfoIds
			? saveScoresSettings(superGroupId, checkedScoringInfoIds)
			: Promise.resolve();

		const settingsPromise = superGroupSettings
			? updateGroupSettings(superGroupId, superGroupSettings)
			: Promise.resolve();

		await Promise.all([settingsPromise, scoresPromise])
			.then(() => {
				Toast.push(texts.afterSuccessSettingUpdateToastText);
			});

		onClose();
	}

	function isScoringInfosChanged(): boolean {
		if(!scoringInfo || !updatedScoringInfo) {
			return false;
		}
		for (let i = 0; i < scoringInfo.length; i++) {
			if(scoringInfo[i].areAdditionalScoresEnabledInThisGroup !== updatedScoringInfo[i].areAdditionalScoresEnabledInThisGroup) {
				return true;
			}
		}
		return false;
	}
}

export default UpdateSubGroupsSettings;
