import React, { FC } from "react";
import styles from './groupScoresSettings.less';
import texts from './GroupScoresSettings.texts';
import { Checkbox } from "ui";
import { GroupScoringGroupInfo } from "../../../../../models/groups";

interface Props {
	initialIndeterminateOnNull?: boolean;
	scoringInfo: GroupScoringGroupInfo;
	onChangeScoringInfo: (updated: GroupScoringGroupInfo) => void;
}

const GroupScoresSettingsItem: FC<Props> = ({ scoringInfo, onChangeScoringInfo, initialIndeterminateOnNull }) => {
	const isChecked = scoringInfo.areAdditionalScoresEnabledInThisGroup ||
		scoringInfo.areAdditionalScoresEnabledForAllGroups;

	const isDisabled = scoringInfo.areAdditionalScoresEnabledForAllGroups ||
		!scoringInfo.canInstructorSetAdditionalScoreInSomeUnit;

	const handleValueChange = (value: boolean) => {
		onChangeScoringInfo({ ...scoringInfo, areAdditionalScoresEnabledInThisGroup: value });
	};
	const initialIndeterminate = !isDisabled && initialIndeterminateOnNull && scoringInfo.areAdditionalScoresEnabledInThisGroup === null;

	return (
		<label className={ styles["settings-checkbox"] }>
			<Checkbox
				initialIndeterminate={ initialIndeterminate }
				checked={ isChecked }
				disabled={ isDisabled }
				onValueChange={ handleValueChange }>
				{ scoringInfo.name }
			</Checkbox>

			{ scoringInfo.description &&
				<div className={ styles["settings-description"] }>
					{ scoringInfo.description }
				</div>
			}

			{ isDisabled &&
				<div className={ styles["settings-description"] }>
					{ renderUnchangeableHint() }
				</div>
			}

		</label>
	);

	function renderUnchangeableHint(): string | undefined {
		if(scoringInfo.areAdditionalScoresEnabledForAllGroups) {
			return texts.defaultEnabledHint;
		}

		if(!scoringInfo.canInstructorSetAdditionalScoreInSomeUnit) {
			return texts.defaultDisabledHint;
		}
	}
};

export default GroupScoresSettingsItem;
