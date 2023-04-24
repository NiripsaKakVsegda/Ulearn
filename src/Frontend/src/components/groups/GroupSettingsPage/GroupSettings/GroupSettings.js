import React, { Component } from "react";
import PropTypes from "prop-types";
import { Input, Loader } from "ui";
import GroupScores from "./GroupScores/GroupScores";
import GroupSettingsCheckbox from "./GroupSettingsCheckbox/GroupSettingsCheckbox";

import styles from './groupSettings.less';

class GroupSettings extends Component {
	render() {
		const {
			scores,
			onChangeSettings,
			loading,
			onChangeScores,
			isManualCheckingEnabled,
			canStudentsSeeGroupProgress,
			isManualCheckingEnabledForOldSolutions,
			defaultProhibitFurtherReview,
			canChangeName
		} = this.props;

		return (
			<Loader type="big" active={loading}>
				<div className={styles.wrapper}>
					{canChangeName && this.renderChangeGroupName()}
					<div className={`${styles["checkbox-block"]} ${styles.settings}`}>
						<h4 className={styles["settings-header"]}>Код-ревью и проверка тестов</h4>
						<GroupSettingsCheckbox
							onChangeSettings={onChangeSettings}
							isManualCheckingEnabled={isManualCheckingEnabled}
							canStudentsSeeGroupProgress={canStudentsSeeGroupProgress}
							isManualCheckingEnabledForOldSolutions={isManualCheckingEnabledForOldSolutions}
							defaultProhibitFurtherReview={defaultProhibitFurtherReview}
						/>
					</div>
					{scores.length > 0 &&
					<div className={styles.settings}>
						<h4 className={styles["settings-header"]}>Баллы</h4>
						<p className={styles["settings-text"]}>Преподаватели могут выставлять студентам группы
							следующие
							баллы:</p>
						{scores.map(score =>
							<GroupScores
								key={score.id}
								score={score}
								onChangeScores={onChangeScores} />
						)}
					</div>
					}
				</div>
			</Loader>
		)
	}

	renderChangeGroupName() {
		return (
			<div className={styles["change-name"]}>
				<header className={styles["change-name-header"]}>
					<h4 className={styles["change-name-label"]}>Название группы</h4>
				</header>
				<div className={styles["change-name-input"]}>
					<Input
						type="text"
						required
						size="small"
						error={this.props.error}
						value={this.inputValue}
						placeholder="Здесь вы можете изменить название группы"
						onValueChange={this.onChangeName}
						width="100%" />
				</div>
			</div>
		)
	}

	get inputValue() {
		const {name} = this.props;

		return name || '';
	}

	onChangeName = (value) => {
		this.props.onChangeName(value);
	};
}

GroupSettings.propTypes = {
	name: PropTypes.string,
	error: PropTypes.bool,
	loading: PropTypes.bool,
	scores: PropTypes.array,
	onChangeSettings: PropTypes.func,
	onChangeName: PropTypes.func,
	onChangeScores: PropTypes.func,
	isManualCheckingEnabled: PropTypes.bool,
	canStudentsSeeGroupProgress: PropTypes.bool,
	isManualCheckingEnabledForOldSolutions: PropTypes.bool,
	defaultProhibitFurtherReview: PropTypes.bool,
};

export default GroupSettings;
