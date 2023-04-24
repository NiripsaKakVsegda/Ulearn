import React, { FC } from 'react';
import { ChangeableGroupSettings, GroupInfo } from "../../../../../models/groups";
import styles from './groupReviewSettings.less';
import { Checkbox } from "ui";
import texts from './GroupReviewSettings.texts';

interface Props {
	settings: ChangeableGroupSettings;
	onChangeSettings: (field: keyof ChangeableGroupSettings,
		value: ChangeableGroupSettings[keyof ChangeableGroupSettings]
	) => void;
}

const enum GroupInfoFields {
	canStudentsSeeGroupProgress = 'canStudentsSeeGroupProgress',
	isManualCheckingEnabled = 'isManualCheckingEnabled',
	isManualCheckingEnabledForOldSolutions = 'isManualCheckingEnabledForOldSolutions',
	defaultProhibitFurtherReview = 'defaultProhibitFurtherReview',
}

const GroupReviewSettings: FC<Props> = ({ settings, onChangeSettings }) => {
	return (
		<div className={ styles["checkbox-block"] }>
			<h4 className={ styles["settings-header"] }>{ texts.reviewSettings }</h4>
			<div>
				{ renderCheckbox(GroupInfoFields.canStudentsSeeGroupProgress, texts.canStudentsSeeGroupProgress) }
				{ renderCheckbox(GroupInfoFields.isManualCheckingEnabled, texts.isManualCheckingEnabled) }
				{ settings.isManualCheckingEnabled && <React.Fragment>
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

	function renderCheckbox(field: keyof ChangeableGroupSettings, text: string, hint?: string) {
		return <label className={ styles["settings-checkbox"] }>
			<Checkbox
				initialIndeterminate={ settings[field] === undefined }
				checked={ settings[field] }
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
