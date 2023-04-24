import React, { Component } from "react";
import PropTypes from "prop-types";
import { Checkbox } from "ui";

import styles from './groupSettingsCheckbox.less';

const mapToServerName = {
	oldSolution: 'isManualCheckingEnabledForOldSolutions',
	manualChecking: 'isManualCheckingEnabled',
	review: 'defaultProhibitFurtherReview',
	progress: 'canStudentsSeeGroupProgress',
};

class GroupSettingsCheckbox extends Component {
	constructor(props) {
		super(props);
		this.bindProgress = this.onChange.bind(this, 'progress');
		this.bindManualChecking = this.onChange.bind(this, 'manualChecking');
		this.bindOldSolution = this.onChange.bind(this, 'oldSolution');
		this.bindReview = this.onChange.bind(this, 'review');
	}

	render() {
		const { isManualCheckingEnabled, canStudentsSeeGroupProgress } = this.props;

		return (
			<React.Fragment>
				<label className={ styles["settings-checkbox"] }>
					{ this.renderSettings(canStudentsSeeGroupProgress, "Открыть ведомость курса студентам", this.bindProgress) }
				</label>
				<label className={ styles["settings-checkbox"] }>
					{ this.renderSettings(isManualCheckingEnabled,
						"Включить код-ревью и ручную проверку тестов для участников группы",
						this.bindManualChecking) }
				</label>
				{ isManualCheckingEnabled && this.renderReviewSettings() }
			</React.Fragment>
		)
	}

	renderReviewSettings() {
		const { isManualCheckingEnabledForOldSolutions, defaultProhibitFurtherReview } = this.props;

		return (
			<React.Fragment>
				<label className={ styles["settings-checkbox"] }>
					{ this.renderSettings(isManualCheckingEnabledForOldSolutions,
						"Отправить на код-ревью и ручную проверку тестов старые решения участников",
						this.bindOldSolution) }
					<p className={ styles["settings-comment"] }>Если эта опция выключена, то при вступлении
						студента в группу его старые решения не будут отправлены на код-ревью</p>
				</label>
				<label className={ styles["settings-checkbox"] }>
					{ this.renderSettings(defaultProhibitFurtherReview, "По умолчанию запрещать второе прохождение код-ревью",
						this.bindReview) }
					<p className={ styles["settings-comment"] }>В каждом код-ревью вы сможете выбирать,
						разрешить ли студенту второй раз отправить свой код на проверку.
						Эта опция задаёт лишь значение по умолчанию</p>
				</label>
			</React.Fragment>
		)
	};

	renderSettings(checked, text, callback) {
		return (
			<Checkbox
				initialIndeterminate={ checked === undefined }
				checked={ checked }
				onValueChange={ callback }>
				{ text }
			</Checkbox>
		)
	};

	onChange = (field, value) => {
		const { onChangeSettings } = this.props;
		onChangeSettings(mapToServerName[field], value);
	};
}

GroupSettingsCheckbox.propTypes = {
	onChangeSettings: PropTypes.func,
	isManualCheckingEnabled: PropTypes.bool,
	canStudentsSeeGroupProgress: PropTypes.bool,
	isManualCheckingEnabledForOldSolutions: PropTypes.bool,
	defaultProhibitFurtherReview: PropTypes.bool,
};

export default GroupSettingsCheckbox;

