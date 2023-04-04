import React, { FC } from 'react';
import { GroupInfo } from "../../../../../models/groups";
import styles from './groupReviewSettings.less';
import { Checkbox } from "ui";
import texts from './GroupReviewSettings.texts';

interface Props {
	group: GroupInfo;
	onChangeSettings: (field: keyof GroupInfo, value: GroupInfo[keyof GroupInfo]) => void;
}

const enum GroupInfoFields {
	canStudentsSeeGroupProgress = 'canStudentsSeeGroupProgress',
	isManualCheckingEnabled = 'isManualCheckingEnabled',
	isManualCheckingEnabledForOldSolutions = 'isManualCheckingEnabledForOldSolutions',
	defaultProhibitFurtherReview = 'defaultProhibitFurtherReview',
}

const GroupReviewSettings: FC<Props> = ({ group, onChangeSettings }) => {
	return (
		<div className={ styles["checkbox-block"] }>
			<h4 className={ styles["settings-header"] }>{ texts.reviewSettings }</h4>
			<div>
				{ renderCheckbox(GroupInfoFields.canStudentsSeeGroupProgress, texts.canStudentsSeeGroupProgress) }
				{ renderCheckbox(GroupInfoFields.isManualCheckingEnabled, texts.isManualCheckingEnabled) }
				{ group.isManualCheckingEnabled && <React.Fragment>
					{ renderCheckbox(
						GroupInfoFields.isManualCheckingEnabledForOldSolutions,
						texts.isManualCheckingEnabledForOldSolutions,
						texts.isManualCheckingEnabledForOldSolutionsHint
					) }
					{ renderCheckbox(
						GroupInfoFields.defaultProhibitFurtherReview,
						texts.defaultProhibitFurtherReview,
						texts.defaultProhibitFurtherReviewHint
					) }
				</React.Fragment> }
			</div>
		</div>

	);

	function renderCheckbox(field: keyof GroupInfo, text: string, hint?: string) {
		return <label className={ styles["settings-checkbox"] }>
			<Checkbox
				checked={ group[field] as boolean }
				onValueChange={ onValueChange }
			>
				{ text }
			</Checkbox>
			{ hint && <p className={ styles["settings-comment"] }>{ hint }</p> }
		</label>;

		function onValueChange(value: boolean) {
			onChangeSettings(field, value);
		}
	}
};

export default GroupReviewSettings;
