import React, { FC } from 'react';
import styles from "./groupScoresSettings.less";
import texts from "./GroupScoresSettings.texts";
import GroupScoresSettingsItem from "./GroupScoresSettingsItem";
import { GroupScoringGroupInfo } from "../../../../../models/groups";

interface Props {
	scoringInfo: GroupScoringGroupInfo[];
	onChangeScoringInfo: (updated: GroupScoringGroupInfo) => void;
	initialIndeterminateOnNull?: boolean;
}

const GroupScoresSettingsList: FC<Props> = ({ scoringInfo, onChangeScoringInfo, initialIndeterminateOnNull }) => {
	return (
		<div>
			<h4 className={ styles["settings-header"] }>{ texts.scores }</h4>
			<p className={ styles["settings-text"] }>{ texts.scoresHint }</p>
			{ scoringInfo.map(score =>
				<GroupScoresSettingsItem
					key={ score.id }
					scoringInfo={ score }
					onChangeScoringInfo={ onChangeScoringInfo }
					initialIndeterminateOnNull={ initialIndeterminateOnNull }
				/>
			) }
		</div>
	);
};

export default GroupScoresSettingsList;
