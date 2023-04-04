import React, { FC, FormEvent, useEffect, useState } from 'react';
import { Button, Loader, Toast } from "ui";
import texts from "./GroupSettings.texts";
import { GroupInfo, GroupScoringGroupInfo } from "../../../../models/groups";
import styles from "./groupSettings.less";
import ReviewSettings from "./GroupReviewSettings/GroupReviewSettings";
import GroupScoresSettingsList from "./GroupScoresSettings/GroupScoresSettingsList";
import GroupNameSettings from "./GroupNameSettings/GroupNameSettings";
import { groupScoresApi } from "../../../../redux/toolkit/api/groups/groupScoresApi";
import { groupSettingsApi } from "../../../../redux/toolkit/api/groups/groupSettingsApi";

interface Props {
	group: GroupInfo;
}

const GroupSettings: FC<Props> = ({ group }) => {
	const [updatedFields, setUpdatedFields] = useState<Partial<GroupInfo>>({});
	useEffect(() => setUpdatedFields({}), [group]);

	const updatedGroup = { ...group, ...updatedFields };

	const { data: scoringInfo, isLoading: isScoringInfoLoading } =
		groupScoresApi.useGetGroupScoresQuery({ groupId: group.id });
	const [updatedScoringInfo, setUpdatedScoringInfo] = useState<GroupScoringGroupInfo[]>([]);
	useEffect(() => setUpdatedScoringInfo(scoringInfo || []), [scoringInfo]);

	const [saveGroupSettings, { isLoading: isGroupSaving }] = groupSettingsApi.useSaveGroupSettingsMutation();
	const [saveScoresSettings, { isLoading: isScoresSaving }] = groupScoresApi.useSaveScoresSettingsMutation();

	const onChangeName = (value: string) => {
		setUpdatedFields({ ...updatedFields, name: value, });
	};

	const onChangeSettings = (field: keyof GroupInfo, value: GroupInfo[keyof GroupInfo]) => {
		setUpdatedFields({ ...updatedFields, [field]: value, });
	};

	const onChangeScoringInfo = (updated: GroupScoringGroupInfo) => {
		setUpdatedScoringInfo(updatedScoringInfo.map(si => si.id === updated.id ? updated : si));
	};

	return (
		<Loader type="big" active={ isScoringInfoLoading }>
			<form onSubmit={ sendSettings }>
				<div className={ styles.wrapper }>
					<GroupNameSettings
						groupName={ updatedGroup.name }
						onChangeName={ onChangeName }
					/>
					<ReviewSettings
						group={ updatedGroup }
						onChangeSettings={ onChangeSettings }
					/>
					{ updatedScoringInfo.length > 0 &&
						<GroupScoresSettingsList
							scoringInfo={ updatedScoringInfo }
							onChangeScoringInfo={ onChangeScoringInfo }
						/> }
				</div>
				<Button
					size="medium"
					use="primary"
					type="submit"
					loading={ isGroupSaving || isScoresSaving }
				>
					{ texts.saveSettings }
				</Button>
			</form>
		</Loader>
	);

	function sendSettings(e: FormEvent) {
		e.preventDefault();

		const promises = [];

		if(Object.values(updatedFields).length) {
			promises.push(
				saveGroupSettings({ groupId: -1, groupSettings: updatedFields }).unwrap()
			);
		}
		if(isScoringInfosChanged()) {
			const checkedScoringInfoIds = updatedScoringInfo
				.filter(scoreInfo =>
					scoreInfo.areAdditionalScoresEnabledInThisGroup &&
					!scoreInfo.areAdditionalScoresEnabledForAllGroups &&
					scoreInfo.canInstructorSetAdditionalScoreInSomeUnit
				).map(item => item.id);

			promises.push(
				saveScoresSettings({ groupId: group.id, checkedScoresSettingsIds: checkedScoringInfoIds }).unwrap()
			);
		}

		Promise.all(promises)
			.then(() => Toast.push(texts.onSaveSuccessfulToast));
	}

	function isScoringInfosChanged(): boolean {
		if(!scoringInfo) {
			return false;
		}
		for (let i = 0; i < scoringInfo.length; i++) {
			if(scoringInfo[i].areAdditionalScoresEnabledInThisGroup !== updatedScoringInfo[i].areAdditionalScoresEnabledInThisGroup) {
				return true;
			}
		}
		return false;
	}
};

export default GroupSettings;
